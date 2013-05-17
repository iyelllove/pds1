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

            var client = new NamedPipeClientStream("PipesP");
            client.Connect();


            StreamString ss = new StreamString(client);

            String text = ss.ReadString();
            Console.WriteLine("recived message:"+text);

            Thread.Sleep(1000);
            Form1 frm = new Form1();
            delPassData del = new delPassData(frm.funData);
            del(text);
            frm.Show();

            Thread.Sleep(8000);

            client.Close();

            // Pause for a moment to provide a delay to make 
            // threads more apparent.*/
            Thread.Sleep(100);
            Console.WriteLine("The instance method (Form) called by the worker thread has ended.");
        }
    }


    public delegate void delPassData(String text);

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
