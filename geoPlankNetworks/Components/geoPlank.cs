using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace geoPlankNetworks.Components
{
    public class geoPlank : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the geoPlank class.
        /// </summary>
        public geoPlank()
          : base("geoPlank", "P",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to transfor into a geodesic plank", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Center Axis", "A", "Center axis of plank", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Mid surface", "MS", "Middle surface of plank", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Top surface","TS","Top surface of plank",GH_ParamAccess.tree);
            pManager.AddBrepParameter("Lower surface","LS","Lower surface of plank",GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("5C3595F3-3D14-4DF5-96F2-51CC5D30FC0B"); }
        }
    }
}