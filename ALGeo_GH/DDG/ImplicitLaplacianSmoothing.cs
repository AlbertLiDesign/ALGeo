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
    public class ImplicitLaplacianSmoothing : GH_Component
    {
        public ImplicitLaplacianSmoothing()
          : base("Implicit Laplacian Smoothing", "Smoothing", "Smooth a mesh using cotangent laplace operator", "ALGeo", "DDG") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lambda", "L", "The smoothness in a single step iteration", GH_ParamAccess.item, 10);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Output a mesh.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            double lambda = 0.0;
            DA.GetData("Mesh", ref mesh);
            DA.GetData("Lambda", ref lambda);

            var pmesh = LaplacianSmoothing.ImplicitMethod(mesh.ToPlanktonMesh(), (float)lambda);
            DA.SetData("Mesh", pmesh.ToRhinoMesh());

        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{3EC02E06-2D0D-4B20-88CD-7392DF93B1ED}"); }
        }
    }
}
