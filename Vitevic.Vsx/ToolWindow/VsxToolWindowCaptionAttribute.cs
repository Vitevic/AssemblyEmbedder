using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Vitevic.Vsx.ToolWindow
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VsxToolWindowCaptionAttribute : Attribute
    {
        public String Caption { get; private set; }
        public int BitmapResourseID { get; set; }
        public int BitmapIndex { get; set; }

        public VsxToolWindowCaptionAttribute(String initialCaption, int bitmapResourceID, int bitmapIndex)
        {
            Caption = initialCaption;
            BitmapResourseID = bitmapResourceID;
            BitmapIndex = bitmapIndex;
        }

        public VsxToolWindowCaptionAttribute(String initialCaption, int bitmapResourceID)
            : this(initialCaption, bitmapResourceID, 1)
        {
        }

        public VsxToolWindowCaptionAttribute(String initialCaption)
            : this(initialCaption, -1, -1) // -1 got from ToolWindowPane ctor decompilation
        {
        }
    }
}