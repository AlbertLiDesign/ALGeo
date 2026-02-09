using ALGeo;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALGeo_GH
{
    public class UnsignedDistanceField : GH_Component
    {
        public UnsignedDistanceField() : base("UnsignedDistanceField", "UDF", "To generate the unsigned distance field from an input geometry", "ALGeo", "VolumetircMesh") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input mesh (preferably closed for correct sign).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Resolution", "R", "Base resolution along the longest bbox dimension (>=2).", GH_ParamAccess.item, 64);
            pManager.AddNumberParameter("Scale", "S", "Scale factor applied to mesh bounding box (>=1 recommended).", GH_ParamAccess.item, 1.1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Centers", "C", "Voxel center points as DataTree. Path {z;y;x}, each branch contains 1 point.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("VoxelSize", "V", "Uniform voxel size (world units).", GH_ParamAccess.item);
            pManager.AddNumberParameter("CellCornerSDF", "CD", "Per-voxel 8 corner SDF values as DataTree. Path {z;y;x}. Corner order: (0,0,0)(1,0,0)(1,1,0)(0,1,0)(0,0,1)(1,0,1)(1,1,1)(0,1,1).", GH_ParamAccess.tree);
            pManager.AddNumberParameter("AllCornerSDF", "AD", "Global corner SDF field (flattened xyz, x fastest). Size = (nx+1)*(ny+1)*(nz+1).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            int res = 64;
            double scale = 1.1;

            if (!DA.GetData(0, ref mesh) || mesh == null) return;
            if (!DA.GetData(1, ref res)) return;
            if (!DA.GetData(2, ref scale)) return;

            if (res < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Resolution must be >= 2.");
                return;
            }
            if (scale <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "BBoxScale must be > 0.");
                return;
            }

            Mesh m = mesh.DuplicateMesh();

            BoundingBox bb = m.GetBoundingBox(true);
            if (!bb.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid bounding box.");
                return;
            }

            Point3d bbCenter = bb.Center;
            bb.Transform(Transform.Scale(bbCenter, scale));

            Vector3d diag = bb.Max - bb.Min;
            double maxDim = Math.Max(diag.X, Math.Max(diag.Y, diag.Z));
            if (maxDim <= 1e-12)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Bounding box is degenerate.");
                return;
            }

            // voxel size so longest axis has res samples (centers)
            double voxel = maxDim / (res - 1);

            int nx = Math.Max(2, (int)Math.Round(diag.X / voxel) + 1);
            int ny = Math.Max(2, (int)Math.Round(diag.Y / voxel) + 1);
            int nz = Math.Max(2, (int)Math.Round(diag.Z / voxel) + 1);

            Point3d bbMin = bb.Min;

            bool closed = m.IsClosed;
            if (!closed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                  "Mesh is not closed. SDF sign may be unreliable (inside/outside test can fail).");
            }

            // Corner grid dims: (nx+1)*(ny+1)*(nz+1)
            int vx = nx + 1;
            int vy = ny + 1;
            int vz = nz + 1;
            int vTotal = vx * vy * vz;

            // Flattened global corner SDF field (xyz, x fastest)
            var allCorner = new double[vTotal];
            int VIndex(int x, int y, int z) => x + vx * (y + vy * z);

            // --- 1) Compute ALL corner SDF values ONCE (parallel) ---
            Parallel.For(0, vz, zz =>
            {
                Mesh mm = m.DuplicateMesh();

                double pz = bbMin.Z + zz * voxel;

                for (int yy = 0; yy < vy; yy++)
                {
                    double py = bbMin.Y + yy * voxel;

                    for (int xx = 0; xx < vx; xx++)
                    {
                        double px = bbMin.X + xx * voxel;

                        int id = VIndex(xx, yy, zz);
                        var p = new Point3d(px, py, pz);

                        allCorner[id] = UnsignedDistance(mm, p, voxel, closed);
                    }
                }
            });

            // --- 2) Build Centers Tree (path {z;y;x}) ---
            // We build it on main thread (GH_Structure not thread-safe).
            var centersTree = new GH_Structure<GH_Point>();
            for (int z = 0; z < nz; z++)
            {
                double minz = bbMin.Z + z * voxel;
                for (int y = 0; y < ny; y++)
                {
                    double miny = bbMin.Y + y * voxel;
                    for (int x = 0; x < nx; x++)
                    {
                        double minx = bbMin.X + x * voxel;

                        var cpt = new Point3d(minx + 0.5 * voxel, miny + 0.5 * voxel, minz + 0.5 * voxel);
                        var path = new GH_Path(z, y, x);
                        centersTree.Append(new GH_Point(cpt), path);
                    }
                }
            }

            // --- 3) Build per-voxel 8-corner tree FROM allCorner (values unchanged) ---
            // Corner order: (0,0,0)(1,0,0)(1,1,0)(0,1,0)(0,0,1)(1,0,1)(1,1,1)(0,1,1)
            int[] ox = { 0, 1, 1, 0, 0, 1, 1, 0 };
            int[] oy = { 0, 0, 1, 1, 0, 0, 1, 1 };
            int[] oz = { 0, 0, 0, 0, 1, 1, 1, 1 };

            var cellCornerTree = new GH_Structure<GH_Number>();

            for (int z = 0; z < nz; z++)
            {
                for (int y = 0; y < ny; y++)
                {
                    for (int x = 0; x < nx; x++)
                    {
                        var path = new GH_Path(z, y, x);
                        for (int i = 0; i < 8; i++)
                        {
                            int vid = VIndex(x + ox[i], y + oy[i], z + oz[i]);
                            cellCornerTree.Append(new GH_Number(allCorner[vid]), path);
                        }
                    }
                }
            }

            // Outputs
            DA.SetDataTree(0, centersTree);  // Centers (tree)
            DA.SetData(1, voxel);            // VoxelSize
            DA.SetDataTree(2, cellCornerTree); // CellCornerSDF (tree)
            DA.SetDataList(3, allCorner);    // AllCornerSDF (list)
        }

        private static double UnsignedDistance(Mesh m, Point3d p, double voxel, bool closed)
        {
            Point3d cp = m.ClosestPoint(p);
            double dist = p.DistanceTo(cp);
            return dist;
        }

        protected override Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{C4CA41FD-DD9C-4836-8688-B0D5E6F37614}");
            }
        }
    }
}
