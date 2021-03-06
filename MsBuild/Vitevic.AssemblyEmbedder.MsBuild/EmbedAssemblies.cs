﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build;
using Microsoft.Build.Utilities;
using System.IO;

namespace Vitevic.AssemblyEmbedder.MsBuild
{
    public class EmbedAssemblies : Task
    {
        [Required]
        public ITaskItem[] Referenses { get; set; }

        [Required]
        public ITaskItem[] Locals { get; set; }

        [Required]
        public ITaskItem[] TargetPath { get; set; }

        [Output]
        public ITaskItem[] EmbeddedFiles { get; private set; }

        public override bool Execute()
        {
            try
            {
                var embedder = new AssemblyEmbedder(Log, TargetPath, Referenses, Locals);
                EmbeddedFiles = embedder.Process();
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e, false);
                return false;
            }
            
            return true;
        }
    }

}
