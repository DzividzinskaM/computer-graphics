using SceneFormat;
using System;
using System.Collections.Generic;

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

            scene.objects = getSceneObjects(readResultJson.SceneObjects);

            return scene;
        }

        private List<IProgramSceneObject> getSceneObjects(Google.Protobuf.Collections.RepeatedField<SceneObject> sceneObjects)
        {
            List<IProgramSceneObject> objects = new List<IProgramSceneObject>();
            foreach (var obj in sceneObjects)
            {
                var newSceneObj = new MeshObject();
                if (obj.Transform != null)
                {
                    newSceneObj.transform = new Transform();
                    newSceneObj.transform.Position = new Vector3((float)obj.Transform.Position.X, (float)obj.Transform.Position.Y, (float)obj.Transform.Position.Z);
                    newSceneObj.transform.Rotation = new Vector3((float)obj.Transform.Rotation.X, (float)obj.Transform.Rotation.Y, (float)obj.Transform.Rotation.Z);
                    newSceneObj.transform.Scale = new Vector3((float)obj.Transform.Scale.X, (float)obj.Transform.Scale.Y, (float)obj.Transform.Scale.Z);
                }

                if (obj.Material != null && obj.Material is LambertMaterial)
                {
                    Vector3 color = new Vector3((float)obj.Material.LambertReflection.Color.R,
                        (float)obj.Material.LambertReflection.Color.G,
                        (float)obj.Material.LambertReflection.Color.B);
                    newSceneObj.material = new LambertMaterial(color);
                }
                else if (obj.Material != null && obj.Material is SpecularReflectionMaterial)
                {
                    //adding info;
                    double eta = (float)obj.Material.SpecularReflection.Eta;
                }


                newSceneObj.filePath = obj.MeshedObject.Reference;


                objects.Add(newSceneObj);
            }
            return objects;
        }
    }
}