using ALGeo_GH.Tools;
using Grasshopper.Kernel;
using Plankton;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ALGeo_GH
{
    public class ClosestVToBrep : GH_Component
    {
        public ClosestVToBrep()
            : base("ClosestVToBreps", "ClosestVToBreps", "Get the index of the mesh vertices corresponding to each point in the point cloud.", "ALGeo", "Utils") { }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Breps", "B", "Input breps.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Input a Mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Threshold", "T", "Distance threshold.", GH_ParamAccess.item, 0.01);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Indices", "I", "Output the point indices.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            List<Brep> breps = new List<Brep>();
            double threshold = 0.01;

            DA.GetDataList("Breps", breps);
            DA.GetData("Mesh", ref m);
            DA.GetData("Threshold", ref threshold);

            List<int> allIds = new List<int>();
            for (int i = 0; i < breps.Count; i++)
            {
                int?[] indices = CloestTools.GetVertOnBrep(m, breps[i], threshold);
                var Ids = (from j in indices where j != null select j).ToList();
                foreach (var item in Ids)
                    allIds.Add((int) item);
            }

            DA.SetDataList("Indices", allIds);
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{DC3D3D07-136E-458B-BB12-E8B5EE59824A}"); }
        }
    }
}