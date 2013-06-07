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

namespace ConsoleService
{
    public partial class Service
    {
        private NamedPipeServerStream server;
        public Service()
        {
            OnStart();
            Service1();
            OnShutdown();// when the CanShutdown property is true
            // the system is shutting down.
            //per simulare gestire evento SessionEndedEventHandler
            OnStop();
        }

        private void Service1()
        {
            //ricezione eventi di sistema
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

            //listening sulla pipe dal form
            //var client = new NamedPipeClientStream(".","FNPipeLocator", PipeDirection.In, PipeOptions.Asynchronous);
            var client = new NamedPipeClientStream("FNPipeLocator");
            client.Connect();
            Console.WriteLine("Service: Connect with server");
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
            /* var server = new NamedPipeServerStream("FNPipeService");
             Console.WriteLine("Service.Main: Waiting for client connect...\n");
             server.WaitForConnection();
             Console.WriteLine("Service.Main:connection with client...\n");
             StreamString ssF = new StreamString(server);

             ssF.WriteString("PIPE da Service a FN");
             Console.WriteLine("Service.Main:message send...\n");
             Thread.Sleep(8000);
             server.Close();*/

        }

        private void OnStart()//string[] args
        {
            Console.WriteLine("Service.Main:AVVIO SERVICE");
            
           /* using(this.server = new NamedPipeServerStream("FNPipeService", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                var asyncResult = server.BeginWaitForConnection(null, null);

                if (asyncResult.AsyncWaitHandle.WaitOne(5000))
                {
                    server.EndWaitForConnection(asyncResult);
                    // success
                    Console.WriteLine("Service:client connect...\n");
                }
                else
                {
                    // fail
                    Console.WriteLine("Service:client NOT connect...\n");
                }
            }*/

            this.server = new NamedPipeServerStream("FNPipeService"); 
           
                Console.WriteLine("Service: wait for client(form) connect");
                server.WaitForConnection();
                Console.WriteLine("Service: Form is connected");
            

            //ATTENZIONE!! il service si blocca sulla wait?Elo scheduler?
            

            //Thread.Sleep(100000);

            //Thread InstanceCaller = new Thread(
            //new ThreadStart(ListenThreadService.InstanceMethod));

            // Start the thread.
            //InstanceCaller.Start();
        }


        private void OnStop()
        {

        }

        private void OnShutdown()
        {

        }


        //Gestione event handler////////////////////////////////////////////////////////

        private void AddressChangedCallback(object sender, EventArgs e)
        {
            Log.trace("indirizzo ip cambiato");
            StreamString ss = new StreamString(server);
            ss.WriteString(Helper.SerializeToString<PipeMessage>(new PipeMessage() { place = null, cmd = "ip change" }));
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            Log.trace("user is trying to log off or shut down the system");
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
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

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            Log.trace("user suspends or resumes the system");
        }
    }
}
