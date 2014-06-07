using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Vitevic.AssemblyEmbedder;

namespace Vitevic.AssemblyEmbedder
{
    [ComVisible(true)]
    public class ProjectReferenceExtender : AssemblyReferenceExtender
    {
        public ProjectReferenceExtender(EnvDTE.IExtenderSite extenderSite, int cookie, VSLangProj.Reference reference, IVsBuildPropertyStorage storage, uint itemId)
            : base(extenderSite, cookie, reference, storage, itemId)
        {
        }

        //[DisplayName("Embed Assembly Pdb")]
        //[Category(CategoryName)]
        //public bool EmbedAssemblyPdb
        //{
        //    get { return GetMsBuildBool(MsBuild.Attributes.EmbedAssemblyPdbName); }
        //    set { SetMsBuildBool(MsBuild.Attributes.EmbedAssemblyPdbName, value); }
        //}

        [DisplayName("Embed Dependensies")]
        [Category(CategoryName)]
        public bool EmbedAssemblyDependensies
        {
            get { return GetMsBuildBool(MsBuild.Attributes.EmbedAssemblyDependensiesName); }
            set { SetMsBuildBool(MsBuild.Attributes.EmbedAssemblyDependensiesName, value); }
        }
    }
}
