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



namespace FNWifiLocator
{
    static class ListenThreadForm
    {
        
        static public void InstanceMethod()
        {
            Console.WriteLine("FN.Thread: ListenThreadForm.InstanceMethod is running on another thread.");

            //var client = new NamedPipeClientStream(".", "FNPipeService", PipeDirection.In, PipeOptions.Asynchronous);
            var client = new NamedPipeClientStream("FNPipeService");
            try
            {
                client.Connect(5000);//avvio service

                StreamString ss = new StreamString(client);
                while (true)
                {
                    String text = ss.ReadString();
                    if (text != null)
                    {
                        CurrentState cs = new CurrentState();
                        PipeMessage pm = Helper.DeserializeFromString<PipeMessage>(text);
                        Log.trace(pm.cmd);
                        Console.WriteLine("FN.Thread:: received message:" + pm.cmd);
                        Notification notifForm = new Notification();
                        notifForm.Show(pm.cmd);
                    }
                    else
                    {
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

      
        
    }


    //public delegate void delPassData(String text);
   
}
