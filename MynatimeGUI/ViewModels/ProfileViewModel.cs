
namespace MynatimeGUI.ViewModels
{
    using ReactiveUI;

    public class ProfileViewModel : ViewModelBase
    {
        private string title;
        private string subtitle;

        public static ProfileViewModel CreateHome()
        {
            var item = new ProfileViewModel();
            item.IsHome = true;
            item.title = "Home";
            item.subtitle = "Authenticate, configure...";
            return item;
        }

        public bool IsHome { get; set; }

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
    }
}
