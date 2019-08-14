using System;

namespace Vitevic.Vsx.Service
{
    [Flags]
    public enum ServiceFlags
    {
        Default = 0,
        AutoCreate = 0x1,
        Global = 0x2,
        ManualBind = 0x4
    }
}