using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;

namespace ArtemisWest.Mayfair.Infrastructure
{
    public static class ModuleExtensions
    {
        private const string ViewResourceSuffix = "view.baml";
        private const string BamlResourcesSuffix = ".g.resources";

        public static void LoadViews(this IModule module)
        {
            var assembly = module.GetType().Assembly;
            var assemblyNamespace = assembly.GetName().Name;
            var resourceName = assembly.GetManifestResourceNames()
                .SingleOrDefault(resName => resName.Equals(assemblyNamespace + BamlResourcesSuffix));

            if (resourceName == null) return;
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream == null) return;
            using (var resourceReader = new ResourceReader(resourceStream))
            {
                
                var views = from DictionaryEntry resourceEntry in resourceReader
                             select resourceEntry.Key.ToString()
                             into key where key.EndsWith(ViewResourceSuffix) 
                                select key.Remove(key.Length - ViewResourceSuffix.Length) + "View.xaml"
                             into xamlFileName 
                                select xamlFileName.Replace(@"/", @"\");
                LoadViews(assembly, views);
            }
        }

        private static void LoadViews(Assembly sourceAssembly, IEnumerable<string> views)
        {
            var viewResources = views
                .Select(view => GetXamlUri(sourceAssembly, view))
                .Select(uri => new ResourceDictionary { Source = uri });

            var appMergedResourceDictionaries = Application.Current.Resources.MergedDictionaries;
            foreach (var viewResource in viewResources)
            {
                appMergedResourceDictionaries.Add(viewResource);    
            }
        }

        private static Uri GetXamlUri(Assembly assembly, string viewPath)
        {
            var packPath = string.Format(@"pack://application:,,,/{0};component/{1}", 
                                         assembly.GetName().Name,
                                         viewPath);
            return new Uri(packPath, UriKind.RelativeOrAbsolute);
        }
    }
}