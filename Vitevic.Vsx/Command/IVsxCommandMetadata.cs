using System;

namespace Vitevic.Vsx.Command
{
    public interface IVsxCommandMetadata
    {
        CommandFlags Flags { get; }
        String CommandGroupId { get; }
        uint CommandId { get; }
        String PackageId { get; }
    }
}