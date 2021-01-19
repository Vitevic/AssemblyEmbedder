using System;
using Microsoft.Build.Framework;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    internal class EmbeddedItemInfo
    {
        internal const string ResourcePrefix = "Vitevic.EmbeddedAssembly.";

        internal string Name { get; private set; }
        internal string Path { get; private set; }
        internal bool Compress { get; set; }

        internal EmbeddedItemInfo(ITaskItem reference, bool compress)
            : this(reference.GetFullPath(), compress)
        {
        }

        internal EmbeddedItemInfo(string path, bool compress)
        {
            Path = path;
            Name = ResourcePrefix + System.IO.Path.GetFileName(Path);
            Compress = compress;
        }
    }
}
