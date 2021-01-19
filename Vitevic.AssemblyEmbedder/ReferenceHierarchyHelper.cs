using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Vitevic.AssemblyEmbedder
{
    class ReferenceHierarchyHelper
    {
        public IVsSolution Solution { get; private set; }

        public ReferenceHierarchyHelper(IVsSolution solution)
        {
            Solution = solution;
        }

        /// <returns>may return null</returns>
        public VsHierarchyItem FindReferenceItemId(VSLangProj.Reference reference)
        {
            string projectName = reference.ContainingProject.UniqueName;
            IVsHierarchy hierarchy;
            ErrorHandler.ThrowOnFailure(Solution.GetProjectOfUniqueName(projectName, out hierarchy));
            var rootItem = new VsHierarchyItem(hierarchy);
            var referencesNode = FindReferencesFolderNode(rootItem);
            var child = referencesNode.GetFirstChild(false);
            while (child != null)
            {
                var browseObject = child.GetBrowseObject();
                if (browseObject == reference)
                {
                    return child;
                }

                child = child.GetNextSibling(false);
            }

            return null;
        }

        /// <returns>not null, or throws</returns>
        private VsHierarchyItem FindReferencesFolderNode(VsHierarchyItem rootItem)
        {
            var propertiesItem = rootItem.GetFirstChild(true);
            if( propertiesItem != null )
            {
                var referensesItem = propertiesItem.GetNextSibling(true);
                if( IsReferensesFolderItem(referensesItem) )
                    return referensesItem;
            }

            VsHierarchyItem result = null;
            // have to traverse full hierarchy
            rootItem.WalkDepthFirst(true, (VsHierarchyItem currentItem, object obj, out object newObj) => {
                newObj = null;

                if( IsReferensesFolderItem(currentItem) )
                {
                    result = currentItem;
                    return -1;
                }

                return 0;
            }, null);

            if (result == null)
                throw new InvalidOperationException("FindReferencesFolderNode");

            return result;
        }

        static bool IsReferensesFolderItem(VsHierarchyItem item)
        {
            if (item == null)
                return false;

            string name = item.GetName();
            string canonicalName = item.GetCanonicalName();
            var brObj = item.GetBrowseObject();

            return canonicalName == null && brObj == null;
        }
    }
}
