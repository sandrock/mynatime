
namespace MynatimeGUI.ViewModels
{
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MainWindowViewModel : ViewModelBase
    {
        private ProfileViewModel selectedProfile;

        public MainWindowViewModel()
        {
            this.Profiles = new ObservableCollection<ProfileViewModel>();
            this.Profiles.Add(ProfileViewModel.CreateHome());
        }

        public ObservableCollection<ProfileViewModel> Profiles { get; }

        public string Greeting => "Welcome to Avalonia!";

        public ProfileViewModel SelectedProfile
        {
            get => this.selectedProfile;
            set => this.RaiseAndSetIfChanged(ref this.selectedProfile, value);
        }

        public async Task Initialize()
        {
            this.SelectedProfile = this.Profiles.FirstOrDefault()!;
            await this.LoadConfiguration();
        }

        private async Task LoadConfiguration()
        {
            
        }
    }
}
