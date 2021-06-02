using System.Collections.Generic;

namespace RendererApp
{
    public class ProgramScene
    {
        public int width { get; set; }
        public List<IProgramSceneObject> objects;
        public List<StaticCameraPositionProvider> cameras;
        public StaticCameraPositionProvider mainCamera { get; set; } 


        public ProgramScene()
        { 
            objects = new List<IProgramSceneObject>();
            cameras = new List<StaticCameraPositionProvider>();
        }

    }
}