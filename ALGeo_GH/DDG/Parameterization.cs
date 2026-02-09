//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using Grasshopper.Kernel;
//using Plankton;
//using Rhino.Geometry;
//using Grasshopper.Kernel.Types;
//using ALGeo;
//using PlanktonGh;

//namespace ALGeo_GH
//{
//    public class Parameterization : GH_Component
//    {
//        public Parameterization() : base("Parameterization", "Parameterization", "Compute Least Squares Approximate Meshes.", "ALGeo", "DDG") { }

//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Mesh", "M", "Input a mesh which is homeomorphic to a disk.", GH_ParamAccess.item);
//            pManager.AddCurveParameter("Boundary", "B", "Input a closted ployline as a boundary for parameterization.", GH_ParamAccess.item);
//        }

//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Mesh", "M", "Output the repaired mesh.", GH_ParamAccess.item);
//        }
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            Mesh mesh = new Mesh();
//            GH_Curve ghc = new GH_Curve();

//            if (!DA.GetData("Mesh", ref mesh)) return;
//            DA.GetData("Boundary", ref ghc);

//            PlanktonMesh pmesh = mesh.ToPlanktonMesh();

//            if (pmesh.IsClosed())
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The mesh is not a open mesh.");
//                return;
//            }

//            List<Vector> textureVerts = new List<Vector>();
//            List<int> boundaryIdx = new List<int>();
//            for (int i = 0; i < pmesh.Vertices.Count; i++)
//            {
//                if (pmesh.Vertices.IsBoundary(i))
//                {
//                    boundaryIdx.Add(i);
//                    textureVerts.Add(pmesh.Vertices[i].ToVector());
//                }
//                textureVerts.Add(Vector.Origin);
//            }

//            Curve c = ghc.Value;
//            c.ClosestPoint(new Point3d(textureVerts[boundaryIdx[0]].X, textureVerts[boundaryIdx[0]].Y, textureVerts[boundaryIdx[0]].Z), out double t);
//            c.ChangeClosedCurveSeam(t);
//            double[] bpts = c.DivideByCount(boundaryIdx.Count+1, false);
//            for (int i = 0; i < bpts.Length; i++)
//            {
//                var pt = c.PointAt(bpts[i]);
//                textureVerts[boundaryIdx[i]] = new Vector(pt.X, pt.Y, pt.Z);
//            }

//            var result = ALGeo.Parameterization.HarmonicMethod(pmesh, textureVerts);
//            DA.SetData("Mesh", result.ToRhinoMesh());
//        }
//        protected override Bitmap Icon => null;
//        public override Guid ComponentGuid { get; } = new Guid("{528585EB-744D-493B-87C3-EE02A2B535B4}");
//    }
//}