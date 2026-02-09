using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALGeo.BasicFunc.Geometry
{
    public class ALBox

    {
        public Vector Min;
        public Vector Max;
        public Vector Center;
        private ALMesh mesh;
        public ALBox()
        {
            Min = new Vector(double.MaxValue, double.MaxValue, double.MaxValue);
            Max = new Vector(double.MinValue, double.MinValue, double.MinValue);
            Center = (Min + Max) * 0.5;
        }
        public ALBox(ALMesh mesh, Vector min, Vector max)
        {
            Min = min;
            Max = max;
            this.mesh = mesh;
        }
    }
}
