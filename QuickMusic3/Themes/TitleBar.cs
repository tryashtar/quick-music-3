using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuickMusic3
{
    public partial class TitleBar : ResourceDictionary
    {
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            if (window.WindowState == WindowState.Normal)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }
    }
}
