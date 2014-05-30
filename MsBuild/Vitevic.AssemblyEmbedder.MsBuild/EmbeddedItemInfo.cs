using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    class EmbeddedItemInfo
    {
        internal const String ResourcePrefix = "Vitevic.Embedded.";

        internal String Name { get; private set; }
        internal String Path { get; private set; }
        internal bool Compress { get; set; }

        internal EmbeddedItemInfo(ITaskItem reference, bool compress)
            : this(reference.GetFullPath(), compress)
        {
        }

        internal EmbeddedItemInfo(String path, bool compress)
        {
            Path = path;
            Name = ResourcePrefix + System.IO.Path.GetFileName(Path);
            Compress = compress;
        }
    }
}
