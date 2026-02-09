using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace ALGeo
{
    public class Octree
    {
        public Mesh mesh;
        // The size of the largest voxel
        public double worldSize { get; set; }
        // Maximum layer
        public int maxLevels { get; set; }
        // The initial point
        public Point3d pt { get; set; }
        // root point
        public OctNode RootOctNode { get; set; }

        public List<OctNode> interiorLeafs { get; set; }
        public List<OctNode> surfaceLeafs { get; set; }


        // 初始化方法，创建八叉树
        public Octree(Mesh mesh, Point3d pt, double ws, int maxlevel)
        {
            if (!mesh.IsClosed)
            {
                throw new Exception("The mesh must be closed.");
            }

            this.pt = pt;
            this.worldSize = ws;
            this.maxLevels = maxlevel;
            this.RootOctNode = new OctNode(pt.X, pt.Y, pt.Z, ws, 0);
            this.interiorLeafs = new List<OctNode>();
            this.surfaceLeafs = new List<OctNode>();
            this.mesh = mesh;
        }

        // 一个获取距离的函数
        public static double getSignedDistance(Mesh mesh, Point3d pt)
        {
            var pt2 = mesh.ClosestMeshPoint(pt, 0.0).Point;
            var dis = pt.DistanceTo(pt2);
            bool bln = mesh.IsPointInside(pt, 0.1, false);
            if (bln)
            {
                dis *= -1;
            }
            return dis;
        }

        // 细分节点
        public void divide(OctNode quadNode, int le)
        {
            // 求节点到网格的距离
            var dis = getSignedDistance(this.mesh, quadNode.pt);
            quadNode.distance = dis;

            // 如果节点的层级小于最大层级
            if (quadNode.level < this.maxLevels)
            {
                // 如果距离的绝对值小于节点的对角线的一半
                if (Math.Abs(dis) < quadNode.diag)
                {
                    // 节点被细分
                    quadNode.divideNode();

                    // 如果节点下仍有子节点，则一直细分下去
                    foreach (var item in quadNode.branches)
                    {
                        this.divide(item, le);
                    }
                }
                else
                {
                    // 输出内部各层节点
                    var inside = this.mesh.IsPointInside(quadNode.pt, 0.0, true);
                    if (inside)
                    {
                        this.interiorLeafs.Add(quadNode);
                    }
                }
            }
            else
            {
                // 输出用于MarchingCubes的节点
                if (Math.Abs(dis) < quadNode.diag)
                {
                    this.surfaceLeafs.Add(quadNode);
                }
                // 输出内部非MarchingCubes节点
                else
                {
                    if (quadNode.level == this.maxLevels)
                    {
                        var inside = this.mesh.IsPointInside(quadNode.pt, 0.0, true);
                        if (inside)
                        {
                            this.interiorLeafs.Add(quadNode);
                        }
                    }
                }
            }
        }

    }

    // 定义节点类
    public class OctNode
    {
        // 节点坐标
        public Point3d pt { get; set; }
        // 体素边长
        public double edge { get; set; }
        // 体素层级
        public int level { get; set; }
        // 它的分支
        public List<OctNode> branches { get; set; }
        // 距离
        public double distance { get; set; }
        // 体素斜对角的一半
        public double diag { get; set; }

        // 初始化方法，创建节点
        public OctNode(double x, double y, double z, double s, int l)
        {
            this.pt = new Point3d(x, y, z);
            this.edge = s;
            this.level = l;
            this.branches = new List<OctNode>();
            this.distance = 0.0;
            this.diag = pt.DistanceTo(new Point3d(pt.X - s * 0.5, pt.Y - s * 0.5, pt.Z - s * 0.5)) * 0.9;
        }

        // 细分节点
        public void divideNode()
        {
            // 新的子节点的边长
            var qs = this.edge / 4.0;

            // 新的子节点的层级
            var nl = this.level + 1;

            // 添加8个新的子节点，新的子节点的体素边长记位edge / 2，层级要+1
            this.branches.Add(new OctNode(this.pt.X - qs, this.pt.Y - qs, this.pt.Z - qs, qs * 2, nl));
            this.branches.Add(new OctNode(this.pt.X + qs, this.pt.Y - qs, this.pt.Z - qs, qs * 2, nl));
            this.branches.Add(new OctNode(this.pt.X - qs, this.pt.Y + qs, this.pt.Z - qs, qs * 2, nl));
            this.branches.Add(new OctNode(this.pt.X + qs, this.pt.Y + qs, this.pt.Z - qs, qs * 2, nl));
            this.branches.Add(new OctNode(this.pt.X - qs, this.pt.Y - qs, this.pt.Z + qs, qs * 2, nl));
            this.branches.Add(new OctNode(this.pt.X + qs, this.pt.Y - qs, this.pt.Z + qs, qs * 2, nl));
            this.branches.Add(new OctNode(this.pt.X - qs, this.pt.Y + qs, this.pt.Z + qs, qs * 2, nl));
            this.branches.Add(new OctNode(this.pt.X + qs, this.pt.Y + qs, this.pt.Z + qs, qs * 2, nl));
        }
    }
}