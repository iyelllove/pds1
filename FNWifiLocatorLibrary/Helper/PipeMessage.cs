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
        public Place place { get; set; }
        public String cmd;

        public PipeMessage()
        {
            cmd = "";
            place = null;
        }

        public PipeMessage(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            place = (Place)info.GetValue("place", typeof(Place));
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
    }


    //Serialization function.
    
}
