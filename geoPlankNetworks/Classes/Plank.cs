using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace geoPlankNetworks.DataTypes
{
    public class Plank
    {
        public Brep MidSurface;
        public Brep TopSurface;
        public Brep BottomSurface;
        public Brep PlankSolid;
        public double PlankThickness;

        public Plank()
        {
        }

        public Plank(Brep midSurface, Brep topSurface, Brep bottomSurface, Brep plankSolid, double plankThickness) 
        { 
            MidSurface = midSurface;
            TopSurface = topSurface;
            BottomSurface = bottomSurface;
            PlankSolid = plankSolid;
            PlankThickness = plankThickness;
        }
    }
}
