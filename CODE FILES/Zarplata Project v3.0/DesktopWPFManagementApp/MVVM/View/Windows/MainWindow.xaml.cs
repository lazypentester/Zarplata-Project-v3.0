using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System;
using System.Windows.Interop;
using DesktopWPFManagementApp.UIHelpers;

namespace DesktopWPFManagementApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Application.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary() { Source = new Uri("") });
        }

        private void ButtonCloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (RefreshSessionPopup.IsOpen)
            {
                RefreshSessionPopup.IsOpen = false;
            }

            Application.Current.Shutdown();
        }

        private void ButtonMaximizeApp_Click(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
        }

        private void ButtonMinimizeApp_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void dragPanelBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                MaximizeWindow();
            }
            else
            {
                WindowDragHelper.DragMove(this);
            }
        }

        private void MaximizeWindow()
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void dragPanelBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
    }
}
