using System.Linq;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using Vitevic.Foundation.Extensions;

namespace Vitevic.Vsx.ToolWindow
{
    public class VsxToolWindow<T> : ToolWindowPane, IBaseVsxObject
        where T : Control, new()
    {
        public new BasePackage Package // Replace ToolWindowPane.Package with IBaseVsxObject one
        {
            get { return (BasePackage) base.Package; }
        }

        public VsxToolWindow() :
            base(null)
        {
            var attr = GetType().AttributesOfType<VsxToolWindowCaptionAttribute>().FirstOrDefault();
            if (attr != null)
            {
                Caption = BasePackage.GetPackageResourceString(attr.Caption, GetType().Assembly);
                BitmapResourceID = attr.BitmapResourseID;
                BitmapIndex = attr.BitmapIndex;
            }

            base.Content = new T();
        }
    }
}
