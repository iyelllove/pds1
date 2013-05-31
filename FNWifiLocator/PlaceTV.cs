using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FNWifiLocatorLibrary;

namespace FNWifiLocator
{

    

    public partial class PlaceTV
    {
        public string Title { get; set; }
        public int Level = 0;
        public Place pl;
        public PlaceTV parentTV { get; set; }
        public List<PlaceTV> childlist = new List<PlaceTV>();
        private ObservableCollection<PlaceTV> childPlacesValue = new ObservableCollection<PlaceTV>();
        public ObservableCollection<PlaceTV> ChildPlaces
        {
            get
            {
                return childPlacesValue;
            }
            set
            {
                ChildPlaces = value;
            }
        }

        public PlaceTV()
        {
            pl = null;
            parentTV = null;
            this.Title = "...";
        }

        public PlaceTV(Place p, int level)
        {
            this.Level = level;
            this.pl = p;
            this.Title = p.name;
            foreach (Place pc in p.Childs)
            {
                PlaceTV ppc = new PlaceTV(pc, this.Level + 1) { parentTV = this };
                ChildPlaces.Add(ppc);
                childlist.Add(ppc);
                childlist.AddRange(ppc.childlist);
            }
        }


        public PlaceTV(Place p) {
            this.Level = 0;
            this.pl = p;
            this.Title = p.name;
           
            if (p.Childs != null)
            {
                foreach (Place pc in p.Childs)
                {
                    PlaceTV ppc = new PlaceTV(pc, this.Level + 1) { parentTV = this };
                    childlist.Add(ppc);
                    ChildPlaces.Add(ppc);
                    childlist.AddRange(ppc.childlist);
                }
            }
        }

        public override string ToString()
        {
            if (pl != null)
            {
                String r = "";
                for (int i = 0; i < Level; i++)
                {
                    r = r + "-";
                }
                return (r + " " + pl.name).Trim();

            }
            else {
                return Title;
            }
         }
        
      
    }
}
