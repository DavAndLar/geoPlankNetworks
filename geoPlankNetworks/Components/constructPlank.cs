using System;
using System.Windows;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using geoPlankNetworks.DataTypes;
using Grasshopper.Kernel.Data;
using Grasshopper;
using Rhino.Commands;

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
              "gPN", "Plank")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to transfor into a geodesic plank", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Base Surface", "S", "Surface onto which the geodesic plank should be created", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Refinement", "R", "No. points along the curve where the Darboux frame should be evaluated", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Thickness", "t", "Thickness of plank", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of plank", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Layers","L","Number of plank bundles (one bundle equals the same number as geodesic directions).",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Plank", "p", "p", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Curve> iCenterLines;
            IGH_GeometricGoo geomInput = null;
            int iRef = 0;
            double iThickness = 0.0;
            double iWidth = 0.0;
            int iNoLayers = 0;

            if (!DA.GetDataTree(0, out iCenterLines)) return;
            if (!DA.GetData(1, ref geomInput)) return;
            if (!DA.GetData(2, ref iRef)) return;
            if (!DA.GetData(3, ref iThickness)) return;
            if (!DA.GetData(4, ref iWidth)) return;
            if (!DA.GetData(5, ref iNoLayers)) return;

            if (iThickness <= 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plank thickness needs to be bigger than 0.0");
                return;
            }

            if (iWidth <= 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plank width needs to be bigger than 0.0");
                return;
            }

            if (iNoLayers <=0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of layers needs to be at least 1 and positive.");
                return;
            }

            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            int planksPerBundle = iCenterLines.Branches.Count;
            DataTree<Plank> oPlankTree = new DataTree<Plank>();

            for (int i = 0; i<iCenterLines.PathCount; i++)
            {
                for (int j = 0; j < iCenterLines.Branches[i].Count;j++)
                {
                    Curve centerLine = iCenterLines.Branches[i][j].Value;
                    List<Vector3d> tangents = new List<Vector3d>();
                    List<Vector3d> normals = new List<Vector3d>();
                    List<Vector3d> biNormals = new List<Vector3d>();
                    List<LineCurve> rulings = new List<LineCurve>();
                    for (int k = 0; k < iRef + 1; k++)
                    {
                        if (geomInput is GH_Brep)
                        {
                            Brep iBaseSrf = ((GH_Brep)geomInput).Value;

                            double t = Convert.ToDouble(k) / Convert.ToDouble(iRef);
                            Vector3d T = centerLine.TangentAt(t);
                            tangents.Add(T);
                            iBaseSrf.ClosestPoint(centerLine.PointAt(t), out Point3d closestPt, out _, out _, out _, tol, out Vector3d N);
                            normals.Add(N);
                            Vector3d binormal = Vector3d.CrossProduct(T, N);
                            biNormals.Add(binormal);
                            rulings.Add(new LineCurve(closestPt - binormal * (iWidth / 2), closestPt + binormal * (iWidth / 2)));
                        }
                        else if (geomInput is GH_Mesh)
                        {
                            Mesh iBaseSrf = ((GH_Mesh)geomInput).Value;

                            double t = Convert.ToDouble(k) / Convert.ToDouble(iRef);
                            Vector3d T = centerLine.TangentAt(t);
                            tangents.Add(T);
                            iBaseSrf.ClosestPoint(centerLine.PointAt(t), out Point3d closestPt, out Vector3d N, tol);
                            normals.Add(N);
                            Vector3d binormal = Vector3d.CrossProduct(T, N);
                            biNormals.Add(binormal);
                            rulings.Add(new LineCurve(closestPt - binormal * (iWidth / 2), closestPt + binormal * (iWidth / 2)));
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The base surface must be either a Brep or Mesh.");
                            return;
                        }
                    }
                    Brep baseMidSurface = Brep.CreateFromLoft(rulings, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];                   
                    for (int l = 0; l < iNoLayers; l++)
                    {
                        for (int m = 0; m < planksPerBundle; m++)
                        {
                            Brep midSurface = Brep.CreateOffsetBrep(baseMidSurface, iThickness*(m + l*planksPerBundle), false, true, tol, out _, out _)[0];
                            Brep bottomSurface = Brep.CreateOffsetBrep(midSurface, -iThickness / 2, false, true, tol, out _, out _)[0];
                            Brep plankSolid = Brep.CreateOffsetBrep(bottomSurface, iThickness, true, true, tol, out _, out _)[0];

                            if (plankSolid.SolidOrientation == BrepSolidOrientation.Inward)
                            {
                                plankSolid.Flip();
                            }

                            List<Point3d> midPts = new List<Point3d>();
                            for (int k = 0; k < iRef + 1; k++)
                            {
                                double u = 1 / Convert.ToDouble(iRef) * Convert.ToDouble(k);
                                midSurface.Faces[0].SetDomain(0, new Interval(0.00, 1.00));
                                midSurface.Faces[0].SetDomain(1, new Interval(0.00, 1.00));
                                midSurface.Faces[0].Evaluate(u, 0.5, 1, out Point3d midPt, out _);
                                midPts.Add(midPt);
                            }
                            NurbsCurve centerCrv = NurbsCurve.Create(false, 3, midPts);

                            GH_Path path = new GH_Path(i, j, l);
                            oPlankTree.Add(new Plank(midSurface, midSurface, new List<Brep> { plankSolid }, iThickness, iWidth, centerCrv.GetLength(), iRef, m, new List<int> { 0 }), path);
                        }
                    }
                }
            }

            DA.SetDataTree(0, oPlankTree); 
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