using ALGeo;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ALGeo_GH
{
    public class MarchingCubes_Field : GH_Component
    {

        public MarchingCubes_Field() : base("MarchingCubes_Field", "MarchingCubes_Field", "To extract an iso-surface using the Marching Cubes algorithm.", "ALGeo", "VolumetircMesh")
        { }
        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddPointParameter("CentersTree", "C", "Voxel centers as DataTree. Path {z;y;x}. Each branch must contain exactly 1 point.", GH_ParamAccess.tree);
            p.AddNumberParameter("VoxelSize", "S", "Voxel edge length (scalar).", GH_ParamAccess.item, 1.0);
            p.AddNumberParameter("CellCornerSDFTree", "CD", "Per-voxel 8 corner SDF values as DataTree. Path {z;y;x}. Each branch must contain 8 values in your corner order.", GH_ParamAccess.tree);
            p.AddNumberParameter("IsoValue", "I", "Iso-value.", GH_ParamAccess.item, 0.0);
            p.AddBooleanParameter("Interpolation", "L", "Whether to use linear interpolation.", GH_ParamAccess.item, true);
        }

    protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddMeshParameter("Mesh", "M", "Iso-surface mesh.", GH_ParamAccess.item);
            p.AddIntegerParameter("TriangleCount", "F", "Number of triangles generated (before cleanup).", GH_ParamAccess.item);
            p.AddIntegerParameter("VertexCount", "V", "Number of vertices (after cleanup).", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> centersTree;
            GH_Structure<GH_Number> cornerTree;
            double s = 1.0;
            double iso = 0.0;
            bool interpolation = true;

            if (!DA.GetDataTree(0, out centersTree)) return;
            if (!DA.GetData(1, ref s)) return;
            if (!DA.GetDataTree(2, out cornerTree)) return;
            if (!DA.GetData(3, ref iso)) return;
            DA.GetData(4, ref interpolation);

            if (s <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "VoxelSize must be > 0.");
                return;
            }

            MarchingCubes mc = new MarchingCubes(1);
            Vector3d voxelSize = new Vector3d(s, s, s);

            var mesh = new Mesh();
            int triCount = 0;

            // 遍历 centersTree 的所有 cell
            // 要求 centersTree 与 cornerTree path 对齐（同样的 {z;y;x}）
            foreach (var path in centersTree.Paths)
            {
                // 取中心
                var cBranch = centersTree.get_Branch(path);
                if (cBranch == null || cBranch.Count < 1) continue;

                var ghPt = cBranch[0] as GH_Point;
                if (ghPt == null) continue;

                Point3d cpt = ghPt.Value;

                // 取 8 个角点值
                var vBranch = cornerTree.get_Branch(path);
                if (vBranch == null || vBranch.Count < 8)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Missing/invalid 8 corner values at path {{{path}}}.");
                    continue;
                }

                double[] values = new double[8];
                for (int i = 0; i < 8; i++)
                {
                    var ghNum = vBranch[i] as GH_Number;
                    values[i] = ghNum.Value;
                }

                    Vector3d minCorner = new Vector3d(cpt.X - 0.5 * s, cpt.Y - 0.5 * s, cpt.Z - 0.5 * s);

                var voxelCorners = new Vector3d[] { minCorner };

                List<Vector3d> tris = mc.ExtractIsosurface(voxelCorners, values, iso, voxelSize, interpolation);
                if (tris == null || tris.Count < 3) continue;

                for (int i = 0; i + 2 < tris.Count; i += 3)
                {
                    var a = tris[i];
                    var b = tris[i + 1];
                    var c = tris[i + 2];

                    int ia = mesh.Vertices.Add(a.X, a.Y, a.Z);
                    int ib = mesh.Vertices.Add(b.X, b.Y, b.Z);
                    int ic = mesh.Vertices.Add(c.X, c.Y, c.Z);
                    mesh.Faces.AddFace(ia, ib, ic);
                    triCount++;
                }
            }

            if (mesh.Faces.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No triangles extracted. Check IsoValue / field sign convention.");
                DA.SetData(0, new Mesh());
                DA.SetData(1, 0);
                DA.SetData(2, 0);
                return;
            }

            mesh.Vertices.CombineIdentical(true, true);
            mesh.Vertices.CullUnused();

            // 2) 统一面朝向 & 法线
            mesh.UnifyNormals();
            mesh.Normals.ComputeNormals();

            // 3) 压缩
            mesh.Compact();

            DA.SetData(0, mesh);
            DA.SetData(1, triCount);
            DA.SetData(2, mesh.Vertices.Count);
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid { get; } = new Guid("{B770CC6A-EEED-4961-9652-D693E45704BD}");
    }
}