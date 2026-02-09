using ALGeo;
using Grasshopper;
using Grasshopper.Kernel;
using PlanktonGh;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALGeo_GH
{
    public class FeatureEdges : GH_Component
    {
        public FeatureEdges() : base("FeatureEdges", "FeatureEdges", "Extract the feature edges of a mesh according to angles. ", "ALGeo", "DDG") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Input a angle for extracting feature edges.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Boundary", "B", "Regarded boundary edges as feature edges.", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("HalfEdges", "HF",
                "Output the indices of all halfedges which has been signed as feature edges.", GH_ParamAccess.list);
            pManager.AddLineParameter("Lines", "L", "Output all feature edges.", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            double angle = 0.0;
            bool boundary = true;

            if (!DA.GetData("Mesh", ref mesh)) return;
            if (!DA.GetData("Angle", ref angle)) return;
            if (!DA.GetData("Boundary", ref boundary)) return;

            var pmesh = mesh.ToPlanktonMesh();
            double feature_cosine = Math.Cos(angle / 180.0 * Math.PI);

            List<Line> fe = new List<Line>();
            int[] sit = new int[pmesh.Halfedges.Count];
            Parallel.For(0, pmesh.Halfedges.Count, i =>
            {
                if (GeoUtils.GetFeatureEages(pmesh, i, angle, boundary))
                {
                    sit[i] = 1;
                }
                else
                {
                    sit[i] = 0;
                }
            });

            List<int> hfs = new List<int>();
            for (int i = 0; i < pmesh.Halfedges.Count; i++)
            {
                if (sit[i] == 1)
                {
                    var x0 = pmesh.Vertices[pmesh.Halfedges[i].StartVertex];
                    var x1 = pmesh.Vertices[pmesh.Halfedges.EndVertex(i)];
                    Point3d p0 = new Point3d(x0.X, x0.Y, x0.Z);
                    Point3d p1 = new Point3d(x1.X, x1.Y, x1.Z);
                    hfs.Add(i);
                    fe.Add(new Line(p0, p1));
                }
            }

            DA.SetDataList("HalfEdges", hfs);
            DA.SetDataList("Lines", fe.ToList());
        }
        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("3ad63e2d-9c9d-4dd7-963a-377985637d83");
    }
}