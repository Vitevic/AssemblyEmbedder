using System;

namespace Vitevic.Vsx.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VsxCommandGroupAttribute : Attribute
    {
        public string PackageId { get; set; }
        public string CommandGroupId { get; private set; }

        public VsxCommandGroupAttribute(string commangGroupId)
        {
            CommandGroupId = commangGroupId;
            PackageId = "";
        }
    }
}
