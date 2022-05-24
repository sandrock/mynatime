
namespace MynatimeGUI.Views
{
    using Avalonia.Controls;
    using MynatimeGUI.ViewModels;
    using System;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
            var task = ((MainWindowViewModel)this.DataContext!).Initialize();
        }
    }
}