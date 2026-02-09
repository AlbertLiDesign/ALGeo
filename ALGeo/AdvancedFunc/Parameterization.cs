using Plankton;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ALGeo
{
    public class Parameterization
    {
        public static PlanktonMesh HarmonicMethod(PlanktonMesh pmesh, List<Vector> textureVerts)
        {
            List<int> free_vertices = new List<int>();
            int[] idx = new int[pmesh.Vertices.Count];

            int num = 0;
            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                if (!pmesh.Vertices.IsBoundary(i))
                {
                    idx[i] = num++;
                    free_vertices.Add(i);
                }
            }
            int n = free_vertices.Count;

            var eweight = LaplaceOperator.CotLaplaceEdgeWeight(pmesh);

            for (int i = 0; i < eweight.Length; i++)
            {
                eweight[i] = Math.Max(0.0, eweight[i]);
            }

            var L = new List<Triplet>();
            var B = new List<Triplet>();

            for (int i = 0; i < n; i++)
            {
                var v = free_vertices[i];
                var hs = pmesh.Vertices.GetHalfedges(v);
                var ww = 0.0;
                Vector b = Vector.Origin();
                for (int j = 0; j < hs.Length; j++)
                {
                    var vv = pmesh.Halfedges.EndVertex(hs[j]);
                    ww += eweight[hs[j] >> 1];
                    if (pmesh.Vertices.IsBoundary(vv))
                    {
                        b -= -eweight[hs[j] >> 1] * textureVerts[vv];
                    }
                    else
                        L.Add(new Triplet(i, idx[vv], -eweight[hs[j] >> 1]));
                }
                B.Add(new Triplet(i, 0, b.X));
                B.Add(new Triplet(i, 1, b.Y));
                L.Add(new Triplet(i, i, ww));
            }

            double[] X = new double[2 * n];
            Solver.Solve(0, L.ToArray(), L.Count, n, B.ToArray(), B.Count, 2, X);

            for (int i = 0; i < n; i++)
            {
                textureVerts[free_vertices[i]] = new Vector(X[i], X[n + i], 0.0);
            }           

            return pmesh;
        }
        public static PlanktonMesh ARAPMethod(PlanktonMesh pmesh)
        {
            PlanktonMesh newMesh = new PlanktonMesh(pmesh);

            var triMesh = SurfaceProcessing.Triangulation(pmesh, true);

            int numVertices = triMesh.Vertices.Count;
            double[] verts = new double[3 * numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                var vert = triMesh.Vertices[i];
                verts[3 * i] = vert.X;
                verts[3 * i + 1] = vert.Y;
                verts[3 * i + 2] = vert.Z;
            }

            int numFaces = triMesh.Faces.Count;
            int[] faces = new int[3 * numFaces];
            for (int i = 0; i < numFaces; i++)
            {
                var face = triMesh.Faces.GetFaceVertices(i);
                faces[3 * i] = face[0];
                faces[3 * i + 1] = face[1];
                faces[3 * i + 2] = face[2];
            }

            double[] uv_coords = new double[2 * numVertices];
            int sucess = ARAP_Para(verts, numVertices, faces, numFaces, uv_coords);

            if (sucess == 1)
            {
                for (int i = 0; i < numVertices; i++)
                {
                    newMesh.Vertices.SetVertex(i, uv_coords[2 * i], uv_coords[2 * i + 1], 0.0);
                }
            }
            else
            {
                throw new Exception("Parameterization failed.");
            }

            return newMesh;
        }

        [DllImport("CGAL_Wrapper.dll")]

        public static extern int ARAP_Para(double[] positions, int numVertices, int[] indices, int numFaces, double[] uv_coords);
    }
}
