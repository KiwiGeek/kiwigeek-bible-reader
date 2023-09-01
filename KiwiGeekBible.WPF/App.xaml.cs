using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;

namespace KiwiGeekBible.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            StyleManager.ApplicationTheme = new Office2019Theme( Office2019Palette.ColorVariation.Gray);
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
