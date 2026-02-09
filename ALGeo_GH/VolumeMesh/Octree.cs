using System;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using Rhino.Geometry.Intersect;
using Grasshopper;
using Grasshopper.Kernel.Data;
using ALGeo;

namespace ALGeo_GH
{
    public class OctreeComponent : GH_Component
    {
        public OctreeComponent() : base("Octree", "Octree", "To voxelize a mesh model with Octree data structure", "ALGeo", "VolumetircMesh") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "G", "The size of the largest voxel.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MaxLevel", "L", "The maximum level of the octree.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Center", "C", "The center of each voxel", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Size", "S", "The size of each voxel", GH_ParamAccess.tree);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var mesh = new Mesh();
            double d = 0.0;
            int maxL = 0;
            DA.GetData("Mesh", ref mesh);
            DA.GetData("Size", ref d);
            DA.GetData("MaxLevel", ref maxL);

            var center = mesh.GetBoundingBox(Plane.WorldXY).Center;
            var oct = new Octree(mesh, center, d, maxL);
            oct.divide(oct.RootOctNode, maxL - 1);


            DataTree<Point3d> pts = new DataTree<Point3d>();
            DataTree<double> size = new DataTree<double>();

            for (int i = 0; i < oct.interiorLeafs.Count; i++)
            {
                GH_Path ghp = new GH_Path(oct.interiorLeafs[i].level);
                pts.Add(oct.interiorLeafs[i].pt, ghp);
                size.Add(oct.interiorLeafs[i].edge, ghp);
            }

            for (int i = 0; i < oct.surfaceLeafs.Count; i++)
            {
                GH_Path ghp = new GH_Path(oct.surfaceLeafs[i].level + 1);
                pts.Add(oct.surfaceLeafs[i].pt, ghp);
                size.Add(oct.surfaceLeafs[i].edge, ghp);
            }

            DA.SetDataTree(0, pts);
            DA.SetDataTree(1, size);
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
                return new Guid("{8AB5F942-E131-46AF-BB92-A27ABF89FEAD}");
            }
        }
    }
}
