using Microsoft.Extensions.DependencyInjection;
using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class ContainerBuilder
    {
        public IServiceProvider Build()
        {
            var container = new ServiceCollection();
            container.AddScoped<ICameraPositionProvider, StaticCameraPositionProvider>();
            container.AddScoped<IObjLoader, StandartObjLoader>();
            container.AddScoped<IRaysProvider, RaysPerspectiveProvider>();

            return container.BuildServiceProvider();
        }
    }
}
