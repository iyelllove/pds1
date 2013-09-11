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
    
      
       public ListenThreadForm(MainWindow mw) {
            this.mw = mw;

            mw.notify = this.UpdateText;
            
             }

       public volatile bool _shouldStop;

       public delegate void UpdateTextCallback(string message);

        
     public  void InstanceMethod()
      {
           int tryconnect = 10;
            Console.WriteLine("FN.Thread: ListenThreadForm.InstanceMethod is running on another thread.");

            //var client = new NamedPipeClientStream(".", "FNPipeService", PipeDirection.In, PipeOptions.Asynchronous);
            Log.trace("Connetto" + Constant.ServicePipeName);
            var client = new NamedPipeClientStream(".",Constant.ServicePipeName, PipeDirection.In);
            while (tryconnect > 0)
            {
                Log.trace(Constant.ServicePipeName + "Connessione");
                try
                {

                    client.Connect();//avvio service
                    tryconnect = 10;

                    StreamString ss = new StreamString(client);
                    while (!_shouldStop)
                    {

                        String text = ss.ReadString();
                        if (text != null)
                        {
                            CurrentState cs = new CurrentState();
                            PipeMessage pm = Helper.DeserializeFromString<PipeMessage>(text);
                            Log.trace("command receveid " + pm.cmd);
                            if (pm.getPlace() != null && pm.getPlace().ID > 0)
                            {
                                Log.trace("receveid: " + pm.getPlace().name);  
                            }
                            //mw.ntfw.label.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText),"PROVA STRINGA");

                            switch (pm.cmd)
                            {
                                case "connected":
                                case "samecheckin":
                                    mw.Dispatcher.Invoke(mw.newPlace, pm.getPlace());
                                    break;
                                case "newplace":
                                    if (pm.getPlace() != null)
                                    {

                                        Place place = pm.getPlace();
                                        mw.Dispatcher.Invoke(mw.newPlace, place);
                                    }
                                    else { Log.trace("Place is null"); }
                                    break;
                                case "refresh2":
                                    Console.WriteLine("Case 2");
                                    break;
                                default:
                                    Console.WriteLine(pm.cmd);
                                    break;
                            }

                            Log.trace("FN.Thread:: received message:" + pm.cmd);
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
                catch (Exception e)
                {
                    Log.trace("FN.Thread: " + e.ToString());
                    Thread.Sleep(Constant.SearchPlaceTimeout);
                    tryconnect--;
                    //check se il service è in esecuzione
                }
            }
            Log.trace("FN.Thread: The instance method (Form) called by the worker thread has ended.");
            client.Close();

            /*
            Console.WriteLine("FN.Thread: ListenThreadForm.InstanceMethod is running on another thread.");

            var client = new NamedPipeClientStream("FNPipeService");
            client.Connect();


            StreamString ss = new StreamString(client);

            String text = ss.ReadString();
            Console.WriteLine("FN.Thread: recived message:" + text);

            Thread.Sleep(2000);
            //Form1 frm = new Form1();
            //delPassData del = new delPassData(frm.funData);
            //del(text);
            //frm.Show();

            //Thread.Sleep(8000);

            client.Close();*/

            // Pause for a moment to provide a delay to make 
            // threads more apparent.
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
