using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RendererApp
{
    public class StandartObjLoader : IObjLoader
    {
        public LoadResult LoadObj(string path)
        {
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();

            FileStream fileStream = new FileStream(path, FileMode.Open);
            var result = objLoader.Load(fileStream);
            return result;
        }
    }
}
