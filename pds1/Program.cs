using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Text;

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
           // Database.SetInitializer(new MigrateDatabaseToLatestVersion<BloggingContext, Configuration>());
            /*
            var db = new BloggingContext();
            
                        
                       
            // Display all Blogs from the database
             * 
            var query = from b in db.Measures
                        orderby b.SSID
                        select b;

            Console.WriteLine("All blogs in the database:");
            foreach (var item in query)
            {
                Console.WriteLine(item.SSID);
            }
            /*/
           

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.Run(new Form1());





        }

        

    }

   
    

}

