using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build;
using Microsoft.Build.Utilities;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    public static class Attributes
    {
        public const String EmbedAssemblyName = "EmbedAssembly";
        public const String CompressEmbededDataName = "CompressEmbededData";
        public const String EmbedAssemblyPdbName = "EmbedAssemblyPdb";
        public const String EmbedAssemblyDependenciesName = "EmbedAssemblyDependencies";

        public static bool IsTrue(String value)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Trim();
                result = (0 == String.Compare(value, "true", StringComparison.OrdinalIgnoreCase)) || value == "1";
            }

            return result;
        }
    }
}
