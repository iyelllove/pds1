﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Media.Animation;
using ScottLogic.Shapes;


namespace FNWifiLocator
{
    /// <summary>
    /// Logica di interazione per statWindow.xaml
    /// </summary>
    public partial class statWindow : Window
    {
        private ObservableCollection<AssetClass> classes;

        public statWindow()
        {   
            InitializeComponent();

            // create our test dataset and bind it
            classes = new ObservableCollection<AssetClass>(AssetClass.ConstructTestData());                          
            this.DataContext = classes;                 
        }

        /// <summary>
        /// Handle clicks on the listview column heading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            GridViewColumn column = ((GridViewColumnHeader)e.OriginalSource).Column;
            piePlotter.PlottedProperty = column.Header.ToString();
        }

        private void AddNewItem(object sender, RoutedEventArgs e)
        {
            AssetClass asset = new AssetClass() { PlaceName = "new class" };
            classes.Add(asset);
        }
    }
}
