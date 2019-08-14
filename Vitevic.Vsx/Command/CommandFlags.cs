using System;

namespace Vitevic.Vsx.Command
{
    [Flags]
    public enum CommandFlags
    {
        Default = 0,
        ManualBind = 0x01,
        OleCommand = 0x02,
        NoBeforeQuery = 0x04,
    }
}