using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;

namespace pds1
{


    public class MeasureContext : DbContext
    {
        public DbSet<Measure> Measures { get; set; }
    }

    

    public class Measure
    {
        public int MeasureId { get; set; }
        public string SSID { get; set; }
        public DateTime timestamp { get; set; }
    }
}
