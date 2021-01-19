using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    public class FindSnTool : Task
    {
        [Required]
        public string FrameworkSdkPath { get; set; }

        public string PredefinedSnToolPath { get; set; }

        [Output]
        public string SnToolPath { get; private set; }

        public override bool Execute()
        {
            SnToolPath = PredefinedSnToolPath;

            if( string.IsNullOrEmpty(SnToolPath) && Directory.Exists(FrameworkSdkPath) )
            {
                string[] temp = Directory.GetFiles(FrameworkSdkPath, "sn.exe", SearchOption.AllDirectories);
                if (temp != null && temp.Length > 0)
                    SnToolPath = temp[0];
            }

            if (string.IsNullOrEmpty(SnToolPath))
            {
                Log.LogError("Cannot find sn.exe");
                return false;
            }

            return true;
        }
    }
}
