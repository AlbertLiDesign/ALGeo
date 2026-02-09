using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ALGeo_GH
{
    public class RemoveDuplicateFacesComponent : GH_Component
    {
        public RemoveDuplicateFacesComponent()
          : base("Remove Duplicate Faces", "Remove Duplicate Faces", "Remove all duplicate mesh faces", "ALGeo", "Utils") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "M", "Input voxelized meshes.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Precision", "P", "Precision.", GH_ParamAccess.item, 3);
            pManager.AddBooleanParameter("Closed", "C" , "Whether to close the holes in the mesh.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Output the surface mesh.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var meshes = new List<Mesh>();
            int precision = 3;
            bool closed = false;

            DA.GetDataList("Meshes", meshes);
            DA.GetData("Precision", ref precision);
            DA.GetData("Closed", ref closed);

            Mesh mesh = new Mesh();
            mesh.Append(meshes);

            mesh = CullDuplicates.RemoveDuplicateFaces(mesh, precision);
            if (!mesh.IsManifold())
            {
                Message = "Non-manifold mesh";
            }
            else
            {
                Message = "No non-manifold edges";
            }
            if (!mesh.IsClosed && closed)
            {
                mesh.FillHoles();
            }
            mesh.Normals.ComputeNormals();
            mesh.UnifyNormals();

            DA.SetData("Mesh", mesh);
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{42027835-5A61-40C1-B4B5-983CE30EC9FF}"); }
        }
    }
}
