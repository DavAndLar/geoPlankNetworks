using System;
using System.Collections.Generic;
using geoPlankNetworks.DataTypes;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace geoPlankNetworks.Components
{
    public class deconstructPlank : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the deconstructPlank class.
        /// </summary>
        public deconstructPlank()
          : base("deconstructPlank", "Nickname",
              "Description",
              "gPN", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Plank", "p", "p", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Center line", "cL", "c", GH_ParamAccess.item);
            pManager.AddBrepParameter("Mid surface", "mS", "mS", GH_ParamAccess.item);
            pManager.AddBrepParameter("Top surface", "tS", "tS", GH_ParamAccess.item);
            pManager.AddBrepParameter("Bottom surface", "bS", "bS", GH_ParamAccess.item);
            pManager.AddBrepParameter("Plank solid","pS","pS",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plank iPlank = null;
            DA.GetData(0, ref iPlank);

            Curve oCenterLine = iPlank.CenterLine; 
            Brep midSurface = iPlank.MidSurface;
            Brep topSurface = iPlank.TopSurface;
            Brep bottomSurface = iPlank.BottomSurface;
            Brep plankSolid = iPlank.PlankSolid;

            DA.SetData(0,oCenterLine);
            DA.SetData(1,midSurface);
            DA.SetData(2,topSurface);   
            DA.SetData(3,bottomSurface);
            DA.SetData(4,plankSolid);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E3C0A9D5-427D-458D-9202-1C0D9BCC140D"); }
        }
    }
}