using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using FNWifiLocatorLibrary;

namespace FNWifiLocatorService
{
    public class ListenThread
    {
        private Service1 s;
        private NamedPipeServerStream service_server;
        public String pipeName;
        public volatile bool _shouldStop;

        public ListenThread(Service1 s, String pipeName, NamedPipeServerStream server)
        {
            this.pipeName = pipeName;
            this.s = s;
            this.service_server = server;
            //server.WaitForConnection();
        }

        public void connectServer()
        {
        }

        public void InstanceMethod()
        {

            Console.WriteLine("Service.Thread: ListenThreadForm.InstanceMethod is running on another thread.");

            Log.trace("Ascolto" + pipeName);




            var client = new NamedPipeClientStream(".", pipeName, PipeDirection.In);

            try
            {

                while (!_shouldStop)
                {
                    client.Connect();//avvio service

                    StreamString ss = new StreamString(client);
                    while (client.IsConnected)
                    {
                        String text = ss.ReadString();
                        if (text != null)
                        {
                            this.s.newCommand.Invoke(Helper.DeserializeFromString<PipeMessage>(text));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (_shouldStop)
                {
                    Log.trace("_shouldStop is set to true");
                }
            }
            catch (TimeoutException e)
            {
                Log.trace("FN.Thread: " + e.ToString());
                //check se il service è in esecuzione
            }
            Console.WriteLine("FN.Thread: The instance method (Form) called by the worker thread has ended.");
            client.Close();
        }

    }



}