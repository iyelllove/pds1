using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class ProgramService
    {
        static void Main(string[] args)
        {
            OnStart();
            Service1();
            OnShutdown();// when the CanShutdown property is true
                         // the system is shutting down.
                         //per simulare gestire evento SessionEndedEventHandler
            OnStop();
        }

        
        
    static void Service1()
    {  
     //ricezione eventi di sistema

    //azioni da intraprendere

    //eventuale comunicazione al form
       /* var server = new NamedPipeServerStream("PipesService");
        server.WaitForConnection();
        StreamReader reader = new StreamReader(server);
        StreamWriter writer = new StreamWriter(server);
        while (true)
        {
            var line = reader.ReadLine();
            writer.WriteLine(String.Join("", line.Reverse()));
            writer.Flush();
        }*/
              
    }

    static void OnStart()//string[] args
    {
        Console.WriteLine("AVVIO SERVICE");
        //Thread.Sleep(100000);

        Thread InstanceCaller = new Thread(
            new ThreadStart(ListenThreadService.InstanceMethod));

        // Start the thread.
        InstanceCaller.Start();
    }


    static void OnStop()
    {
        
    }
    
     static void OnShutdown()
    {

    }

   
    
    }
}
