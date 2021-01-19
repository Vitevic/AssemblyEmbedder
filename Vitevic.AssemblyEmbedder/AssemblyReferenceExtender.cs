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
            this._extenderSite = extenderSite;
            this._cookie = cookie;
            this._storage = storage;
            this._itemid = itemId;

            Reference = reference;
        }

        ~AssemblyReferenceExtender()
        {
            try
            {
                if(this._extenderSite != null ) this._extenderSite.NotifyDelete(this._cookie);
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
            get { return GetMsBuildBool(MsBuild.Attributes.EmbedAssemblyName); }
            set
            {
                SetMsBuildBool(MsBuild.Attributes.EmbedAssemblyName, value);
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
            this._storage.GetItemAttribute(this._itemid, attributeName, out value);

            bool result = MsBuild.Attributes.IsTrue(value);

            return result;
        }

        protected void SetMsBuildBool(String attributeName, bool value)
        {
            string valueStr = value ? "true" : ""; // "" - will remove attribute
            ErrorHandler.ThrowOnFailure(this._storage.SetItemAttribute(this._itemid, attributeName, valueStr));
        }

    }
}
