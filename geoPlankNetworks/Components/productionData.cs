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
            pManager.AddPointParameter("Hole positions", "H", "Position of holes", GH_ParamAccess.tree);
            pManager.AddPointParameter("Name positions", "N", "Position of lamella tag", GH_ParamAccess.tree);
            pManager.AddPointParameter("Bolt points", "P", "Bolt points", GH_ParamAccess.tree);
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
            DataTree<Point3d> oBoltPoints = new DataTree<Point3d>();
            DataTree<Point3d> oBoltMarkings = new DataTree<Point3d>();
            DataTree<Point3d> oTagPositions = new DataTree<Point3d>();
            DataTree<Curve> oOrigOutline = new DataTree<Curve>();

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

                    double xCoord = plank.PlankWidth * (1 + 1 / 4.0) * (i * iPlankTree.Branches[i].Count + j);
                    Curve firstLn = new Line(new Point3d(xCoord, 0, 0), new Point3d(xCoord, plank.PlankLength, 0)).ToNurbsCurve();
                    Curve secondLn = new Line(new Point3d(xCoord + plank.PlankWidth, 0, 0), new Point3d(xCoord + plank.PlankWidth, plank.PlankLength, 0)).ToNurbsCurve();
                    Brep unrolledPlank = Brep.CreateFromLoft(new List<Curve> { firstLn, secondLn }, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
                    Curve unrolledPlankOutline = Curve.JoinCurves(unrolledPlank.Edges, tol)[0];
                    Curve unrolledCenterLine = new Line(new Point3d(xCoord + plank.PlankWidth / 2, 0, 0), new Point3d(xCoord + plank.PlankWidth / 2, plank.PlankLength, 0)).ToNurbsCurve();
                    unrolledPlankOutline.Domain = new Interval(0.00, 1.00);
                    oOutline.Add(unrolledPlankOutline, new GH_Path(iPlankTree.Paths[i]));
                    oOrigOutline.Add(origMidSrfOutline, new GH_Path(iPlankTree.Paths[i]));

                    foreach (Curve intersectionCrv in plankInteriorEdges)
                    {
                        var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(origMidSrfOutline, intersectionCrv, tol, tol);
                        Point3d ptA = unrolledPlankOutline.PointAt(events[0].ParameterA);
                        Point3d ptB = unrolledPlankOutline.PointAt(events[1].ParameterA);
                        oCuts.Add(new Line(ptA, ptB), new GH_Path(iPlankTree.Paths[i]).AppendElement(j));
                    }

                    double length = 0.0;
                    Curve centerCrv = plank.OriginalCenterCrv;
                    centerCrv.Domain = new Interval(0.0, 1.0);
                    List<double> intersectionTs = new List<double>();
                    List<Curve> unrolledCenterCrvs = new List<Curve>();
                    for (int k = 0; k < plank.CenterCurves.Count; k++)
                    {
                        unrolledCenterCrvs.Add(new Line(new Point3d(xCoord + plank.PlankWidth / 2, length,0), new Point3d(xCoord + plank.PlankWidth / 2, length + plank.CenterCurves[k].GetLength(),0)).ToNurbsCurve());
                        if (plank.CullValues[k] < 1)
                        {
                            //oTagPositions.Add(unrolledCenterCrvs[k].PointAtNormalizedLength(0.5), new GH_Path(iPlankTree.Paths[i].AppendElement(j)));
                            //oTagPositions.Add(unrolledCenterCrvs[k], new GH_Path(iPlankTree.Paths[i].AppendElement(j)));
                            for (int l = 0; l < iPlankTree.PathCount; l++)
                            {
                                if (iPlankTree.Paths[i].Indices[0] != iPlankTree.Paths[l].Indices[0] && iPlankTree.Paths[i].Indices[2] == iPlankTree.Paths[l].Indices[2])
                                {
                                    for (int m = 0; m < iPlankTree.Branches[l].Count; m++)
                                    {
                                        iPlankTree.Branches[iPlankTree.Paths.IndexOf(iPlankTree.Paths[l])][m].CastTo(out Plank cutter);
                                        if (plank.PlankPosition == cutter.PlankPosition)
                                        {
                                            Curve cutterCrv = Curve.JoinCurves(cutter.CenterCurves, tol)[0];
                                            if (Rhino.Geometry.Intersect.Intersection.CurveCurve(plank.CenterCurves[k], cutterCrv, plank.PlankThickness, plank.PlankThickness).Count > 0)
                                            {
                                                var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(centerCrv, cutterCrv, plank.PlankThickness, plank.PlankThickness);
                                                foreach (var ev in events)
                                                {
                                                    oBoltPoints.Add(ev.PointA, new GH_Path(iPlankTree.Paths[i].AppendElement(j)));
                                                    intersectionTs.Add(ev.ParameterA);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        length += plank.CenterCurves[k].GetLength();
                    }
                    int[] sortedUnrolledCrvSegmentIndices= SortCrvsAlongCurve(unrolledCenterCrvs, unrolledCenterLine);

                    for (int index = 0; index < sortedUnrolledCrvSegmentIndices.Length; index++)
                    {
                        if (plank.CullValues[index] < 1)
                            oTagPositions.Add(unrolledCenterCrvs[sortedUnrolledCrvSegmentIndices[index]].PointAtNormalizedLength(0.5), new GH_Path(iPlankTree.Paths[i].AppendElement(j)));
                    }

                    if (intersectionTs.Count > 0)
                    {
                        LineCurve unrolledCenterCrv = new LineCurve(new Point2d(xCoord + plank.PlankWidth / 2, 0), new Point2d(xCoord + plank.PlankWidth / 2, plank.PlankLength));
                        unrolledCenterCrv.Domain = new Interval(0.0, 1.0);
                        foreach(double t in intersectionTs)
                        { 
                            oBoltMarkings.Add(unrolledCenterCrv.PointAtNormalizedLength(t), new GH_Path(iPlankTree.Paths[i].AppendElement(j)));
                        }
                    }
                }
            }

            DA.SetDataTree(0, oOutline);
            DA.SetDataTree(1, oCuts);
            DA.SetDataTree(2, oBoltMarkings);
            DA.SetDataTree(3, oTagPositions);
            DA.SetDataTree(4, oBoltPoints);
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
                return Properties.Resources.productionData;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E3704416-3612-4F68-91B7-3E0CECACA8DC"); }
        }
        public int[] SortCrvsAlongCurve(List<Curve> curves, Curve crv)
        {
            int L = curves.Count;
            List<Point3d> midPts = new List<Point3d>();
            Curve[] crvsArray = curves.ToArray();
            foreach (Curve curve in crvsArray)
                midPts.Add(curve.PointAtNormalizedLength(0.5));

            int[] iA = new int[L];
            double[] tA = new double[L];
            for (int i = 0; i < L; i++)
            {
                double t;
                crv.ClosestPoint(midPts[i], out t);
                iA[i] = i;
                tA[i] = t;
            }
            Array.Sort(tA, iA);
            return iA;
        }
    }
}