using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using FNWifiLocatorLibrary;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.ServiceProcess;
using System.Timers;
using System.Security.Principal;



public delegate void refreshListDelegate();

namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private int tryconnect = Constant.DefaultTryToConnect;

        public delegate void changePlace(Place p);
        public delegate void cmd(PipeMessage p);
        public delegate void notifyText(String str);
        public delegate void clientConnect(PipeMessage p, NamedPipeServerStream s);
        public delegate void writeString(PipeMessage p, NamedPipeServerStream s);
        public delegate void SendCommandDelegate(PipeMessage p);

        public SendCommandDelegate SendCommand;
        public changePlace newPlace;
        public cmd cmdDelegate;
        public notifyText notify;
        public clientConnect clientConnectDelegate;
        public writeString writeStringDelegate;


        

        static public ObservableCollection<PlaceTV> placesList = new ObservableCollection<PlaceTV>();
        static public Dictionary<PlaceTV, Place> ParentList = new Dictionary<PlaceTV, Place>();
        public refreshListDelegate rlistdelegate;
        public NamedPipeServerStream server;
        System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        private System.Timers.Timer aTimer = new System.Timers.Timer(Constant.SearchPlaceTimeout);

        private ListenThreadForm listener = null;
        private Thread InstanceCaller = null;


        public slideWindow slw = new slideWindow();
        public notifyWindow ntfw = new notifyWindow("AVVIO");

        private readonly object serverLock = new object();
        private readonly object streamLock = new object();

        private static AutoResetEvent waitHandle = new AutoResetEvent(false);
        private Thread th;
        private PipeMessage lastmessage = new PipeMessage() { cmd = "hello" };

        private bool _slideOpen;
        public bool SlideOpen
        {
            get { return _slideOpen; }
            set
            {
                this._slideOpen = value;
                if (value == true)
                {
                    this.slideAnimation.To = 500;
                    this.toggleWindow.Content = "<<";
                }
                else
                {
                    this.toggleWindow.Content = ">>";
                    this.slideAnimation.To = 250;
                }
            }
        }




        private Place selectedPlace;
        public Place SelectedPlace
        {
            get { return selectedPlace; }
            set
            {
                this.selectedPlace = value;
                if (value != null)
                {
                    this.placeDetail.IsEnabled = true;
                    this.delete.IsEnabled = true;
                }
                else
                {
                    this.placeDetail.IsEnabled = false;
                    this.delete.IsEnabled = false;
                }
            }
        }
        private Place currentPlace;
        private Checkin currentCheckin;
        public PlaceTV CurrentPlaceTV    // the Name property
        {
            set
            {
                this.comboplace.SelectedValue = value;
                this.CurrentPlace = value.pl;
            }
        }

        public Place CurrentPlace    // the Name property
        {
            get { return currentPlace; }
            set
            {


                //if (currentCheckin != null)
                //{
                //    using (var db = Helper.getDB())
                //    {
                //        currentCheckin = db.Checkins.Where(c => c.ID == currentCheckin.ID).FirstOrDefault();
                //        currentCheckin.@out = DateTime.Now;
                //        //db.Checkins.Attach(currentCheckin);
                //        db.SaveChanges();
                //    }
                //}   

                if ((currentPlace != null && value == null) || (currentPlace == null && value != null) || (currentPlace != null && value != null && currentPlace.ID != value.ID))
                {
                    this.currentPlace = value;
                    if (this.currentPlace != null)
                    {
                        notifyIcon.Text = Constant.ApplicationName + " - " + this.currentPlace.name;
                        notify(this.currentPlace.name);
                    }
                    else
                    {
                        notifyIcon.Text = Constant.ApplicationName;

                    }

                    // using (var db = Helper.getDB())
                    //{
                    //if(value != null){
                    //value = db.Places.Where(c => c.ID == value.ID).FirstOrDefault();
                    //    currentCheckin = new Checkin() { Place = value, @in = DateTime.Now, @out = DateTime.Now };
                    //    value.Checkins.Add(currentCheckin);
                    //}

                    //db.SaveChanges();
                    //} 


                    if (value != null)
                    {
                        currentCheckin = new Checkin() { Place = value, @in = DateTime.Now, @out = DateTime.Now };
                        value.Checkins.Add(currentCheckin);

                        execFunction(value.file_in);
                        //Log.trace("ma da qui?");
                        this.positionName.Content = value.name;
                        this.wrongPosition.IsEnabled = true;
                        this.radiob1.IsChecked = true;
                        this.radiob1.IsEnabled = false;
                        //this.radiob2.IsEnabled = false;
                        new_place_name.IsEnabled = false;
                        this.radiob.IsEnabled = false;
                        this.comboplace.IsEnabled = false;
                        this.submitPlace.IsEnabled = false;
                        notifyIcon.Icon = new Icon(Constant.iconPathGreen);
                    }
                    else
                    {
                        if (this.currentCheckin != null && currentCheckin.Place != null) execFunction(this.currentCheckin.Place.file_out);
                        currentCheckin = null;
                        this.positionName.Content = "Sconosciuta";
                        notifyIcon.Icon = new Icon(Constant.iconPathYellow);
                        this.radiob.IsChecked = true;
                        this.radiob.IsEnabled = true;
                        this.radiob1.IsEnabled = true;
                        //this.radiob2.IsEnabled = true;
                        this.wrongPosition.IsEnabled = false;
                        this.comboplace.IsEnabled = true;
                        this.submitPlace.IsEnabled = true;
                        new_place_name.IsEnabled = true;
                    }
                }


            }
        }


        private int? _MyFoo;
        public int? MyFoo
        {
            get { return 500; }
            set { _MyFoo = value; }
        }

        public RoutedEvent RoutedEvent { get; set; }

        public MainWindow()
        {
            /* stati in cui può trovarsi il service
            try
            {
            ServiceController sc = new ServiceController("kgvjhg");

                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        Console.WriteLine("Running\n");
                        break;
                    case ServiceControllerStatus.Stopped:
                        Console.WriteLine("Stopped\n");
                        break;
                    case ServiceControllerStatus.Paused:
                        Console.WriteLine("Paused\n");
                        break;
                    case ServiceControllerStatus.StopPending:
                        Console.WriteLine("stopping\n");
                        break;
                    case ServiceControllerStatus.StartPending:
                        Console.WriteLine("Starting\n");
                        break;
                    default:
                        Console.WriteLine("Status Changing\n");
                        break;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);

            }*/

            this.SendCommand += this.SendCommandEvent;
             this.clientConnectDelegate += this.waitClientConnect;
            this.writeStringDelegate += this.WriteStingMethod;
            //clientConnectDelegate(new PipeMessage { cmd = "Test Connessione", place_id = 0 }, this.server);


            newPlace = new changePlace(ChangePlaceMethod);
            cmdDelegate = new cmd(cmdMethod);


            //notify = new notifyText(NotifyTextMethod);
            InitializeComponent();
            listener = new ListenThreadForm(this);

            th = new Thread(doWork);
            th.Start();

            Thread InstanceCaller = new Thread(new ThreadStart(listener.InstanceMethod));
            InstanceCaller.Start();

            
            Log.trace("Non parto in quanto non connesso");
            SendCommand(new PipeMessage { cmd = "hello" });

            notifyIcon.Icon = new Icon(Constant.iconPathYellow);
            notifyIcon.Text = Constant.ServiceName;
            notifyIcon.BalloonTipTitle = "Suggest";
            notifyIcon.Visible = true;
            notifyIcon.Text = Constant.ApplicationName;
            notifyIcon.DoubleClick +=
                delegate(object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };


           
            /*using (this.server = new NamedPipeServerStream("FNPipeLocator", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                var asyncResult = server.BeginWaitForConnection(null, null);

                if (asyncResult.AsyncWaitHandle.WaitOne(5000))
                {
                    server.EndWaitForConnection(asyncResult);
                    // success
                    Log.trace("FN.Main:client connect...\n");
                }
                else
                {
                    // fail
                    Log.trace("FN.Main:client NOT connect...\n");
                }
            }*/

            Log.trace("Creo" + Constant.LocatorPipeName);

            // this.SendCommand(new PipeMessage { cmd= "connected" });

            //Console.WriteLine("FN.Main:connection with client...\n");
            //StreamString ss = new StreamString(server);
            //FNMain_ss = ss;

            //ss.WriteString("PIPE da FN a Service");
            //Console.WriteLine("FN.Main:message send...\n");
            //Thread.Sleep(2000);
            //server.Close();


            this.SlideOpen = false;


            this.slw = new slideWindow();
            placeTreView.DataContext = placesList;
            comboplace.DataContext = placesList;



            
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(Constant.serverIconDown);
            bi3.EndInit();
            serviceIcon.Source = bi3;
            /*
            System.Windows.Controls.Image myImage3 = new System.Windows.Controls.Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(Constant.serverIconUp);
            bi3.EndInit();
            YourImage = bi3;
            */


            this.rlistdelegate += this.refreshPlaceTree;
            rlistdelegate();
            CurrentPlace = null;
            //Helper.printAllNetworks();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = Constant.SearchPlaceTimeout * 2;
            aTimer.Enabled = true;
            this.startService();



        }

        private void WriteStingMethod(PipeMessage p, NamedPipeServerStream s)
        {
            try
            {
                StreamString ss = new StreamString(server);
                ss.WriteString(Helper.SerializeToString<PipeMessage>(p));
            }
            catch (Exception exc)
            {
                throw exc;
            }



        }



        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //this.SendCommand(new PipeMessage { cmd = "refresh" });
        }

        private void startService()
        {
            try
            {
                    ServiceController sc = new ServiceController(Constant.ServiceName);
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {   Log.trace("Starting the " + Constant.ServiceName + " service...");
                        try
                        {
                            TimeSpan timeout= new TimeSpan(0, 0, 30);
                            // Start the service, and wait until its status is "Running".
                            sc.Start();
                            try
                            {
                                sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
                            }
                            catch (System.TimeoutException tex) {
                                Log.error("Inpossibile far partire il servizio" + tex);
                            }
                            // Display the current service status.
                            Log.trace("The WifiLocator service status is now set to " + sc.Status.ToString());
                        }
                        catch (InvalidOperationException)
                        {
                            //System.Windows.Application.Current.Shutdown();
                            Log.error("Could not start the  " + Constant.ServiceName + "  service.");
                        }
                    }
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        if (this.server == null)
                            this.server = new NamedPipeServerStream(Constant.LocatorPipeName, PipeDirection.Out,1,PipeTransmissionMode.Byte, PipeOptions.Asynchronous);



                        if (InstanceCaller == null || !InstanceCaller.IsAlive)
                        {
                            InstanceCaller = new Thread(new ThreadStart(listener.InstanceMethod));
                            InstanceCaller.Start();
                        }

                        Log.trace("Wait for service connect");
                        this.server.WaitForConnection();
                        Log.trace("Service is connected");
                        this.SendCommand(new PipeMessage { cmd = "hello" });

                    }

                }
            catch (Exception ex)
            {
                Log.error(ex.Message);
            }
        }

        private void waitClientConnect(PipeMessage pm, NamedPipeServerStream s)
        {

            if (Monitor.TryEnter(serverLock, 1))
            {
                //Monitor.Enter(xmppLock);
                try
                {
                    if (this.server == null)
                        this.server = new NamedPipeServerStream(Constant.LocatorPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    if (this.server != null && !this.server.IsConnected)
                    {

                        
                    }
                }
                catch (Exception exc)
                {
                    this.tryconnect--;
                    this.server.Close();
                    this.server = new NamedPipeServerStream(Constant.LocatorPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    Log.error(exc);
                    if (this.tryconnect > 0) this.SendCommand(new PipeMessage { cmd = "connected" });
                }
                finally
                {

                    Monitor.Exit(serverLock);
                }
            }
        }




        private void NotifyTextMethod(String str)
        {
            new notifyWindow(str);
        }

        private void ChangePlaceMethod(Place p)
        {
            //if ((this.CurrentPlace != null && p == null) || (this.CurrentPlace == null && p != null)  || this.CurrentPlace.ID != p.ID)
            //{
            this.CurrentPlace = p;
            //}


        }

        private void cmdMethod(PipeMessage p)
        {
           
            aTimer.Stop();

            System.Windows.Controls.Image myImage3 = new System.Windows.Controls.Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            String url = Constant.serverIconDown;
            if (p != null)
            {
                this.cmdrec.Content = p.cmd+" "+p.place_id;
                switch (p.cmd)
                {
                    case "disconnected":
                        currentPlace = null;
                        url = Constant.serverIconDown;
                        break;
                    default:
                        url = Constant.serverIconUp;
                        break;
                }
            }

            bi3.UriSource = new Uri(url);
            bi3.EndInit();

           
            serviceIcon.Source = bi3;
            aTimer.Start();
        }


        private void doWork() {
            int tryagain = 1;
            waitHandle.Reset();
            while(tryagain >0){
            try{
                Log.trace("stop");

                waitHandle.WaitOne();
                    PipeMessage pm = new PipeMessage { cmd = "refresh" };
                    StreamString ss = new StreamString(server);
                    ss.WriteString(Helper.SerializeToString<PipeMessage>(lastmessage));
                    Log.trace("ok");
                waitHandle.Reset();
            }catch(ThreadAbortException abe){
                return;
            }
        }
        }





        private void SendCommandEvent(PipeMessage pm)
        {
            if (Monitor.TryEnter(streamLock))
            {
                
                try
                {
                    Log.trace("Send: " + pm.cmd);
                    if (server != null && server.IsConnected)
                    {
                        //.Abort();+
                        lock (lastmessage)
                        {
                            lastmessage = pm;
                        }
                        waitHandle.Set();
                       
                        /*
                        Log.trace("SEND:" + pm.cmd);
                        StreamString ss = new StreamString(server);
                        ss.WriteString(Helper.SerializeToString<PipeMessage>(pm));
                        this.tryconnect = Constant.DefaultTryToConnect;
                         */ 
                        Log.trace("sended");
                    }
                    else
                    {
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri(Constant.serverIconDown);
                        bi3.EndInit();
                        serviceIcon.Source = bi3;
                        Log.trace("Server Is null or disconnected" + pm.cmd);
                        this.clientConnectDelegate(pm, this.server);
                    }
                }
                catch (Exception exc)
                {
                    this.tryconnect--;
                    if (this.tryconnect > 0)
                        this.SendCommand(pm);
                    Log.error(exc);
                }
                finally {
                    Log.trace("finally");
                    Monitor.Exit(streamLock);
                }
            }
        }

        private void refreshPlaceTree()
        {
            placesList.Clear();
            ParentList.Clear();
            SendCommand(new PipeMessage { cmd = "refresh" });

            try
            {


                using (var db = Helper.getDB())
                {
                    IEnumerable<Place> places = db.Places.Where(c => c.Parent.Equals(null));

                    if (places.Any() != false)
                    {
                        foreach (Place p in places.ToList())
                        {
                            PlaceTV pp = new PlaceTV(p);
                            ParentList.Add(pp, pp.pl);

                            placesList.Add(pp);
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                Log.error(ex);
            }

            this.SelectedPlace = null;
            
        }



        /*  private String selectFile()
          {
              OpenFileDialog openDialog = new OpenFileDialog();
              try
              {
                  openDialog.Title = "Seleziona il file da eseguire";
                  openDialog.Filter = "Bat|*.bat|Tutti i file|*.*";

                  if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                  {
                      if (String.IsNullOrEmpty(openDialog.FileName) == false)
                      {
                          Log.trace(openDialog.FileName);
                      }
                  }


              }
              catch (Exception ex)
              {
                  Log.error(ex);
                  System.Windows.Forms.MessageBox.Show(ex.ToString());
              }
              if (openDialog != null && openDialog.FileName != null) return openDialog.FileName;
              else return "";
          }
          */

        private void execFunction(string filename)
        {

            if (File.Exists(filename))
            {
                //  System.Diagnostics.Process.Start(@"C:\Windows\system32\cmd.exe", " /c " + "\"" + filename + "\"");
                System.Diagnostics.Process.Start("cmd.exe", "/c " + filename);

            }

        }






        //EVENTI


        //Selezionao un luogo nella visuala ad albero.
        private void placeTreView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            PlaceTV p = (PlaceTV)e.NewValue;
            if (p != null)
            {
                //PROVVISORIO
                this.SelectedPlace = p.pl;
            }
        }



        /*
        private void Delete_Place_Click(object sender, RoutedEventArgs e)
        {
            using (var db = Helper.getDB())
            {
                PlaceTV p = (PlaceTV)this.placeTreView.SelectedValue;


                if (p != null && p.pl != null)
                {
                    Place pp = db.Places.First(c => c.ID == p.pl.ID);
                    foreach (PlacesNetworsValue pnv in db.PlacesNetworsValues.Where(c => c.Place.ID == pp.ID).ToList())
                    {
                        db.PlacesNetworsValues.Remove(pnv);
                    }
                    foreach (Place pch in pp.Childs)
                    {
                        pch.Parent = pp.Parent;
                    }
                    db.Places.Remove(pp);
                    Helper.saveChanges();

                    this.rlistdelegate();
                }
            }

        }*/



        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        //DEPRECATA
        private void update_Click(object sender, RoutedEventArgs e)
        {
            this.rlistdelegate();
            SendCommand(new PipeMessage() { place = 0, cmd = "refresh" });
        }

        private void Save_Place_Copy1_Click(object sender, RoutedEventArgs e)
        {
            PlaceTV p = (PlaceTV)this.placeTreView.SelectedValue;
            if (p != null)
            {
                Helper.saveAllCurrentNetworkInPlace(p.pl, false);
                this.rlistdelegate();
            }

        }

        private void delete_ClickList(object sender, RoutedEventArgs e)
        {
            if (this.selectedPlace != null)
            {

                using (var db = Helper.getDB())
                {
                    //Place sp = this.selectedPlace;
                    //db.Places.Attach(this.selectedPlace);
                    Place sp = db.Places.Where(c => c.ID == this.selectedPlace.ID).FirstOrDefault();

                    if (sp != null)
                    {
                        if (this.currentPlace != null && this.currentPlace.ID == sp.ID) currentPlace = null;
                        /*
                        try
                        {
                            foreach (PlacesNetworsValue pnv in sp.PlacesNetworsValues)
                            {
                                db.PlacesNetworsValues.Remove(pnv);
                            }

                        }
                        catch { }
                        try
                        {
                            foreach (Checkin ck in sp.Checkins)
                            {
                                db.Checkins.Remove(ck);
                            }

                        }
                        catch { }
                         * */

                        //  sp.Checkins.Clear();
                        //  sp.PlacesNetworsValues.Clear();

                        var cks = db.Checkins.Where(c => c.Place.ID == sp.ID).ToList();
                        foreach (Checkin ck in cks)
                        {
                            db.Checkins.Remove(ck);
                        }


                        var pnvs = db.PlacesNetworsValues.Where(c => c.Place.ID == sp.ID).ToList();
                        foreach (PlacesNetworsValue pnv in pnvs)
                        {
                            db.PlacesNetworsValues.Remove(pnv);
                        }

                        db.Places.Remove(sp);
                        db.SaveChanges();
                    }
                }
            }

            this.rlistdelegate();

        }

        private void update_ClickList(object sender, RoutedEventArgs e)
        {
            this.rlistdelegate();

        }

        private void wrongPosition_Click_1(object sender, RoutedEventArgs e)
        {
            this.CurrentPlace = null;
            this.comboplace.SelectedValue = null;
            this.SendCommand(new PipeMessage() { place = 0, cmd = "wrong" });
        }

        private void positionName_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void submitPlace_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.radiob1.IsChecked == true && this.radiob.IsChecked == false)
            {
                PlaceTV ptv = (PlaceTV)this.comboplace.SelectedItem;
                if (ptv != null && ptv.pl != null && ptv.pl.ID > 0)
                {
                    SendCommand(new PipeMessage { cmd = "force", place = ptv.pl.ID });
                }
                else
                {
                    SendCommand(new PipeMessage { cmd = "force", place = 0 });
                }
                //posto esistente
            }
            else
            {
                //posto nuovo
                Place p = null;

                if (this.radiob1.IsChecked == false && this.radiob.IsChecked == true)
                {
                    try
                    {
                        using (var db = Helper.getDB()) //Helper.getDB())
                        {

                            p = new Place();
                            p.name = this.new_place_name.Text;
                            p.m_num = 1;
                            //Aggiungo il nuovo posto. Unico punto.
                            db.Places.Add(p);
                            db.SaveChanges();
                            
                            SendCommand(new PipeMessage { place_id = p.ID, cmd = "newPlace", place = p.ID });
                            this.rlistdelegate();
                        }
                        //Helper.saveAllCurrentNetworkInPlace(p);



                        //this.getSlw();
                        //this.slw.CurrentPlace = p;
                        //this.slw.Show();




                        /*using (var db = Helper.getDB()) //Helper.getDB())
                        {
                            db.Places.Attach(p);
                            db.SaveChanges();
                        }*/


                    }
                    catch (Exception ex)
                    {
                        Log.error(ex.Message);
                    }
                }

                //this.CurrentPlace = p;

            }
            //this.rlistdelegate();

        }



        private void toggleWindow_Click_1(object sender, RoutedEventArgs e)
        {
            this.SlideOpen = !this.SlideOpen;
        }

        void slw_Closed(object sender, EventArgs e)
        {
            this.slw = new slideWindow();
        }

        private void placeDetail_click(object sender, RoutedEventArgs e)
        {
            if (this.selectedPlace != null)
            {
                getSlw();
                slw.CurrentPlace = this.selectedPlace;
                slw.Show();
                this.slw.Closed += slw_Closed;
            }
        }

        private void stats_click(object sender, RoutedEventArgs e)
        {
            statWindow stw = new statWindow();
            stw.ShowDialog();
        }

        private slideWindow getSlw()
        {
            if (this.slw == null) { this.slw = new slideWindow(); }
            return this.slw;
        }

        private void radiob_Copy_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void toggleWindow_Copy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void comboplace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                notifyIcon.Visible = false;
                if (listener != null) listener._shouldStop = true;
                if (InstanceCaller != null && InstanceCaller.IsAlive)
                {
                    InstanceCaller.Abort();
                    InstanceCaller.Join(1000);
                }
                if(th != null && th.IsAlive){
                    th.Abort();
                }
                base.OnClosed(e);

            }
            finally
            {
                System.Windows.Application.Current.Shutdown();
            }

        }

        

    }
}