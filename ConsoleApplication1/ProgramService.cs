using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using FNWifiLocatorLibrary;

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
        var server = new NamedPipeServerStream("FNPipeService");
        Console.WriteLine("Service.Main: Waiting for client connect...\n");
        server.WaitForConnection();
        Console.WriteLine("Service.Main:connection with client...\n");
        StreamString ss = new StreamString(server);

        ss.WriteString("PIPE da Service a FN");
        Console.WriteLine("Service.Main:message send...\n");
        Thread.Sleep(8000);
        server.Close();
              
    }

    static void OnStart()//string[] args
    {
        Console.WriteLine("Service.Main:AVVIO SERVICE");
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
