using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using geoPlankNetworks.DataTypes;
using Grasshopper;

namespace geoPlankNetworks.Components
{
    public class productionData : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the productionData class.
        /// </summary>
        public productionData()
          : base("productionData", "Nickname",
              "Description",
              "gPN", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Planks", "P", "Planks from which production data should be created", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Plank outline", "O", "Plank outline", GH_ParamAccess.tree);
            pManager.AddLineParameter("Cuts", "C", "Cuts", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> iPlankTree;
 
            if (!DA.GetDataTree(0, out iPlankTree)) return;

            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            DataTree<Curve> oOutline = new DataTree<Curve>();
            DataTree<Line> oCuts = new DataTree<Line>();
            for (int i = 0; i < iPlankTree.PathCount; i++)
            {
                for (int j = 0; j < iPlankTree.Branches[i].Count; j++)
                {
                    iPlankTree.Branches[i][j].CastTo(out Plank plank);
                    List<Curve> plankInteriorEdges = new List<Curve>();
                    foreach (BrepEdge edge in plank.IntersectedMidSrf.Edges)
                    {
                        if (edge.Valence == EdgeAdjacency.Interior)
                        {
                            plankInteriorEdges.Add(edge);
                        }
                    }

                    Curve origMidSrfOutline = Curve.JoinCurves(plank.OriginalMidSurface.Edges, tol)[0];
                    origMidSrfOutline.Reverse();
                    origMidSrfOutline.Domain = new Interval(0.00, 1.00);

                    double xCoord = plank.PlankWidth*(1+1/4.0)*(i* iPlankTree.Branches[i].Count+j);
                    Curve firstLn = new Line(new Point3d(xCoord, 0,0),new Point3d(xCoord, plank.PlankLength,0)).ToNurbsCurve();
                    Curve secondLn = new Line(new Point3d(xCoord+plank.PlankWidth, 0,0), new Point3d(xCoord + plank.PlankWidth, plank.PlankLength,0)).ToNurbsCurve();
                    Brep unrolledPlank = Brep.CreateFromLoft(new List<Curve> { firstLn, secondLn }, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
                    Curve unrolledPlankOutline = Curve.JoinCurves(unrolledPlank.Edges, tol)[0];
                    unrolledPlankOutline.Domain = new Interval(0.00, 1.00);


                    foreach (Curve intersectionCrv in plankInteriorEdges)
                    {
                        var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(origMidSrfOutline, intersectionCrv, tol, tol);
                        Point3d ptA = unrolledPlankOutline.PointAt(events[0].ParameterA);
                        Point3d ptB = unrolledPlankOutline.PointAt(events[1].ParameterA);
                        oCuts.Add(new Line(ptA, ptB), new GH_Path(iPlankTree.Paths[i]));
                    }

                    oOutline.Add(unrolledPlankOutline , new GH_Path(iPlankTree.Paths[i]));
                }
            }

            DA.SetDataTree(0, oOutline);
            DA.SetDataTree(1, oCuts);
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
            get { return new Guid("E3704416-3612-4F68-91B7-3E0CECACA8DC"); }
        }
    }
}