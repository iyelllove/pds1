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
using System.Threading;
using System.Net.NetworkInformation;

    
namespace pds1
{

    
    static class Program
    {

        
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// modificare da qui
        /// </summary>
        /// 
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
            
            
            Thread InstanceCaller = new Thread(
            new ThreadStart(ListenThreadForm.InstanceMethod));
            InstanceCaller.Start();
            Application.Run(new Form1());          
        }

        static void AddressChangedCallback(object sender, EventArgs e)
        {

           

            Log.trace("indirizzo ip cambiato");
        }

        static void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            Log.trace("user is trying to log off or shut down the system");
        }

        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Log.trace("currently logged-in user has changed");
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Log.trace("locked at {0}");
                Log.trace(DateTime.Now.ToString());
            }
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Log.trace("unlocked at {0}");
                Log.trace(DateTime.Now.ToString());
            } 
        }

        static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            Log.trace("user suspends or resumes the system");
        }


        public static void update(){
             CurrentState cs; 
            cs = new CurrentState();
            cs.searchPlace();
        }


        

       
    }

   
    

}

