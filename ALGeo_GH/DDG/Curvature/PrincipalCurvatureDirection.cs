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
    public class PrincipalCurvatureDirectionComponent : GH_Component
    {
        public PrincipalCurvatureDirectionComponent()
            : base("Principal Curvature Direction", "PrincipalCurvatureDirection", "To calculate the principal curvature directions on each vertex.", "ALGeo", "DDG") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Maximum Curvature Direction", "Max", "Output the maximum curvature direction on each vertex.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Minimum Curvature Direction", "Min", "Output the minimum curvature direction on each vertex.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData("Mesh", ref mesh);

            double[] kmax = new double[mesh.Vertices.Count];
            double[] kmin = new double[mesh.Vertices.Count];
            var pmesh = mesh.ToPlanktonMesh();
            Curvature.PrincipalCurvature(pmesh,ref kmax, ref kmin);
            var maxDirts = Curvature.FindMaxDirections(pmesh, kmax);

            List<Vector> maxV = new List<Vector>();
            foreach (var item in maxDirts)
                maxV.Add(new Vector(item.X,item.Y,item.Z));

                DA.SetDataList("Maximum Curvature Direction", maxV.ToList());
            DA.SetDataList("Minimum Curvature Direction", maxV.ToList());
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{F3FFA2B1-B41E-4058-B6E7-1111340A70E3}"); }
        }
    }
}