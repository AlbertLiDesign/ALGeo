using Plankton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALDGP
{
    class MeshMartix : DifferentialGeometry
    {

        /// <summary>
        /// Build an adjacent matrix from a vertex to vertex
        /// </summary>
        /// <param name="pmesh"></param>
        /// <returns></returns>
        public static List<Triplet> AdjacentMatrixVV(PlanktonMesh pmesh)
        {
            List<Triplet> sm = new List<Triplet>();

            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                int[] adjacentVV = pmesh.Vertices.GetVertexNeighbours(i);
                for (int j = 0; j < adjacentVV.Length; j++)
                {
                    sm.Add(new Triplet(i, adjacentVV[j], 1));
                }
            }

            return sm;
        }

        /// <summary>
        /// Build an adjacent matrix from a face to vertex
        /// </summary>
        /// <param name="pmesh">Input a PlanktonMesh</param>
        /// <returns></returns>
        public static List<Triplet> AdjacentMatrixFV(PlanktonMesh pmesh)
        {
            List<Triplet> sm = new List<Triplet>();

            for (int i = 0; i < pmesh.Faces.Count; i++)
            {
                int[] adjacentFV = pmesh.Faces.GetFaceVertices(i);
                for (int j = 0; j < adjacentFV.Length; j++)
                {
                    sm.Add(new Triplet(i, adjacentFV[j], 1));
                }
            }

            return sm;
        }

        /// <summary>
        /// Build a degree matrix (or called valency matrix)
        /// </summary>
        /// <param name="pmesh">Input a PlanktonMesh</param>
        /// <returns></returns>
        public static List<Triplet> ValencyMatrix(PlanktonMesh pmesh)
        {
            List<Triplet> sm = new List<Triplet>();

            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                int[] adjacentVV = pmesh.Vertices.GetVertexNeighbours(i);
                sm.Add(new Triplet(i, i, adjacentVV.Length));
            }

            return sm;
        }

        /// <summary>
        /// Build a Laplace Matrix. It is defined as K = D - W.
        /// </summary>
        /// <param name="pmesh">Input a PlanktonMesh</param>
        /// <returns>Return the Laplace Matrix.</returns>
        public static List<Triplet> UniformLaplaceMatrix(PlanktonMesh pmesh)
        {
            List<Triplet> laplacian = new List<Triplet>();

            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                int[] adjacentVV = pmesh.Vertices.GetVertexNeighbours(i);
                for (int j = 0; j < adjacentVV.Length; j++)
                {
                    laplacian.Add(new Triplet(i, adjacentVV[j], -1.0f));
                }

                laplacian.Add(new Triplet(i, i, adjacentVV.Length));
            }

            return laplacian;
        }

        /// <summary>
        /// Build a Tutte Laplace Matrix. It is a non-symmetric matrix and can be defined as T = D^-1 * K.
        /// </summary>
        /// <param name="pmesh">Input a PlanktonMesh</param>
        /// <returns>Return the Tutte Laplace Matrix.</returns>
        public static List<Triplet> TutteLaplaceMatrix(PlanktonMesh pmesh)
        {
            var tutteLP = new List<Triplet>();
            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                var neighbours = pmesh.Vertices.GetVertexNeighbours(i);
                for (int j = 0; j < neighbours.Length; j++)
                {
                    double result = -1.0 / pmesh.Vertices.GetValence(i);
                    tutteLP.Add(new Triplet(i, neighbours[j], result));
                }

                tutteLP.Add(new Triplet(i, i, 1));
            }

            return tutteLP;
        }

        /// <summary>
        /// Build a cotangent laplace matrix from triangular meshes.
        /// </summary>
        /// <param name="pmesh">Input a plankton mesh.</param>
        /// <returns>Return a cotangent laplace matrix.</returns>
        public static List<Triplet> CotLaplaceMatrix(PlanktonMesh pmesh, double timestep)
        {
            int n = pmesh.Vertices.Count;
            List<Triplet> L = new List<Triplet>();

            var eweight = LaplaceOperator.CotLaplaceEdgeWeight(pmesh);
            var vweight = new double[pmesh.Vertices.Count];
            for (int h = 0; h < pmesh.Halfedges.Count; h++)
            {
                int i = pmesh.Halfedges[h].StartVertex;
                int j = pmesh.Halfedges.EndVertex(h);

                vweight[i] += eweight[h >> 1];
                L.Add(new Triplet(i, j, eweight[h >> 1] * timestep));
                L.Add(new Triplet(j, i, eweight[h >> 1] * timestep));
            }

            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                L.Add(new Triplet(i, i, -timestep * vweight[i]));
            }

            return L;
        }

        public static List<Triplet> MassMatrix(PlanktonMesh pmesh)
        {
            int n = pmesh.Vertices.Count;
            List<Triplet> M = new List<Triplet>();

            for (int i = 0; i < n; i++)
                M.Add(new Triplet(i, i, MixedVoronoiArea(pmesh, i)));

                return M;
        }
    }
}
