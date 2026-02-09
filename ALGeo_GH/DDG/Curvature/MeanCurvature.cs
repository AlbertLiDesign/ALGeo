using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using PlanktonGh;
using System.Linq;
using ALGeo;

namespace ALGeo_GH
{
    public class MeanCurvatureComponent : GH_Component
    {
        public MeanCurvatureComponent()
            : base("Mean Curvature", "MeanCurvature", "To calculate the mean curvature on each vertex.", "ALGeo", "DDG") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input a mesh.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Curvature", "C", "Output the mean curvature on each vertex.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData("Mesh", ref mesh);

            var crvs = Curvature.LaplacianMeanCurvature(mesh.ToPlanktonMesh());
            DA.SetDataList("Curvature", crvs.ToList());
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid
        {
            get { return new Guid("{19092A60-8DFE-415D-95D2-72A9EC7CA5AB}"); }
        }
    }
}