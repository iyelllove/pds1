using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Win32;

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
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

            Application.Run(new Form1());
           
            
        }

        static void AddressChangedCallback(object sender, EventArgs e)
        {              
                Log.trace("indirizzo ip cambiato");
        }

        static void SystemEvents_SessionEnded(object sender, EventArgs e)
        {
            Log.trace("user is trying to log off or shut down the system");
        }

        static void SystemEvents_SessionSwitch(object sender, EventArgs e)
        {
            Log.trace("currently logged-in user has changed");
        }

        static void SystemEvents_PowerModeChanged(object sender, EventArgs e)
        {
            Log.trace("user suspends or resumes the system");
        }
        


       
    }

   
    

}

