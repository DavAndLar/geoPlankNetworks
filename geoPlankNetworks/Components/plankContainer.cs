using System;
using System.Collections.Generic;
using geoPlankNetworks.DataTypes;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace geoPlankNetworks.Components
{
    public class plankContainer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the plankContainer class.
        /// </summary>
        public plankContainer()
          : base("Plank", "P",
              "Contains a collection of planks",
              "gPN", "Params")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plank iPlank = null;
            if (!DA.GetData(0, ref iPlank)) return;

            DA.SetData(0, iPlank);
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
            get { return new Guid("74D7511A-22AF-4BD7-A5D7-99FDE21FA19B"); }
        }
    }
}