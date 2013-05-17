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
using Progetto.TransDlg;
using System.Diagnostics;
using System.IO;

namespace pds1
{
    public partial class Form1 : Form
    {
        CurrentState oldstate;
        Dictionary<string, RightPlace> rightplaces = new Dictionary<string, RightPlace>();

        public Form1()
        {

            InitializeComponent();
            notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);
            Resize += new EventHandler(Form1_Resize);
            
        }


        public void funData(String txt)
        {
            Notification notifForm = new Notification();
            notifForm.Show("txt");
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
            int j = 0;
            Log.trace("start Update");

                

                this.comboBox1.Enabled = false;
                this.button2.Enabled = false;

                this.comboBox1.Items.Clear();
                this.comboBox1.Items.Add("- Seleziona -");
                this.comboBox1.Text = "- Seleziona -";
           
                

                if (oldstate == null)
                {
                    oldstate = new CurrentState();
                }
                oldstate.searchPlace();

                if (oldstate.getCurrentPlace() != null)
                {

                    execFunction();
                    
                    foreach(Place p in oldstate.getPossiblePlaces()) {
                       
                        this.comboBox1.Items.Add(p);
                        if (p.Equals(oldstate.getCurrentPlace()))
                        {
                             this.comboBox1.Text = oldstate.getCurrentPlace().name;
                        }
                        
                    }
                    this.comboBox1.Enabled = true;
                    this.button2.Enabled = true;
                    this.showMessage("POSTO CORRENTE" + oldstate.getCurrentPlace().name, "");

                   
                }
               
            //}

            //oldstate = newstate;
            
            try
            {
                tlp.Controls.Clear();
                
//                    foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
//                    {
//                    Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();
//                    foreach (Wlan.WlanBssEntry network in wlanBssEntries)
                        foreach (Wlan.WlanBssEntry network in Helper.getCurrentNetworks())
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
                            
                        }


                        Label name = new Label();
                        Label signal = new Label();
                        Label strenght = new Label();
                        Label bss = new Label();
                        Label mac = new Label();


                       string tMac = Helper.getMacAddress(network);




                       name.Text = Helper.getSSIDName(network);
                        signal.Text = network.linkQuality.ToString();

                        strenght.Text = network.rssi.ToString();
                        bss.Text = network.dot11BssType.ToString();
                        mac.Text = tMac;


                        var db = new datapds1Entities2();

  
                       /*
                        var m = db.Networks.Where(c=>c.SSID==name.Text).FirstOrDefault();

                        if (m == null)
                        {
                            Log.trace("Trovata una nuova rete");
                            m = new Networks { SSID = name.Text, MAC = mac.Text };
                            db.Networks.Add(m);
                            db.SaveChanges();
                        }
                      */

                        var ms = new Measure {SSID =name.Text, MAC=mac.Text,  timestamp = DateTime.Now, signal = Convert.ToInt16(signal.Text), strength=Convert.ToInt16(strenght.Text) };
                        db.Measures.Add(ms);
                        db.SaveChanges();


                        
                        tlp.Controls.Add(name, 0, j);
                        tlp.Controls.Add(signal, 1, j);
                        tlp.Controls.Add(strenght, 2, j);
                        tlp.Controls.Add(bss, 3, j);
                        tlp.Controls.Add(mac, 4, j);

                        j++;
                    }
//                }
            }
            catch (Exception ex)
            {
                Log.error(ex.Message);
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
            }
             Log.trace("end update");
             */

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
                    //notifyIcon1.Visible = true;
                    //Thread.Sleep(3000);//pause for 3 seconds
                    //shows a balloon for 1 sec with a title, some text, and the info icon
                    //other possibilities are: TooltipIcon.None, Tooltipicon.Error, and TooltipIcon.Warning
                    //notifyIcon1.ShowBalloonTip(2000, "Attenzione", "doppio click per riaprire!", ToolTipIcon.Info);
                    Notification notifForm = new Notification();
                    notifForm.Show("doppio click per riaprire");
                }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tlp_Paint(object sender, PaintEventArgs e)
        {

        }
        
        public void showMessage(string s1, string s2){
            Notification notifForm = new Notification();
            notifForm.Show(s1);
             //notifyIcon1.ShowBalloonTip(2000, s1, s2, ToolTipIcon.Info);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            using (var db = new datapds1Entities2())
            {
                
                
                WlanClient client = new WlanClient();
                try
                {
                    Place p = new Place();
                    p.name = placeName.Text;
                    p.m_num = 1;
                    db.Places.Add(p);
                    db.SaveChanges();

       //             for (int k = 0; k < 10; k++){
       //                  foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
       //                 {
       //                     wlanIface.Scan();
       //                     System.Threading.Thread.Sleep(5000);
                    
       //                     Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();
       //                     foreach (Wlan.WlanBssEntry network in wlanBssEntries)
                           foreach (Wlan.WlanBssEntry network in Helper.getCurrentNetworks())
                            {
                                if (network.linkQuality >= Properties.Settings.Default.delta_signal_value)
                                {
                                    string thename = Helper.getSSIDName(network);
                                    string themac = Helper.getMacAddress(network);
                                    var m = db.Networks.Where(c => c.SSID == thename).Where(c => c.MAC == themac).FirstOrDefault();
                                    if (m == null)
                                    {
                                        m = new Network { SSID = thename, MAC = Helper.getMacAddress(network) };
                                        db.Networks.Add(m);
                                        db.SaveChanges();
                                    }
                                    var already_exist = db.PlacesNetworsValues.Where(c => c.Place.ID == p.ID).Where(c => c.Network.ID == m.ID).FirstOrDefault();
                                    if (already_exist == null)
                                    {
                                        Log.trace("Aggiungo nuovo posto ( ID:" + m.ID + " SSID:" + m.SSID + " MAC:" + m.MAC + ") a " + p.name);
                                        PlacesNetworsValue pnv = new PlacesNetworsValue();
                                        pnv.Network = m;
                                        pnv.Place = p;
                                        pnv.media = Convert.ToInt16(network.rssi.ToString());
                                        pnv.variance = (short)Properties.Settings.Default.delta_signal_value;
                                        pnv.rilevance = 1; //Convert.ToInt16(network.linkQuality.ToString());

                                        db.PlacesNetworsValues.Add(pnv);
                                        p.PlacesNetworsValues.Add(pnv);
                                    }
                                }
                            }
                           db.SaveChanges();
             //           }
                           for (int k = 0; k < 10; k++) {
                               this.oldstate.forceCurrentPlace(p);
                               this.oldstate.searchPlace();
                           }    
   

                }
               catch (Exception ex)
                {
                    Log.error(ex.Message);
                }
                
            }

            
        }

        private void placeName_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (this.comboBox1.SelectedIndex > 0)
            {
                this.oldstate.forceCurrentPlace(oldstate.getPossiblePlaces()[this.comboBox1.SelectedIndex-1]);
            }
            this.oldstate.wrongPlace();    
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void execFunction()
        {
            string filename = @"C:\\Users\\Nico\\Desktop\\test.bat"; 
            if (File.Exists(filename)) {

/*
                try
                {
                    Process.Start(@"dsd");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Batch file failed", MessageBoxButtons.OK);
                }*/

                /*

                Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = filename;
                //proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();

                int exitCode = proc.ExitCode;

                Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
                Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
                // Console.WriteLine("ExitCode: " + proc.ToString(), "ExecuteCommand");
                proc.Close();
                 * */
            }

        }


      
    }
}