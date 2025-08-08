using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using DesktopWPFManagementApp.MVVM.ViewModel.Windows;
using DesktopWPFManagementApp.UIHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopWPFManagementApp.MVVM.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        #region WinControlButtonAndDragControl
        private void ButtonCloseApp_Click(object sender, RoutedEventArgs e)
        {
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
            if (e.ClickCount == 2)
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
        #endregion

        public AuthWindow()
        {
            InitializeComponent();
        }
    }
}
