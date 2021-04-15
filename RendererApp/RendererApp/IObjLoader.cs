using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public interface IObjLoader
    {
        public LoadResult LoadObj(string path);
    }
}
