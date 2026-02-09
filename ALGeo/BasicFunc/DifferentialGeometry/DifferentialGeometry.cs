using Plankton;
using System.Threading.Tasks;

namespace ALGeo
{
    public class DifferentialGeometry : MathUtils
    {
        /// <summary>
        /// Compute the normal of a mesh face.
        /// </summary>
        /// <param name="a">The first vertex on the mesh face.</param>
        /// <param name="b">The second vertex on the mesh face.</param>
        /// <param name="c">The third vertex on the mesh face.</param>
        /// <returns>Return the normal of a mesh face.</returns>
        public static Vector FaceNormal(Vector a, Vector b, Vector c)
        {
            return (b - a).CrossProduct(c - a); 
        }

        /// <summary>
        /// Compute the normal of each vertex.
        /// </summary>
        /// <param name="pmesh"> Input a plankton mesh.</param>
        /// <returns> Return the normal of each vertex.</returns>
        public static Vector[] VertNormals(PlanktonMesh pmesh)
        {
            var normals = new Vector[pmesh.Vertices.Count];
            Parallel.For(0, pmesh.Vertices.Count, V =>
            {
                // get normals
                Vector Vertex = pmesh.Vertices[V].ToVector();
                Vector Norm = new Vector();

                // get all outgoing halfedges
                int[] OutEdges = pmesh.Vertices.GetHalfedges(V);
                // get all neighbour vertices
                int[] Neighbours = pmesh.Vertices.GetVertexNeighbours(V);
                // get vanlance
                int Valence = pmesh.Vertices.GetValence(V);

                Vector[] OutVectors = new Vector[Neighbours.Length];

                for (int j = 0; j < Valence; j++)
                {
                    OutVectors[j] = pmesh.Vertices[Neighbours[j]].ToVector() - Vertex;
                }

                for (int j = 0; j < Valence; j++)
                {
                    if (pmesh.Halfedges[OutEdges[(j + 1) % Valence]].AdjacentFace != -1)
                    {
                        Norm += OutVectors[(j + 1) % Valence].CrossProduct(OutVectors[j]);
                    }
                }
                normals[V] = Norm.Unitize();
            });
            return normals;
        }
        /// <summary>
        /// Compute the mixed voronoi area of a vertex.
        /// </summary>
        /// <param name="pmesh">Input a plankton mesh.</param>
        /// <param name="v">The index of a vertex.</param>
        /// <returns>The mixed voronoi area of a vertex.</returns>
        public static double MixedVoronoiArea(PlanktonMesh pmesh, int v)
        {
            double area = 0.0f;
            var hfs = pmesh.Vertices.GetHalfedges(v);
            int h0, h1, h2;
            Vector p, q, r, pq, qr, pr;
            double dotp, dotq, dotr, triArea;
            double cotq, cotr;
            for (int i = 0; i < hfs.Length; i++)
            {
                h0 = hfs[i];
                h1 = pmesh.Halfedges[h0].NextHalfedge;
                h2 = pmesh.Halfedges[h1].NextHalfedge;

                if (pmesh.Halfedges[h0].AdjacentFace == -1)
                    continue;

                // three vertex positions
                p = pmesh.Vertices[pmesh.Halfedges.EndVertex(h2)].ToVector();
                q = pmesh.Vertices[pmesh.Halfedges.EndVertex(h0)].ToVector();
                r = pmesh.Vertices[pmesh.Halfedges.EndVertex(h1)].ToVector();

                // edge vectors
                pq = q - p;
                qr = r - q;
                pr = r - p;

                // compute and check triangle area
                triArea = pq.CrossProduct(qr).Length;
                if (triArea <= 0.000001f)
                    continue;

                // dot products for each corner (of its two emanating edge vectors)
                dotp = pq*pr;
                dotq = qr.Reverse() * pq;
                dotr = qr*pr;

                // angle at p is obtuse
                if (dotp < 0.0f)
                {
                    area += 0.25f * triArea;
                }

                // angle at q or r obtuse
                else if (dotq < 0.0f || dotr < 0.0f)
                {
                    area += 0.125f * triArea;
                }

                // no obtuse angles
                else
                {
                    // cot(angle) = cos(angle)/sin(angle) = dot(A,B)/norm(cross(A,B))
                    cotq = dotq / triArea;
                    cotr = dotr / triArea;

                    // clamp cot(angle) by clamping angle to [1,179]
                    area += 0.125f * (pr.SqrLength * Clamp_Cot(cotq) +
                      pq.SqrLength * Clamp_Cot(cotr));
                }
            }
            return area;
        }
        public static double TriangularArea(Vector a, Vector b, Vector c)
        {
            return (b - a).CrossProduct(c - a).Length / 2.0f;
        }
        /// <summary>
        /// Get the circumcenter of a face.
        /// </summary>
        /// <param name="a">The first vertex on the mesh face.</param>
        /// <param name="b">The second vertex on the mesh face.</param>
        /// <param name="c">The third vertex on the mesh face.</param>
        /// <returns> Return the circumcenter of the face. </returns>
        public static Vector GetCircumcenter(Vector a, Vector b, Vector c)
        {
            var ac = c - a;
            var ab = b - a;
            var abXac = ab.CrossProduct(ac);

            var circumcenter = ((abXac.CrossProduct(ab) * ac.SqrLength) +
                ac.CrossProduct(abXac) * ab.SqrLength) * (1.0f / (2.0f * abXac.SqrLength));
            circumcenter += a;

            return circumcenter;
        }
    }
}
