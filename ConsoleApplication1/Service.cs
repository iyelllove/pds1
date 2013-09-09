﻿using System;
using System.Timers;
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

    public partial class Service
    {

        
        private bool searching = false;
        private readonly object xmppLock = new object();
        private readonly object serverLock = new object();

        public delegate void cmdReceived(PipeMessage p);
        public delegate void clientConnect(PipeMessage p, NamedPipeServerStream s);

        public cmdReceived newCommand;
        public clientConnect clientConnectDelegate;
        public ListenThread listener;


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

                    if (value != null)
                    {
                        using (var db = Helper.getDB())
                        {
                            value = db.Places.Where(c => c.ID == value.ID).FirstOrDefault();
                            currentCheckin = new Checkin() { Place = value, @in = DateTime.Now, @out = DateTime.Now };

                            value.Checkins.Add(currentCheckin);
                            db.SaveChanges();
                        }
                        //   
                    }
                    this.cs.update_values_checkin(value);
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
                else
                {
                    if (currentCheckin != null)
                    {
                        //UPDATE DEL VALORE OUT DI CURRENT CHECKIN. SONO SICURO CHE FINO A QUESTO MOMENTO SONO STATO LI'
                        using (var db = Helper.getDB())
                        {
                            currentCheckin = db.Checkins.Where(c => c.ID == currentCheckin.ID).FirstOrDefault();
                            if (currentCheckin != null)
                            {
                                currentCheckin.@out = DateTime.Now;
                                //db.Checkins.Attach(currentCheckin);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        private static AutoResetEvent waitHandle = new AutoResetEvent(false);
        private NamedPipeServerStream server;
        private CurrentState cs = new CurrentState();
        private static System.Timers.Timer aTimer;

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

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            this.searchPlace();
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        }

        private void searchPlace()
        {
            if (Monitor.TryEnter(xmppLock, 1))
            {
                //Monitor.Enter(xmppLock);
                try
                {
                    aTimer.Stop();
                    CurrentPlace = cs.searchPlace();
                    aTimer.Start();
                }
                finally
                {

                    Monitor.Exit(xmppLock);
                }
            }
        }

        private void Service1()
        {
           


        }

        private void waitClientConnect(PipeMessage pm, NamedPipeServerStream s)
        {
            if (Monitor.TryEnter(serverLock, 1))
            {
                //Monitor.Enter(xmppLock);
                try
                {
                    if (this.server == null)
                    {
                        this.server = new NamedPipeServerStream(Constant.ServicePipeName);
                    }

                    if (this.server != null && !this.server.IsConnected)
                    {
                        Console.WriteLine("Service: wait for client(form) connect");
                        this.server.WaitForConnection();
                        Console.WriteLine("Service: Form is connected");
                        //this.SendCommand(new PipeMessage() { cmd = "CONNESSO" });
                        this.SendCommand(pm);
                    }
                }
                finally
                {

                    Monitor.Exit(serverLock);
                }
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

            newCommand += newCommandEvent;
            this.clientConnectDelegate += this.waitClientConnect;
            //ricezione eventi di sistema
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);


            // Create a timer with a ten second interval.
            aTimer = new System.Timers.Timer(10000);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            // Set the Interval to 2 seconds (2000 milliseconds).
            aTimer.Interval = Constant.SearchPlaceTimeout;
            aTimer.Enabled = true;



            Console.WriteLine("Press the Enter key to exit the program.");
            //  Console.ReadLine();



            WlanClient client = new WlanClient();
            try
            {
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    wlanIface.WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(wlanIfacenNotification);
                }
            }
            catch (Exception ex)
            {
            }

            

            //Console.WriteLine("Service: wait for client(form) connect");
            
          
          //  Console.WriteLine("Service: Form is connected");
          //  this.SendCommand(new PipeMessage() { cmd = "CONNESSO" });


            //this.CurrentPlace = this.cs.searchPlace();

            //CREO IL SECONDO THREAD PER LA COMUNICAZIONE DEL SERVICE
            listener = new ListenThread(this, Constant.LocatorPipeName, server);
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



        public void newCommandEvent(PipeMessage pm)
        {
            if (pm != null)
            {
                switch (pm.cmd)
                {
                    case "force":

                        //Deve aggioranre tutti i valori della media
                    case "newPlace":
                        
                        //this.WrongPlace();
                        Place newplace  = null;
                        using (var db = Helper.getDB())
                        {
                            newplace = db.Places.Where(c => c.ID == pm.place_id).FirstOrDefault();
                        }
                        if (newplace != null)
                        {
                            this.CurrentPlace = newplace;
                            Helper.saveAllCurrentNetworkInPlace(newplace, pm.cmd.Equals("force"));
                        }

                        break;
                    case "wrong":
                        this.WrongPlace();
                        break;
                    case "refresh":
                        this.searchPlace();
                        break;
                }
                Log.trace("RICEVUTO:" + pm.cmd);
            }

        }

        public bool SendCommand(PipeMessage pm)
        {

            if (server != null && server.IsConnected)
            {
                Log.trace("SEND:" + pm.cmd);
                StreamString ss = new StreamString(server);
                ss.WriteString(Helper.SerializeToString<PipeMessage>(pm));
                return true;
            }
            else {
                this.clientConnectDelegate(pm, this.server);
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
            //StreamString ss = new StreamString(server);
            //ss.WriteString(Helper.SerializeToString<PipeMessage>(new PipeMessage() { place = 0, cmd = "ip change" }));
            this.searchPlace();
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            this.searchPlace();
            Log.trace("user is trying to log off or shut down the system");
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Log.trace("currently logged-in user has changed");
            this.searchPlace();
            /*if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Log.trace("locked at {0}");
                Log.trace(DateTime.Now.ToString());
            }
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Log.trace("unlocked at {0}");
                Log.trace(DateTime.Now.ToString());
            }*/
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            this.searchPlace();
            Log.trace("user suspends or resumes the system");
        }

        private void wlanIfacenNotification(Wlan.WlanNotificationData notifyData)
        {
            Log.trace("IL SERVICE HA SENTITO UN EVENTO:" + notifyData.NotificationCode.ToString());

            if (notifyData.NotificationCode.Equals(Wlan.WlanNotificationCodeMsm.SignalQualityChange))
            {
                this.searchPlace();
            }
            //Console.WriteLine("{0} to {1} with quality level {2}",connNotifyData.wlanConnectionMode, connNotifyData.profileName, "-");
        }

        
    }



}
