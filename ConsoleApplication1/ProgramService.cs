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
        var server = new NamedPipeServerStream("PipesP");
        Console.WriteLine("Waiting for client connect...\n");
        server.WaitForConnection();
        Console.WriteLine("connection with client...\n");
        StreamString ss = new StreamString(server);

        ss.WriteString("Dai CAZZO PIPEEEE!!!");
        Console.WriteLine("message send...\n");
        Thread.Sleep(8000);
        server.Close();
              
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


    ////////////////////////////////////////////////////////////////////////
    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    } 
}
