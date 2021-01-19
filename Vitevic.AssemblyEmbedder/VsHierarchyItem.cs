using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Vitevic.AssemblyEmbedder
{
    /// <summary>
    /// Represent a particular Hierarchy node
    /// based on nuget sources
    /// </summary>
    internal class VsHierarchyItem : IEquatable<VsHierarchyItem>
    {
       private readonly uint _vsitemid;
       private readonly IVsHierarchy _hierarchy;

       // ProcessItemDelegate return values: 
       //     0   continue, 
       //     1   don't recurse into, 
       //    -1   stop
       internal delegate int ProcessItemDelegate(VsHierarchyItem item, object callerObject, out object newCallerObject);
    
       internal VsHierarchyItem(IVsHierarchy hierarchy, uint id)
       {
           Debug.Assert(hierarchy != null);
           this._vsitemid = id;
           this._hierarchy = hierarchy;
       }
    
       internal VsHierarchyItem(IVsHierarchy hierarchy) : this(hierarchy, VSConstants.VSITEMID_ROOT)
       {
       }
    
       internal uint VsItemID
       {
           get { return this._vsitemid; }
       }

       internal IVsUIHierarchy UIHierarchy
       {
           get
           {
               return this._hierarchy as IVsUIHierarchy;
           }
       }    
    
       internal bool IsExpandable()
       {
           object o = GetProperty(__VSHPROPID.VSHPROPID_Expandable);
           if (o is bool)
           {
               return (bool)o;
           }
           return (o is int) && (int)o != 0;
       }
    
       internal String GetName()
       {
           return (String)GetProperty(__VSHPROPID.VSHPROPID_Name);
       }

       internal String GetCanonicalName()
       {
           string canonicalName = null;
           ErrorHandler.ThrowOnFailure(this._hierarchy.GetCanonicalName(this._vsitemid, out canonicalName), VSConstants.E_NOTIMPL);

           return canonicalName;
       }

       internal object GetBrowseObject()
       {
           return GetProperty(__VSHPROPID.VSHPROPID_BrowseObject);
       }


       private object GetProperty(__VSHPROPID propid)
       {
           return GetProperty(this._vsitemid, (int)propid);
       }
    
       private object GetProperty(uint itemid, int propid)
       {
           try
           {
               object o = null;
               if (this._hierarchy != null)
               {
                   this._hierarchy.GetProperty(itemid, propid, out o);
               }
    
               return o;
           }
           catch (Exception)
           {
               return null;
           }
       }
    
       internal int WalkDepthFirst(bool fVisible, ProcessItemDelegate processCallback, object callerObject)
       {
           // 
           // TODO Need to see what to do if this is a sub project
           //
           // Get the node type
           // Guid nodeGuid;
           // if (hier.GetGuidProperty(_vsitemid, (int)__VSHPROPID.VSHPROPID_TypeGuid, out nodeGuid) != 0)
    
           if (processCallback == null)
           {
               return 0;
           }
    
           object newCallerObject;
           int processReturn = processCallback(this, callerObject, out newCallerObject);
           if (processReturn != 0)
           {
               // Callback says to skip (1) or stop (-1)
               return processReturn;
           }
    
           // The process callback can change the caller object. If not we just use the one originally
           // passed in.
           if (newCallerObject == null)
           {
               newCallerObject = callerObject;
           }
    
           // Walk children if there are any
           if (IsExpandable())
           {
               VsHierarchyItem child = GetFirstChild(fVisible);
               while (child != null)
               {
                   object isNonMemberItemValue = child.GetProperty(__VSHPROPID.VSHPROPID_IsNonMemberItem);
                   // Some project systems (e.g. F#) don't support querying for the VSHPROPID_IsNonMemberItem property. 
                   // In that case, we treat this child as belonging to the project
                   bool isMemberOfProject = isNonMemberItemValue == null || (bool)isNonMemberItemValue == false;
                   if (isMemberOfProject)
                   {
                       int returnVal = child.WalkDepthFirst(fVisible, processCallback, newCallerObject);
                       if (returnVal == -1)
                       {
                           return returnVal;
                       }
                   }
                   child = child.GetNextSibling(fVisible);
               }
           }
           return 0;
       }
    
       internal VsHierarchyItem GetNextSibling(bool fVisible)
       {
           uint childId = GetNextSiblingId(fVisible);
           if (childId != VSConstants.VSITEMID_NIL)
           {
               return new VsHierarchyItem(this._hierarchy, childId);
           }
           return null;
       }
    
       internal uint GetNextSiblingId(bool fVisible)
       {
           object o = GetProperty(fVisible ? __VSHPROPID.VSHPROPID_NextVisibleSibling : __VSHPROPID.VSHPROPID_NextSibling);
    
           if (o is int)
           {
               return unchecked((uint)((int)o));
           }
           else if (o is uint)
           {
               return (uint)o;
           }
           else
           {
               return VSConstants.VSITEMID_NIL;
           }
       }
    
       internal VsHierarchyItem GetFirstChild(bool fVisible)
       {
           uint childId = GetFirstChildId(fVisible);
           if (childId != VSConstants.VSITEMID_NIL)
           {
               return new VsHierarchyItem(this._hierarchy, childId);
           }
           return null;
       }
    
       internal uint GetFirstChildId(bool fVisible)
       {
           object o = GetProperty(fVisible ? __VSHPROPID.VSHPROPID_FirstVisibleChild : __VSHPROPID.VSHPROPID_FirstChild);
    
           if (o is int)
           {
               return unchecked((uint)((int)o));
           }
           else if (o is uint)
           {
               return (uint)o;
           }
           else
           {
               return VSConstants.VSITEMID_NIL;
           }
       }
    
       public bool Equals(VsHierarchyItem other)
       {
           return VsItemID == other.VsItemID;
       }
    
       public override bool Equals(object obj)
       {
           var other = obj as VsHierarchyItem;
           return other != null && Equals(other);
       }
    
       public override int GetHashCode()
       {
           return this._vsitemid.GetHashCode();
       }
    }
}
