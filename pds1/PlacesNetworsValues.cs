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
    public partial class PlacesNetworsValues
    {
        public PlacesNetworsValues()
        {
            this.rilevance = 0;
        }
    
        public int PlacesID { get; set; }
        public int NetworksID { get; set; }
        public short rilevance { get; set; }
        public short media { get; set; }
        public short variance { get; set; }
    
        public virtual Places Place { get; set; }
        public virtual Networks Network { get; set; }
    }
    
}
