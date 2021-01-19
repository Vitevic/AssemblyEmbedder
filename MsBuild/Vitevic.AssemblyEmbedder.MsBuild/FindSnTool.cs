using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    public class FindSnTool : Task
    {
        [Required]
        public String FrameworkSdkPath { get; set; }

        public String PredefinedSnToolPath { get; set; }

        [Output]
        public String SnToolPath { get; private set; }

        public override bool Execute()
        {
            SnToolPath = PredefinedSnToolPath;

            if( String.IsNullOrEmpty(SnToolPath) && Directory.Exists(FrameworkSdkPath) )
            {
                string[] temp = Directory.GetFiles(FrameworkSdkPath, "sn.exe", SearchOption.AllDirectories);
                if (temp != null && temp.Length > 0)
                    SnToolPath = temp[0];
            }

            if (String.IsNullOrEmpty(SnToolPath))
            {
                Log.LogError("Cannot find sn.exe");
                return false;
            }

            return true;
        }
    }
}
