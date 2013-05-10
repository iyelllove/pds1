using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pds1
{

    class RightPlaceStep
    {
        public
            int e;
        public
            int d;
        public
            int w;
       
    }

    class RightPlace
    {
        public
           Place place;
        public float avg;
        private
            
            int se;
            int sd;
            List<RightPlaceStep> steps;

        public RightPlace() {
                this.avg = 0;
                this.se = 0;
                this.sd = 0;
                steps = new List<RightPlaceStep>();
        }


        public float addStep(int e, int d, int w)
        {
            Log.trace("ADD STEP " + e + "_" + d + "_" + w + "_" + this.avg);
           



            steps.Add(new RightPlaceStep() { e = e, d = d, w = w });
            this.se += e * w;
            this.sd += d * w;


            float avg = this.sd + this.se;
            avg = this.se / avg * 100;
            this.avg = avg;


            avg = e + d;
            avg = e / avg * 100;
            
            Log.trace(this.avg + " VS "+avg);

            return this.avg;
        }



    }
}
