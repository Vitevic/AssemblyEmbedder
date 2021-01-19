using System;

namespace Vitevic.Vsx.Service
{
    public interface IVsxServiceMetadata
    {
        Type ServiceType { get; }
        ServiceFlags Flags { get; }
        string PackageId { get; }
    }
}
