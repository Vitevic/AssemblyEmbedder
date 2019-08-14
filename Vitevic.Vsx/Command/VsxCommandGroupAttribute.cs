using System;

namespace Vitevic.Vsx.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VsxCommandGroupAttribute : Attribute
    {
        public String PackageId { get; set; }
        public String CommandGroupId { get; private set; }

        public VsxCommandGroupAttribute(String commangGroupId)
        {
            CommandGroupId = commangGroupId;
            PackageId = "";
        }
    }
}
