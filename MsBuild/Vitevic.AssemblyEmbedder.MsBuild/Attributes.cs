using System;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    public static class Attributes
    {
        public const string EmbedAssemblyName = "EmbedAssembly";
        public const string CompressEmbededDataName = "CompressEmbededData";
        public const string EmbedAssemblyPdbName = "EmbedAssemblyPdb";
        public const string EmbedAssemblyDependenciesName = "EmbedAssemblyDependencies";

        public static bool IsTrue(string value)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim();
                result = (0 == string.Compare(value, "true", StringComparison.OrdinalIgnoreCase)) || value == "1";
            }

            return result;
        }
    }
}
