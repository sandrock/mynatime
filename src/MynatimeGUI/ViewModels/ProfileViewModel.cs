
namespace MynatimeGUI.ViewModels
{
    using Mynatime.Infrastructure;
    using MynatimeClient;
    using Newtonsoft.Json.Linq;
    using ReactiveUI;
    using System;

    public class ProfileViewModel : ViewModelBase
    {
        private string title;
        private string subtitle;
        private string username;
        private DateTime lastCheckTimeUtc;
        private bool isHome;
        private IManatimeWebClient client;
        private string? configurationPath;
        private string status;
        private MynatimeProfile? configuration;

        public static ProfileViewModel CreateHome()
        {
            var item = new ProfileViewModel();
            item.IsHome = true;
            item.title = "Home";
            item.subtitle = "Authenticate, configure...";
            return item;
        }

        public bool IsHome
        {
            get => this.isHome;
            set => this.isHome = value;
        }

        public string Title
        {
            get { return this.title; }
            set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

        public string Subtitle
        {
            get { return this.subtitle; }
            set { this.RaiseAndSetIfChanged(ref this.subtitle, value); }
        }

        public IManatimeWebClient Client
        {
            get => this.client;
            set => this.client = value;
        }

        public string Username
        {
            get => this.username;
            set
            {
                this.username = value;
                this.Title = value;
            }
        }

        public string? ConfigurationPath
        {
            get => this.configurationPath;
            set => this.configurationPath = value;
        }

        public DateTime LastCheckTimeUtc
        {
            get => this.lastCheckTimeUtc;
            set => this.RaiseAndSetIfChanged(ref this.lastCheckTimeUtc, value);
        }

        public string Status
        {
            get => this.status;
            set => this.RaiseAndSetIfChanged(ref this.status, value);
        }

        public string? Password { get; set; }

        public void SetConfiguration(MynatimeProfile root)
        {
            this.configuration = root ?? throw new ArgumentNullException(nameof(root));

            var identity = root.Element["Identity"] as JObject;
            this.Username = root.LoginUsername;
            this.Password = root.LoginPassword;
        }

        public MynatimeProfile? GetConfiguration()
        {
            return this.configuration;
        }
    }
}
