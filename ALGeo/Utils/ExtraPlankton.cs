using Plankton;
using System.Collections.Generic;
using System.Linq;

namespace ALDGP
{
    public static class ExtraPlankton
    {
        public static int[] GetNakedFaces(this PlanktonMesh pmesh)
        {
            List<int> nakedFaces = new List<int>();
            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                if (pmesh.Vertices.IsBoundary(i))
                {
                    var faces = pmesh.Vertices.GetVertexFaces(i);
                    foreach (var item in faces)
                    {
                        if (item != -1)
                        {
                            nakedFaces.Add(item);
                        }
                    }
                }
            }
            return nakedFaces.Distinct().ToArray();
        }
        public static void MoveVertex(this PlanktonVertex vert, Vector3D vec)
        {
            vert.X += (float)vec.X;
            vert.Y += (float)vec.Y;
            vert.Z += (float)vec.Z;
        }
        public static Vector3D ToVector3D(this PlanktonVertex v)
        {
            return new Vector3D(v.X, v.Y, v.Z);
        }
        public static Vector3D ToVertor3D(this PlanktonXYZ xyz)
        {
            return new Vector3D(xyz.X, xyz.Y,xyz.Z);
        }

        public static float DotProduct(this PlanktonXYZ a, PlanktonXYZ b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
    }
}
