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
        public Brep PlankSolid;
        public double PlankThickness;
        public double PlankWidth;
        public int PlankRefinement;
        public Brep OriginalMidSurface;
        public int PlankPosition;

        public Plank()
        {
        }

        public Plank(Brep midSurface, Brep plankSolid, double plankThickness, double plankWidth, int plankRefinement)
        {
            MidSurface = midSurface;
            PlankSolid = plankSolid;
            PlankThickness = plankThickness;
            PlankWidth = plankWidth;
            PlankRefinement = plankRefinement;
        }
        public Plank(Brep midSurface, Brep plankSolid, double plankThickness, double plankWidth, int plankRefinement, Brep originalMidSurface, int plankPosition)
        {
            MidSurface = midSurface;
            PlankSolid = plankSolid;
            PlankThickness = plankThickness;
            PlankWidth = plankWidth;
            PlankRefinement = plankRefinement;
            OriginalMidSurface = originalMidSurface;
            PlankPosition = plankPosition;
        }
    }
}
