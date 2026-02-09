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
    public class PrincipalCurvatureComponent : GH_Component
    {
        public PrincipalCurvatureComponent()
            : base("Principal Curvature", "PrincipalCurvature", "To calculate the principal curvature on each vertex.", "ALGeo", "DDG") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Maximum Curvature", "Max", "Output the maximum curvature on each vertex.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Minimum Curvature", "Min", "Output the minimum curvature on each vertex.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData("Mesh", ref mesh);

            double[] kmax = new double[mesh.Vertices.Count];
            double[] kmin = new double[mesh.Vertices.Count];
            Curvature.PrincipalCurvature(mesh.ToPlanktonMesh(),ref kmax, ref kmin);

            DA.SetDataList("Maximum Curvature", kmax.ToList());
            DA.SetDataList("Minimum Curvature", kmin.ToList());
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{63F98AEC-594E-44E7-8DA1-8755DB88D97C}"); }
        }
    }
}