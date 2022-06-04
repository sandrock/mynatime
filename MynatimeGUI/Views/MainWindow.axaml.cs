
namespace MynatimeGUI.Views
{
    using Avalonia.Controls;
    using MynatimeGUI.Views;
    using MynatimeGUI.ViewModels;
    using System;
    using System.Collections.Generic;

    public partial class MainWindow : Window
    {
        public MainWindow(/*ILogger<MainWindow> logger*/)
        {
            this.InitializeComponent();
            this.ProfileWindows = new List<ProfileWindow>();
        }

        public List<ProfileWindow> ProfileWindows { get; }

        private void OnInitialized(object? sender, EventArgs e)
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
                        found = true;
                        try
                        {
                            // this crashes once the window closed :(
                            window.Show(this);
                            window.Focus();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

                if (!found)
                {
                    var window = new ProfileWindow(e.Data);
                    this.ProfileWindows.Add(window);
                    window.Show(this);
                }
            };
        }
    }
}