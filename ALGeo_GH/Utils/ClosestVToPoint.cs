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
    public class ClosestVToPoint : GH_Component
    {
        public ClosestVToPoint()
            : base("ClosestVToPoint", "ClosestVToPoint", "Get the index of the mesh vertices corresponding to each point in the point cloud.", "ALGeo", "Utils") { }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Pts", "P", "Input points.", GH_ParamAccess.list);
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
            List<Point3d> pts = new List<Point3d>();
            double threshold = 0.01;

            DA.GetDataList("Pts", pts);
            DA.GetData("Mesh", ref m);
            DA.GetData("Threshold", ref threshold);

            int?[] indices = CloestTools.GetVertOnPt(m, pts, threshold);
            var Ids = (from j in indices where j != null select j).ToList();

            DA.SetDataList("Indices", Ids);
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{E27DBD46-F3DF-40C6-AC95-BCD510FA4735}"); }
        }
    }
}