using System;
using System.Collections.Generic;
using System.Linq;
using Plankton;

namespace ALDGP
{
    public class LaplacianSmoothing
    {
        public static PlanktonMesh ExplicitMethod(PlanktonMesh pmesh, double lambda, int iterations, bool keepBoundary = true)
        {
            var laplace = new Vector3D[pmesh.Vertices.Count];
            var ew = LaplaceOperator.CotLaplaceEdgeWeight(pmesh);

            for (int iter = 0; iter < iterations; iter++)
            {
                for (int i = 0; i < pmesh.Vertices.Count; i++)
                {
                    laplace[i] = Vector3D.Origin;
                    if (!(keepBoundary && pmesh.Vertices.IsBoundary(i)))
                    {
                        double w = 0;
                        var hes = pmesh.Vertices.GetHalfedges(i);
                        for (int j = 0; j < hes.Length; j++)
                        {
                            w += ew[hes[j] / 2];
                            laplace[i] += (pmesh.Vertices[pmesh.Halfedges.EndVertex(hes[j])].ToVector3D() - pmesh.Vertices[i].ToVector3D()) * ew[hes[j] / 2];
                        }
                        laplace[i] /= w;
                    }
                }
                for (int i = 0; i < pmesh.Vertices.Count; i++)
                {
                    if (!(keepBoundary && pmesh.Vertices.IsBoundary(i)))
                        pmesh.Vertices.SetVertex(i, pmesh.Vertices[i].X + lambda * laplace[i].X, pmesh.Vertices[i].Y + lambda * laplace[i].Y, pmesh.Vertices[i].Z + lambda * laplace[i].Z);
                }
            }
            return pmesh;
        }
        public static PlanktonMesh ImplicitMethod(PlanktonMesh pmesh, double timestep)
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
                var vweight = 0.5 / DifferentialGeometry.MixedVoronoiArea(pmesh, v);
                var hs = pmesh.Vertices.GetHalfedges(v);
                var ww = 0.0;
                Vector3D b = pmesh.Vertices[v].ToVector3D() / vweight;
                for (int j = 0; j < hs.Length; j++)
                {
                    var vv = pmesh.Halfedges.EndVertex(hs[j]);
                    ww += eweight[hs[j] >> 1];
                    if (pmesh.Vertices.IsBoundary(vv))
                        b -= -timestep * eweight[hs[j] >> 1] * pmesh.Vertices[vv].ToVector3D();
                    else
                        L.Add(new Triplet(i, idx[vv], -timestep * eweight[hs[j] >> 1]));
                }
                B.Add(new Triplet(i, 0, b.X));
                B.Add(new Triplet(i, 1, b.Y));
                B.Add(new Triplet(i, 2, b.Z));
                L.Add(new Triplet(i, i, 1.0 / vweight + timestep * ww));
            }

            double[] X = new double[3 * n];
            Solver.Solve(1, L.ToArray(), L.Count, n, B.ToArray(), B.Count, 3, X);

            for (int i = 0; i < n; i++)
            {
                pmesh.Vertices.SetVertex(free_vertices[i], X[i], X[n + i], X[2 * n + i]);
            }

            return pmesh;
        }

        public static PlanktonMesh ImplicitMethodLU(PlanktonMesh pmesh)
        {
            var L = new List<Triplet>();
            var B = new List<Triplet>();

            int n = pmesh.Vertices.Count;
            var eweight = LaplaceOperator.CotLaplaceEdgeWeight(pmesh);
            for (int i = 0; i < n; i++)
            {
                if (pmesh.Vertices.IsBoundary(i))
                {
                    L.Add(new Triplet(i, i, 1.0));
                    B.Add(new Triplet(i, 0,pmesh.Vertices[i].X));
                    B.Add(new Triplet(i, 1,pmesh.Vertices[i].Y));
                    B.Add(new Triplet(i, 2,pmesh.Vertices[i].Z));
                }
                else
                {
                    double ww = 0.0;
                    var hfs = pmesh.Vertices.GetHalfedges(i);
                    for (int j = 0; j < hfs.Length; j++)
                    {
                        var vv = pmesh.Halfedges.EndVertex(hfs[j]);
                        var w = -eweight[hfs[j] >> 1];
                        ww += w;

                        L.Add(new Triplet(i, vv, w));
                    }
                    L.Add(new Triplet(i,i,-ww));
                }
            }

            double[] X = new double[3 * n];
            Solver.Solve(0, L.ToArray(), L.Count, n, B.ToArray(), B.Count, 3, X);

            for (int i = 0; i < n; i++)
            {
                pmesh.Vertices.SetVertex(i, X[i], X[n + i], X[2 * n + i]);
            }

            return pmesh;

        }
    }
}
