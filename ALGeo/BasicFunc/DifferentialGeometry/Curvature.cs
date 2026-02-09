using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plankton;

namespace ALDGP
{
    public class Curvature : DifferentialGeometry
    {
        /// <summary>
        /// Compute the Mean Curvature of a mesh using laplacian operators.
        /// </summary>
        /// <param name="pmesh"> Input the mesh. </param>
        /// <returns> return the mean curvatures of all vertices. </returns>
        public static double[] LaplacianMeanCurvature(PlanktonMesh pmesh)
        {
            double[] curvature = new double[pmesh.Vertices.Count];
            Vector3D[] cotL = LaplaceOperator.CotangentLaplace(pmesh);
            for (int i = 0; i < cotL.Length; i++)
            {
                curvature[i] = pmesh.Vertices.IsBoundary(i) ? 0.0f : cotL[i].Length / 2.0f;
            }
            return curvature;
        }


        /// <summary>
        /// Compute the Mean Curvature of a mesh using vertex normals.
        /// </summary>
        /// <param name="pmesh"> Input the mesh. </param>
        /// <returns> return the mean curvatures of all vertices. </returns>
        public static double[] MeanCurvature(PlanktonMesh pmesh)
        {
            var edgeCurvature = new double[pmesh.Halfedges.Count / 2];
            var vertCurvature = new double[pmesh.Vertices.Count];

            var ns = VertNormals(pmesh);
            Parallel.For(0, pmesh.Halfedges.Count / 2, i =>
            {
                var v0 = pmesh.Halfedges[2 * i].StartVertex;
                var v1 = pmesh.Halfedges.EndVertex(2 * i);

                var p0 = pmesh.Vertices[v0].ToVector3D();
                var p1 = pmesh.Vertices[v1].ToVector3D();

                var n0 = ns[v0];
                var n1 = ns[v1];

                edgeCurvature[i] = (n1 - n0)*(p1 - p0) / (p1 - p0).Length / (p1 - p0).Length;
            });
            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                var hfs = pmesh.Vertices.GetHalfedges(i);
                double sum = 0.0f;
                foreach (var item in hfs)
                {
                    sum += edgeCurvature[item >> 1];
                }
                vertCurvature[i] = sum / hfs.Length;
            }
            return vertCurvature;
        }

        /// <summary>
        /// Compute the Gaussian Curvature of a mesh.
        /// </summary>
        /// <param name="pmesh"> Input the mesh. </param>
        /// <returns> return the gaussian curvatures of all vertices. </returns>
        public static double[] GaussianCurvature(PlanktonMesh pmesh)
        {
            double[] curvature = new double[pmesh.Vertices.Count];

            Parallel.For(0, pmesh.Vertices.Count, i =>
            {
                int[] neighbours = pmesh.Vertices.GetVertexNeighbours(i);
                Vector3D pi = pmesh.Vertices[i].ToVector3D();
                int valance = pmesh.Vertices.GetValence(i);
                double sum = 0.0f;
                double area = MixedVoronoiArea(pmesh, i);
                for (int j = 0; j < valance; j++)
                {
                    Vector3D pj = pmesh.Vertices[neighbours[j]].ToVector3D();
                    Vector3D pjNext = pmesh.Vertices[neighbours[(j + 1) % valance]].ToVector3D();

                    Vector3D v1 = pi - pj;
                    Vector3D v2 = pi - pjNext;

                    double cos = v1*v2 / v1.Length / v2.Length;

                    double theta = Math.Acos(Clamp_Cos(cos));

                    sum += theta;
                }
                curvature[i] = (Math.PI * 2.0f - sum) / area;
                // pmesh.Vertices.IsBoundary(i) ? (Math.PI - sum) / area : (Math.PI * 2.0f - sum) / area;
            });

            return curvature;
        }

        /// <summary>
        /// Compute the two Principal Curvature of a mesh.
        /// </summary>
        /// <param name="pmesh"> Input the mesh. </param>
        /// <param name="kmax"> return the first principal curvature of all vertices. </param>
        /// <param name="k2"> return the second  principal curvature of all vertices. </param>
        public static void PrincipalCurvature(PlanktonMesh pmesh, ref double[] kmax, ref double[] kmin)
        {
            double[] mean = LaplacianMeanCurvature(pmesh);
            double[] gaussian = GaussianCurvature(pmesh);

            double[] ka = new double[kmax.Length];
            double[] kb = new double[kmin.Length];
            Parallel.For(0, pmesh.Vertices.Count, i =>
            {
                double delta = Math.Sqrt(Math.Max(0.0f, mean[i] * mean[i] - gaussian[i]));
                ka[i] = mean[i] + delta;
                kb[i] = mean[i] - delta;
            });
            kmax = ka;
            kmin = kb;
        }


        /// <summary>
        /// Get the direction of the maximum principal curvature on each vertex
        /// </summary>
        /// <param name="pmesh"> Input a plankton mesh.</param>
        /// <param name="maxCurvatures"> The maximum curvature of each vertex.</param>
        /// <returns></returns>
        public static Vector3D[] FindMaxDirections(PlanktonMesh pmesh, double[] maxCurvatures)
        {
            Vector3D[] maxDirections = new Vector3D[maxCurvatures.Length];
            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                var hes = pmesh.Vertices.GetIncomingHalfedges(i);
                double[] vv_values = new double[hes.Length];

                // To find the id of the largest number
                int maxID = 0;
                for (int j = 0; j < vv_values.Length; j++)
                {
                    vv_values[j] = maxCurvatures[pmesh.Halfedges[hes[j]].StartVertex];
                    if (vv_values[j] == vv_values.Max()) maxID = hes[j];
                }

                var pre_vert_id = pmesh.Halfedges.EndVertex(pmesh.Halfedges[pmesh.Halfedges.GetPairHalfedge(maxID)].NextHalfedge);
                var next_vert_id = pmesh.Halfedges[pmesh.Halfedges[maxID].PrevHalfedge].StartVertex;

                var first_vert = pmesh.Vertices[i];
                var second_vert = pmesh.Halfedges[maxID].StartVertex;
                var third_vert = maxCurvatures[pre_vert_id] > maxCurvatures[next_vert_id] ? pre_vert_id : next_vert_id;

                var third_weight = maxCurvatures[third_vert] / (maxCurvatures[second_vert] + maxCurvatures[third_vert]);
                var endV = third_weight * pmesh.Vertices[third_vert].ToVector3D() +
                           (1 - third_weight) * pmesh.Vertices[second_vert].ToVector3D();
                maxDirections[i] = (endV - pmesh.Vertices[i].ToVector3D()).Unitize();
            }

            return maxDirections;
        }
    }
}
