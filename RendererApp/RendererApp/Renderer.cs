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
        private float[,] transformedMatrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
        };
        private ProgramScene scene;

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
            getTransformMatrix();
        }

        public Renderer(ProgramScene scene)
        {
            this.scene = scene;
            _objLoader = Container.GetService<IObjLoader>();
            _camera = Container.GetService<ICameraPositionProvider>();
            _objLoader = Container.GetService<IObjLoader>();
            _raysProvider = Container.GetService<IRaysProvider>();
            _width = 560;
            _height = (int)(_width / _aspectRatio);
            _viewportWidth = _aspectRatio * _viewportHeight;
        }

        internal void Render(ProgramScene scene, string outputPath)
        {
            List<ILightSource> lightList = new List<ILightSource>();
            PointLight pointLightSource = new PointLight { Intencity = 0.5f, Position = new Vector3(0, 0, -1) };
            AmbientLight ambientLightSource = new AmbientLight { Intencity = 0.1f };
            DirectionalLight directonalLightSource = new DirectionalLight { Intencity = 0.4f, Direction = new Vector3(1, 0.7f, 3) };
            lightList.Add(pointLightSource);
            lightList.Add(ambientLightSource);
            lightList.Add(directonalLightSource);

            string path = ((MeshObject)scene.objects[0]).filePath;
            var obj = _objLoader.LoadObj(path);
            var cameraPos = _camera.GetCameraPosition();
            Screen screen = new Screen(_viewportHeight, _viewportWidth, focalLength, cameraPos);
            var rays = _raysProvider.GetRays(_width, _height, cameraPos, screen);
            var triangles = getTrianglesList(obj);
            var tree = new Octree(triangles);
            byte[] rgb = new byte[_width * _height * 3];

            for (int i = 0; i < rays.Count; i++)
            {
                var ray = rays[i];
                Vector3 pixelColor = new Vector3(0, 1, 0);
                if (tree.CheckChildNodes(ray, out var closestTriangle))
                {
                    Vector3 norm = (closestTriangle.b - closestTriangle.a).CrossProduct(closestTriangle.c - closestTriangle.a);
                    Ray ray1 = new Ray(ray.Orig, norm);
                    var color = (float)ComputeLightening(ray1, lightList) * pixelColor;
                    WriteColor(ref rgb, color, i);
                }
                else
                {
                    WriteColor(ref rgb, new Vector3(1, 1, 1), i);
                }
            }

            Image image = new Image(_width, _height, rgb);
            PpmWriter writer = new PpmWriter();
            writer.Write(outputPath, image);


        }

        public void Render(string sourcePath, string outputPath)
        {
            //light
            List<ILightSource> lightList = new List<ILightSource>();
            PointLight pointLightSource = new PointLight { Intencity = 0.5f, Position = new Vector3(0, 0, -1) };
            AmbientLight ambientLightSource = new AmbientLight { Intencity = 0.1f };
            DirectionalLight directonalLightSource = new DirectionalLight { Intencity = 0.4f, Direction = new Vector3(1, 0.7f, 3) };
            lightList.Add(pointLightSource);
            //lightList.Add(ambientLightSource);
            //lightList.Add(directonalLightSource);


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
                Vector3 pixelColor = new Vector3(0, 1, 0);
                if (tree.CheckChildNodes(ray, out var closestTriangle))
                {
                    Vector3 norm = (closestTriangle.b - closestTriangle.a).CrossProduct(closestTriangle.c - closestTriangle.a);
                    Ray ray1 = new Ray(ray.Orig, norm);
                    var color = (float)ComputeLightening(ray1, lightList) * pixelColor;
                    WriteColor(ref rgb, color, i);
                }
                else
                {
                    WriteColor(ref rgb, new Vector3(1, 1, 1), i);
                }
            }

            Image image = new Image(_width, _height, rgb);
            PpmWriter writer = new PpmWriter();
            writer.Write(outputPath, image);

        }

        private double ComputeLightening(Ray ray, List<ILightSource> lightList)
        {
            double i = 0;
            double max = double.PositiveInfinity;
            foreach (var light in lightList)
            {
                if (light is AmbientLight)
                {
                    i += light.Intencity;
                }
                else
                {
                    Vector3 L;
                    if (light is PointLight)
                    {
                        L = ((PointLight)light).Position - ray.Orig;
                        max = 1;
                    }
                    else
                    {
                        L = ((DirectionalLight)light).Direction;
                        max = double.PositiveInfinity;
                    }

                    float n_dot_l = ray.Dir.DotProduct(L);
                    if (n_dot_l > 0)
                        i += (light.Intencity * n_dot_l) / (ray.Dir.Length * L.Length);
                }


            }
            return i;
        }


       /* private Vector3 RayColor(Ray r, List<Triangle> TrigsLst, PointLight lightSource)
        {
            for (int i = 0; i < TrigsLst.Count; i++)
            {
                if (IntersectionRayTriangle(r, TrigsLst[i]))
                {
                    return ComputeLightening(r, lightSource) * new Vector3(0, 1, 0);
                }
            }

            return new Vector3(1, 1, 1);
        }*/

/*        private Vector3 RayColor(Ray r, PointLight lightSource)
        {
            return ComputeLightening(r, lightSource) * new Vector3(0, 1, 0);   
        }*/

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

                TriangleLst.Add(Transform(triangel));

            }

            return TriangleLst;
        }

        private Triangle Transform(Triangle triangel)
        {
            Vector3 a = MultipleMatrixVector(transformedMatrix, triangel.a);
            Vector3 b = MultipleMatrixVector(transformedMatrix, triangel.b);
            Vector3 c = MultipleMatrixVector(transformedMatrix, triangel.c);

            return new Triangle(a, b, c);
        }

        private Vector3 MultipleMatrixVector(float[,] transformedMatrix, Vector3 v)
        {
            float[,] matrixB = new float[4, 1] { { v.X }, { v.Y }, { v.Z }, { 1 } };
            var resMatrix = Multiple(transformedMatrix, matrixB);
            return new Vector3(resMatrix[0, 0], resMatrix[1, 0], resMatrix[2, 0]);
        }

        private void getTransformMatrix()
        {
            TranslateX();
            TranslateY();
            TranslateZ();
            RotateX();
            RotateY();
            RotateZ();
            ScaleX();
            ScaleY();
            ScaleZ();

        }

        private void TranslateX()
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, OffsetX},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void TranslateY()
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, OffsetY},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void TranslateZ()
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, OffsetZ},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void RotateX()
        {
            float cosAngel = (float)Math.Cos(AngelX);
            float sinAngel = (float)Math.Sin(AngelX);
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, cosAngel, -sinAngel, 0},
                {0, sinAngel, cosAngel, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void RotateY()
        {
            float cosAngel = (float)Math.Cos(AngelY);
            float sinAngel = (float)Math.Sin(AngelY);
            float[,] matrix = new float[4, 4] {
                {cosAngel, 0, sinAngel, 0},
                {0, 1, 0, 0},
                {-sinAngel, 0, cosAngel, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void RotateZ()
        {
            float cosAngel = (float)Math.Cos(AngelZ);
            float sinAngel = (float)Math.Sin(AngelZ);
            float[,] matrix = new float[4, 4] {
                {cosAngel, -sinAngel, 0, 0},
                {sinAngel, cosAngel, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void ScaleX()
        {
            float[,] matrix = new float[4, 4] {
                {ScaleXValue, 0, 0, 0},
                {0, 1, 0, 0},
                { 0, 0, 1, 0},
                { 0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void ScaleY()
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, ScaleYValue, 0, 0},
                { 0, 0, 1, 0},
                { 0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void ScaleZ()
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                { 0, 0, ScaleZValue, 0},
                { 0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private float[,] Multiple(float[,] matrixA, float[,] matrixB)
        {
            int rowsCount = matrixA.GetUpperBound(0) + 1;
            int colsCount = matrixB.GetUpperBound(1) + 1;

            var matrixC = new float[rowsCount, colsCount];

            for (var i = 0; i < rowsCount; i++)
            {
                for (var j = 0; j < colsCount; j++)
                {
                    matrixC[i, j] = 0;

                    for (var k = 0; k < matrixA.GetUpperBound(1) + 1; k++)
                    {
                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }

            return matrixC;
        }

    }
}
