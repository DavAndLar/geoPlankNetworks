using System;
using System.Collections.Generic;
using geoPlankNetworks.DataTypes;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace geoPlankNetworks.Components
{
    public class intersectPlank : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the intersectPlank class.
        /// </summary>
        public intersectPlank()
          : base("intersectPlank", "Nickname",
              "Description",
              "gPN", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Plank", "P", "Plank to intersect", GH_ParamAccess.item);
            pManager.AddGenericParameter("Cutter Plank","C","Plank to intersect with",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Plank segments", "S", "Plank segments after intersection", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plank iPlank = null; 
            Plank iCutter = null;

            if (!DA.GetData(0, ref iPlank)) return;
            if (!DA.GetData(1, ref iCutter)) return;

            List<Plank> plankSegments = new List<Plank>();

            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            foreach (Brep midSrfSegment in iPlank.MidSurface.Split(iCutter.PlankSolid, 0.01, out _))
            {
                if(!iCutter.PlankSolid.IsPointInside(AreaMassProperties.Compute(midSrfSegment).Centroid, tol, true)) 
                {
                    Brep topSurface = Brep.CreateOffsetBrep(midSrfSegment, iPlank.PlankThickness / 2, false, true, tol, out _, out _)[0];
                    Brep bottomSurface = Brep.CreateOffsetBrep(midSrfSegment, -iPlank.PlankThickness / 2, false, true, tol, out _, out _)[0];
                    Brep plankSolid = Brep.CreateOffsetBrep(bottomSurface, iPlank.PlankThickness, true, true, tol, out _, out _)[0];

                    plankSegments.Add(new Plank(midSrfSegment, topSurface, bottomSurface, plankSolid, iPlank.PlankThickness));
                }
            }

            DA.SetDataList(0, plankSegments);
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
            get { return new Guid("DFE3C213-AB5E-48E3-BE7E-3A381A293CEB"); }
        }
    }
}