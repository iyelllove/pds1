using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FNWifiLocatorLibrary;



namespace FNWifiLocator
{

    class Helper : FNWifiLocatorLibrary.Helper
    {




        static public List<Place> getAllRootPlaces()
        {

            try
            {
                IEnumerable<Place> places;

                using (var db = Helper.getNewDB())
                {
                    places = db.Places.Where(c => c.Parent.Equals(null));

                    if(places.Any() != false)
                    {
                        foreach (Place p in places.ToList())
                        { 
                            Log.trace(p.name);
                        }
                        return places.ToList(); // this line is the problem
                    }
                }


            }
            catch (Exception ex)
            {
                Log.error(ex);
            }
            return null;

           // datapds1Entities2
           // return db.Places.Where(c => c.Parent.Equals(null)).ToList();
        }
    }
}
