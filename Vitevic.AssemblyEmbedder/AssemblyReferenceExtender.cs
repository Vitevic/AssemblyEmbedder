using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using Vitevic.Foundation.Extensions;
using Microsoft.VisualStudio;

namespace Vitevic.AssemblyEmbedder
{
    [ComVisible(true)]
    public class AssemblyReferenceExtender
    {
        protected const String CategoryName = "Advanced";
        const String EmbedAssemblyName = "EmbedAssembly";
        const String EmbedAssemblyDependensiesName = "EmbedAssemblyDependensies";

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
            get { return GetMsBuildBool(EmbedAssemblyName); }
            set
            {
                SetMsBuildBool(EmbedAssemblyName, value);
                if (value)
                    Reference.CopyLocal = false;
            }
        }

        [DisplayName("Embed Dependensies")]
        [Category(CategoryName)]
        public bool EmbedAssemblyDependensies
        {
            get { return GetMsBuildBool(EmbedAssemblyDependensiesName); }
            set { SetMsBuildBool(EmbedAssemblyDependensiesName, value); }
        }

        protected bool GetMsBuildBool(String attributeName)
        {
            String value;
            _storage.GetItemAttribute(_itemid, attributeName, out value);

            bool result = false;
            if (!String.IsNullOrEmpty(value))
            {
                result = value.IsEqualNoCase("true") || value == "1";
            }

            return result;
        }

        protected void SetMsBuildBool(String attributeName, bool value)
        {
            var valueStr = value ? "true" : ""; // "" - will remove attribute
            ErrorHandler.ThrowOnFailure(_storage.SetItemAttribute(_itemid, attributeName, valueStr));
        }

    }
}
