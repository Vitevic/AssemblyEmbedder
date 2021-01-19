using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace Vitevic.AssemblyEmbedder
{
    [ComVisible(true)]
    public class AssemblyReferenceExtender
    {
        protected const String CategoryName = "Advanced";

        private EnvDTE.IExtenderSite _extenderSite;
        private int _cookie;
        private IVsBuildPropertyStorage _storage;
        private uint _itemid;

        public VSLangProj.Reference Reference { get; private set; }

        public AssemblyReferenceExtender(EnvDTE.IExtenderSite extenderSite, int cookie, VSLangProj.Reference reference, IVsBuildPropertyStorage storage, uint itemId)
        {
            _extenderSite = extenderSite;
            _cookie = cookie;
            _storage = storage;
            _itemid = itemId;

            Reference = reference;
        }

        ~AssemblyReferenceExtender()
        {
            try
            {
                if( _extenderSite != null )
                    _extenderSite.NotifyDelete(_cookie);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error in ReferenceExtender finalizer: {0}", ex);
            }
        }

        [DisplayName("Embed Assembly")]
        [Category(CategoryName)]
        public bool EmbedAssembly
        {
            get { return GetMsBuildBool(Vitevic.AssemblyEmbedder.MsBuild.Attributes.EmbedAssemblyName); }
            set
            {
                SetMsBuildBool(Vitevic.AssemblyEmbedder.MsBuild.Attributes.EmbedAssemblyName, value);
                OnSetEmbedAssembly(value);
            }
        }

        protected virtual void OnSetEmbedAssembly(bool value)
        {
            if (value)
                Reference.CopyLocal = false;
        }

        protected bool GetMsBuildBool(String attributeName)
        {
            String value;
            _storage.GetItemAttribute(_itemid, attributeName, out value);

            bool result = Vitevic.AssemblyEmbedder.MsBuild.Attributes.IsTrue(value);

            return result;
        }

        protected void SetMsBuildBool(String attributeName, bool value)
        {
            string valueStr = value ? "true" : ""; // "" - will remove attribute
            ErrorHandler.ThrowOnFailure(_storage.SetItemAttribute(_itemid, attributeName, valueStr));
        }

    }
}
