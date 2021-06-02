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
        private float[,] transformedMatrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
        };
        private ProgramScene scene;

        public Renderer(ProgramScene scene)
        {
            this.scene = scene;
            _objLoader = Container.GetService<IObjLoader>();
            _camera = Container.GetService<ICameraPositionProvider>();
            _objLoader = Container.GetService<IObjLoader>();
            _raysProvider = Container.GetService<IRaysProvider>();
            _width = scene.width;
            _height = (int)(_width / _aspectRatio);
            _viewportWidth = _aspectRatio * _viewportHeight;

        }

        public void ren(ProgramScene scene, string outputPath)
        {
            List<ILightSource> lightList = new List<ILightSource>();
            PointLight pointLightSource = new PointLight { Intencity = 0.5f, Position = new Vector3(0, 0, -1) };
            AmbientLight ambientLightSource = new AmbientLight { Intencity = 0.1f };
            DirectionalLight directonalLightSource = new DirectionalLight { Intencity = 0.4f, Direction = new Vector3(1, 0.7f, 3) };

            lightList.Add(pointLightSource);
            lightList.Add(ambientLightSource);
            lightList.Add(directonalLightSource);

            var cameraPos = scene.mainCamera.GetCameraPosition();
            Screen screen = new Screen(_viewportHeight, _viewportWidth, focalLength, cameraPos);
            var rays = _raysProvider.GetRays(_width, _height, cameraPos, screen);
            byte[] rgb = new byte[_width * _height * 3];

            List<SphereObject> spheresLst = new List<SphereObject>();
            List<MeshObject> meshObjects = new List<MeshObject>();

            foreach (var obj in scene.objects)
            {
                if (obj is SphereObject)
                {
                    spheresLst.Add((SphereObject)obj);
                }
                if (obj is MeshObject)
                {
                    meshObjects.Add((MeshObject)obj);
                }
            }
            
            if(spheresLst.Count > 0)
                renderSpheres(spheresLst, rays, ref rgb, lightList);
            if (meshObjects.Count > 0)
                renderMeshs(meshObjects, rays, ref rgb, lightList);
            Image image = new Image(_width, _height, rgb);
            PpmWriter writer = new PpmWriter();
            writer.Write(outputPath, image);
        }

        private void renderMeshs(List<MeshObject> meshObjects, List<Ray> rays, ref byte[] rgb, List<ILightSource> lightList)
        {
            foreach(var mesh in meshObjects)
            {
                var obj = _objLoader.LoadObj(mesh.filePath);
                getTransformMatrix(mesh.transform);
                var triangles = getTrianglesList(obj);
                var tree = new Octree(triangles);

                for (int i = 0; i < rays.Count; i++)
                {
                    var ray = rays[i];
                    Vector3 pixelColor = new Vector3(0, 1, 0);
                    if (tree.CheckChildNodes(ray, out var closestTriangle))
                    {
                        var startColor = new Vector3(0, 0, 0);
                        if(mesh.material is LambertMaterial)
                        {
                            var material = (LambertMaterial)mesh.material;
                            startColor = new Vector3(material.color.X, material.color.Y, material.color.Z);
                        }
                        Vector3 norm = (closestTriangle.b - closestTriangle.a).CrossProduct(closestTriangle.c - closestTriangle.a);
                        Ray ray1 = new Ray(ray.Orig, norm);


                       /* var vNormal = obj.Normals[closestTriangle.a.normalIndex];
                        var n0 = new Vector3(vNormal.X, vNormal.Y, vNormal.Z);
                        var vNormal1 = obj.Normals[closestTriangle.b.normalIndex];
                        var n1 = new Vector3(vNormal1.X, vNormal1.Y, vNormal1.Z);
                        var vNormal2 = obj.Normals[closestTriangle.c.normalIndex];
                        var n2 = new Vector3(vNormal2.X, vNormal2.Y, vNormal2.Z);

                        IntersectionRayTriangle(ray, closestTriangle, out float u, out float v);

                        var hitNormal = u * n0 + v * n1 + (1 - u - v) * n2;

                        Ray ray1 = new Ray(ray.Orig, hitNormal);*/


                        var color = (float)ComputeLightening(ray1, lightList) * startColor;
                        WriteColor(ref rgb, color, i);
                    }
                    else
                    {
                        WriteColor(ref rgb, new Vector3(1, 1, 1), i);
                    }
                }
            }
        }

        private void renderSpheres(List<SphereObject> spheresLst, List<Ray> rays, ref byte[] rgb, List<ILightSource> lightList)
        {
            for (int i = 0; i < rays.Count; i++)
            {
                var color = DrawSphere(rays[i], i, lightList, spheresLst, 100);
                WriteColor(ref rgb, color, i);
            }

        }

        private Vector3 DrawSphere(Ray ray, int point, List<ILightSource> lightList, List<SphereObject> sphereLst, int depth)
        {

            var closestSphere = findHittedSphere(sphereLst, ray, out double closest);


            if (closestSphere == null)
                return new Vector3(1, 1, 1);


            var p = ray.Orig + (float)closest * ray.Dir;
            var n = p - closestSphere.transform.Position;
            n = n.Norm();
            var ray1 = new Ray(p, n);

            float eta = 0;
            if(closestSphere.material is SpecReflMaterial)
            {
                eta = ((SpecReflMaterial)closestSphere.material).eta;
            }
            var sphereColor = new Vector3(1, 1, 1);
            if(closestSphere.material is LambertMaterial)
            {
                var material = (LambertMaterial)closestSphere.material;
                sphereColor = new Vector3(material.color.X, material.color.Y, material.color.Z);
            }
            var color = (float)ComputeLightening(ray1, lightList, sphereLst) * sphereColor;

            var r = eta;

            if (depth <= 0 || r <= 0)
                return color;

            var ray2 = new Ray(ray.Orig, -1 * ray.Dir);
            var R = ReflectRay(ray2, n);

            var ray3 = new Ray(p, R);
            Vector3 reflected = DrawSphere(ray3, point, lightList, sphereLst, depth - 1);


            return (1 - r) * color + r * reflected;
            
        }


        bool hit_sphere(Vector3 center, double radius, Ray ray, out float t1, out float t2)
        {
            t1 = 0;
            t2 = 0;
            Vector3 oc = ray.Orig - center;
            var a = ray.Dir.DotProduct(ray.Dir);
            var b = 2 * oc.DotProduct(ray.Dir);
            var c = oc.DotProduct(oc) - radius * radius;
            var d = b * b - 4 * a * c;

            if (d < 0) return false;

            t1 = (float)(-1 * b + Math.Sqrt(d)) / (2 * a);
            t2 = (float)(-1 * b - Math.Sqrt(d)) / (2 * a);

            return true;

        }

        private Vector3 ReflectRay(Ray ray, Vector3 N)
        {
            return 2 * N.DotProduct(ray.Dir) * N - ray.Dir;
        }


        private SphereObject findHittedSphere(List<SphereObject> sphereLst, Ray ray, out double closest,
            double min = 0,
            double max = double.PositiveInfinity)
        {
            closest = Double.PositiveInfinity;
            SphereObject closestSphere = null;
            foreach (var sphere in sphereLst)
            {
                if (hit_sphere(sphere.transform.Position, sphere.radius, ray, out float t1, out float t2))
                {
                    if (t1 >= min && t1 <= max && t1 <= closest)
                    {
                        closest = t1;
                        closestSphere = sphere;
                    }
                    if (t2 >= min && t2 <= max && t2 <= closest)
                    {
                        closest = t2;
                        closestSphere = sphere;
                    }
                }
            }
            return closestSphere;
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

                    float n_dot_l = L.DotProduct(ray.Dir);
                    if (n_dot_l > 0)
                        i += (light.Intencity * n_dot_l) / (ray.Dir.Length * L.Length);
                }

            }

            return i;
        }


        private double ComputeLightening(Ray ray, List<ILightSource> lightList,  List<SphereObject> sphereLst)
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

                    var shadowSphere = findHittedSphere(sphereLst, ray, out double closest, 0.001, max);
                    if (shadowSphere != null)
                        continue;

                    float n_dot_l = L.DotProduct(ray.Dir);
                    if (n_dot_l > 0)
                        i += (light.Intencity * n_dot_l) / (ray.Dir.Length * L.Length);

                    /*if (s != -1)
                    {
                        var v = -1 * ray.Dir;
                        var R = ReflectRay(ray, L);
                        var rDotV = R.DotProduct(v);
                        if (rDotV > 0)
                        {
                            double a = rDotV / (R.Length * v.Length);
                            i += light.Intencity * Math.Pow(a, s);
                        }

                    }*/
                }

            }

            return i;
        }

        private void WriteColor(ref byte[] rgb, Vector3 pixelColor, int point)
        {
            point *= 3;
            rgb[point] = (byte)(pixelColor.X * 255);
            rgb[point+1] = (byte)(pixelColor.Y * 255);
            rgb[point+2] = (byte)(pixelColor.Z * 255);
        }

        public static bool IntersectionRayTriangle(Ray ray, Triangle inTriangle, out float u, out float v)
        {
            var vertex0 = inTriangle.a;
            var vertex1 = inTriangle.b;
            var vertex2 = inTriangle.c;
            var edge1 = vertex1 - vertex0;
            var edge2 = vertex2 - vertex0;
            var h = ray.Dir.CrossProduct(edge2);
            var a = edge1.DotProduct(h);
            var EPSILON = 1e-5f;
            var f = 1 / a;
            var s = ray.Orig - vertex0;
            u = f * s.DotProduct(h);
            var q = s.CrossProduct(edge1);
            v = f * ray.Dir.DotProduct(q);
            if (a > -EPSILON && a < EPSILON)
            {
                return false;
            }
            if (u < 0.0 || u > 1.0)
            {
                return false;
            }
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
                var vertex1 = new Vector3(vertices[vertexIndex1].X, vertices[vertexIndex1].Y, vertices[vertexIndex1].Z, face[0].NormalIndex);

                var vertexIndex2 = face[1].VertexIndex - 1;
                var vertex2 = new Vector3(vertices[vertexIndex2].X, vertices[vertexIndex2].Y, vertices[vertexIndex2].Z, face[1].NormalIndex);

                var vertexIndex3 = face[2].VertexIndex - 1;
                var vertex3 = new Vector3(vertices[vertexIndex3].X, vertices[vertexIndex3].Y, vertices[vertexIndex3].Z, face[2].NormalIndex);

                Triangle triangel = new Triangle(vertex1, vertex2, vertex3);
                var transformed = Transform(triangel);
                TriangleLst.Add(transformed);

            }

            return TriangleLst;
        }

        private Triangle Transform(Triangle triangel)
        {
            Vector3 a = MultipleMatrixVector(transformedMatrix, triangel.a);
            a.normalIndex = triangel.a.normalIndex;
            Vector3 b = MultipleMatrixVector(transformedMatrix, triangel.b);
            b.normalIndex = triangel.b.normalIndex;
            Vector3 c = MultipleMatrixVector(transformedMatrix, triangel.c);
            c.normalIndex = triangel.c.normalIndex;

            return new Triangle(a, b, c);
        }

        private Vector3 MultipleMatrixVector(float[,] transformedMatrix, Vector3 v)
        {
            float[,] matrixB = new float[4, 1] { { v.X }, { v.Y }, { v.Z }, { 1 } };
            var resMatrix = Multiple(transformedMatrix, matrixB);
            return new Vector3(resMatrix[0, 0], resMatrix[1, 0], resMatrix[2, 0]);
        }

        private void getTransformMatrix(Transform transform)
        {
            TranslateX(transform.Position.X);
            TranslateY(transform.Position.Y);
            TranslateZ(transform.Position.Z);
            RotateX(transform.Rotation.X);
            RotateY(transform.Rotation.Y);
            RotateZ(transform.Rotation.Z);
            ScaleX(transform.Scale.X);
            ScaleY(transform.Scale.Y);
            ScaleZ(transform.Scale.Z);

        }

        private void TranslateX(float x)
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, x},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void TranslateY(float y)
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, y},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void TranslateZ(float z)
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, z},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void RotateX(float x)
        {
            float cosAngel = (float)Math.Cos(x);
            float sinAngel = (float)Math.Sin(x);
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, cosAngel, -sinAngel, 0},
                {0, sinAngel, cosAngel, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void RotateY(float y)
        {
            float cosAngel = (float)Math.Cos(y);
            float sinAngel = (float)Math.Sin(y);
            float[,] matrix = new float[4, 4] {
                {cosAngel, 0, sinAngel, 0},
                {0, 1, 0, 0},
                {-sinAngel, 0, cosAngel, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void RotateZ(float z)
        {
            float cosAngel = (float)Math.Cos(z);
            float sinAngel = (float)Math.Sin(z);
            float[,] matrix = new float[4, 4] {
                {cosAngel, -sinAngel, 0, 0},
                {sinAngel, cosAngel, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void ScaleX(float x)
        {
            float[,] matrix = new float[4, 4] {
                {x, 0, 0, 0},
                {0, 1, 0, 0},
                { 0, 0, 1, 0},
                { 0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void ScaleY(float y)
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, y, 0, 0},
                { 0, 0, 1, 0},
                { 0, 0, 0, 1}
            };

            transformedMatrix = Multiple(transformedMatrix, matrix);
        }

        private void ScaleZ(float z)
        {
            float[,] matrix = new float[4, 4] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                { 0, 0, z, 0},
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
