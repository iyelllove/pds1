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
using System.Net.Sockets;
using NativeWifi;






namespace ConsoleService
{

    public  partial class Service
    {

        public delegate void cmdReceived(PipeMessage p);
        public cmdReceived newCommand;
        public ListenThread listener;

        Timer TimeoutTimer;
        
        const int TimeoutSeconds = 25;

        private Checkin currentCheckin;

        private Place currentPlace;
        public Place CurrentPlace
        {
            get { return currentPlace; }
            set
            {
               
                if ((currentPlace != null && value == null) || (currentPlace == null && value != null) || (currentPlace != null && value != null && currentPlace.ID != value.ID))
                {
                    //VUOL DIRE CHE IL LUOVO IN VALUE è DIVERSO DA QUELLO CHE HO MEMORIZZATO IO
                    this.currentPlace = value;

                    using (var db = Helper.getDB())
                    {
                        if (value != null)
                        {
                            value = db.Places.Where(c => c.ID == value.ID).FirstOrDefault();
                            currentCheckin = new Checkin() { Place = value, @in = DateTime.Now, @out = DateTime.Now };
                            value.Checkins.Add(currentCheckin);
                        }

                        db.SaveChanges();
                    }

                    StreamString ss = new StreamString(server);
                    if (value != null)
                    {
                        this.SendCommand(new PipeMessage() { place = value.ID, cmd = "refresh" });
                    }
                    else
                    {
                        this.SendCommand(new PipeMessage() { place = 0, cmd = "refresh" });
                    }
                }
                else {
                    if (currentCheckin != null)
                    {
                        //UPDATE DEL VALORE OUT DI CURRENT CHECKIN. SONO SICURO CHE FINO A QUESTO MOMENTO SONO STATO LI'
                        using (var db = Helper.getDB())
                        {
                            currentCheckin = db.Checkins.Where(c => c.ID == currentCheckin.ID).FirstOrDefault();
                            currentCheckin.@out = DateTime.Now;
                            //db.Checkins.Attach(currentCheckin);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
        
        private static AutoResetEvent waitHandle = new AutoResetEvent(false);
        private NamedPipeServerStream server;
        private CurrentState cs = new CurrentState();
        
        public Service()
        {
            OnStart();
            Service1();
            
            waitHandle.WaitOne(); //MI SERVE PER NON FAR MORIRE IL SERVICE
            
            OnShutdown();// when the CanShutdown property is true
            // the system is shutting down.
            //per simulare gestire evento SessionEndedEventHandler
            OnStop();
        }
       
        private void Service1()
        {
            newCommand += newCommandEvent;
            //ricezione eventi di sistema
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);



             WlanClient client = new WlanClient();
             try
             {
                 foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                 {
                     wlanIface.WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(wlanIfacenNotification);
                 }
             }
             catch (Exception ex) { 
             }

            
        }
        private void WrongPlace()
        {
            //DEVO DECREMENTARE  TUTTE LE IMPORTANZE DELLE RETI IN CURRENT PLACE
            Log.trace("WRONG PLACE");
            this.CurrentPlace = null;
            
        }

        private void ForceCheckin(Place p)
        {
            // Log.trace("FORCE PLACE");
            // this.WrongPlace();

            //DEVO INCREMENTARE  TUTTE LE IMPORTANZE DELLE RETI IN p
            this.CurrentPlace = p;
        }
       

       
        private void OnStart()//string[] args
        {
            Console.WriteLine("Service.Main:AVVIO SERVICE");
            
        

            this.server = new NamedPipeServerStream("FNPipeService"); 
           
            Console.WriteLine("Service: wait for client(form) connect");
            server.WaitForConnection();
            Console.WriteLine("Service: Form is connected");
            this.SendCommand(new PipeMessage(){cmd="CONNESSO"});

            
            this.CurrentPlace = this.cs.searchPlace();

            //CREO IL SECONDO THREAD PER LA COMUNICAZIONE DEL SERVICE
            listener = new ListenThread(this, "FNPipeLocator");
            Thread InstanceCaller = new Thread(new ThreadStart(listener.InstanceMethod));
            InstanceCaller.Start();

            //ATTENZIONE!! il service si blocca sulla wait?Elo scheduler?

        }


        private void OnStop()
        {

        }

        private void OnShutdown()
        {
            listener._shouldStop = true;

        }



        public void newCommandEvent(PipeMessage pm) {
            if (pm != null) {
                switch (pm.cmd)
                {
                    case "wrong":
                        this.WrongPlace();
                        break;
                    case "refresh":
                        this.CurrentPlace = this.cs.searchPlace();
                        break;
                }
                Log.trace("RICEVUTO:" + pm.cmd);
            }
            
        }

        private bool SendCommand(PipeMessage pm) {
           
            if (server != null)
            {
                Log.trace("SEND:" + pm.cmd);
                StreamString ss = new StreamString(server);
                ss.WriteString(Helper.SerializeToString<PipeMessage>(pm));
                return true;
            }
            return false;
        }

        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        //          Gestione event handler                   ///
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////

        private void AddressChangedCallback(object sender, EventArgs e)
        {
            Log.trace("indirizzo ip cambiato");
            StreamString ss = new StreamString(server);
            ss.WriteString(Helper.SerializeToString<PipeMessage>(new PipeMessage() { place = 0, cmd = "ip change" }));
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

        private void wlanIfacenNotification(Wlan.WlanNotificationData notifyData)
        {
            Log.trace("IL SERVICE HA SENTITO UN EVENTO:"+notifyData.NotificationCode.ToString());
            
            if (notifyData.NotificationCode.Equals(Wlan.WlanNotificationCodeMsm.SignalQualityChange))
            {
               this.CurrentPlace = this.cs.searchPlace();
            }
            //Console.WriteLine("{0} to {1} with quality level {2}",connNotifyData.wlanConnectionMode, connNotifyData.profileName, "-");
        }
    }



}
