using System;
using System.Collections.Generic;
using System.IO;
using geoPlankNetworks.DataTypes;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
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
            pManager.AddGenericParameter("Plank segments", "P", "Plank segments after intersection", GH_ParamAccess.tree);
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

            for(int i=0; i < iPlankTree.PathCount; i++)
            {
                if (iPlankTree.Paths[i] != iCutterTree.Paths[i])
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tree structures need to match.");
                    return;
                }
            }

            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            DataTree<Plank> oPlankSegments = new DataTree<Plank>();
            DataTree<Brep> scaledCutters = new DataTree<Brep>();

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
                    Brep scaledBotSrf = Brep.CreateOffsetBrep(scaledMidSrf, -(cutter.PlankWidth+iGap)/2, false, true, tol, out _, out _)[0];
                    Brep scaledCutter = Brep.CreateOffsetBrep(scaledBotSrf, cutter.PlankWidth + iGap, true, true, tol, out _, out _)[0];
                    scaledCutters.Add(scaledCutter, iCutterTree.Paths[i]);
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
                                Brep cutterPlank = scaledCutters.Branch(cutterPath)[0];
                                if (!cutterCenter.Contains(AreaMassProperties.Compute(cutterPlank).Centroid))
                                {
                                    cutterSolids.Add(cutterPlank);
                                    cutterCenter.Add(AreaMassProperties.Compute(cutterPlank).Centroid);
                                }

                            }
                        }
                    }

                    if (plank.MidSurface.Split(cutterSolids, tol).Length > 0)
                    {
                        foreach (Brep midSrfSegment in plank.MidSurface.Split(cutterSolids, tol))
                        {
                            Point3d testPt = midSrfSegment.ClosestPoint(AreaMassProperties.Compute(midSrfSegment).Centroid);
                            int testValue = 0;
                            foreach (Brep cutter in cutterSolids)
                            {
                                if (cutter.SolidOrientation == BrepSolidOrientation.Inward)
                                {
                                    cutter.Flip();
                                }

                                if (cutter.IsPointInside(testPt, tol, true))
                                {
                                    testValue += 1;
                                }
                            }

                            if (testValue < 1)
                            {
                                Brep bottomSurface = Brep.CreateOffsetBrep(midSrfSegment, -plank.PlankThickness / 2, false, true, tol, out _, out _)[0];
                                Brep plankSolid = Brep.CreateOffsetBrep(bottomSurface, plank.PlankThickness, true, true, tol, out _, out _)[0];

                                GH_Path path = new GH_Path(i, j);
                                oPlankSegments.Add(new Plank(midSrfSegment, plankSolid, plank.PlankThickness, plank.PlankWidth, plank.PlankRefinement, plank.OriginalMidSurface, plank.PlankPosition), path);
                            }
                        }
                    }

                    else
                    {
                        Brep bottomSurface = Brep.CreateOffsetBrep(plank.MidSurface, -plank.PlankThickness / 2, false, true, tol, out _, out _)[0];
                        Brep plankSolid = Brep.CreateOffsetBrep(bottomSurface, plank.PlankThickness, true, true, tol, out _, out _)[0];

                        GH_Path path = new GH_Path(i, j);
                        oPlankSegments.Add(new Plank(plank.MidSurface, plankSolid, plank.PlankThickness, plank.PlankWidth, plank.PlankRefinement, plank.OriginalMidSurface, plank.PlankPosition), path);
                    }
                }
            }

            DA.SetDataTree(0, oPlankSegments);
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