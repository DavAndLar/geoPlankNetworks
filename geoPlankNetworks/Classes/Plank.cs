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
        public Curve CenterLine;
        public Brep MidSurface;
        public Brep TopSurface;
        public Brep BottomSurface;
        public Brep PlankSolid;

        public Plank()
        {
        }

        public Plank(Curve centerLine, Brep midSurface, Brep topSurface, Brep bottomSurface, Brep plankSolid) 
        { 
            CenterLine = centerLine;
            MidSurface = midSurface;
            TopSurface = topSurface;
            BottomSurface = bottomSurface;
            PlankSolid = plankSolid;
        }
    }
}
