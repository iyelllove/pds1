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




namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public ObservableCollection<PlaceTV> placesList = new ObservableCollection<PlaceTV>();
        static public List<PlaceTV> ParentList = new List<PlaceTV>();
        private NamedPipeServerStream server;
       
        public MainWindow()
        {
           
            
            Thread InstanceCaller = new Thread(
            new ThreadStart(ListenThreadForm.InstanceMethod));
            InstanceCaller.Start();

            using (this.server = new NamedPipeServerStream("FNPipeLocator", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
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
            }
            //Console.WriteLine("FN.Main: Waiting for client connect...\n");
            //server.WaitForConnection();
            //Console.WriteLine("FN.Main:connection with client...\n");
            //StreamString ss = new StreamString(server);
            //FNMain_ss = ss;

            //ss.WriteString("PIPE da FN a Service");
            //Console.WriteLine("FN.Main:message send...\n");
            //Thread.Sleep(2000);
            //server.Close();
            
            InitializeComponent();
            placeTreView.DataContext = placesList;
            Parent.DataContext = ParentList;
            refreshPlaceTree();
            Helper.printAllNetworks();


        }

        private void refreshPlaceTree()
        {


            placesList.Clear();
            ParentList.Clear();
            ParentList.Add(new PlaceTV());
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
                            ParentList.Add(pp);
                            placesList.Add(pp);
                            ParentList.AddRange(pp.childlist);
                        }
                       
                    }
                }


            }
            catch (Exception ex)
            {
                Log.error(ex);
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



        private String selectFile()
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


        private void execFunction(string filename)
        {

            if (File.Exists(filename))
            {
                System.Diagnostics.Process.Start("cmd.exe", "/c " + filename);

            }

        }






        //EVENTI

        private void placeTreView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            PlaceTV p = (PlaceTV)e.NewValue;
            if (p != null)
            {
                if (p.pl.Parent == null)
                {
                    Parent.SelectedValue = ParentList.First();
                }
                else
                {
                    Parent.SelectedValue = p.parentTV;
                }
                this.place_name.Text = p.pl.name;
                this.checkinFile.Text = p.pl.file_in;
                this.checkoutFile.Text = p.pl.file_out;
            }
            else Parent.SelectedValue = null;
        }

        private void bOpenFileDialogIn_Click(object sender, RoutedEventArgs e)
        {
            this.checkinFile.Text = this.selectFile();
        }

        private void bOpenFileDialogOut_Click(object sender, RoutedEventArgs e)
        {
            this.checkoutFile.Text = this.selectFile();
        }

        private void Save_Place_Click(object sender, RoutedEventArgs e)
        {
            using (var dddb = Helper.getDB())
            {
                PlaceTV p = (PlaceTV)this.placeTreView.SelectedValue;
                if (p != null)
                {
                    Place pp = dddb.Places.First(c => c.ID == p.pl.ID);
                    PlaceTV prnt = (PlaceTV)Parent.SelectedValue;
                    pp.name = this.place_name.Text;
                    pp.file_in = this.checkinFile.Text;
                    pp.file_out = this.checkoutFile.Text;
                    if(prnt == null || prnt.pl == null){
                         pp.Parent = null;
                    }
                    else if (pp.ID != prnt.pl.ID)
                    {
                        Place ppparent = dddb.Places.First(c => c.ID == p.pl.ID); 
                        pp.Parent = ppparent;
                    }
                    dddb.SaveChanges();
                    this.refreshPlaceTree();
                }
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

                    refreshPlaceTree();
                }
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Place p = null;
            using (var db = Helper.getDB()) //Helper.getDB())
            {
                try
                {
                    p = new Place();
                    p.name = this.new_place_tetbox.Text;
                    p.m_num = 1;
                    db.Places.Add(p);
                    Helper.saveAllCurrentNetworkInPlace(p);
                    
                    
                }
                catch (Exception ex)
                {
                    Log.error(ex.Message);
                }
            }
            
            refreshPlaceTree();
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void update_Click(object sender, RoutedEventArgs e)
        {

            
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
                refreshPlaceTree();
            }

        }

        private void update_ClickList(object sender, RoutedEventArgs e)
        {
            this.refreshPlaceTree();

        }
    }
}