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
using FNWifiLocatorLibrary;
using System.Windows.Forms;



namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static public ObservableCollection<PlaceTV> placesList = new ObservableCollection<PlaceTV>();
        static public List<PlaceTV> ParentList = new List<PlaceTV>();
        public MainWindow()
        {

            
            
            InitializeComponent();
            placeTreView.DataContext = placesList;
            Parent.DataContext = ParentList;
            refreshPlaceTree();
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
                
                checkinFile.Text = p.pl.file_in;
                checkoutFile.Text = p.pl.file_out;
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
                if (prnt == null || prnt.pl == null || p.pl.ID != prnt.pl.ID)
                {
                    p.pl.Parent = prnt.pl;
                    Helper.getDB().SaveChanges();
                    refreshPlaceTree();
                }
                p.pl.file_in = this.checkinFile.Text;
                p.pl.file_out = this.checkoutFile.Text;
                Helper.getDB().SaveChanges();
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


       
    }
}
