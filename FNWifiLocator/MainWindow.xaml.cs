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


public delegate void refreshListDelegate();

namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        public delegate void changePlace(Place p);
        public delegate void notifyText(String str);
        public changePlace newPlace;
        public notifyText notify;


        static public ObservableCollection<PlaceTV> placesList = new ObservableCollection<PlaceTV>();
        static public Dictionary<PlaceTV, Place> ParentList = new Dictionary<PlaceTV, Place>();
        public refreshListDelegate rlistdelegate;
        private NamedPipeServerStream server;


        public slideWindow slw = new slideWindow();
        public notifyWindow ntfw = new notifyWindow("AVVIO");
        


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
                }
                else {
                    this.placeDetail.IsEnabled = false;
                }
            }
        }
        private Place currentPlace;
        private Checkin currentCheckin;
        public PlaceTV CurrentPlaceTV    // the Name property
        {
            set {
                this.comboplace.SelectedValue = value;
                this.CurrentPlace = value.pl; 
            }
        }

        public Place CurrentPlace    // the Name property
        {
            get { return currentPlace; }
            set{

               
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
                        Log.trace("ma da qui?");
                        this.positionName.Content = value.name;
                        this.wrongPosition.IsEnabled = true;
                        this.radiob1.IsChecked = true;
                        this.radiob1.IsEnabled = false;
                        this.radiob2.IsEnabled = false;
                        new_place_name.IsEnabled = false;
                        this.radiob.IsEnabled = false;
                        this.comboplace.IsEnabled = false;
                        this.submitPlace.IsEnabled = false;
                    }
                    else
                    {
                        currentCheckin = null;
                        this.positionName.Content = "Sconosciuta";
                        this.radiob.IsChecked = true;
                        this.radiob.IsEnabled = true;
                        this.radiob1.IsEnabled = true;
                        this.radiob2.IsEnabled = true;
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

        public volatile bool _shouldStop;

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
            try
            {
                ServiceController sc = new ServiceController("Power");

            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                // Start the service if the current status is stopped.

                Log.trace("Starting the WifiLocator service...");
                try
                {
                    // Start the service, and wait until its status is "Running".
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);

                    // Display the current service status.
                    Log.trace("The WifiLocator service status is now set to " + sc.Status.ToString());
                }
                catch (InvalidOperationException)
                {
                    Log.trace("Could not start the WifiLocator service.");
                }
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            

            
            newPlace = new changePlace(ChangePlaceMethod);
            //notify = new notifyText(NotifyTextMethod);
            
            ListenThreadForm listener = new ListenThreadForm(this);
            Thread InstanceCaller = new Thread(new ThreadStart(listener.InstanceMethod));
            InstanceCaller.Start();

             
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

            this.server = new NamedPipeServerStream("FNPipeLocator", PipeDirection.Out);
      
            Console.WriteLine("FN.Main: Waiting for client connect...\n");
            this.server.WaitForConnection();
            Console.WriteLine("FN.Main:connection with client...\n");

            
            //Console.WriteLine("FN.Main:connection with client...\n");
            //StreamString ss = new StreamString(server);
            //FNMain_ss = ss;

            //ss.WriteString("PIPE da FN a Service");
            //Console.WriteLine("FN.Main:message send...\n");
            //Thread.Sleep(2000);
            //server.Close();
            
            InitializeComponent();
            this.SlideOpen = false;

            
            this.slw = new slideWindow();
            placeTreView.DataContext = placesList;
            comboplace.DataContext = placesList;

            this.rlistdelegate += this.refreshPlaceTree;
            rlistdelegate();
            CurrentPlace = null;
            Helper.printAllNetworks();

            
            

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

        private bool SendCommand(PipeMessage pm){
             if (server != null) {
                StreamString ss = new StreamString(server);
                ss.WriteString(Helper.SerializeToString<PipeMessage>(pm));
                return true;
            }
             return false;
        }

        private void refreshPlaceTree()
        {
            Log.trace("Refresho la lista");
            if (server != null) {
                StreamString ss = new StreamString(server);
                this.SendCommand(new PipeMessage() { place = 0, cmd = "refresh" });
            }
            placesList.Clear();
            ParentList.Clear();
            
           // var gar = Helper.getAllRootPlaces();

            try
            {


                using (var db = Helper.getDB())
                {
                    IEnumerable<Place>  places = db.Places.Where(c => c.Parent.Equals(null));

                    if (places.Any() != false)
                    {
                        foreach (Place p in places.ToList())
                        {
                            PlaceTV pp = new PlaceTV(p);
                            ParentList.Add(pp,pp.pl);
                            
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
            foreach (PlaceTV ptv in this.placeTreView.Items) {
                Log.trace(ptv.pl.name);
            }
            //if (gar != null)
            //{
            //    foreach (Place p in gar)
            //    {
            //        Log.trace(p.name);
                    
            //        PlaceTV pp = new PlaceTV(p);
            //        ParentList.Add(pp);
            //        placesList.Add(pp);
            //        ParentList.AddRange(pp.childlist);
            //    }
            //}
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

       

        
        private void Delete_Place_Click(object sender, RoutedEventArgs e)
        {
            using (var db = Helper.getDB()) {
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
            
        }

        

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        //DEPRECATA
        private void update_Click(object sender, RoutedEventArgs e)
        {
            this.rlistdelegate();
           
            if (server != null)
            {
                Log.trace("hei service.... perchè non ti aggiorni un pò?");
                StreamString ss = new StreamString(server);
                ss.WriteString(Helper.SerializeToString<PipeMessage>(new PipeMessage() { place = 0, cmd = "update" }));
            }
            else {
                Log.error("Service è null... qualcosa non va con la pipe");
            }
        }

        private void Save_Place_Copy1_Click(object sender, RoutedEventArgs e)
        {
            PlaceTV p = (PlaceTV)this.placeTreView.SelectedValue;
            if (p != null)
            {
                Helper.saveAllCurrentNetworkInPlace(p.pl);
                Helper.saveChanges();
                this.rlistdelegate();
            }

        }

        private void update_ClickList(object sender, RoutedEventArgs e)
        {
            this.rlistdelegate();

        }

        private void wrongPosition_Click_1(object sender, RoutedEventArgs e)
        {
            this.CurrentPlace = null;

            if (!this.SendCommand(new PipeMessage() { place = 0, cmd = "wrong" }))
            {
                Log.error("Qualcosa non va con la pipe");
            }
        }

        private void positionName_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void submitPlace_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.radiob1.IsChecked == true && this.radiob.IsChecked == false)
            {
                PlaceTV ptv = (PlaceTV)this.comboplace.SelectedItem;
                PipeMessage pm = new PipeMessage() { cmd = "force", place = ptv.pl.ID };
                //posto esistente
            }
            else {
                //posto nuovo
                Place p = null;
                using (var db = Helper.getDB()) //Helper.getDB())
                {
                    try
                    {
                        p = new Place();
                        p.name = this.new_place_name.Text;
                        p.m_num = 1;
                        db.Places.Add(p);
                        Helper.saveAllCurrentNetworkInPlace(p);


                    }
                    catch (Exception ex)
                    {
                        Log.error(ex.Message);
                    }
                }
               
                this.rlistdelegate();
                this.CurrentPlace = p;
              
                this.getSlw();
                this.slw.CurrentPlace = p;
                this.slw.Show();
            }

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

        private slideWindow getSlw() {
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

       
        
    }
}