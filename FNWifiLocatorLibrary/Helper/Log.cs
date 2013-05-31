﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNWifiLocatorLibrary
{
    public class Log
    {
        static public void trace(string s) { Console.WriteLine(DateTime.Now+">>\t" + s+ "."); }
        static public void error(string s) { trace("***ERROR***" + s); }
        static public void warning(string s) { trace("**WARNING" + s); }
        static public void error(Exception ex) { trace("***ERROR***" + ex.Message); }

        
    }
}
