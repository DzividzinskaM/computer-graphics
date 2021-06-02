using System;
using SceneFormat;


namespace RendererApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            /*string filePath = "D:/testValue/cow.scene";
            string output = "D:/testValue/withInterpolation.ppm";*/
            //string[] args = { $"--source={filePath}", $"--output={output}" };
            try
            {
               RendererApp app = new RendererApp();
               app.Start(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static readonly ISceneIO _sceneIO = new SceneIO();

        /*static void createFormat()
        {
            
            var scene = new SceneFormat.Scene
            {
                Version = 1,
                SceneObjects =
                {
                    new SceneObject
                    {
                        Id = 1,

                        Material = new Material
                        {
                            LambertReflection = new LambertReflectionMaterial
                            {
                                Color = new Color{
                                    R=1,
                                    G=0,
                                    B=0,
                                }

                            },
                        },
                        Transform = new SceneFormat.Transform
                        {
                            Rotation = new SceneFormat.Vector3{X=4.7124, Y=0, Z=-0},

                        },
                        MeshedObject = new MeshedObject
                        {
                            Reference = "D:/testValue/cow.obj",
                        }
                    },
                },
                Cameras =
                {
                    new Camera
                    {
                        Id = 2,
                        Transform = new SceneFormat.Transform {
                            Position = new SceneFormat.Vector3 { X = 0, Y = 0, Z = 1, }
                        },
                    },
                    new Camera
                    {
                        Id = 3,
                        Transform = new SceneFormat.Transform {
                            Position = new SceneFormat.Vector3 { X = -0.5, Y = 0, Z = 1, }
                        },
                    }
                },
                RenderOptions = new RenderOptions
                {
                    CameraId = 2,
                    Width = 1024,
                }
            };

            _sceneIO.Save(scene, "D:/testValue/cow.scene");
            _sceneIO.SaveAsJson(scene, "D:/testValue/cow.scene");
        }*/
    }
}
