using System;
using Microsoft.Build.Framework;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    static class WhellKnownMetadata
    {
        internal static String GetFullPath(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("FullPath");
        }

        internal static String GetFilename(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("Filename");
        }

        internal static String GetExtension(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("Extension");
        }

        internal static String GetMSBuildSourceProjectFile(this ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            return taskItem.GetMetadata("MSBuildSourceProjectFile");
        }
        
    }
}
