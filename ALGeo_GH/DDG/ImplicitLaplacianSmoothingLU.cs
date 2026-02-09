using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ALGeo;
using PlanktonGh;

namespace ALGeo_GH
{
    public class ImplicitLaplacianSmoothingLU : GH_Component
    {
        public ImplicitLaplacianSmoothingLU()
          : base("Implicit Laplacian Smoothing(LU)", "Smoothing", "Smooth a mesh using cotangent laplace operator", "ALGeo", "DDG") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Output a mesh.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData("Mesh", ref mesh);
            var pmesh = LaplacianSmoothing.ImplicitMethodLU(mesh.ToPlanktonMesh());
            DA.SetData("Mesh", pmesh.ToRhinoMesh());

        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{8E486D4B-C8D5-4F57-BAE0-A34C80FD793E}"); }
        }
    }
}
