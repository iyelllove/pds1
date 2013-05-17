﻿using System;
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

namespace ConsoleService
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

    //listening sulla pipe dal form
        Console.WriteLine("Service: ListenThreadForm.InstanceMethod is running on another thread.");

        var client = new NamedPipeClientStream("FNPipeLocator");
        client.Connect();
        StreamString ss = new StreamString(client);
        while (true)
        {
            String text = ss.ReadString();
            if (text != null)
            {
                CurrentState cs = new CurrentState();
                PipeMessage pm = Helper.DeserializeFromString<PipeMessage>(text);
                Log.trace(pm.cmd);
                Console.WriteLine("Service: received message:" + pm.cmd);
            }
            else
            {
                break;
            }
            Thread.Sleep(4000);
        }
        client.Close();


    //eventuale comunicazione al form (avviene solo nel caso nuovo posto)
        var server = new NamedPipeServerStream("FNPipeService");
        Console.WriteLine("Service.Main: Waiting for client connect...\n");
        server.WaitForConnection();
        Console.WriteLine("Service.Main:connection with client...\n");
        StreamString ssF = new StreamString(server);

        ssF.WriteString("PIPE da Service a FN");
        Console.WriteLine("Service.Main:message send...\n");
        Thread.Sleep(8000);
        server.Close();
              
    }

    static void OnStart()//string[] args
    {
        Console.WriteLine("Service.Main:AVVIO SERVICE");
        //Thread.Sleep(100000);

        //Thread InstanceCaller = new Thread(
        //new ThreadStart(ListenThreadService.InstanceMethod));

        // Start the thread.
        //InstanceCaller.Start();
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
