using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNWifiLocatorLibrary
{
    public class Log
    {
        static public void trace(string s)
        {
            if (enableLogEvent == true)
            {



                
                

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                EventLog.WriteEntry(sSource, s);



            }
            else
            {
                Console.WriteLine(DateTime.Now + ">>\t" + s + ".");
            }
            
                 
            
        }
        static public void error(string s) { trace("***ERROR***" + s); }
        static public void warning(string s) { trace("**WARNING" + s); }
        static public void error(Exception ex) { trace("***ERROR***" + ex.Message); }

        static public string filename = null;
        static public string sLog = "Application";
        static public string sSource = "ServiceLog";
        static public bool enableLogEvent = false;
       
        public static void setLogEvent() {

            enableLogEvent = true;
            EventLog eventLog = new System.Diagnostics.EventLog();

            if (!System.Diagnostics.EventLog.SourceExists(sSource))
            {
                System.Diagnostics.EventLog.CreateEventSource(sSource, sLog);
            }
            eventLog.Source = sSource;
            eventLog.Log = sLog;
            eventLog.Clear();

            //if (!EventLog.SourceExists(sSource)) EventLog.DeleteEventSource(sSource);
        }

        public static void setFileName(String str) {
            filename = str;
          
            
        }


        }


    
}
