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
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace ConsoleService
{
    public partial class ProgramService
    {
        public NamedPipeServerStream server;
        static void Main(string[] args)
        {
            new Service();    
        }
    }
}
