using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNWifiLocatorLibrary
{
    public sealed class MyModelDbContextSingleton
    {
        private static readonly datapds1Entities2 instance = new datapds1Entities2();

        static MyModelDbContextSingleton() { }

        private MyModelDbContextSingleton() { }

        public static datapds1Entities2 Instance
        {
            get
            {
                return instance;
            }
        }
    }  
}
