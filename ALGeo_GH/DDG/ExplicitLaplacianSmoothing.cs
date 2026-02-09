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
    public class ExplicitLaplacianSmoothing : GH_Component
    {
        public ExplicitLaplacianSmoothing()
          : base("Explicit Laplacian Smoothing", "Smoothing", "Smooth a mesh using cotagent laplace operator", "ALGeo", "DDG") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lambda", "L", "The smoothness in a single step iteration", GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("Iteration", "I", "The number of iteration.", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("KeepBoundary", "B", "Keep boundary or not.", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Output a mesh.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            double lambda = 0.0;
            int iter = 1;
            bool keepBoundary = true;
            DA.GetData("Mesh", ref mesh);
            DA.GetData("Lambda", ref lambda);
            DA.GetData("Iteration", ref iter);
            DA.GetData("KeepBoundary", ref keepBoundary);

            var pmesh = LaplacianSmoothing.ExplicitMethod(mesh.ToPlanktonMesh(), (float)lambda, iter, keepBoundary);
            DA.SetData("Mesh", pmesh.ToRhinoMesh());

        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{59656BF0-49B4-4AA9-BFB8-71202BEDEE38}"); }
        }
    }
}
