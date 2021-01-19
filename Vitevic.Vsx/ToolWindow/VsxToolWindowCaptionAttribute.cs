using System;

namespace Vitevic.Vsx.ToolWindow
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VsxToolWindowCaptionAttribute : Attribute
    {
        public string Caption { get; private set; }
        public int BitmapResourseID { get; set; }
        public int BitmapIndex { get; set; }

        public VsxToolWindowCaptionAttribute(string initialCaption, int bitmapResourceID, int bitmapIndex)
        {
            Caption = initialCaption;
            BitmapResourseID = bitmapResourceID;
            BitmapIndex = bitmapIndex;
        }

        public VsxToolWindowCaptionAttribute(string initialCaption, int bitmapResourceID)
            : this(initialCaption, bitmapResourceID, 1)
        {
        }

        public VsxToolWindowCaptionAttribute(string initialCaption)
            : this(initialCaption, -1, -1) // -1 got from ToolWindowPane ctor decompilation
        {
        }
    }
}
