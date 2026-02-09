using ALGeo;
using Plankton;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ALGeo
{
    public class SurfaceProcessing
    {
        public static PlanktonMesh LeastSquareMeshes(PlanktonMesh pmesh,List<int> constraint)
        {
            int n = pmesh.Vertices.Count;

            List<Triplet> L = MeshMartix.UniformLaplaceMatrix(pmesh);

            if (!pmesh.IsClosed())
            {
                for (int i = 0; i < pmesh.Vertices.Count; i++)
                    if (pmesh.Vertices.IsBoundary(i))
                        constraint.Add(i);
                constraint = constraint.Distinct().ToList();
            }
            
            var B = new List<Triplet>();

            // Apply the boundary constraint
            int m = constraint.Count;

            for (int i = 0; i <m; i++)
            {
                B.Add(new Triplet(n + i, 0, pmesh.Vertices[constraint[i]].X));
                B.Add(new Triplet(n + i, 1, pmesh.Vertices[constraint[i]].Y));
                B.Add(new Triplet(n + i, 2, pmesh.Vertices[constraint[i]].Z));
                                                   
                L.Add(new Triplet(n + i, constraint[i], 1.0));
            }

            var X = new double[(n + m) * 3];

            Solver.LeastSquareMethod(L.ToArray(), L.Count, n + m, n, B.ToArray(), B.Count, 3, X);

            for (int i = 0; i < n; i++)
                pmesh.Vertices.SetVertex(i, X[i], X[n + i], X[2 * n + i]);

            return pmesh;
        }

        public static bool GetFeatureEages(PlanktonMesh pmesh, int he, double angle, bool includeBoundary)
        {
            double feature_cosine = Math.Cos(angle / 180.0 * 3.1415926);
            if (pmesh.Halfedges.IsBoundary(he))
            {
                if (includeBoundary) { return true; }
                else { return false; }
            }
            else
            {
                var f0 = pmesh.Halfedges[he].AdjacentFace;
                var f1 = pmesh.Halfedges[pmesh.Halfedges.GetPairHalfedge(he)].AdjacentFace;

                var ps0 = pmesh.Faces.GetFaceVertices(f0);
                var ps1 = pmesh.Faces.GetFaceVertices(f1);

                var v1 = DifferentialGeometry.FaceNormal(pmesh.Vertices[ps0[0]].ToVector(), pmesh.Vertices[ps0[1]].ToVector(), pmesh.Vertices[ps0[2]].ToVector());
                var v2 = DifferentialGeometry.FaceNormal(pmesh.Vertices[ps1[0]].ToVector(), pmesh.Vertices[ps1[1]].ToVector(), pmesh.Vertices[ps1[2]].ToVector());

                if (v1*v2 / v1.Length / v2.Length < feature_cosine) { return true; }
                else { return false; }
            }
        }

        /// <summary>
        /// Triangulate a plankton mesh.
        /// </summary>
        /// <param name="pmesh">Input a plankton mesh.</param>
        /// <param name="StellateAll"></param>
        /// <returns>Return a triangulated mesh.</returns>
        public static PlanktonMesh Triangulation(PlanktonMesh pmesh, bool StellateAll)
        {
            int FaceCount = pmesh.Faces.Count;
            for (int i = 0; i < FaceCount; i++)
            {
                int[] FaceHEs = pmesh.Faces.GetHalfedges(i);
                if (FaceHEs.Length == 4 && !StellateAll)
                {
                    double D0 = ((pmesh.Vertices[pmesh.Halfedges[FaceHEs[0]].StartVertex].ToXYZ())
                        - (pmesh.Vertices[pmesh.Halfedges[FaceHEs[2]].StartVertex].ToXYZ())).Length;
                    double D1 = ((pmesh.Vertices[pmesh.Halfedges[FaceHEs[1]].StartVertex].ToXYZ())
                        - (pmesh.Vertices[pmesh.Halfedges[FaceHEs[3]].StartVertex].ToXYZ())).Length;

                    if (D0 < D1 || i % 2 == 0) // split face either along the shorter cross corners or at every even one in the case of a regular grid
                    {
                        pmesh.Faces.SplitFace(FaceHEs[2], FaceHEs[0]);
                    }
                    else
                    {
                        pmesh.Faces.SplitFace(FaceHEs[3], FaceHEs[1]);
                    }
                }
                else if (FaceHEs.Length > 4 || (StellateAll && FaceHEs.Length == 4))
                {
                    pmesh.Faces.Stellate(i);
                }
            }
            return pmesh;
        }

        /// <summary>
        /// Get the dual mesh of a mesh.
        /// </summary>
        /// <param name="pmesh">Input a plankton mesh.</param>
        /// <param name="withBoundary">Whether to consider the mesh boundary.</param>
        /// <returns>Return the dual mesh of a mesh.</returns>
        public static PlanktonMesh Dual(PlanktonMesh pmesh, bool withBoundary)
        {
            var dual = new PlanktonMesh();

            // create vertices from face centers
            for (int i = 0; i < pmesh.Faces.Count; i++)
            {
                var pt = pmesh.Faces.GetFaceCenter(i);
                dual.Vertices.Add(pt.X, pt.Y, pt.Z);
            }

            // create faces from the adjacent face indices of non-boundary vertices
            for (int i = 0; i < pmesh.Vertices.Count; i++)
            {
                if (pmesh.Vertices.IsBoundary(i))
                {
                    if ((withBoundary == true))
                    {
                        var vertex = pmesh.Vertices[i].ToXYZ();
                        // After getting adjacent faces, there is a halfedge which hasn't any adjacent faces.
                        // in this case will get -1, so we need to remove it.
                        var adjF = pmesh.Vertices.GetVertexFaces(i).Where(p => p >= 0).ToArray();

                        int[] ids = new int[adjF.Length + 3];
                        for (int k = 2; k < adjF.Length + 2; k++)
                        {
                            ids[k] = adjF[k - 2];
                        }
                        var hfe = pmesh.Vertices.GetHalfedges(i);

                        var key = pmesh.Halfedges.EndVertex(hfe[0]);
                        var pp = pmesh.Vertices[key].ToVector();
                        var Mid_Point = new Vector((pp + pmesh.Vertices[i].ToVector()).X / 2.0f,
                          (pp + pmesh.Vertices[i].ToVector()).Y / 2.0f, (pp + pmesh.Vertices[i].ToVector()).Z / 2.0f);
                        int id = dual.Vertices.Add(Mid_Point.X, Mid_Point.Y, Mid_Point.Z);
                        ids[1] = id;

                        var key2 = pmesh.Halfedges.EndVertex(hfe[hfe.Length - 1]);
                        var pp2 = pmesh.Vertices[key2].ToVector();
                        var Mid_Point2 = new Vector((pp2 + pmesh.Vertices[i].ToVector()).X / 2.0f,
                          (pp2 + pmesh.Vertices[i].ToVector()).Y / 2.0f, (pp2 + pmesh.Vertices[i].ToVector()).Z / 2.0f);
                        int id2 = dual.Vertices.Add(Mid_Point2.X, Mid_Point2.Y, Mid_Point2.Z);
                        ids[adjF.Length + 2] = id2;

                        int newid = dual.Vertices.Add(vertex.X, vertex.Y, vertex.Z);
                        ids[0] = newid;
                        dual.Faces.AddFace(ids);
                    }
                    else { continue; }
                }
                else
                {
                    dual.Faces.AddFace(pmesh.Vertices.GetVertexFaces(i));
                }
            }
            return dual;
        }
    }
}
