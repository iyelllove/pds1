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

public delegate void refreshListDelegate();

namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public ObservableCollection<PlaceTV> placesList = new ObservableCollection<PlaceTV>();
        static public Dictionary<PlaceTV, Place> ParentList = new Dictionary<PlaceTV, Place>();
        public refreshListDelegate rlistdelegate;
        private NamedPipeServerStream server;

        public slideWindow slw = new slideWindow();



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
                this.currentPlace = value;
                
                if (value != null)
                {
                    this.positionName.Content = value.name;
                    this.wrongPosition.IsEnabled = true;
                    this.radiob1.IsChecked = true;
                    this.radiob1.IsEnabled = false;
                    new_place_name.IsEnabled = false;
                    this.radiob.IsEnabled = false;
                    this.comboplace.IsEnabled = false;
                    this.submitPlace.IsEnabled = false;
                }
                else {
                    this.positionName.Content = "Sconosciuta";
                    this.radiob.IsChecked = true;
                    this.radiob.IsEnabled = true;
                    this.radiob1.IsEnabled = true;
                    this.wrongPosition.IsEnabled = false;
                    this.comboplace.IsEnabled = true;
                    this.submitPlace.IsEnabled = true;
                    new_place_name.IsEnabled = true;
                }
            }
        }

       
        public MainWindow()
        {


            
            Thread InstanceCaller = new Thread(
            new ThreadStart(ListenThreadForm.InstanceMethod));
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

            this.server = new NamedPipeServerStream("FNPipeLocator");
      
            Console.WriteLine("FN.Main: Waiting for client connect...\n");
            server.WaitForConnection();
            Console.WriteLine("FN.Main:connection with client...\n");
            
            
            //Console.WriteLine("FN.Main:connection with client...\n");
            //StreamString ss = new StreamString(server);
            //FNMain_ss = ss;

            //ss.WriteString("PIPE da FN a Service");
            //Console.WriteLine("FN.Main:message send...\n");
            //Thread.Sleep(2000);
            //server.Close();
            
            InitializeComponent();
           

            this.slw = new slideWindow();
            placeTreView.DataContext = placesList;
            comboplace.DataContext = placesList;


            this.rlistdelegate += this.refreshPlaceTree;
            rlistdelegate();
            CurrentPlace = null;
            Helper.printAllNetworks();


        }

        private void refreshPlaceTree()
        {
            Log.trace("Refresho la lista");

            StreamString ss = new StreamString(server);
            ss.WriteString(Helper.SerializeToString<PipeMessage>(new PipeMessage() { place = null, cmd = "refresh" }));

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
                this.CurrentPlaceTV = p;

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

        private void update_Click(object sender, RoutedEventArgs e)
        {
            this.rlistdelegate();
           
            if (server != null)
            {
                Log.trace("hei service.... perchè non ti aggiorni un pò?");
                StreamString ss = new StreamString(server);
                ss.WriteString(Helper.SerializeToString<PipeMessage>(new PipeMessage() { place = null, cmd = "update" }));
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

        }

        private void positionName_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void submitPlace_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.radiob1.IsChecked == true && this.radiob.IsChecked == false)
            {
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
                if (this.slw == null) { this.slw = new slideWindow(); }
                this.slw.CurrentPlace = p;
                this.slw.Show();
                
            }

        }



        private void toggleWindow_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.slw == null) { this.slw = new slideWindow(); }
            this.placeDetail.Opacity = this.placeTreView.Opacity = 1 - placeTreView.Opacity;
        }

        void slw_Closed(object sender, EventArgs e)
        {
            this.slw = new slideWindow(); 
        }

        private void placeDetail_click(object sender, RoutedEventArgs e)
        {
            if (this.selectedPlace != null)
            {
                slw.CurrentPlace = this.currentPlace;
                slw.Show();
                this.slw.Closed += slw_Closed;
            }
        }

       
        
    }
}