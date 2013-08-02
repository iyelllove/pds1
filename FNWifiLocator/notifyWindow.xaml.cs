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
using System.Windows.Threading;

namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per notifyWindow.xaml
    /// </summary>
    public partial class notifyWindow : Window
    {
        public bool canClose = true;
        public notifyWindow()
        {
            InitializeComponent();
            this.label.Content = "";
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = SystemParameters.WorkArea.Bottom - this.Height;
            //this.Show();
        }

        public notifyWindow(string msg)
        {
            InitializeComponent();
            this.label.Content = msg;
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = SystemParameters.WorkArea.Bottom - this.Height;
        }




        public void ShowNotify() {

            this.Show();
            DispatcherTimer dt = new DispatcherTimer(); //a new DispatcherTimer
            dt.Interval = new TimeSpan(0, 0, 3); //set the interval 1 second
            dt.Start(); //start the timer
            dt.Tick += new EventHandler(dt_Tick); //set the timer tick
        
        }

     void dt_Tick(object sender, EventArgs e) //timer tick
     {
         this.Close();
     }

        
    }
}
