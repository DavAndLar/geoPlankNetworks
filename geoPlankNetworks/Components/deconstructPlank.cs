using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using geoPlankNetworks.DataTypes;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
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
              "gPN", "Plank")
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
            pManager.AddGenericParameter("Plank", "P", "P", GH_ParamAccess.item);
            pManager.AddBrepParameter("Mid surface", "mS", "mS", GH_ParamAccess.item);
            pManager.AddBrepParameter("Plank solid","pS","pS",GH_ParamAccess.list);
            pManager.AddIntegerParameter("Cull Values", "CV", "CV", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //GH_Structure<IGH_Goo> iPlankTree;

            Plank iPlank = null;

            if (!DA.GetData(0, ref iPlank)) return;

            DA.SetData(0, iPlank);
            DA.SetData(1,iPlank.IntersectedMidSrf);
            DA.SetDataList(2,iPlank.PlankSolid);
            DA.SetDataList(3, iPlank.CullValues);
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