using DesktopWPFManagementApp.MVVM.View.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopWPFManagementApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void ApplicationStart(object sender, StartupEventArgs e)
        {
            AuthWindow authWindow = new AuthWindow();
            MainWindow mainWindow;
            authWindow.Show();
            authWindow.IsVisibleChanged += (s, e) =>
            {
                if(!authWindow.IsVisible && authWindow.IsLoaded)
                {
                    mainWindow = new MainWindow();
                    mainWindow.IsVisibleChanged += (s, e) =>
                    {
                        if (!mainWindow.IsVisible && mainWindow.IsLoaded)
                        {
                            Application.Current.MainWindow = authWindow;
                            try
                            {
                                mainWindow.Close();
                            }
                            catch { }
                            authWindow.Show();
                        }
                    };
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                }
            };
        }
    }
}
