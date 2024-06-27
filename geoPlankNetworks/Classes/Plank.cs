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
        public Curve OriginalCenterCrv;
        public List<Curve>  CenterCurves;
        public Brep OriginalMidSurface; 
        public Brep IntersectedMidSrf;
        public List<Brep> PlankSolid;
        public double PlankThickness;
        public double PlankWidth;
        public double PlankLength;
        public int PlankRefinement;
        public int PlankPosition;
        public List<int> CullValues;

        public Plank()
        {
        }

        public Plank(Brep intersectedMidSrf, List<Brep> plankSolid, double plankThickness, double plankWidth, int plankRefinement)
        {
            IntersectedMidSrf = intersectedMidSrf;
            PlankSolid = plankSolid;
            PlankThickness = plankThickness;
            PlankWidth = plankWidth;
            PlankRefinement = plankRefinement;
        }
        public Plank(Curve originalCenterCrv, List<Curve> centerCurves, Brep originalMidSurface, Brep intersectedMidSrf, List<Brep> plankSolid, double plankThickness, double plankWidth,double plankLength, int plankRefinement, int plankPosition, List<int> cullValues)
        {
            OriginalCenterCrv = originalCenterCrv;
            CenterCurves = centerCurves;
            OriginalMidSurface = originalMidSurface;
            IntersectedMidSrf = intersectedMidSrf;
            PlankSolid = plankSolid;
            PlankThickness = plankThickness;
            PlankWidth = plankWidth;
            PlankLength = plankLength;
            PlankRefinement = plankRefinement;
            PlankPosition = plankPosition;
            CullValues = cullValues;
        }
    }
}
