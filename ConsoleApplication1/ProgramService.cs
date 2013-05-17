using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using FNWifiLocatorLibrary;
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace ConsoleApplication1
{
    class ProgramService
    {
        static void Main(string[] args)
        {
            OnStart();
            Service1();
            OnShutdown();// when the CanShutdown property is true
                         // the system is shutting down.
                         //per simulare gestire evento SessionEndedEventHandler
            OnStop();
        }

        
        
    static void Service1()
    {  
     //ricezione eventi di sistema
        NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
        SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
        SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);


    //azioni da intraprendere



    //eventuale comunicazione al form
        var server = new NamedPipeServerStream("FNPipeService");
        Console.WriteLine("Service.Main: Waiting for client connect...\n");
        server.WaitForConnection();
        Console.WriteLine("Service.Main:connection with client...\n");
        StreamString ss = new StreamString(server);

        ss.WriteString("PIPE da Service a FN");
        Console.WriteLine("Service.Main:message send...\n");
        Thread.Sleep(8000);
        server.Close();
              
    }

    static void OnStart()//string[] args
    {
        Console.WriteLine("Service.Main:AVVIO SERVICE");
        //Thread.Sleep(100000);

        Thread InstanceCaller = new Thread(
            new ThreadStart(ListenThreadService.InstanceMethod));

        // Start the thread.
        InstanceCaller.Start();
    }


    static void OnStop()
    {
        
    }
    
     static void OnShutdown()
    {

    }

   

    //Gestione event handler////////////////////////////////////////////////////////

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
    //////////////////////////////////////////////////////////////////////////////////////////////7

    }
}
