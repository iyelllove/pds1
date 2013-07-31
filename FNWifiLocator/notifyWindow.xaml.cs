using System;
using System.Collections.Generic;
using System.Linq;
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

namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per notifyWindow.xaml
    /// </summary>
    public partial class notifyWindow : Window
    {
        public notifyWindow(string msg)
        {
            InitializeComponent();
           
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = SystemParameters.WorkArea.Bottom - this.Height;
            this.Show();
            // Screen.PrimaryScreen.WorkingArea.Size;
        }
    }
}
