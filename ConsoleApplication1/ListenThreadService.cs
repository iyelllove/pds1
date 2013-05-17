using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;

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


            //while (true)
            //{
                String text = ss.ReadString();
                Console.WriteLine("Service.Thread: recived message:" + text);
            //}

            //Thread.Sleep(2000);
            
            client.Close();

            // Pause for a moment to provide a delay to make 
            // threads more apparent.*/
            Thread.Sleep(100);
            Console.WriteLine("Service.Thread: The instance method (Form) called by the worker thread has ended.");
        }
    }


   
}
