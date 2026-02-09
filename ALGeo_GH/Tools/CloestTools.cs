using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALGeo_GH.Tools
{
    class CloestTools
    {
        public static int?[] GetVertOnCrv(Mesh mesh, Curve curve, double threshold)
        {
            var vertices = mesh.TopologyVertices.ToArray();
            int?[] closestVerticeId = new int?[vertices.Length];
            Parallel.For(0, vertices.Length, i =>
            {
                var close = curve.ClosestPoint(vertices[i], out double t);
                if (close)
                {
                    var cP = curve.PointAt(t);
                    if (vertices[i].DistanceTo(new Point3f((float)cP.X, (float)cP.Y, (float)cP.Z)) < threshold)
                    {
                        closestVerticeId[i] = i;
                    }
                }
            });
            return closestVerticeId;
        }
        public static int?[] GetVertOnSrf(Mesh mesh, Surface surface, double threshold)
        {
            var vertices = mesh.TopologyVertices.ToArray();
            int?[] closestVerticeId = new int?[vertices.Length];
            Parallel.For(0, vertices.Length, i =>
            {
                var close = surface.ClosestPoint(vertices[i], out double u, out double v);
                if (close)
                {
                    var cP = surface.PointAt(u, v);
                    if (vertices[i].DistanceTo(new Point3f((float)cP.X, (float)cP.Y, (float)cP.Z)) < threshold)
                    {
                        closestVerticeId[i] = i;
                    }
                }
            });
            return closestVerticeId;
        }

        public static int?[] GetVertOnBrep(Mesh mesh, Brep brep, double threshold)
        {
            var vertices = mesh.TopologyVertices.ToArray();
            int?[] closestVerticeId = new int?[vertices.Length];
            Parallel.For(0, vertices.Length, i =>
            {
                var cP = brep.ClosestPoint(vertices[i]);
                if (vertices[i].DistanceTo(new Point3f((float)cP.X, (float)cP.Y, (float)cP.Z)) < threshold)
                {
                    closestVerticeId[i] = i;
                }
            });
            return closestVerticeId;
        }
        public static int?[] GetVertOnPt(Mesh mesh, List<Point3d> points, double threshold)
        {
            PointCloud meshPtCloud = new PointCloud(mesh.TopologyVertices.Select(v => (Point3d)v));
            int?[] closestVerticeId = new int?[points.Count];
            Parallel.For(0, points.Count, i =>
            {
                int index = meshPtCloud.ClosestPoint(points[i]);
                if (points[i].DistanceTo(new Point3d(mesh.TopologyVertices[index])) < threshold)
                {
                    closestVerticeId[i] = index;
                }
            });
            return closestVerticeId;
        }

    }
}
