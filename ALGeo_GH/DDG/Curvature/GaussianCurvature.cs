using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ALGeo;
using Plankton;
using PlanktonGh;
using System.Linq;
using System.Threading.Tasks;

namespace ALGeo_GH
{
    public class GaussianCurvatureComponent : GH_Component
    {
        public GaussianCurvatureComponent()
            : base("Gaussian Curvature", "GaussianCurvature", "To calculate the gaussian curvature on each vertex.", "ALGeo", "DDG") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Curvature", "C", "Output the gaussian curvature on each vertex.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData("Mesh", ref mesh);

            var crvs = Curvature.GaussianCurvature(mesh.ToPlanktonMesh());
            DA.SetDataList("Curvature", crvs.ToList());
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{ED2B24BC-A0CF-474D-8323-6182E6A3979D}"); }
        }
    }
}