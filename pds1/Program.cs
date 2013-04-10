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
            /*
            using (var db = new MeasureContext())
            {
                var m = new Measure { SSID = "dsa", timestamp = DateTime.Now };
                db.Measures.Add(m);
                db.SaveChanges();
                var query = from b in db.Measures
                            orderby b.SSID
                            select b;

                Console.WriteLine("All Measures in the database:");
                foreach (var item in query)
                {
                    Console.WriteLine(item.SSID);
                }
            }
             * */
            //new db_pdsEntities();
            //var db = new db_pdsEntities();
           // Database.SetInitializer(new MigrateDatabaseToLatestVersion<BloggingContext, Configuration>());
            /*
            var db = new BloggingContext();
            
                        
                       
            // Display all Blogs from the database
             */


            Helper.printAllNetworks();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.Run(new Form1());





        }

        

    }

   
    

}

