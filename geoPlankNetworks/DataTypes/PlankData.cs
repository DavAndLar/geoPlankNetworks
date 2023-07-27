using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace geoPlankNetworks.DataTypes
{
    public class PlankData
    {
        public Curve CenterLine;
        public Surface MidSurface;
        public Surface TopSurface;
        public Surface BottomSurface;

        public PlankData()
        {
        }

        
    }
}
