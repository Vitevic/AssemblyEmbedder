using System;
using System.Collections.Generic;

namespace Vitevic.AssemblyEmbedder.InjectTest
{
    internal class Program
    {
        private static Dictionary<string, System.Reflection.Assembly> fields = new Dictionary<string, System.Reflection.Assembly>();

        private static System.Reflection.Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            string assemblyName = "Vitevic.Embedded." + new System.Reflection.AssemblyName(args.Name).Name + ".dll";
            if (fields.ContainsKey(assemblyName))
            {
                return fields[assemblyName];
            }

            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName))
            {
                if (stream != null)
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    var assembly = System.Reflection.Assembly.Load(data);
                    fields[assemblyName] = assembly;
                    return assembly;
                }
            }

            return null;
        }

        private static void Main(string[] args)
        {
        }
    }
}
