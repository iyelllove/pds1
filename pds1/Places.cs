//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace pds1
{
    public partial class Places
    {
        public Places()
        {
            this.PlacesNetworsValues = new HashSet<PlacesNetworsValues>();
            this.children = new HashSet<Places>();
            this.Checkins = new HashSet<Checkin>();
        }
    
        public int ID { get; set; }
        public string name { get; set; }
        public bool measures_num { get; set; }
    
        public virtual ICollection<PlacesNetworsValues> PlacesNetworsValues { get; set; }
        public virtual Places Parent { get; set; }
        public virtual ICollection<Places> children { get; set; }
        public virtual ICollection<Checkin> Checkins { get; set; }
    }
    
}