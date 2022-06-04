
namespace MynatimeGUI.Views
{
    using Avalonia.Controls;
    using Microsoft.Extensions.Logging;
    using Mynatime.Infrastructure;
    using MynatimeGUI.Views;
    using MynatimeGUI.ViewModels;
    using System;
    using System.Collections.Generic;

    public partial class MainWindow : Window
    {
        private readonly ILogger log = Log.GetLogger<MainWindow>();

        public MainWindow()
        {
            this.InitializeComponent();
            this.ProfileWindows = new List<ProfileWindow>();
            this.log.LogInformation("hey");
        }

        public List<ProfileWindow> ProfileWindows { get; }

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
        }
    }
}