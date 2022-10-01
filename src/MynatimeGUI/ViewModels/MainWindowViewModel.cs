
namespace Mynatime.GUI.ViewModels
{
    using Microsoft.Extensions.Logging;
    using Mynatime.Client;
    using Mynatime.Infrastructure;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class MainWindowViewModel : ViewModelBase
    {         
        public const string DateTimeFlatPrecision3 = "yyyyMMdd'T'HHmmssfffK";

        private readonly ILogger log = Log.GetLogger<MainWindowViewModel>();

        private ProfileViewModel? selectedProfile;
        private IManatimeWebClient client;
        private string? loginStatus;
        private string loginUsername;
        private string loginPassword;
        private bool loginRememberPassword;

        public MainWindowViewModel()
        {
            this.Profiles = new ObservableCollection<ProfileViewModel?>();
            this.LoginCommand = ReactiveCommand.CreateFromTask(this.DoLogin);
            this.client = new ManatimeWebClient();
            this.OpenProfileCommand = ReactiveCommand.Create<ProfileViewModel, Unit>(this.OpenProfile);
        }

        public ObservableCollection<ProfileViewModel?> Profiles { get; }

        public ProfileViewModel? SelectedProfile
        {
            get => this.selectedProfile;
            set => this.RaiseAndSetIfChanged(ref this.selectedProfile, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        public ReactiveCommand<ProfileViewModel,Unit> OpenProfileCommand { get; set; }

        public event EventHandler<DataEventArgs<string>> OpenProfileWindow;

        public string? LoginStatus
        {
            get { return this.loginStatus; }
            set { this.RaiseAndSetIfChanged(ref this.loginStatus, value); }
        }

        public string LoginUsername
        {
            get { return this.loginUsername; }
            set { this.RaiseAndSetIfChanged(ref this.loginUsername, value); }
        }

        public string LoginPassword
        {
            get { return this.loginPassword; }
            set { this.RaiseAndSetIfChanged(ref this.loginPassword, value); }
        }

        public bool LoginRememberPassword
        {
            get { return this.loginRememberPassword; }
            set { this.RaiseAndSetIfChanged(ref this.loginRememberPassword, value); }
        }

        public async Task Initialize()
        {
            this.SelectedProfile = this.Profiles.FirstOrDefault()!;
            this.LoginStatus = "Loading profiles... ";
            log.LogInformation("Loading profiles... ");
            await this.LoadConfiguration();
            log.LogInformation("Loading profiles... done. ");
            this.LoginStatus = "Ready. ";
        }

        private async Task LoadConfiguration()
        {
            var directory = MynatimeConfiguration.GetConfigDirectory();
            foreach (var file in directory.EnumerateFiles("profile.*.json"))
            {
                ProfileViewModel? profile = null;
                try
                {
                    this.log.LogInformation("Loading profile <{0}>... ", file.FullName);
                    MynatimeProfile config;
                    try
                    {
                        config = await MynatimeProfile.LoadFromFile(file.FullName);
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.log.LogWarning("Loading profile <{0}> failed: {1}", file.FullName, ex.Message);
                        continue;
                    }

                    profile = new ProfileViewModel();
                    profile.ConfigurationPath = file.FullName;
                    profile.SetConfiguration(config);

                    profile.Client = new ManatimeWebClient();
                    if (config.Cookies != null)
                    {
                        profile.Client.SetCookies(config.Cookies);
                    }

                    profile.Status = "Restoring... ";
                    this.Profiles.Add(profile);

                    this.log.LogInformation("Refreshing profile <{0}>... ", file.FullName);
                    var checkTask = Task.Factory.StartNew(async () => await this.RefreshProfile(profile));
                }
                catch (Exception ex)
                {
                    this.log.LogWarning("Failed to load profile <{0}>: {1}", file.FullName, ex.ToString());
                    if (profile != null)
                    {
                        profile.Status = ex.Message;
                    }
                }
            }
        }

        private async Task RefreshProfile(ProfileViewModel profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var config = profile.GetConfiguration();
            if (config == null)
            {
                return;
            }

            var home = await profile.Client.GetHomepage();
            if (home.Succeed)
            {
                this.log.LogInformation("Refreshed profile <{0}>. ", profile.ConfigurationPath);
                profile.LastCheckTimeUtc = DateTime.UtcNow;
                profile.Status = "OK";
            }
            else if (home.GetErrorCode() == "LoggedOut" && profile.Password != null)
            {
                this.log.LogInformation("Profile <{0}> is <{1}>, re-authenticating... ", profile.ConfigurationPath, home.GetErrorCode());
                var result = await profile.Client.EmailPasswordAuthenticate(profile.Username, profile.Password);
                home = result;
                if (result.Succeed)
                {
                    this.log.LogInformation("Refreshed profile <{0}>. ", profile.ConfigurationPath);
                    profile.Client = this.client;
                    profile.LastCheckTimeUtc = DateTime.UtcNow;
                }
                else
                {
                    this.log.LogInformation("Failed to re-authenticate profile <{0}>: {1}. ", profile.ConfigurationPath, result.GetErrorCode());
                    this.LoginStatus = result.GetErrorMessage();
                    profile.Status = result.GetErrorMessage() ?? "???";
                }
            }
            else
            {
                this.log.LogInformation("Failed to refresh profile <{0}>: {1}. ", profile.ConfigurationPath, home.GetErrorCode());
                profile.Status = home.GetErrorMessage() ?? "???";
            }

            await this.SaveProfile(profile, home);
        }

        private async Task DoLogin()
        {
            var username = this.loginUsername;
            var password = this.loginPassword;

            this.LoginStatus = "Checking... ";
            var prepare = await this.client.PrepareEmailPasswordAuthenticate();
            if (prepare.Succeed)
            {
                this.LoginStatus = "Online. Authenticating... ";
            }
            else
            {
                this.LoginStatus = "Offline! ";
                return;
            }

            var result = await this.client.EmailPasswordAuthenticate(username, password);
            if (result.Succeed)
            {
                var profile = new ProfileViewModel();
                profile.Username = username;
                profile.Client = this.client;
                profile.LastCheckTimeUtc = DateTime.UtcNow;
                profile.Password = this.loginRememberPassword ? password : null;
                this.Profiles.Add(profile);

                this.client = new ManatimeWebClient();

                await this.SaveProfile(profile, result);
                this.LoginStatus = "Ready. ";
                profile.Status = "Ready. ";
            }
            else
            {
                this.LoginStatus = result.GetErrorMessage();
            }
        }

        private async Task SaveProfile(ProfileViewModel profile, PageResult result)
        {
            MynatimeProfile config = profile.GetConfiguration()! ?? new MynatimeProfile();

            var directory = MynatimeConfiguration.EnsureConfigDirectory();
            string path = profile.ConfigurationPath ?? config.FilePath ?? Path.Combine(directory.FullName, MynatimeConfiguration.GetNewProfileFileName());
            profile.ConfigurationPath = path;

            config.LoginUsername = profile.Username;
            config.LoginPassword = profile.Password;

            if (result.Succeed)
            {
                config.UserId = result.UserId;
                config.GroupId = result.GroupId;

                if (result.Identity != null)
                {
                    config.Identity = (JObject)result.Identity.DeepClone();
                }

                if (result.Group != null)
                {
                    config.Group = (JObject)result.Group.DeepClone();
                }
            }

            config.Cookies = profile.Client.GetCookies();

            await config.SaveToFile(path);
        }

        private Unit OpenProfile(ProfileViewModel param)
        {
            this.OpenProfileWindow?.Invoke(this, new DataEventArgs<string>(param.ConfigurationPath));
            return Unit.Default;
        }
    }
}
