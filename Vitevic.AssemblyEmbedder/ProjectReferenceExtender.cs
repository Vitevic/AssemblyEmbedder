using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;

namespace Vitevic.AssemblyEmbedder
{
    [ComVisible(true)]
    public class ProjectReferenceExtender : AssemblyReferenceExtender
    {
        const String EmbedAssemblyPdbName = "EmbedAssemblyPdb";

        public ProjectReferenceExtender(EnvDTE.IExtenderSite extenderSite, int cookie, VSLangProj.Reference reference, IVsBuildPropertyStorage storage, uint itemId)
            : base(extenderSite, cookie, reference, storage, itemId)
        {
        }

        [DisplayName("Embed Assembly Pdb")]
        [Category(CategoryName)]
        public bool EmbedAssemblyPdb
        {
            get { return GetMsBuildBool(EmbedAssemblyPdbName); }
            set { SetMsBuildBool(EmbedAssemblyPdbName, value); }
        }
    }
}
