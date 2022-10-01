
namespace Mynatime.GUI.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mynatime.GUI.ViewModels;
using System;

public partial class ProfileWindow : Window
{
    public ProfileWindow()
        : this("NNNNNNNNNN")
    {
    }

    public ProfileWindow(string profileFilePath)
    {
        this.ProfileFilePath = profileFilePath;
        this.InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public string ProfileFilePath { get; }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        var context = (ProfileWindowViewModel)this.DataContext!; 
        var task = context.Initialize(this.ProfileFilePath);
    }
}
