﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Threading;


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


        private void notifyIcon1_DoubleClick(object sender,
                                     System.EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

       /* private void button1_Click(object sender, EventArgs e)
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

                    Label strenght = new Label();
                    name.Text = new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));
                    power.Text = network.wlanSignalQuality.ToString();
                    strenght.Text = network.rssi.ToString();
                    tlp.Controls.Add(name,0,i);
                    tlp.Controls.Add(power, 1, i);
                    i++;

                }
            }
            
        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            WlanClient client = new WlanClient();
            // Wlan = new WlanClient();
            int j = 0;
            try
            {
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();
                    foreach (Wlan.WlanBssEntry network in wlanBssEntries)
                    {
                        int rss = network.rssi;
                        byte[] macAddr = network.dot11Bssid;
                        string tMac = "";
                        for (int i = 0; i < macAddr.Length; i++)
                        {
                            tMac += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper();
                        }

                        Label name = new Label();
                        Label signal = new Label();
                        Label strenght = new Label();
                        Label bss = new Label();
                        Label mac = new Label();

                        name.Text = System.Text.ASCIIEncoding.ASCII.GetString(network.dot11Ssid.SSID).ToString();
                        signal.Text = network.linkQuality.ToString();
                        strenght.Text = rss.ToString();
                        bss.Text = network.dot11BssType.ToString();
                        mac.Text = tMac;
                        /*
                        var db = new BloggingContext();
                        var m = new Measure { SSID = name.Text, timestamp = DateTime.Now };
                        db.Measures.Add(m);
                        db.SaveChanges();
                        */

                        tlp.Controls.Add(name, 0, j);
                        tlp.Controls.Add(signal, 1, j);
                        tlp.Controls.Add(strenght, 2, j);
                        tlp.Controls.Add(bss, 3, j);
                        tlp.Controls.Add(mac, 4, j);

                        j++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }



        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void showWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
                Hide();
        }

        private void notifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {

        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                {                                               
                    Hide();
                    notifyIcon1.Visible = true;
                    Thread.Sleep(3000);//pause for 3 seconds
                    //shows a balloon for 1 sec with a title, some text, and the info icon
                    //other possibilities are: TooltipIcon.None, Tooltipicon.Error, and TooltipIcon.Warning
                    notifyIcon1.ShowBalloonTip(1000, "Attenzione", "doppio click per riaprire!", ToolTipIcon.Info);
                }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }


      
    }
}