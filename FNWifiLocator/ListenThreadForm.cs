﻿using System;
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
           
            Console.WriteLine("FN.Thread: ListenThreadForm.InstanceMethod is running on another thread.");

            //var client = new NamedPipeClientStream(".", "FNPipeService", PipeDirection.In, PipeOptions.Asynchronous);

            var client = new NamedPipeClientStream(Constant.ServicePipeName);
            try
            {
                client.Connect();//avvio service

                StreamString ss = new StreamString(client);
                while (!_shouldStop)
                {

                    String text = ss.ReadString();
                    if (text != null)
                    {
                        CurrentState cs = new CurrentState();
                        PipeMessage pm = Helper.DeserializeFromString<PipeMessage>(text);
                        Log.trace("command receveid " + pm.cmd);
                        Log.trace("---------------------------------notifyWPF");
                        //mw.ntfw.label.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText),"PROVA STRINGA");

                        switch (pm.cmd)
                        {
                            case "refresh":
                                if (pm.getPlace() != null)
                                {

                                    Place place = pm.getPlace();
                                    Log.trace("Place is not null" + place.ID + place.name);
                                    mw.Dispatcher.Invoke(mw.newPlace, place);
                                    mw.Dispatcher.Invoke(mw.notify, place.name);
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

                        Console.WriteLine("FN.Thread:: received message:" + pm.cmd);
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
            catch (TimeoutException e)
            {
                Log.trace("FN.Thread: " + e.ToString());
                //check se il service è in esecuzione
            }
             Console.WriteLine("FN.Thread: The instance method (Form) called by the worker thread has ended.");
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
