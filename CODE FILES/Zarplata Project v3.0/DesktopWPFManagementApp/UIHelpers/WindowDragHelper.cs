using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace DesktopWPFManagementApp.UIHelpers
{
    public static class WindowDragHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        public static void DragMove(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            SendMessage(helper.Handle, 161, 2, 0);
        }
    }
}
