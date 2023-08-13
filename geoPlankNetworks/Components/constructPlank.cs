using System;
using System.Windows;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using geoPlankNetworks.DataTypes;

namespace geoPlankNetworks.Components
{
    public class constructPlank : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the geoPlank class.
        /// </summary>
        public constructPlank()
          : base("Construct plank", "Plank",
              "Constructs a geodesic plank",
              "gPN", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to transfor into a geodesic plank", GH_ParamAccess.item);
            pManager.AddBrepParameter("Base Surface", "S", "Surface onto which the geodesic plank should be created", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Refinement", "R", "No. points along the curve where the Darboux frame should be evaluated", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Thickness", "t", "Thickness of plank", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of plank", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Plank", "p", "p", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Curve iCenterLine = null;
            DA.GetData(0, ref iCenterLine);

            Brep iBaseSrf = null;
            DA.GetData(1, ref iBaseSrf);

            int iRef = 0;
            DA.GetData(2, ref iRef);

            double iThickness = 0.0;
            DA.GetData(3, ref iThickness);

            double iWidth = 0.0;
            DA.GetData(4, ref iWidth);

            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            List<double> tList = new List<double>();
            List<Point3d> ptsOnSrf = new List<Point3d>();
            List<Vector3d> tangents = new List<Vector3d>();
            List<Vector3d> normals = new List<Vector3d>();
            List<Vector3d> biNormals = new List<Vector3d>();    
            List<LineCurve> rulings = new List<LineCurve>();
            for (int i = 0; i < iRef+1; i++)
            {
                double t = Convert.ToDouble(i) / Convert.ToDouble(iRef);
                tList.Add(t);
                Vector3d T = iCenterLine.TangentAt(t);
                tangents.Add(T);
                iBaseSrf.ClosestPoint(iCenterLine.PointAt(t), out Point3d closestPt, out _, out _, out _, tol, out Vector3d N);
                normals.Add(N);
                ptsOnSrf.Add(closestPt);
                Vector3d binormal = Vector3d.CrossProduct(T, N);
                biNormals.Add(binormal);
                rulings.Add(new LineCurve(closestPt - binormal * (iWidth / 2), closestPt + binormal * (iWidth / 2)));
            }

            var midSurface = Brep.CreateFromLoft(rulings,Point3d.Unset,Point3d.Unset,LoftType.Normal, false)[0];
            Brep topSurface = Brep.CreateOffsetBrep(midSurface, iThickness / 2, false, true, tol, out _, out _)[0];
            Brep bottomSurface = Brep.CreateOffsetBrep(midSurface, -iThickness / 2, false, true, tol, out _, out _)[0];
            Brep oSolid = Brep.CreateOffsetBrep(bottomSurface, iThickness, true, true, tol, out _, out _)[0];

            Plank oPlank = new Plank(iCenterLine, midSurface, topSurface, bottomSurface,oSolid);

            DA.SetData(0, oPlank);
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