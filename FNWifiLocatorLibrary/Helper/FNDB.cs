using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNWifiLocatorLibrary
{
    public sealed class FNDB
    {
         private static readonly datapds1Entities2 instance = new datapds1Entities2();
        private datapds1Entities2 db = null;
         static FNDB() { }

             private FNDB() { }

             public static datapds1Entities2 Instance
          {
              get
              {
                  return instance;
              }
          }
    }

   

 

  
}
