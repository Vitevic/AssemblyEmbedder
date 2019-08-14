using System;
using System.ComponentModel.Composition;

namespace Vitevic.Vsx.Service
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VsxServiceAttribute : ExportAttribute
    {
        public Type ServiceType { get; private set; }
        public String PackageId { get; set; }
        public ServiceFlags Flags { get; set; }

        public VsxServiceAttribute(Type serviceType)
            : base(typeof(IVsxService))
        {
            ServiceType = serviceType;
            Flags = ServiceFlags.Default;
            PackageId = "";
        }
    }
}
