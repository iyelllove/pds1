using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Text;
using System.Data.SQLite;
using System.Data.Entity;

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
            var db = new BloggingContext();
            var m = new Measure { SSID = "dsa2", timestamp = DateTime.Now };
            db.Measures.Add(m);
            db.SaveChanges();
            // Display all Blogs from the database
            var query = from b in db.Measures
                        orderby b.SSID
                        select b;

            Console.WriteLine("All blogs in the database:");
            foreach (var item in query)
            {
                Console.WriteLine(item.SSID);
            }


            */
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);


                Application.Run(new Form1());





        }

        

    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }

        public virtual List<Post> Posts { get; set; }
    }



    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }
    }

    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Measure> Measures { get; set; }
    }

    

}

