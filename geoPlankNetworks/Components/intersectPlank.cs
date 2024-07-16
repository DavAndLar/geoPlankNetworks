using System;
using System.Collections.Generic;
using System.IO;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using geoPlankNetworks.DataTypes;
using geoPlankNetworks.Utilities;
using System.Diagnostics;
using System.Linq;

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
              "gPN", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Plank", "P", "Plank to intersect", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Cutter Plank","C","Plank to intersect with",GH_ParamAccess.tree);
            pManager.AddNumberParameter("Cut offset", "O", "Size of the gap between cut and continous lamella", GH_ParamAccess.item,0.0);

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Planks ", "P", "Planks after intersection", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> iPlankTree;
            GH_Structure<IGH_Goo> iCutterTree;
            double iGap = 0.0;

            if (!DA.GetDataTree(0, out iPlankTree)) return;
            if (!DA.GetDataTree(1, out iCutterTree)) return;
            if (!DA.GetData(2,ref iGap)) return;

            Stopwatch timer = new Stopwatch();

            if (iCutterTree.Paths[0].Length == 3 && iPlankTree.Paths[0].Length == 3)
            {
                for (int i = 0; i < iPlankTree.PathCount; i++)
                {
                    if (iPlankTree.Paths[i] != iCutterTree.Paths[i])
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tree structures need to match.");
                        return;
                    }
                }

                double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
                DataTree<Plank> oPlanks = new DataTree<Plank>();
                DataTree<Brep> scaledCutters = new DataTree<Brep>();
                DataTree<Point3d> scaledCutterCenters = new DataTree<Point3d>();

                string msg = "";


                for (int i = 0; i < iCutterTree.PathCount; i++)
                {
                    for (int j = 0; j < iCutterTree.Branches[i].Count; j++)
                    {
                        iCutterTree.Branches[i][j].CastTo(out Plank cutter);
                        List<Curve> scaledRulings = new List<Curve>();
                        for (int k = 0; k < cutter.PlankRefinement + 1; k++)
                        {
                            double u = 1 / Convert.ToDouble(cutter.PlankRefinement) * Convert.ToDouble(k);
                            cutter.OriginalMidSurface.Faces[0].SetDomain(0, new Interval(0.00, 1.00));
                            cutter.OriginalMidSurface.Faces[0].SetDomain(1, new Interval(0.00, 1.00));

                            cutter.OriginalMidSurface.Faces[0].Evaluate(u, 0, 1, out Point3d sPt, out _);
                            cutter.OriginalMidSurface.Faces[0].Evaluate(u, 1, 1, out Point3d ePt, out _);
                            LineCurve ruling = new LineCurve(sPt, ePt);

                            scaledRulings.Add(ruling.Extend(CurveEnd.Both, iGap, CurveExtensionStyle.Line));
                        }
                        Brep scaledMidSrf = Brep.CreateFromLoft(scaledRulings, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
                        Brep scaledBotSrf = Brep.CreateOffsetBrep(scaledMidSrf, -(cutter.PlankWidth + iGap) / 2, false, true, tol, out _, out _)[0];
                        Brep scaledCutter = Brep.CreateOffsetBrep(scaledBotSrf, cutter.PlankWidth + iGap, true, true, tol, out _, out _)[0];
                        scaledCutters.Add(scaledCutter, iCutterTree.Paths[i]);
                        timer.Start();
                        scaledCutterCenters.Add(AreaMassProperties.Compute(scaledCutter,false,true,false,false).Centroid, iCutterTree.Paths[i]);
                        timer.Stop();
                        msg += "First loop took " + timer.ElapsedMilliseconds.ToString() + "ms\n";
                    }
                }

                for (int i = 0; i < iPlankTree.PathCount; i++)
                {
                    for (int j = 0; j < iPlankTree.Branches[i].Count; j++)
                    {
                        iPlankTree.Branches[i][j].CastTo(out Plank plank);
                        List<Brep> cutterSolids = new List<Brep>();

                        foreach (GH_Path cutterPath in scaledCutters.Paths)
                        {
                            List<Point3d> cutterCenter = new List<Point3d>();
                            if (cutterPath.Indices[0] != iPlankTree.Paths[i].Indices[0] && cutterPath.Indices[2] == iPlankTree.Paths[i].Indices[2])
                            {
                                iCutterTree.Branches[iCutterTree.Paths.IndexOf(cutterPath)][0].CastTo(out Plank cutter);
                                if (plank.PlankPosition == cutter.PlankPosition)
                                {
                                    Stopwatch timer2 = new Stopwatch(); 
                                    Brep cutterPlank = scaledCutters.Branch(cutterPath)[0];
                                    if (!cutterCenter.Contains(scaledCutterCenters.Branch(cutterPath)[0]));
                                    {
                                        cutterSolids.Add(cutterPlank);
                                        cutterCenter.Add(scaledCutterCenters.Branch(cutterPath)[0]);
                                    }
                                }
                            }
                        }

                        List<Brep> midSrfSegments = new List<Brep>();
                        List<Brep> solidSegments = new List<Brep>();
                        List<Curve> curveSegments = new List<Curve>();
                        List<int> cullValues = new List<int>();
                        for (int l = 0; l < plank.IntersectedMidSrf.Faces.Count; l++)
                        {
                            BrepFace segment = plank.IntersectedMidSrf.Faces[l];
                            Brep brepSegment = segment.DuplicateFace(false);            //convert from BrepFace to Brep
                            if (brepSegment.Split(cutterSolids, tol).Length > 0)
                            {
                                foreach (Brep midSrfSegment in brepSegment.Split(cutterSolids, tol))
                                {
                                    midSrfSegments.Add(midSrfSegment);

                                    List<Curve> parallellFaceEdges = new List<Curve>();
                                    foreach (Curve edgeCurve in midSrfSegment.Edges)
                                    {
                                        if (Rhino.Geometry.Intersect.Intersection.CurveCurve(plank.CenterCurves[l], edgeCurve, tol, tol).Count < 1)
                                        {
                                            parallellFaceEdges.Add(edgeCurve);
                                        }
                                    }

                                    Curve fCrv = parallellFaceEdges[0];
                                    Curve sCrv = parallellFaceEdges[1];
                                    List<Point3d> midPts = new List<Point3d>();
                                    for (int k = 0; k < plank.PlankRefinement + 1; k++)
                                    {
                                        double t = 1 / Convert.ToDouble(plank.PlankRefinement) * Convert.ToDouble(k);
                                        Point3d sPt = fCrv.PointAtNormalizedLength(t);
                                        Point3d ePt = sCrv.PointAtNormalizedLength(1 - t);
                                        midPts.Add((sPt + ePt) / 2);
                                    }
                                    curveSegments.Add(Curve.CreateInterpolatedCurve(midPts, 3));


                                    Point3d testPt = midSrfSegment.ClosestPoint(AreaMassProperties.Compute(midSrfSegment, false, true, false, false).Centroid);
                                    int cullValue = 0;
                                    foreach (Brep cutter in cutterSolids)
                                    {
                                        if (cutter.SolidOrientation == BrepSolidOrientation.Inward)
                                        {
                                            cutter.Flip();
                                        }

                                        if (cutter.IsPointInside(testPt, tol, true))
                                        {
                                            cullValue += 1;
                                        }
                                    }

                                    if (cullValue < 1)
                                    {
                                        Brep bottomSurface = Brep.CreateOffsetBrep(midSrfSegment, -plank.PlankThickness / 2, false, true, tol, out _, out _)[0];
                                        Brep plankSolid = Brep.CreateOffsetBrep(bottomSurface, plank.PlankThickness, true, true, tol, out _, out _)[0];
                                        solidSegments.Add(plankSolid);
                                    }
                                    cullValues.Add(cullValue);
                                }
                            }

                            else
                            {
                                midSrfSegments.Add(brepSegment);
                                cullValues.Add(plank.CullValues[segment.FaceIndex]);

                                List<Curve> parallellFaceEdges = new List<Curve>();
                                foreach (Curve edgeCurve in brepSegment.Edges)
                                {
                                    if (Rhino.Geometry.Intersect.Intersection.CurveCurve(plank.CenterCurves[l], edgeCurve, tol, tol).Count < 1)
                                    {
                                        parallellFaceEdges.Add(edgeCurve);
                                    }
                                }

                                Curve fCrv = parallellFaceEdges[0];
                                Curve sCrv = parallellFaceEdges[1];
                                List<Point3d> midPts = new List<Point3d>();
                                for (int k = 0; k < plank.PlankRefinement + 1; k++)
                                {
                                    double t = 1 / Convert.ToDouble(plank.PlankRefinement) * Convert.ToDouble(k);
                                    Point3d sPt = fCrv.PointAtNormalizedLength(t);
                                    Point3d ePt = sCrv.PointAtNormalizedLength(1 - t);
                                    midPts.Add((sPt + ePt) / 2);
                                }
                                curveSegments.Add(Curve.CreateInterpolatedCurve(midPts, 3));


                                if (plank.CullValues[segment.FaceIndex] < 1)
                                {
                                    Brep bottomSurface = Brep.CreateOffsetBrep(brepSegment, -plank.PlankThickness / 2, false, true, tol, out _, out _)[0];
                                    Brep plankSolid = Brep.CreateOffsetBrep(bottomSurface, plank.PlankThickness, true, true, tol, out _, out _)[0];
                                    solidSegments.Add(plankSolid);
                                }
                            }
                        }

                        Brep intersectedMidSrf = Brep.JoinBreps(midSrfSegments, tol)[0];

                        int[] sortedCrvSegmentIndices = SortCrvsAlongCurve(curveSegments, plank.OriginalCenterCrv);
                        List<Curve> sortedCrvSegments = new List<Curve>();
                        List<int> sortedCullValues= new List<int>();
                        for (int index = 0; index < sortedCrvSegmentIndices.Length; index++)
                        {
                            sortedCrvSegments.Add(curveSegments[sortedCrvSegmentIndices[index]]);
                            sortedCullValues.Add(cullValues[sortedCrvSegmentIndices[index]]);
                        }

                        GH_Path path = new GH_Path(iPlankTree.Paths[i]);
                        oPlanks.Add(new Plank(plank.OriginalCenterCrv, sortedCrvSegments, plank.OriginalMidSurface, intersectedMidSrf, solidSegments, plank.PlankThickness, plank.PlankWidth, plank.PlankLength, plank.PlankRefinement, plank.PlankPosition, sortedCullValues), path);
                    }
                }

                gpnConsole.WriteLine(msg);
                DA.SetDataTree(0, oPlanks);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The data trees needs to have the following structure {direction; axis; package}");
                return;
            }
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
                return Properties.Resources.intersectPlank;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("DFE3C213-AB5E-48E3-BE7E-3A381A293CEB"); }
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