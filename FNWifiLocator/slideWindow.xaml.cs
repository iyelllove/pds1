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
using System.Windows.Shapes;
using FNWifiLocatorLibrary;
using System.Windows.Forms;

namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per slideWindow.xaml
    /// </summary>
    public partial class slideWindow : Window
    {
        private Place currentPlace;
        public Place CurrentPlace    // the Name property
        {
            get { return currentPlace; }
            set
            {
                this.Title = value.name;
                this.placename.Text = value.name;
                this.checkinFile.Text = value.file_in;
                this.checkoutFile.Text = value.file_out;
                this.currentPlace = value;
            }
        }


        public slideWindow()
        {
            InitializeComponent();
        }

        private void bOpenFileDialogIn_Click(object sender, RoutedEventArgs e)
        {
            this.checkinFile.Text = this.selectFile();
        }

        private void bOpenFileDialogOut_Click(object sender, RoutedEventArgs e)
        {
            this.checkoutFile.Text = this.selectFile();
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

        private void savePlace_Click(object sender, RoutedEventArgs e)
        {
            using (var dddb = Helper.getDB())
            {
                if (this.CurrentPlace != null)
                {
                    Place pp = dddb.Places.First(c => c.ID == this.CurrentPlace.ID);
                    pp.name = this.placename.Text;
                    pp.file_in = this.checkinFile.Text;
                    pp.file_out = this.checkoutFile.Text;
                    dddb.SaveChanges();
                }
            }

        }

        private void deletePlace_Click(object sender, RoutedEventArgs e)
        {
            using (var db = Helper.getDB())
            {
               
                    Place pp = db.Places.First(c => c.ID == this.CurrentPlace.ID);
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
            }
        }

        private void resetPlace_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentPlace != null)
            {
                Helper.saveAllCurrentNetworkInPlace(this.CurrentPlace);
                Helper.saveChanges();
            }

        }

    }
}
