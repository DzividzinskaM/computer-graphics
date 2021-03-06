using Google.Protobuf.Collections;
using SceneFormat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RendererApp
{
    public class SceneFileLoader
    {
        private static readonly ISceneIO _sceneIO = new SceneIO();
        private ProgramScene scene;
       

        public ProgramScene load(string path)
        {
            scene = new ProgramScene();
            var readResultJson = _sceneIO.Read(path);

            scene.cameras = getSceneCameras(readResultJson.Cameras);
            scene.objects = getSceneObjects(readResultJson.SceneObjects);
           
            if(readResultJson.RenderOptions != null)
            {
                scene.width = readResultJson.RenderOptions.Width;
                scene.mainCamera = scene.cameras.Where(c => c.id == (int)readResultJson.RenderOptions.CameraId).FirstOrDefault();
            }

            return scene;
        }

        private List<StaticCameraPositionProvider> getSceneCameras(RepeatedField<Camera> cameras)
        {
            List<StaticCameraPositionProvider> objects = new List<StaticCameraPositionProvider>();
            foreach (var camera in cameras)
            {
                StaticCameraPositionProvider newCamera = new StaticCameraPositionProvider();
                var pos = new Vector3((float)camera.Transform.Position.X, (float)camera.Transform.Position.Y, (float)camera.Transform.Position.Z);
                newCamera.SetCameraPosition(pos, camera.Id);

                objects.Add(newCamera);
            }
            return objects;
        }

        private List<IProgramSceneObject> getSceneObjects(Google.Protobuf.Collections.RepeatedField<SceneObject> sceneObjects)
        {
            List<IProgramSceneObject> objects = new List<IProgramSceneObject>();
            foreach (var obj in sceneObjects)
            {
                Transform transform = new Transform();
                if (obj.Transform.ToString().Length !=0)
                {
                    transform.Position = new Vector3((float)obj.Transform.Position.X, (float)obj.Transform.Position.Y, (float)obj.Transform.Position.Z);
                    transform.Rotation = new Vector3((float)obj.Transform.Rotation.X, (float)obj.Transform.Rotation.Y, (float)obj.Transform.Rotation.Z);
                    transform.Scale = new Vector3((float)obj.Transform.Scale.X, (float)obj.Transform.Scale.Y, (float)obj.Transform.Scale.Z);
                }


                IMaterial material = null;
                if (obj.Material.LambertReflection != null)
                {
                    Vector3 color = new Vector3((float)obj.Material.LambertReflection.Color.R,
                        (float)obj.Material.LambertReflection.Color.G,
                        (float)obj.Material.LambertReflection.Color.B);
                    material = new LambertMaterial(color);
                }
                else if (obj.Material.SpecularReflection.ToString().Length != 0)
                {
                    material = new SpecReflMaterial((float)obj.Material.SpecularReflection.Eta);
                }

                if (obj.MeshedObject != null)
                {
                    MeshObject newSceneObj = new MeshObject();
                    newSceneObj.material = material;
                    newSceneObj.transform = transform;

                    newSceneObj.filePath = obj.MeshedObject.Reference;
                    objects.Add(newSceneObj);
                }
                else if (obj.Sphere.ToString().Length != 0)
                {
                    SphereObject newSceneObj = new SphereObject();
                    newSceneObj.material = material;
                    newSceneObj.transform = transform;

                    newSceneObj.radius = (float)obj.Sphere.Radius;
                    objects.Add(newSceneObj);
                }

            }
            return objects;
        }
    }
}