using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using System.Threading.Tasks;

namespace FNWifiLocatorLibrary{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class PipeMessage : ISerializable
    {
        public int place_id;
        public int place
        {
            get
            { 
            
            return place_id;
            } 
            
            set { if(value == null) place_id = 0; else place_id=value; } }
        public String cmd;

        public PipeMessage()
        {
            cmd = "";
            place = 0;
        }

        public PipeMessage(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            place = (int)info.GetValue("place", typeof(int));
            cmd = (String)info.GetValue("command", typeof(string));
        }


        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("place", place);
            info.AddValue("command", cmd);
        }

        public Place getPlace() {
            if (this.place > 0)
                return Helper.getDB().Places.Where(c => c.ID == place).FirstOrDefault();
            return null;
        }

        public void setPlace(Place p)
        {
            place = p.ID;
        }


    }


    //Serialization function.
    
}
