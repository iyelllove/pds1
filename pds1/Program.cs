using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
    
namespace pds1
{

    
    static class Program
    {

        
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// modificare da qui
        /// </summary>
        [STAThread]
        static void Main()
        {
            

            Helper.printAllNetworks();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.Run(new Form1());





        }

        

    }

   
    

}

