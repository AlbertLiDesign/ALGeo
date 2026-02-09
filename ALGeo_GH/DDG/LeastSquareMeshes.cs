using ALGeo;
using Grasshopper.Kernel;
using PlanktonGh;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALGeo_GH
{
    public class LeastSquareMeshes : GH_Component
    {
        public LeastSquareMeshes() : base("Least Square Meshes", "LeastSquareMeshes", "Compute Least Squares Approximate Meshes.", "ALGeo", "DDG") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Indices", "i", "ID", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Output a mesh.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            List<int> idxs = new List<int>();

            if (!DA.GetData("Mesh", ref mesh)) return;
            if (!DA.GetDataList("Indices", idxs)) return;

            var result = SurfaceProcessing.LeastSquareMeshes(mesh.ToPlanktonMesh(), idxs);

            DA.SetData("Mesh", result.ToRhinoMesh());
        }
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid { get; } = new Guid("{C0E191E4-462E-4FA9-82B4-E0D9972A48EA}");
    }
}
