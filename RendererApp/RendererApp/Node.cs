using System;
using System.Collections.Generic;
using System.Linq;

namespace RendererApp
{
    public interface INode
    {
        Vector3 A { get; }

        Vector3 B { get; }

        Vector3 C { get; }

        Vector3 D { get; }

        Vector3 A1 { get; }

        Vector3 B1 { get; }

        Vector3 C1 { get; }

        Vector3 D1 { get; }

        List<Triangle> BasicPolygons { get; }

        List<Triangle> InnerTriangles { get; }

        List<Node> ChildNodes { get; }

        void SplitNode();

        bool IsLeaf();

        void SpreadTriangles();

        bool IsTriangleInside(Triangle Triangle);

        bool CheckNode(Ray ray, out Triangle closestTriangle);
    }

    public class Node : INode
    {
        public Vector3 A { get; set; }

        public Vector3 B { get; set; }

        public Vector3 C { get; set; }

        public Vector3 D { get; set; }

        public Vector3 A1 { get; set; }

        public Vector3 B1 { get; set; }

        public Vector3 C1 { get; set; }

        public Vector3 D1 { get; set; }

        public List<Triangle> BasicPolygons { get; set; }

        public List<Triangle> InnerTriangles { get; set; }

        public List<Node> ChildNodes { get; set; }

        public Node(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 A1, Vector3 B1, Vector3 C1, Vector3 D1)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
            this.A1 = A1;
            this.B1 = B1;
            this.C1 = C1;
            this.D1 = D1;
            InnerTriangles = new List<Triangle>();
            BasicPolygons = new List<Triangle>()
            {
                new Triangle(A, B, C),
                new Triangle(A, D, C),//ABCD

                new Triangle(A1, B1, C1),
                new Triangle(A1, D1, C1),//A1B1C1D1

                new Triangle(A, A1, D1),
                new Triangle(A, D, D1),//AA1D1D

                new Triangle(B, B1, C1),
                new Triangle(B, C, C1),//BB1C1C

                new Triangle(A, A1, B1),
                new Triangle(A, B, B1),//AA1B1B

                new Triangle(D, D1, C1),
                new Triangle(D, C, C1)//DD1C1C
            };
        }

        public void SplitNode()
        {
            ChildNodes = new List<Node>()
            {
                new Node(A,//A
                        Vector3.Middle(A, B),//B
                        Vector3.Middle(A, C),//C
                        Vector3.Middle(A, D),//D
                        Vector3.Middle(A, A1),//A1
                        Vector3.Middle(A, B1),//B1
                        Vector3.Middle(A, C1),//C1
                        Vector3.Middle(A, D1)),//D1
                new Node(Vector3.Middle(B, A),//A
                        B,//B
                        Vector3.Middle(B, C),//C
                        Vector3.Middle(B, D),//D
                        Vector3.Middle(B, A1),//A1
                        Vector3.Middle(B, B1),//B1
                        Vector3.Middle(B, C1),//C1
                        Vector3.Middle(B, D1)),//D1
                new Node(Vector3.Middle(C, A),//A
                        Vector3.Middle(C, B),//B
                        C,//C
                        Vector3.Middle(C, D),//D
                        Vector3.Middle(C, A1),//A1
                        Vector3.Middle(C, B1),//B1
                        Vector3.Middle(C, C1),//C1
                        Vector3.Middle(C, D1)),//D1
                new Node(Vector3.Middle(D, A),//A
                        Vector3.Middle(D, B),//B
                        Vector3.Middle(D, C),//C
                        D,//D
                        Vector3.Middle(D, A1),//A1
                        Vector3.Middle(D, B1),//B1
                        Vector3.Middle(D, C1),//C1
                        Vector3.Middle(D, D1)),//D1
                new Node(Vector3.Middle(A1, A),//A
                        Vector3.Middle(A1, B),//B
                        Vector3.Middle(A1, C),//C
                        Vector3.Middle(A1, D),//D
                        A1,//A1
                        Vector3.Middle(A1, B1),//B1
                        Vector3.Middle(A1, C1),//C1
                        Vector3.Middle(A1, D1)),//D1
                new Node(Vector3.Middle(B1, A),//A
                        Vector3.Middle(B1, B),//B
                        Vector3.Middle(B1, C),//C
                        Vector3.Middle(B1, D),//D
                        Vector3.Middle(B1, A1),//A1
                        B1,//B1
                        Vector3.Middle(B1, C1),//C1
                        Vector3.Middle(B1, D1)),//D1
                new Node(Vector3.Middle(C1, A),//A
                        Vector3.Middle(C1, B),//B
                        Vector3.Middle(C1, C),//C
                        Vector3.Middle(C1, D),//D
                        Vector3.Middle(C1, A1),//A1
                        Vector3.Middle(C1, B1),//B1
                        C1,//C1
                        Vector3.Middle(C1, D1)),//D1
                new Node(Vector3.Middle(D1, A),//A
                        Vector3.Middle(D1, B),//B
                        Vector3.Middle(D1, C),//C
                        Vector3.Middle(D1, D),//D
                        Vector3.Middle(D1, A1),//A1
                        Vector3.Middle(D1, B1),//B1
                        Vector3.Middle(D1, C1),//C1
                        D1),//D1
            };

            SpreadTriangles();

            for (int i = 0; i < 8; i++)
            {
                var child = ChildNodes[i];
                if (child.IsLeaf())
                {
                    continue;
                }
                child.SplitNode();
            }
        }

        public bool IsLeaf()
        {
            return InnerTriangles.Count < 2;
        }

        public void SpreadTriangles()
        {
            for (int i = 0; i < 8; i++)
            {
                var child = ChildNodes[i];
                var resTriangleangles = InnerTriangles.GetRange(0, InnerTriangles.Count);
                foreach (var Triangle in InnerTriangles)
                {
                    if (child.IsTriangleInside(Triangle))
                    {
                        child.InnerTriangles.Add(Triangle);
                        resTriangleangles.Remove(Triangle);
                    }
                }
                InnerTriangles = resTriangleangles;
                ChildNodes[i] = child;
            }
        }

        public bool IsTriangleInside(Triangle Triangle)
        {
            return IsPointInside(Triangle.a) && IsPointInside(Triangle.b) && IsPointInside(Triangle.c);
        }

        protected bool IsPointInside(Vector3 point)
        {
            return point.IsInside(A, C1);
        }

        protected Triangle DoesIntersectAnyInnerTriangles(Ray ray)
        {
            if (InnerTriangles == null || !InnerTriangles.Any())
            {
                return null;
            }

            float currZ = 0;
            Triangle res = null;

            foreach (var triangle in InnerTriangles)
            {
                if (Renderer.IntersectionRayTriangle(ray, triangle, out float u, out float v))
                {
                    if (res == null)
                    {
                        res = triangle;
                        currZ = (triangle.a.Z + triangle.b.Z + triangle.c.Z) / 3;
                    }
                    else
                    {
                        float z = (triangle.a.Z + triangle.b.Z + triangle.c.Z) / 3;
                        if (z < currZ)
                        {
                            res = triangle;
                            currZ = z;
                        }
                    }
                }
            }

            return res;
        }

        protected bool DoesIntersectNode(Ray ray)
        {
            for (int i = 0; i < 12; i++)
            {
                if (Renderer.IntersectionRayTriangle(ray, BasicPolygons[i], out float u, out float v))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckNode(Ray ray, out Triangle closestTriangle)
        {
            closestTriangle = null;
            if (!DoesIntersectNode(ray))
            {
                return false;
            }

            closestTriangle = DoesIntersectAnyInnerTriangles(ray);

            if (ChildNodes == null || !ChildNodes.Any())
            {
                return closestTriangle != null;
            }

            float currZ = closestTriangle == null ? 0 : (closestTriangle.a.Z + closestTriangle.b.Z + closestTriangle.c.Z) / 3;

            foreach (var child in ChildNodes)
            {
                if (child.CheckNode(ray, out var childClosestTriangle))
                {
                    if (closestTriangle == null)
                    {
                        closestTriangle = childClosestTriangle;
                        currZ = (childClosestTriangle.a.Z + childClosestTriangle.b.Z + childClosestTriangle.c.Z) / 3;
                    }
                    else
                    {
                        float z = (childClosestTriangle.a.Z + childClosestTriangle.b.Z + childClosestTriangle.c.Z) / 3;
                        if (z < currZ)
                        {
                            closestTriangle = childClosestTriangle;
                            currZ = z;
                        }
                    }

                }
            }

            return closestTriangle != null;
        }
    }
}
