using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Text;


namespace pds1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);
            Resize += new EventHandler(Form1_Resize);
        }


        private void Form1_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void notifyIcon1_DoubleClick(object sender,
                                     System.EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WlanClient wlan = new WlanClient();
            int i = 0;
            foreach (WlanClient.WlanInterface wlanIface in wlan.Interfaces)
            {
                Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                //tlp.RowCount= networks.Length-1;
               
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    Wlan.Dot11Ssid ssid = network.dot11Ssid;
                    Console.Out.WriteLine();
                    Label name = new Label();
                    Label power = new Label();
                    name.Text = new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));
                    power.Text = network.wlanSignalQuality.ToString();
                    tlp.Controls.Add(name,0,i);
                    tlp.Controls.Add(power, 1, i);
                    i++;

                }
            }
            
        }

      

      
    }
}
