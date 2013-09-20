using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using FNWifiLocatorLibrary;
using FNWifiLocator.TransDlg;
using System.Windows.Threading;




namespace FNWifiLocator
{


    public class ListenThreadForm
    {

        MainWindow mw;


        public ListenThreadForm(MainWindow mw)
        {
            this.mw = mw; mw.notify = this.UpdateText;

        }

        public volatile bool _shouldStop;

        public delegate void UpdateTextCallback(string message);


        public void InstanceMethod()
        {
            int tryconnect = 10;
            Console.WriteLine("FN.Thread: ListenThreadForm.InstanceMethod is running on another thread.");

            //var client = new NamedPipeClientStream(".", "FNPipeService", PipeDirection.In, PipeOptions.Asynchronous);
            Log.trace("Connetto" + Constant.ServicePipeName);
            var client = new NamedPipeClientStream(".", Constant.ServicePipeName, PipeDirection.In);
            Log.trace(Constant.ServicePipeName + "Connessione");
            while (tryconnect > 0 && !_shouldStop)
                {
                    try
                    {
                        //mw.Dispatcher.Invoke(mw.cmdDelegate, new PipeMessage { cmd = "disconnected" });
                        if (!client.IsConnected) client.Connect();//avvio service


                        tryconnect = 10;

                        StreamString ss = new StreamString(client);
                        while (!_shouldStop)
                        {

                            String text = ss.ReadString();
                            if (text != null)
                            {
                                //CurrentState cs = new CurrentState();
                                PipeMessage pm = Helper.DeserializeFromString<PipeMessage>(text);
                                if (pm != null)
                                {
                                    Log.trace("command receveid " + pm.cmd);
                                    if (pm.getPlace() != null && pm.getPlace().ID > 0)
                                    {
                                        Log.trace("receveid: " + pm.getPlace().name);
                                    }
                                    //mw.ntfw.label.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText),"PROVA STRINGA");
                                    mw.Dispatcher.Invoke(mw.cmdDelegate, pm);
                                    switch (pm.cmd)
                                    {
                                        case "connected":
                                        case "refresh":
                                        case "samecheckin":
                                        case "newplace":

                                            mw.Dispatcher.Invoke(mw.newPlace, pm.getPlace());
                                            break;
                                            /*
                                        case "newplace":
                                            if (pm.getPlace() != null)
                                            {

                                                Place place = pm.getPlace();
                                                mw.Dispatcher.Invoke(mw.newPlace, place);
                                            }
                                            else { Log.trace("Place is null"); }
                                            break;*/
                                        case "refresh2":
                                            Console.WriteLine("Case 2");
                                            break;
                                        default:
                                            Console.WriteLine(pm.cmd);
                                            break;
                                    }

                                    Log.trace("FN.Thread:: received message:" + pm.cmd);
                                }
                                //Notification notifForm = new Notification();
                                //notifForm.Show(pm.cmd);  

                            }
                            else
                            {
                                break;
                            }

                            if (_shouldStop)
                            {
                                Log.trace("_shouldStop is set to true");
                                break;
                            }

                        }

                    }
                    catch (ThreadAbortException abortException)
                    {
                        
                        client.Close();
                        return;
                    }
                    catch (Exception e)
                    {
                        Log.trace("FN.Thread: " + e.ToString());
                        tryconnect = -1;
                        //check se il service è in esecuzione
                    }
                    
                }
            
            
            


            if (client.IsConnected) client.Close();

        }

        private void UpdateText(string message)
        {
            //mw.ntfw.label.Content=message;
            //mw.ntfw.Show();
            notifyWindow nw = new notifyWindow(message);
            nw.ShowNotify();


        }
    }


    //public delegate void delPassData(String text);

}
