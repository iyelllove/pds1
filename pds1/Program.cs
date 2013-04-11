using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

using System.Net.NetworkInformation;

    
namespace pds1
{

    
    static class Program
    {

        
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// modificare da qui
        /// </summary>
        [STAThread]
        static void Main()
        {
            

            Helper.printAllNetworks();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

           
            Application.Run(new Form1());
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);

        }

        static void AddressChangedCallback(object sender, EventArgs e)
        {

                //Console.WriteLine("   {0} is {1}", n.Name, n.OperationalStatus);
                Log.trace("indirizzo ip cambiato");
            
        }
        
        

    }

   
    

}

