using System;
using System.Collections.Generic;
using System.Drawing;
using ALGeo;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ALGeo_GH
{
    public class MarchingCubesSingle : GH_Component
    {

        public MarchingCubesSingle() : base("MarchingCubes_Single", "MarchingCubes", "To extract an iso-surface using the Marching Cubes algorithm.", "ALGeo", "VolumetircMesh")
        { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Voxel", "V", "Input a voxel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Values", "V", "Input the values of each corners on the voxel.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Isovalue", "I", "Input an Iso-value for extracting the iso-surface.", GH_ParamAccess.item);
            pManager.AddVectorParameter("VoxelSize", "S", "The size of the voxel elements.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Interpolation", "I", "Whether to use linear interpolation.", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Output an iso-surface.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Cases", "C", "Output all the cases of voxels in the marching cubes table.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh voxel = new Mesh();
            List<double> values = new List<double>();
            double isovalue = 0.0;
            Vector3d voxelSize = new Vector3d();
            bool interpolation = true;

            if (!DA.GetData("Voxel", ref voxel)) return;
            if (!DA.GetDataList("Values", values)) return;
            if (!DA.GetData("Isovalue", ref isovalue)) return;
            if (!DA.GetData("VoxelSize", ref voxelSize)) return;
            if (!DA.GetData("Interpolation", ref interpolation)) return;

            if (voxel.Vertices.Count != 8)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of the vertices on the mesh must be 8.");
            if (values.Count != 8)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of the values must be 8.");

            Vector3d[] corners = new Vector3d[8];
            for (int i = 0; i < 8; i++)
            {
                corners[i] = new Vector3d(voxel.Vertices[i].X, voxel.Vertices[i].Y, voxel.Vertices[i].Z);
            }

            MarchingCubes mc = new MarchingCubes(1);
            var allVertices = mc.ExtractIsosurface(corners, values.ToArray(), isovalue, voxelSize, interpolation);

            Mesh mesh = new Mesh();
            List<Mesh> meshes = new List<Mesh>();
            int FCount = allVertices.Count / 3;
            for (int i = 0; i < FCount; i++)
            {
                Point3d a = new Point3d(allVertices[i * 3].X, allVertices[i * 3].Y, allVertices[i * 3].Z);
                Point3d b = new Point3d(allVertices[i * 3 + 1].X, allVertices[i * 3 + 1].Y, allVertices[i * 3 + 1].Z);
                Point3d c = new Point3d(allVertices[i * 3 + 2].X, allVertices[i * 3 + 2].Y, allVertices[i * 3 + 2].Z);

                Mesh subMesh = new Mesh();
                subMesh.Vertices.Add(a);
                subMesh.Vertices.Add(b);
                subMesh.Vertices.Add(c);
                subMesh.Faces.AddFace(0, 1, 2);
                meshes.Add(subMesh);
            }
            mesh.Append(meshes);
            mesh.Normals.ComputeNormals();
            mesh.UnifyNormals();
            DA.SetData(0, mesh);
            DA.SetDataList(1, mc.Cases);
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid { get; } = new Guid("{5498FE9C-2296-490C-A65E-DDBF8F5FD56A}");
    }
}