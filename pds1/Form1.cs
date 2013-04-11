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
using System.Threading;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;


namespace pds1
{
    public partial class Form1 : Form
    {
        CurrentState oldstate;

        public Form1()
        {
            oldstate = new CurrentState();
           
            InitializeComponent();
            notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);
            Resize += new EventHandler(Form1_Resize);
            NetworkChange.NetworkAddressChanged += new
            NetworkAddressChangedEventHandler(AddressChangedCallback);
        }

        static void AddressChangedCallback(object sender, EventArgs e)
        {

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface n in adapters)
            {
                //Console.WriteLine("   {0} is {1}", n.Name, n.OperationalStatus);
                Log.trace("indirizzo ip cambiato");
                WlanClient wlan = new WlanClient();
            }

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

            
            oldstate.Equals(new CurrentState());

            try
            {
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {

                    
                    Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();
                    tlp.Controls.Clear();



                    foreach (Wlan.WlanBssEntry network in wlanBssEntries)
                    {

                     if (j == 0)
                        {
                            Label name1 = new Label();
                            Label signal1 = new Label();
                            Label strenght1 = new Label();
                            Label bss1 = new Label();
                            Label mac1 = new Label();

                            name1.Text = "SSID";
                            
                            signal1.Text = "Signal";
                            strenght1.Text = "strenght";
                            bss1.Text = "bss";
                            mac1.Text = "MAC";

                            tlp.Controls.Add(name1, 0, j);
                            tlp.Controls.Add(signal1, 1, j);
                            tlp.Controls.Add(strenght1, 2, j);
                            tlp.Controls.Add(bss1, 3, j);
                            tlp.Controls.Add(mac1, 4, j);
                            j = 1;
                            Log.trace("hjkl");

                            
                        }


                        Label name = new Label();
                        Label signal = new Label();
                        Label strenght = new Label();
                        Label bss = new Label();
                        Label mac = new Label();
                       

                        int rss = network.rssi;
                        byte[] macAddr = network.dot11Bssid;
                        string tMac = "";
                        for (int i = 0; i < macAddr.Length; i++)
                        {
                            tMac += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper();
                        }



                        name.Text = System.Text.ASCIIEncoding.ASCII.GetString(network.dot11Ssid.SSID).ToString();
                        signal.Text = network.linkQuality.ToString();
                        strenght.Text = rss.ToString();
                        bss.Text = network.dot11BssType.ToString();
                        mac.Text = tMac;

                        
                        var db = new Model1Container1();

  
                       
                        var m = db.Networks.Where(c=>c.SSID==name.Text).FirstOrDefault();

                        if (m == null)
                        {
                            System.Console.WriteLine("La rete non esiste");
                            m = new Networks { SSID = name.Text, MAC = mac.Text };
                            db.Networks.Add(m);
                            db.SaveChanges();
                        }
                        else {
                            System.Console.WriteLine("La rete  esiste");
                        }

                        var ms = new Measures {SSID =name.Text, MAC=mac.Text,  timestamp = DateTime.Now, signal = Convert.ToInt16(signal.Text), strength=Convert.ToInt16(strenght.Text) };
                        db.Measures.Add(ms);
                        db.SaveChanges();


                        
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
            /*
            using (var db = new MeasureContext())
            {
                var query = from b in db.Measures
                        
                            select b;

                Console.WriteLine("All Measures in the database:");
                /*foreach (var item in query)
                {
                    Console.WriteLine("SSID: " + item.SSID + " " + item.MAC + " " + item.signal.ToString() + " " + item.strenght.ToString());
                }
                 * /
            }*/
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

        private void tlp_Paint(object sender, PaintEventArgs e)
        {

        }
        




      
    }
}