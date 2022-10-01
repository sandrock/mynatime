namespace Mynatime.GUI.ViewModels;

using ReactiveUI;
using System.Threading.Tasks;

public class ProfileWindowViewModel : ViewModelBase
{
    private string profileFilePath;

    public string ProfileFilePath
    {
        get => this.profileFilePath;
        set => this.RaiseAndSetIfChanged(ref this.profileFilePath, value);
    }

    public Task Initialize(string profileFilePath)
    {
        this.ProfileFilePath = profileFilePath;
        return Task.CompletedTask;
    }
}
