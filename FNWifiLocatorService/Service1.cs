using System;
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
using System.ComponentModel;
using System.Data;
using System.ServiceProcess;



namespace FNWifiLocatorService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            this.CanStop = true;
            InitializeComponent();
        }

        private int tryconnect = Constant.DefaultTryToConnect;

        protected override void OnStart(string[] args)
        {
             Log.setLogEvent();
            Log.trace("Service.Main:AVVIO SERVICE");
            
            Log.setFileName(Constant.ServiceName);
            
            newCommand += newCommandEvent;
            this.clientConnectDelegate += this.waitClientConnect;
            Log.trace("waitClientConnectDelegate setted");
            
            //clientConnectDelegate(new PipeMessage { cmd = "Test Connessione", place_id = 0 }, this.server);
            Log.trace("waitClientConnectDelegate triggered");
            //ricezione eventi di sistema
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);


            // Create a timer with a ten second interval.
            //aTimer = new System.Timers.Timer(10000);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = Constant.SearchPlaceTimeout;
            aTimer.Enabled = true;

            //Console.WriteLine("Service: wait for client(form) connect");


            //  Console.WriteLine("Service: Form is connected");
            //  this.SendCommand(new PipeMessage() { cmd = "CONNESSO" });


            //this.CurrentPlace = this.cs.searchPlace();
            
            //CREO IL SECONDO THREAD PER LA COMUNICAZIONE DEL SERVICE
            Log.trace("Creo" + Constant.ServicePipeName);


            this.server = new NamedPipeServerStream(Constant.ServicePipeName,PipeDirection.Out);

            listener = new ListenThread(this, Constant.LocatorPipeName, server);
            Thread InstanceCaller = new Thread(new ThreadStart(listener.InstanceMethod));
            InstanceCaller.Start();

            

            //ATTENZIONE!! il service si blocca sulla wait?Elo scheduler?
            
        }

        protected override void OnStop()
        {
            listener._shouldStop = true;
        }



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
                    if (value != null)
                    {
                        this.SendCommand(new PipeMessage() { place = value.ID, cmd = "newplace" });
                    }
                    else
                    {
                        this.SendCommand(new PipeMessage() { place = 0, cmd = "newplace" });
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
                                this.SendCommand(new PipeMessage() { place = currentCheckin.Place.ID, cmd = "refresh" });
                                currentCheckin.@out = DateTime.Now;
                                //db.Checkins.Attach(currentCheckin);
                                db.SaveChanges();
                            }
                        }
                    }
                    else {
                        this.SendCommand(new PipeMessage() { place = 0, cmd = "nope" });
                    }
                }
            }
        }

        private static AutoResetEvent waitHandle = new AutoResetEvent(false);
        private NamedPipeServerStream server;
        private CurrentState cs = new CurrentState();
        private System.Timers.Timer aTimer = new System.Timers.Timer(Constant.SearchPlaceTimeout);


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            this.searchPlace();
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




        private void waitClientConnect(PipeMessage pm, NamedPipeServerStream s)
        {
            if (Monitor.TryEnter(serverLock, 1))
            {
                //Monitor.Enter(xmppLock);
                try
                {
                    
                    if(this.server == null)
                        this.server = new NamedPipeServerStream(Constant.ServicePipeName);
                   
                    Log.trace("Service: wait for client(form) connect");
                    this.server.WaitForConnection();
                    if (this.currentCheckin != null && this.currentCheckin.Place != null)

                    {
                        this.SendCommand(new PipeMessage{ cmd = "connected", place_id = this.currentCheckin.Place.ID });
                    }
                    else {
                        this.SendCommand(new PipeMessage { cmd = "connected" });
                    }
                    
                }
                    catch(Exception exc){
                        this.tryconnect--;
                        this.server.Close();
                        this.server = new NamedPipeServerStream(Constant.ServicePipeName);
                        Log.error(exc);
                        if (this.tryconnect>0) this.SendCommand(new PipeMessage { cmd = "connected" });
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


        public void newCommandEvent(PipeMessage pm)
        {
            if (pm != null)
            {
                Log.trace("riceved:" + pm.cmd);
                switch (pm.cmd)
                {
                    
                    case "force":
                       
                    //Deve aggioranre tutti i valori della media
                    case "newPlace":
                        //this.WrongPlace();
                        Place newplace = null;
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
                    case "connected":
                    case "refresh":
                        this.searchPlace();
                        break;
                }
                
            }

        }

        public bool SendCommand(PipeMessage pm)
        {

            if (server != null && server.IsConnected)
            {
                try
                {
                    Log.trace("SEND:" + pm.cmd);
                    StreamString ss = new StreamString(server);
                    ss.WriteString(Helper.SerializeToString<PipeMessage>(pm));
                    this.tryconnect = Constant.DefaultTryToConnect;
                    return true;
                }
                catch(Exception exc) {
                    this.tryconnect--;
                    if(this.tryconnect>0)
                     this.SendCommand(pm);
                    Log.error(exc);
                }
            }
            else
            {
                Log.trace("Server Is null or disconnected");
                this.clientConnectDelegate(pm, this.server);
            }
            return false;
        }

        private void AddressChangedCallback(object sender, EventArgs e)
        {
            Log.trace("indirizzo ip cambiato");
            this.searchPlace();
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            Log.trace("user is trying to log off or shut down the system");
            this.searchPlace();
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Log.trace("currently logged-in user has changed");
            this.searchPlace();
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            this.searchPlace();

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
        protected override void OnShutdown()
        {
            try
            {
                Log.trace("OnShutdown");
                Monitor.Enter(xmppLock);
                aTimer.Stop();
            }
            finally
            {

                Monitor.Exit(xmppLock);
            }
            
            base.OnShutdown();
        }


        protected override void OnPause()
        {
            try
            {
                Log.trace("OnPause");
                Monitor.Enter(xmppLock);
                aTimer.Stop();
            }
            finally
            {

                Monitor.Exit(xmppLock);
            }

            base.OnPause();
        }

        protected override void OnContinue()
        {
            try
            {
                Log.trace("OnContinue");
                Monitor.Enter(xmppLock);
                aTimer.Start();
            }
            finally
            {

                Monitor.Exit(xmppLock);
            }

            base.OnContinue();
        }

    }
}
