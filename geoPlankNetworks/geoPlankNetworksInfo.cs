using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace geoPlankNetworks
{
    public class geoPlankNetworksInfo : GH_AssemblyInfo
    {
        public override string Name => "geoPlankNetworks";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("daf5ba31-d667-4486-90d0-5a1cabb73ff5");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}