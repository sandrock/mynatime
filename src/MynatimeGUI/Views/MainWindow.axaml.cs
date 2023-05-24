
namespace Mynatime.GUI.Views
{
    using Avalonia.Controls;
    using Microsoft.Extensions.Logging;
    using Mynatime.GUI.ViewModels;
    using Mynatime.GUI.Views;
    using Mynatime.Infrastructure;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Avalonia.Interactivity;
    using Mynatime.GUI.Things;

    public partial class MainWindow : Window
    {
        private readonly ILogger log = Log.GetLogger<MainWindow>();

        public MainWindow()
        {
            this.InitializeComponent();
            this.ProfileWindows = new List<ProfileWindow>();
            this.ActivityWindows = new List<ActivityWindow>();
            this.log.LogInformation("hey");
        }

        public List<ProfileWindow> ProfileWindows { get; }

        public List<ActivityWindow> ActivityWindows { get; }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            var context = (MainWindowViewModel)this.DataContext!; 
            var task = context.Initialize();

            context.OpenProfileWindow += (s, e) =>
            {
                if (e.Data is null)
                {
                    return;
                }

                var found = false;
                foreach (var window in this.ProfileWindows)
                {
                    if (window.ProfileFilePath == e.Data)
                    {
                        this.log.LogInformation("Window exists for profile {0}", e.Data);
                        found = true;
                        try
                        {
                            // this crashes once the window closed :(
                            window.Activate();
                            window.Focus();
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex.ToString());
                        }
                    }
                }

                if (!found)
                {
                    this.log.LogInformation("Create window for profile {0}", e.Data);
                    var window = new ProfileWindow(e.Data);
                    this.ProfileWindows.Add(window);
                    window.Show(this);
                }
            };
            context.OpenActivityWindow += (s, e) =>
            {
                if (e.Data is null)
                {
                    return;
                }

                var found = false;
                foreach (var window in this.ActivityWindows)
                {
                    if (window.ProfileFilePath == e.Data)
                    {
                        this.log.LogInformation("Activity window exists for profile {0}", e.Data);
                        found = true;
                        try
                        {
                            // this crashes once the window closed :(
                            window.Activate();
                            window.Focus();
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex.ToString());
                        }
                    }
                }

                if (!found)
                {
                    this.log.LogInformation("Create activity window for profile {0}", e.Data);
                    var window = new ProfileWindow(e.Data);
                    this.ProfileWindows.Add(window);
                    window.Show(this);
                }
            };
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            var url = "https://github.com/sandrock/mynatime";
            Utility.OpenUrl(url);
        }

        private void Button_OnClick2(object? sender, RoutedEventArgs e)
        {
            Utility.OpenUrl(MynatimeConstants.ServiceBaseUrl);
        }

        private void Button_OnClick3(object? sender, RoutedEventArgs e)
        {
            Utility.OpenUrl("https://avaloniaui.net/");
        }
    }
}
