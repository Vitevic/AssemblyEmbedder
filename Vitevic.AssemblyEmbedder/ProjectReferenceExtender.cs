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

        [DisplayName("Embed Dependencies")]
        [Category(CategoryName)]
        public bool EmbedAssemblyDependencies
        {
            get
            {
                return GetMsBuildBool(MsBuild.Attributes.EmbedAssemblyDependenciesName);
            }
            set
            {
                SetMsBuildBool(MsBuild.Attributes.EmbedAssemblyDependenciesName, value);
                if (value)
                    EmbedAssembly = true;
            }
        }

        protected override void OnSetEmbedAssembly(bool value)
        {
            base.OnSetEmbedAssembly(value);
            if (!value)
                EmbedAssemblyDependencies = false;
        }
    }
}
