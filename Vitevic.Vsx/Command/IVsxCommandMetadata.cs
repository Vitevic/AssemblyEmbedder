using System;

namespace Vitevic.Vsx.Command
{
    public interface IVsxCommandMetadata
    {
        CommandFlags Flags { get; }
        string CommandGroupId { get; }
        uint CommandId { get; }
        string PackageId { get; }
    }
}
