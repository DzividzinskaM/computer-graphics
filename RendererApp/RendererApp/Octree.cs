using System.Collections.Generic;

namespace RendererApp
{
    public interface ITree
    {
        Node Top { get; }

        List<Triangle> Triangles { get; }
    }

    public class Octree : ITree
    {
        public Node Top { get; set; }

        public List<Triangle> Triangles { get; }

        public Octree(List<Triangle> Triangles)
        {

            this.Triangles = Triangles;
            Top = new Node(new Vector3(-1, -1, -1),
                                new Vector3(-1, 1, -1),
                                new Vector3(1, 1, -1),
                                new Vector3(1, -1, -1),
                                new Vector3(-1, -1, 1),
                                new Vector3(-1, 1, 1),
                                new Vector3(1, 1, 1),
                                new Vector3(1, -1, 1));
            SetTreeTriangles();
            Top.SplitNode();
        }

        public bool CheckChildNodes(Ray ray, out Triangle closestTriangle)
        {
            return Top.CheckNode(ray, out closestTriangle);
        }

        protected void SetTreeTriangles()
        {
            foreach (var Triangle in Triangles)
            {
                if (Top.IsTriangleInside(Triangle))
                {
                    Top.InnerTriangles.Add(Triangle);
                }
            }
        }
    }
}
