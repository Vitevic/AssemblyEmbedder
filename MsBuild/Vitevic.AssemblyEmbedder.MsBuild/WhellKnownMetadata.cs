using System;
using Microsoft.Build.Framework;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    internal static class WhellKnownMetadata
    {
        internal static string GetFullPath(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("FullPath");
        }

        internal static string GetFilename(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("Filename");
        }

        internal static string GetExtension(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("Extension");
        }

        internal static string GetMSBuildSourceProjectFile(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("MSBuildSourceProjectFile");
        }
        
    }
}
