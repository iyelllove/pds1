﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace pds1
{
    public partial class datapds1Entities2 : DbContext
    {
        public datapds1Entities2()
            : base("name=datapds1Entities2")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Checkin> Checkins { get; set; }
        public DbSet<Measure> Measures { get; set; }
        public DbSet<Network> Networks { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<PlacesNetworsValue> PlacesNetworsValues { get; set; }
    }
}
