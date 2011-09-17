using System;
using Microsoft.Practices.Prism.Modularity;

namespace ArtemisWest.Mayfair.Infrastructure
{
    public static class ModuleCatalogExtensions
    {
        public static void AddModule<T>(this IModuleCatalog moduleCatalog) where T : IModule
        {
            Type moduleType = typeof(T);
            moduleCatalog.AddModule(new ModuleInfo(moduleType.Name, moduleType.AssemblyQualifiedName));
        }
    }
}