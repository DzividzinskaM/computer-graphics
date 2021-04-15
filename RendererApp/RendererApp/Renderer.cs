using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ImageConverter;
using Microsoft.Extensions.DependencyInjection;
using ObjLoader.Loader.Loaders;

namespace RendererApp
{
    public class Renderer
    {
        private readonly IServiceProvider Container = new ContainerBuilder().Build();

        private ICameraPositionProvider _camera { get; set; }
        private IObjLoader _objLoader { get; set; }
        private IRaysProvider _raysProvider { get; set; }
        private int _width { get; set; }
        private int _height { get; set; }
        private const float _aspectRatio = 16.0f / 9.0f;
        private const float focalLength = 1.0f;
        private float _viewportHeight = 2f;
        private float _viewportWidth;
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float OffsetZ { get; set; }
        public double AngelX { get; set; }
        public double AngelY { get; set; }
        public double AngelZ { get; set; }
        public float ScaleXValue { get; set; }
        public float ScaleYValue { get; set; }
        public float ScaleZValue { get; set; }

        public Renderer(int width = 100, float offsetX = 0, float offsetY = 0, float offsetZ = 0,
            double angelX = 0, double angelY = 0, double angelZ = 0, float scaleX = 1, float scaleY = 1,
            float scaleZ = 1)
        {
            _camera = Container.GetService<ICameraPositionProvider>();
            _objLoader = Container.GetService<IObjLoader>();
            _raysProvider = Container.GetService<IRaysProvider>();
            _width = width;
            _height = (int)(_width / _aspectRatio);
            _viewportWidth = _aspectRatio * _viewportHeight;
            OffsetX = offsetX;
            OffsetY = offsetY;
            OffsetZ = offsetZ;
            AngelX = angelX;
            AngelY = angelY;
            AngelZ = angelZ;
            ScaleXValue = scaleX;
            ScaleYValue = scaleY;
            ScaleZValue = scaleZ;
        }

        public void Render(string sourcePath, string outputPath)
        {

            PointLight lightSource = new PointLight { Intencity = 0.7f, Position = new Vector3(5,2,-2) };
            var obj = _objLoader.LoadObj(sourcePath);
            var cameraPos = _camera.GetCameraPosition();
            Screen screen = new Screen(_viewportHeight, _viewportWidth, focalLength, cameraPos);
            var rays = _raysProvider.GetRays(_width, _height, cameraPos, screen);
            var triangles = getTrianglesList(obj);
            var tree = new Octree(triangles);
            byte[] rgb = new byte[_width * _height * 3];

            for(int i=0; i<rays.Count; i++)
            {
                var ray = rays[i];
                Vector3 pixelColor = new Vector3(1, 1, 1);
                if (tree.CheckChildNodes(ray))
                {
                    pixelColor = RayColor(ray, lightSource);
                }
                //Vector3 pixelColor = RayColor(rays[i], triangles, lightSource);
                WriteColor(ref rgb, pixelColor, i);
            }

            Image image = new Image(_width, _height, rgb);
            PpmWriter writer = new PpmWriter();
            writer.Write(outputPath, image);

        }

        private float ComputeLightening(Ray ray, PointLight light)
        {
            float i = 0;
            Vector3 L = light.Position - ray.Orig;
            float n_dot_l = ray.Dir.DotProduct(L);
            if (n_dot_l > 0)
                i += (light.Intencity * n_dot_l) / (ray.Dir.Length * L.Length);
            return i;
        }

        private Vector3 RayColor(Ray r, List<Triangle> TrigsLst, PointLight lightSource)
        {
            for (int i = 0; i < TrigsLst.Count; i++)
            {
                if (IntersectionRayTriangle(r, TrigsLst[i]))
                {
                    return ComputeLightening(r, lightSource) * new Vector3(0, 1, 0);
                }
            }

            return new Vector3(1, 1, 1);
        }

        private Vector3 RayColor(Ray r, PointLight lightSource)
        {
            return ComputeLightening(r, lightSource) * new Vector3(0, 1, 0);   
        }

        private void WriteColor(ref byte[] rgb, Vector3 pixelColor, int point)
        {
            point *= 3;
            rgb[point] = (byte)(pixelColor.X * 255);
            rgb[point+1] = (byte)(pixelColor.Y * 255);
            rgb[point+2] = (byte)(pixelColor.Z * 255);
        }

        public static bool IntersectionRayTriangle(Ray ray, Triangle inTriangle)
        {
            var vertex0 = inTriangle.a;
            var vertex1 = inTriangle.b;
            var vertex2 = inTriangle.c;
            var edge1 = vertex1 - vertex0;
            var edge2 = vertex2 - vertex0;
            var h = ray.Dir.CrossProduct(edge2);
            var a = edge1.DotProduct(h);
            var EPSILON = 1e-5f;
            if (a > -EPSILON && a < EPSILON)
            {
                return false;
            }
            var f = 1 / a;
            var s = ray.Orig - vertex0;
            var u = f * s.DotProduct(h);
            if (u < 0.0 || u > 1.0)
            {
                return false;
            }
            var q = s.CrossProduct(edge1);
            var v = f * ray.Dir.DotProduct(q);
            if (v < 0.0 || u + v > 1.0)
            {
                return false;
            }
            var t = f * edge2.DotProduct(q);
            return t > EPSILON;
        }

        private List<Triangle> getTrianglesList(LoadResult obj)
        {
            List<Triangle> TriangleLst = new List<Triangle>();
            var faces = obj.Groups[0].Faces;
            var vertices = obj.Vertices;
            for (int i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                var vertexIndex1 = face[0].VertexIndex - 1;
                var vertex1 = new Vector3(vertices[vertexIndex1].X, vertices[vertexIndex1].Y, vertices[vertexIndex1].Z);
                var vertexIndex2 = face[1].VertexIndex - 1;
                var vertex2 = new Vector3(vertices[vertexIndex2].X, vertices[vertexIndex2].Y, vertices[vertexIndex2].Z);
                var vertexIndex3 = face[2].VertexIndex - 1;
                var vertex3 = new Vector3(vertices[vertexIndex3].X, vertices[vertexIndex3].Y, vertices[vertexIndex3].Z);

                Triangle triangel = new Triangle(vertex1, vertex2, vertex3);

                TriangleLst.Add(RotateX(triangel, AngelX));
                
            }

            return TriangleLst;
        }

        private Triangle TranslateX(Triangle trig, float offset)
        {
            float[,] transformMatrix = new float[4, 4] {
                {0, 0, 0, offset},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        private Triangle TranslateY(Triangle trig, float offset)
        {
            float[,] transformMatrix = new float[4, 4] {
                {0, 0, 0, 0},
                {0, 1, 0, offset},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        private static Triangle TranslateZ(Triangle trig, float offset)
        {
            float[,] transformMatrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, offset},
                {0, 0, 0, 1}
            };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        private Triangle RotateX(Triangle trig, double angel)
        {
            float cosAngel = (float)Math.Cos(angel);
            float sinAngel = (float)Math.Sin(angel);
            float[,] transformMatrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, cosAngel, -sinAngel, 0},
                {0, sinAngel, cosAngel, 0},
                {0, 0, 0, 1}
            };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        private Triangle RotateY(Triangle trig, double angel)
        {
            float cosAngel = (float)Math.Cos(angel);
            float sinAngel = (float)Math.Sin(angel);
            float[,] transformMatrix = new float[4, 4] {
                {cosAngel, 0, sinAngel, 0},
                {0, 1, 0, 0},
                {-sinAngel, 0, cosAngel, 0},
                {0, 0, 0, 1}
            };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        private Triangle RotateZ(Triangle trig, double angel)
        {
            float cosAngel = (float)Math.Cos(angel);
            float sinAngel = (float)Math.Sin(angel);
            float[,] transformMatrix = new float[4, 4] {
                {cosAngel, -sinAngel, 0, 0},
                {sinAngel, cosAngel, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        private Triangle ScaleX(Triangle trig, float scale)
        {
            float[,] transformMatrix = new float[4, 4] {
                {scale, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);

        }

        private Triangle ScaleY(Triangle trig, float scale)
        {
            float[,] transformMatrix = new float[4, 4] {
                  {1, 0, 0, 0},
                  {0, scale, 0, 0},
                  {0, 0, 1, 0},
                  {0, 0, 0, 1}
              };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        private static Triangle ScaleZ(Triangle trig, float scale)
        {
            float[,] transformMatrix = new float[4, 4] {
                  {1, 0, 0, 0},
                  {0, 1, 0, 0},
                  {0, 0, scale, 0},
                  {0, 0, 0, 1}
              };

            Vector3 a = MultipleArrs(transformMatrix, trig.a);
            Vector3 b = MultipleArrs(transformMatrix, trig.b);
            Vector3 c = MultipleArrs(transformMatrix, trig.c);

            return new Triangle(a, b, c);
        }

        static Vector3 MultipleArrs(float[,] matrixA, Vector3 vector)
        {
           
            float[,] matrixB = new float[4, 1] { { vector.X }, { vector.Y }, { vector.Z }, { 1 } };

            int matrixARows = 4;
            int matrixACols = 4;
            int matrixBCols = 1;
            float[,] matrixC = new float[matrixARows, matrixBCols];

            for (var i = 0; i < matrixARows; i++)
            {
                for (var j = 0; j < matrixBCols; j++)
                {
                    matrixC[i, j] = 0;

                    for (var k = 0; k < matrixACols; k++)
                    {
                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }
            Vector3 resVector = new Vector3(matrixC[0, 0], matrixC[1, 0], matrixC[2, 0]);
            return resVector;
        }

    }
}
