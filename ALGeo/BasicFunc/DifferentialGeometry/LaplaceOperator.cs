using System.Collections.Generic;
using System.Linq;
using Plankton;

namespace ALGeo
{
    public class LaplaceOperator : DifferentialGeometry
    {
        /// <summary>
        /// Compute the uniform laplace position of each vertex.
        /// </summary>
        /// <param name="pmesh">Input a plankton mesh.</param>
        /// <returns>The uniform laplace position of each vertex.</returns>
        public static Vector[] UniformLaplace(PlanktonMesh pmesh)
        {
            Vector[] laplace = new Vector[pmesh.Vertices.Count];
            for (int i = 0; i < pmesh.Halfedges.Count; i++)
            {
                if (!(pmesh.Halfedges[i].AdjacentFace == -1))
                {
                    var a = pmesh.Halfedges[i].StartVertex;
                    var b = pmesh.Halfedges.EndVertex(i);
                    laplace[a] += pmesh.Vertices[b].ToVector();
                }
            }

            for (int i = 0; i < laplace.Length; i++)
                laplace[i] /= pmesh.Vertices.GetValence(i);

            return laplace;
        }

        /// <summary>
        /// Compute the cotangent laplace position of each vertex.
        /// </summary>
        /// <param name="pmesh">Input a plankton mesh.</param>
        /// <returns>The cotangent laplace positions of each vertex.</returns>
        public static Vector[] CotangentLaplace(PlanktonMesh pmesh)
        {
            var laplace = new Vector[pmesh.Vertices.Count];
            var ew = CotLaplaceEdgeWeight(pmesh);
            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                laplace[i] = Vector.Origin();
                if (!pmesh.Vertices.IsBoundary(i))
                {
                    double w = 0;
                    var hes = pmesh.Vertices.GetHalfedges(i);
                    for (int j = 0; j < hes.Length; j++)
                    {
                        w += ew[hes[j] / 2];
                        laplace[i] +=
                            (pmesh.Vertices[pmesh.Halfedges.EndVertex(hes[j])].ToVector() -
                             pmesh.Vertices[i].ToVector()) * ew[hes[j] / 2];
                    }

                    laplace[i] /= w;
                }
            }

            return laplace;
        }

        /// <summary>
        /// Compute the cotangent laplace weight of each edge.
        /// </summary>
        /// <param name="pmesh">Input a plankton mesh</param>
        /// <returns>Return the cotangent laplace weight of each edge.</returns>
        public static double[] CotLaplaceEdgeWeight(PlanktonMesh pmesh)
        {
            var weights = new double[pmesh.Halfedges.Count / 2];
            for (int h = 0; h < pmesh.Halfedges.Count; h++)
            {
                // get the opposite halfedge
                var ho = pmesh.Halfedges.GetPairHalfedge(h);

                var a = pmesh.Vertices[pmesh.Halfedges[h].StartVertex].ToVector();
                var c = pmesh.Vertices[pmesh.Halfedges[ho].StartVertex].ToVector();

                if (pmesh.Halfedges[h].AdjacentFace != -1)
                {
                    var b = pmesh.Vertices[pmesh.Halfedges[pmesh.Halfedges[h].PrevHalfedge].StartVertex]
                        .ToVector(); // prev vertex

                    var ba = a - b;
                    var bc = c - b;

                    double cot = Cotan(ba, bc);
                    weights[h >> 1] += Clamp_Cot(cot) * 0.5;
                }

                if (pmesh.Halfedges[ho].AdjacentFace != -1)
                {
                    var d = pmesh.Vertices[pmesh.Halfedges.EndVertex(pmesh.Halfedges[ho].NextHalfedge)]
                        .ToVector(); // prev vertex

                    var da = a - d;
                    var dc = c - d;

                    double cot = Cotan(da, dc);
                    weights[h >> 1] += Clamp_Cot(cot) * 0.5;
                }
            }

            return weights;
        }
    }
}
