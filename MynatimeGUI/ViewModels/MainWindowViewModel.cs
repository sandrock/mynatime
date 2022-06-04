
namespace MynatimeGUI.ViewModels
{
    using MynatimeClient;
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
    using System.Threading.Tasks;

    public class MainWindowViewModel : ViewModelBase
    {         
        public const string DateTimeFlatPrecision3 = "yyyyMMdd'T'HHmmssfffK"; 
        
        private ProfileViewModel? selectedProfile;
        private IManatimeWebClient client;
        private string? loginStatus;
        private string loginUsername;
        private string loginPassword;

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

        public async Task Initialize()
        {
            this.SelectedProfile = this.Profiles.FirstOrDefault()!;
            this.LoginStatus = "Loading profiles... ";
            await this.LoadConfiguration();
            this.LoginStatus = "Ready. ";
        }

        private async Task LoadConfiguration()
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            foreach (var file in directory.EnumerateFiles("profile.*.json"))
            {
                ProfileViewModel? profile = null;
                try
                {
                    var contents = await File.ReadAllTextAsync(file.FullName, Encoding.UTF8);
                    var root = (JObject)JsonConvert.DeserializeObject(contents);
                    profile = new ProfileViewModel();
                    profile.ConfigurationPath = file.FullName;
                    profile.SetConfiguration(root);
                    
                    profile.Client = new ManatimeWebClient();
                    profile.Client.SetCookies((JArray)root["Cookies"]);
                    profile.Status = "Restoring... ";
                    this.Profiles.Add(profile);

                    var checkTask = Task.Factory.StartNew(async () => await this.RefreshProfile(profile));
                }
                catch (Exception ex)
                {
                    if (profile != null)
                    {
                        profile.Status = ex.Message;
                    }
                }
            }
        }

        private async Task RefreshProfile(ProfileViewModel? profile)
        {
            var home = await profile.Client.GetHomepage();
            if (home.Succeed)
            {
                profile.LastCheckTimeUtc = DateTime.UtcNow;
                profile.Status = "OK";
            }
            else
            {
                profile.Status = home.GetErrorMessage() ?? "???";
            }
            
            await this.SaveProfile(profile, home);
        }

        private async Task DoLogin()
        {
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

            var username = this.loginUsername;
            var result = await this.client.EmailPasswordAuthenticate(username, this.loginPassword);
            if (result.Succeed)
            {
                var profile = new ProfileViewModel();
                profile.Username = username;
                profile.Client = this.client;
                profile.LastCheckTimeUtc = DateTime.UtcNow;
                this.Profiles.Add(profile);

                this.client = new ManatimeWebClient();

                await this.SaveProfile(profile, result);
                this.LoginStatus = "Ready. ";
            }
            else
            {
                this.LoginStatus = result.GetErrorMessage();
            }
        }

        private async Task SaveProfile(ProfileViewModel? profile, PageResult result)
        {
            string path;
            if (profile.ConfigurationPath != null)
            {
                path = profile.ConfigurationPath;
            }
            else
            {
                path = Path.Combine(Environment.CurrentDirectory, "profile." + DateTime.UtcNow.ToString(DateTimeFlatPrecision3) + ".json");
                profile.ConfigurationPath = path;
            }

            var root = new JObject();
            root.Add("__manifest", "MynatimeProfile");
            root.Add("UserId", new JValue(result.UserId));
            root.Add("GroupId", new JValue(result.GroupId));
            root.Add("Identity", result.Identity?.DeepClone());
            root.Add("Group", result.Group?.DeepClone());
            root.Add("Cookies", profile.Client.GetCookies());
            await File.WriteAllTextAsync(path, root.ToString(Formatting.Indented), Encoding.UTF8);
        }

        private Unit OpenProfile(ProfileViewModel param)
        {
            this.OpenProfileWindow?.Invoke(this, new DataEventArgs<string>(param.ConfigurationPath));
            return Unit.Default;
        }
    }
}
