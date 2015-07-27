using System;
using System.Collections.Generic;
using System.Text;

namespace Vitevic.AssemblyEmbedder.InjectTest
{
    class Program
    {
        private static Dictionary<string, System.Reflection.Assembly> fields = new Dictionary<string, System.Reflection.Assembly>();

        static System.Reflection.Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = "Vitevic.Embedded." + new System.Reflection.AssemblyName(args.Name).Name + ".dll";
            if (fields.ContainsKey(assemblyName))
            {
                return fields[assemblyName];
            }

            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName))
            {
                if (stream != null)
                {
                    var data = new Byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    var assembly = System.Reflection.Assembly.Load(data);
                    fields[assemblyName] = assembly;
                    return assembly;
                }
            }

            return null;
        }

        static void Main(string[] args)
        {
        }
    }
}
