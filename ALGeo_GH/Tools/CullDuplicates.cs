using Grasshopper.Kernel.Geometry.SpatialTrees;
using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALGeo_GH
{
    public class CullDuplicates
    {
        private static string SortKey(MeshFace face)
        {
            List<int> list = new List<int>();
            if (face.IsTriangle)
            {
                list = new List<int> { face.A, face.B, face.C };
                list.Sort();
            }
            if (face.IsQuad)
            {
                list = new List<int> { face.A, face.B, face.C, face.D };
                list.Sort();
            }
            return string.Join(",", list);
        }
    public static Mesh BoundaryFacets(Mesh mesh)
    {
        Dictionary<string, List<int>> dirFaces = new Dictionary<string, List<int>>();
        List<int> del = new List<int>();
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            var face = mesh.Faces[i];
            string meshKey = SortKey(face);
            if (!dirFaces.ContainsKey(meshKey))
            {
                var ids = new List<int>();
                ids.Add(i);
                dirFaces.Add(meshKey, ids);
            }
            else
            {
                dirFaces[meshKey].Add(i);
                del.Add(dirFaces[meshKey][1]);
                del.Add(dirFaces[meshKey][0]);
            }
        }

        mesh.Faces.DeleteFaces(del);
        mesh.Normals.ComputeNormals();
        mesh.UnifyNormals();
        return mesh;
    }
    private static void PointIndexCoordinates(PointIndex pi, out double x, out double y, out double z)
        {
            x = pi.Point.X;
            y = pi.Point.Y;
            z = pi.Point.Z;
        }
        public static List<Point3d> CullDuplicatePoints(IEnumerable<Point3d> points, double tolerance)
        {
            if (points == null)
            {
                return null;
            }
            Point3dList point3dList = new Point3dList(points);
            int count = point3dList.Count;
            if (count == 0)
            {
                return null;
            }
            bool[] array = new bool[count];
            Point3dList point3dList2 = new Point3dList(count);
            for (int i = 0; i < count; i++)
            {
                if (!array[i])
                {
                    point3dList2.Add(point3dList[i]);
                    for (int j = i + 1; j < count; j++)
                    {
                        if (point3dList[i].DistanceTo(point3dList[j]) <= tolerance)
                        {
                            array[j] = true;
                        }
                    }
                }
            }
            return point3dList2.ToList();
        }
        public static Mesh RemoveDuplicateFaces(Mesh mesh, int precision = 3)
        {
            Dictionary<string, List<int>> dirFaces = new Dictionary<string, List<int>>();
            List<string> meshkeys = new List<string>();
            List<int> del = new List<int>();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var center = mesh.Faces.GetFaceCenter(i);
                string meshKey = Math.Round(center.X, precision).ToString() + ',' + Math.Round(center.Y, precision).ToString() + ',' + Math.Round(center.Z, precision).ToString();
                meshkeys.Add(meshKey);
                if (!dirFaces.ContainsKey(meshKey))
                {
                    var ids = new List<int>();
                    ids.Add(i);
                    dirFaces.Add(meshKey, ids);
                }
                else
                {
                    dirFaces[meshKey].Add(i);
                    del.Add(dirFaces[meshKey][1]);
                    del.Add(dirFaces[meshKey][0]);
                }
            }

            mesh.Faces.DeleteFaces(del);
            if (!mesh.IsManifold())
            {
                Point3d[] centers = new Point3d[mesh.Faces.Count];
                MeshFace[] faces = new MeshFace[mesh.Faces.Count];
                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    centers[i] = mesh.Faces.GetFaceCenter(i);
                    faces[i] = mesh.Faces[i];
                }
                var savedFace = CullDuplicatePoints(centers.ToList());

                mesh.Faces.Clear();
                foreach (var item in savedFace)
                {
                    mesh.Faces.AddFace(faces[item]);
                }
            }
            return mesh;
        }
        private static List<int> CullDuplicatePoints(List<Point3d> list, bool cullAll = true, double tol = 0.01)
        {
            BoundingBox empty = BoundingBox.Empty;
            List<PointIndex> list2 = new List<PointIndex>(list.Count);
            int num2 = list.Count - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (list[i] != null && list[i].IsValid)
                {
                    PointIndex pointIndex = new PointIndex();
                    pointIndex.Index = i;
                    pointIndex.Point = list[i];
                    pointIndex.Weight = 1;
                    list2.Add(pointIndex);
                    empty.Union(pointIndex.Point);
                }
            }

            empty.Inflate(Math.Max(unchecked(empty.Diagonal.Length * 0.01), 0.001));

            if (list2.Count != 0)
            {
                if (list2.Count == 1)
                {
                    return null;
                }
                Node3d<PointIndex> node3d = new Node3d<PointIndex>(new Coordinates3d<PointIndex>(PointIndexCoordinates), empty, 30);
                node3d.Add(list2[0]);
                int num3 = list2.Count - 1;
                int j = 1;
                while (j <= num3)
                {
                    PointIndex pointIndex2 = list2[j];
                    PointIndex item = null;
                    for (; ; )
                    {
                        Index3d<PointIndex> index3d = node3d.NearestItem(pointIndex2);
                        if (index3d == null)
                        {
                            node3d.Add(pointIndex2);
                        }
                        item = index3d.Item;
                        double num4 = pointIndex2.Point.DistanceTo(item.Point);
                        if (num4 > tol || double.IsNaN(num4))
                        {
                            node3d.Add(pointIndex2);
                        }
                        else
                        {
                            if (cullAll) item.Weight = -1;
                            else item.Weight++;
                        }
                        break;
                    }
                    j++;
                }
                List<PointIndex> itemsGlobal = node3d.ItemsGlobal;
                List<int> removedID = new List<int>(itemsGlobal.Count);
                try
                {
                    foreach (PointIndex pointIndex3 in itemsGlobal)
                    {
                        if (pointIndex3.Weight > 0)
                        {
                            removedID.Add(pointIndex3.Index);
                        }
                    }
                }
                finally
                {
                    List<PointIndex>.Enumerator enumerator = new List<PointIndex>.Enumerator();
                    ((IDisposable)enumerator).Dispose();
                }
                return removedID;
            }
            else
            {
                return null;
            }
        }
        private enum CullMode
        {
            LeaveOne = 1,
            CullAll,
            Average
        }
        private class PointIndex
        {
            public string DebuggerDisplay
            {
                get
                {
                    return string.Format("[{3}]({0:0.00},{1:0.00},{2:0.00}) w={4}", new object[]
                    {
                        this.Point.X,
                        this.Point.Y,
                        this.Point.Z,
                        this.Index,
                        this.Weight
                    });
                }
            }

            // Token: 0x0400006C RID: 108
            public Point3d Point;

            // Token: 0x0400006D RID: 109
            public int Index;

            // Token: 0x0400006E RID: 110
            public int Weight;
        }
    }
}
