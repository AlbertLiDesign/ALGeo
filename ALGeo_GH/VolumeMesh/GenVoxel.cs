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
    public class GenVoxelComponent : GH_Component
    {
        public GenVoxelComponent() : base("GenVoxel", "GenVoxel", "To generate voxel based on a center and a size number.", "ALGeo", "VolumetircMesh") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Center", "C", "The center of the voxel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "S", "The size of the voxel", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Voxel", "V", "Output the voxel.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double size = 0.0;
            var center = new Point3d();
            DA.GetData("Center", ref center);
            DA.GetData("Size", ref size);

            Mesh mesh = GenVoxel(center, size);
            DA.SetData("Voxel", mesh);
        }
        public static Mesh GenVoxel(Point3d center, double size)
        {
            Mesh box = new Mesh();

            var Xmin = center.X - size * 0.5;
            var Xmax = center.X + size * 0.5;
            var Ymin = center.Y - size * 0.5;
            var Ymax = center.Y + size * 0.5;
            var Zmin = center.Z - size * 0.5;
            var Zmax = center.Z + size * 0.5;

            box.Vertices.Add(Xmin, Ymin, Zmin);
            box.Vertices.Add(Xmax, Ymin, Zmin);
            box.Vertices.Add(Xmax, Ymax, Zmin);
            box.Vertices.Add(Xmin, Ymax, Zmin);

            box.Vertices.Add(Xmin, Ymin, Zmax);
            box.Vertices.Add(Xmax, Ymin, Zmax);
            box.Vertices.Add(Xmax, Ymax, Zmax);
            box.Vertices.Add(Xmin, Ymax, Zmax);

            box.Faces.AddFace(1, 0, 3, 2);
            box.Faces.AddFace(0, 1, 5, 4);
            box.Faces.AddFace(1, 2, 6, 5);
            box.Faces.AddFace(6, 2, 3, 7);
            box.Faces.AddFace(3, 0, 4, 7);
            box.Faces.AddFace(6, 7, 4, 5);

            box.Normals.ComputeNormals();
            box.UnifyNormals();

            return box;
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
                return new Guid("{D2D8FAA1-BD20-4C14-BB09-13AECB3A1866}");
            }
        }
    }
}