using System.Diagnostics;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Vitevic.Foundation.Extensions;

namespace Vitevic.AssemblyEmbedder
{
    [ComVisible(true)]
    public sealed class ReferenceExtenderProvider : IExtenderProvider
    {
       public const string ExtenderName = "Vitevic.AssemblyEmbedderExtender";
       private DTE _ide;
       private ReferenceHierarchyHelper _helper;

       public ReferenceExtenderProvider(DTE ide, IVsSolution solution)
       {
           this._ide = ide;
           this._helper = new ReferenceHierarchyHelper(solution);
       }
    
       public bool CanExtend(string extenderCATID, string extenderName, object extendeeObject)
       {
           var reference = extendeeObject as VSLangProj.Reference;
           if( reference == null )
               return false;

           if (reference.Type == VSLangProj.prjReferenceType.prjReferenceTypeActiveX)
               return false;

           if (!extenderCATID.IsEqualNoCase(VSLangProj.PrjBrowseObjectCATID.prjCATIDCSharpReferenceBrowseObject))
               return false;

           if (!extenderName.IsEqualNoCase(ExtenderName))
               return false;

           return true;
       }
    
       public object GetExtender(string extenderCATID, string extenderName, object extendeeObject, IExtenderSite extenderSite, int cookie)
       {
           Debug.Assert( extenderCATID.IsEqualNoCase(VSLangProj.PrjBrowseObjectCATID.prjCATIDCSharpReferenceBrowseObject) );
           Debug.Assert( extenderName.IsEqualNoCase(ExtenderName) );

           var reference = (VSLangProj.Reference)extendeeObject;
           var hierarchyItem = this._helper.FindReferenceItemId(reference);
           if( hierarchyItem == null )
           {
               Debug.WriteLine("{0}: null hierarchy item for '{1}'", ExtenderName, reference.Name);
               return null;
           }

           if( reference.SourceProject != null )
               return new ProjectReferenceExtender(extenderSite, cookie, reference, (IVsBuildPropertyStorage)hierarchyItem.UIHierarchy, hierarchyItem.VsItemID);

           return new AssemblyReferenceExtender(extenderSite, cookie, reference, (IVsBuildPropertyStorage)hierarchyItem.UIHierarchy, hierarchyItem.VsItemID);
       }
    }

}
