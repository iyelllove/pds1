﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace FNWifiLocatorLibrary
{
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
            //ioStream.ReadTimeout=1000;
            try
            {
                len = ioStream.ReadByte() * 256;
                len += ioStream.ReadByte();
                if (len > 0)
                {
                    byte[] inBuffer = new byte[len];
                    ioStream.Read(inBuffer, 0, len);
                    
                    return streamEncoding.GetString(inBuffer);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                Log.error(exc);
                Log.trace("ReadString");
            }
            return null;
            
            
            
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            try
            {
                //ioStream.WriteTimeout = 1000;
                ioStream.Flush();
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();
            }
            catch (Exception exc)
            {
                throw new Exception(exc.Message);
            }
            return outBuffer.Length + 2;
        }
    } 
}
