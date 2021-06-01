using System.Collections.Generic;

namespace RendererApp
{
    public class ProgramScene
    {
        public List<IProgramSceneObject> objects;

        public ProgramScene()
        {
            objects = new List<IProgramSceneObject>();
        }

    }
}