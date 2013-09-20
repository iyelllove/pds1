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
            InitializeComponent();
        }




        private readonly object xmppLock = new object();
        private readonly object serverLock = new object();
        private readonly object atimerLock = new object();

        public delegate void cmdReceived(PipeMessage p);
        public delegate void clientConnect(PipeMessage p, NamedPipeServerStream s);

        public cmdReceived newCommand;
        public clientConnect clientConnectDelegate;
        public ListenThread listener;


        private Checkin currentCheckin;

        private Place currentPlace;
        private int currentPlace_counter = 0;

        public Place CurrentPlace
        {
            get { return currentPlace; }
            set
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
                            db.SaveChanges();
                        }
                    }

                }


                this.cs.printResult();
                if ((currentPlace != null && value == null) || (currentPlace == null && value != null) || (currentPlace != null && value != null && currentPlace.ID != value.ID))
                {
                    //VUOL DIRE CHE IL LUOVO IN VALUE è DIVERSO DA QUELLO CHE HO MEMORIZZATO IO
                    this.currentPlace = value;
                    if (this.currentPlace_counter == 1 && this.currentCheckin != null && this.currentCheckin.Place != value )
                    {
                        this.SendCommand(new PipeMessage() { place = 0, cmd = "newplace"});
                    }
                    else
                    {
                        this.currentPlace_counter = 1;
                        this.SendCommand(new PipeMessage() { place = 0, cmd = "please wait:" + currentPlace_counter });
                    }
                    
                    /*                    if (value == null)
                                        {
                                            this.SendCommand(new PipeMessage() { place = 0, cmd = "newplace" });
                                        }*/
                }
                else
                {

                    if (currentPlace_counter == Constant.tryForCheckin) {
                        if (value != null)
                        {
                            this.SendCommand(new PipeMessage() { place = value.ID, cmd = "newplace" });
                            using (var db = Helper.getDB())
                            {
                                value = db.Places.Where(c => c.ID == value.ID).FirstOrDefault();
                                if (value != null)
                                {
                                    currentCheckin = new Checkin() { Place = value, @in = DateTime.Now, @out = DateTime.Now };

                                    value.Checkins.Add(currentCheckin);
                                    db.SaveChanges();
                                }
                            }
                            this.cs.update_values_checkin(value);
                        }
                        else
                        {
                            this.SendCommand(new PipeMessage() { place = 0, cmd = "newplace" });
                        }
                    }else if (currentPlace_counter >= Constant.tryForCheckin)
                    {
                        if (this.currentPlace_counter % Constant.CurrentPlaceCounter == 0 && value != null)
                        {
                            this.SendCommand(new PipeMessage() { place = value.ID, cmd = "updatevalue" + this.currentPlace_counter });
                            this.cs.update_values(value);
                        }
                        else if(value!=null) { 
                        this.SendCommand(new PipeMessage() { place = value.ID, cmd = "refresh" });
                        }
                    }
                    else
                    {
                        this.SendCommand(new PipeMessage() { place = 0, cmd = "please wait:" + currentPlace_counter });
                    }

                    currentPlace_counter++;

                    /*
                    String cmd = "refresh";
                    
                    if (value != null) {
                        if (this.currentPlace_counter == Constant.tryForCheckin)
                        {
                            using (var db = Helper.getDB())
                            {
                                value = db.Places.Where(c => c.ID == value.ID).FirstOrDefault();
                                if (value != null)
                                {
                                    currentCheckin = new Checkin() { Place = value, @in = DateTime.Now, @out = DateTime.Now };

                                    value.Checkins.Add(currentCheckin);
                                    db.SaveChanges();
                                }
                            }
                            this.SendCommand(new PipeMessage() { place = value.ID, cmd = "newplace" });
                            this.cs.update_values_checkin(value);
                        }
                        else if (this.currentPlace_counter > Constant.tryForCheckin)
                        {
                            if (this.currentPlace_counter % Constant.CurrentPlaceCounter == 0)
                            {
                                cmd = "updatevalue";
                                this.cs.update_values(value);
                            }
                            this.SendCommand(new PipeMessage() { place = value.ID, cmd = cmd });
                        }
                    }else{
                        if (currentPlace_counter >= Constant.tryForCheckin) {
                            this.SendCommand(new PipeMessage() { place = 0, cmd = "newplace" });
                        }
                        
                    }
                    currentPlace_counter++;
                    */
                    
                }
            }
        }

        private static AutoResetEvent waitHandle = new AutoResetEvent(false);
        private NamedPipeServerStream server;
        private CurrentState cs = new CurrentState();
        private System.Timers.Timer aTimer = new System.Timers.Timer(Constant.SearchPlaceTimeout);


        private int tryconnect = Constant.DefaultTryToConnect;
        public Thread InstanceCaller = null;

        protected override void OnStart(string[] args)
        {
            Log.setLogEvent();
            newCommand += newCommandEvent;
            //this.clientConnectDelegate += this.waitClientConnect;
            //Log.trace("waitClientConnectDelegate setted");

            //clientConnectDelegate(new PipeMessage { cmd = "Test Connessione", place_id = 0 }, this.server);
            //Log.trace("waitClientConnectDelegate triggered");
            //ricezione eventi di sistema
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);
            //SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            //SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
           // wlanIface.WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(wlanIfacenNotification);

            // Create a timer with a ten second interval.
            //aTimer = new System.Timers.Timer(10000);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            lock (atimerLock)
            {
                aTimer.Interval = Constant.SearchPlaceTimeout;
            }
            aTimer.Enabled = true;
            
        }

        protected override void OnStop()
        {
            listener._shouldStop = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            this.searchPlace();
        }

        private void searchPlace()
        {
            if (Monitor.TryEnter(xmppLock, 1))
            {
                aTimer.Stop();
                lock (atimerLock)
                {
                    aTimer.Interval = Constant.SearchPlaceTimeout;
                }
                if (this.server == null)
                    this.server = new NamedPipeServerStream(Constant.ServicePipeName, PipeDirection.Out);

                if (!this.server.IsConnected)
                {
                    Log.trace("Service: wait for client(form) connect");
                    Log.trace("Il mio servizio non parte se non ho ricevuto una connessione");
                    try
                    {
                        if (InstanceCaller != null)
                        {
                            InstanceCaller.Abort();
                            InstanceCaller.Join(1000);
                        }
                        this.server.WaitForConnection();
                    }
                    catch (IOException ioe)
                    {
                        Log.error(ioe);
                        this.server.Disconnect();
                        this.server.Close();
                        InstanceCaller.Abort();
                        InstanceCaller.Join(1000);
                        this.server = new NamedPipeServerStream(Constant.ServicePipeName, PipeDirection.Out);
                        this.server.WaitForConnection();

                    }
                    SendCommand(new PipeMessage { cmd = "hello" });

                    listener = new ListenThread(this, Constant.LocatorPipeName, server);
                    InstanceCaller = new Thread(new ThreadStart(listener.InstanceMethod));
                    InstanceCaller.Start();
                    Log.trace("Service.Main:AVVIO SERVICE");
                }

                //Monitor.Enter(xmppLock);
                try
                {
                    Place cp = cs.searchPlace();
                    lock (atimerLock)
                    {
                        if (Constant.SearchPlaceTimeout == aTimer.Interval)
                        {
                            CurrentPlace = cp;
                        }
                        //SendCommand(new PipeMessage { cmd = "tmp" + aTimer.Interval });
                    }
                    
                }
                finally
                {
                    aTimer.Start();
                    Monitor.Exit(xmppLock);
                }
            }
        }



        private void WrongPlace()
        {
            aTimer.Stop();
            //this.currentPlace = null;
            //DEVO DECREMENTARE  TUTTE LE IMPORTANZE DELLE RETI IN CURRENT PLACE
            Log.trace("WRONG PLACE");
            Log.trace("Inposto il timer a" + Constant.SearchPlaceWrongTimeout);
            //this.CurrentPlace = null;
            lock (atimerLock)
            {
                aTimer.Interval = Constant.SearchPlaceWrongTimeout;
            }
            aTimer.Start();

        }

        private void ForceCheckin(Place p)
        {
                this.CurrentPlace = p;
        }


        public void newCommandEvent(PipeMessage pm)
        {
            if (pm != null)
            {
                Place newplace = null;
                Log.trace("riceved:" + pm.cmd);
                SendCommand(new PipeMessage { cmd = "ack-"+pm.cmd });
                switch (pm.cmd)
                {

                    case "force":

                        using (var db = Helper.getDB())
                        {
                            newplace = db.Places.Where(c => c.ID == pm.place_id).FirstOrDefault();
                        }
                        if (newplace != null)
                        {
                            
                            SendCommand(new PipeMessage { cmd = "refresh", place_id = newplace.ID, place = newplace.ID });
                            this.CurrentPlace = newplace;
                            for (int i = 0; i < Constant.UpdateRepeat; i++)
                            {
                                Helper.saveAllCurrentNetworkInPlace(newplace, true);
                            }
                        }

                        break;
                    //Deve aggioranre tutti i valori della media
                    case "newPlace":
                        //this.WrongPlace();

                        using (var db = Helper.getDB())
                        {
                            newplace = db.Places.Where(c => c.ID == pm.place_id).FirstOrDefault();
                        }
                        if (newplace != null)
                        {
                            this.CurrentPlace = newplace;
                            for (int i = 0; i < Constant.UpdateRepeatNew; i++)
                            {
                                Helper.saveAllCurrentNetworkInPlace(newplace, false);
                            }
                        }

                        break;
                    case "wrong":
                        this.WrongPlace();
                        
                        break;
                    case "hello":
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
                    //this.listener.waitHandle.Set();
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
            aTimer.Stop();
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Log.trace("currently logged-in user has changed");
            aTimer.Stop();
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
