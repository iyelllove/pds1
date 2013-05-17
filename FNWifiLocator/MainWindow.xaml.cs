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
        static public StreamString FNMain_ss;
        static public ObservableCollection<PlaceTV> placesList = new ObservableCollection<PlaceTV>();
        static public List<PlaceTV> ParentList = new List<PlaceTV>();
        private NamedPipeServerStream server;
        public MainWindow()
        {
            
            Thread InstanceCaller = new Thread(
            new ThreadStart(ListenThreadForm.InstanceMethod));
            InstanceCaller.Start();

            this.server = new NamedPipeServerStream("FNPipeLocator");
            //Console.WriteLine("FN.Main: Waiting for client connect...\n");
            server.WaitForConnection();
            //Console.WriteLine("FN.Main:connection with client...\n");
            //StreamString ss = new StreamString(server);
            //FNMain_ss = ss;
            
            //ss.WriteString("PIPE da FN a Service");
            //Console.WriteLine("FN.Main:message send...\n");
            //Thread.Sleep(2000);
            //server.Close();
            
            InitializeComponent();
            //placeTreView.DataContext = placesList;
            //Parent.DataContext = ParentList;
            //refreshPlaceTree();
            //Helper.printAllNetworks();
            
           
        }

        private void refreshPlaceTree()
        {
            

            placesList.Clear();
            ParentList.Clear();
            ParentList.Add(new PlaceTV());

            foreach (Place p in Helper.getAllRootPlaces())
            {   
                PlaceTV pp = new PlaceTV(p);
                ParentList.Add(pp);
                placesList.Add(pp);
                ParentList.AddRange(pp.childlist);
             }
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
                else {
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
            PlaceTV p = (PlaceTV)this.placeTreView.SelectedValue;
            if (p != null)
            {
                PlaceTV prnt = (PlaceTV)Parent.SelectedValue;
                
                p.pl.name = this.place_name.Text;
                p.pl.file_in = this.checkinFile.Text;
                p.pl.file_out = this.checkoutFile.Text;
                if (prnt == null || prnt.pl == null || p.pl.ID != prnt.pl.ID)
                {
                    p.pl.Parent = prnt.pl;
                }
                Helper.getDB().SaveChanges();
                refreshPlaceTree();
            }
        }

        private void Delete_Place_Click(object sender, RoutedEventArgs e)
        {
            datapds1Entities2 db = Helper.getDB();
            PlaceTV p = (PlaceTV)this.placeTreView.SelectedValue;
            if (p != null && p.pl != null) {
                foreach (PlacesNetworsValue pnv in db.PlacesNetworsValues.Where(c => c.Place.ID == p.pl.ID).ToList()) { 
                }
                foreach (Place pch in p.pl.Childs) {
                    pch.Parent = p.pl.Parent;
                }
                db.Places.Remove(p.pl);
                db.SaveChanges();
                refreshPlaceTree();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            using (var db = Helper.getDB())
            {
                 try
                {
                    Place p = new Place();
                    p.name = this.new_place_tetbox.Text;
                    p.m_num = 1;
                    db.Places.Add(p);
                    db.SaveChanges();
                    PipeMessage m = new PipeMessage();
                     
                    

                    Helper.saveAllCurrentNetworkInPlace(p);
                }
                catch (Exception ex)
                {
                    Log.error(ex.Message);
                }

            }






        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void update_Click(object sender, RoutedEventArgs e)
        {
            
            Log.trace("hei service.... perchè non ti aggiorni un pò?");
            StreamString ss = new StreamString(server);
            ss.WriteString(Helper.SerializeToString<PipeMessage>(new PipeMessage(){place=null, cmd = "update" }));
        }



       
    }
}
