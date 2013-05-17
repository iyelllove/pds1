using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using FNWifiLocatorLibrary;

namespace ConsoleApplication1
{
    static class ListenThreadService
    {

        static public void InstanceMethod()
        {
            Console.WriteLine("Service.Thread: ListenThreadForm.InstanceMethod is running on another thread.");
            
            var client = new NamedPipeClientStream("FNPipeLocator");
            client.Connect();
             StreamString ss = new StreamString(client);
                while (true)
                 {
                    String text = ss.ReadString();
                    if (text != null)
                    {
                        PipeMessage pm = Helper.DeserializeFromString<PipeMessage>(text);
                        Log.trace(pm.cmd);
                        Console.WriteLine("Service.Thread: recived message:" + pm.cmd);
                    }
                    else {
                        break;
                    }
                 }
            client.Close();
        }
    }


   
}
