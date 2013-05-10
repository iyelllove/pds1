using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;

namespace pds1
{
    static class ListenThreadForm
    {
        
        static public void InstanceMethod()
        {
            Console.WriteLine("ListenThreadForm.InstanceMethod is running on another thread.");

            /*var client = new NamedPipeClientStream("PipesService");
            client.Connect();
            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);

            while (true)
            {
                string input = Console.ReadLine();
                if (String.IsNullOrEmpty(input)) break;
                writer.WriteLine(input);
                writer.Flush();
                Console.WriteLine(reader.ReadLine());
            }
            // Pause for a moment to provide a delay to make 
            // threads more apparent.*/
            Thread.Sleep(100);
            Console.WriteLine("The instance method (Form) called by the worker thread has ended.");
        }
    }
}
