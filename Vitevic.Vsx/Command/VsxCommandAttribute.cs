using System;
using System.ComponentModel.Composition;

namespace Vitevic.Vsx.Command
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VsxCommandAttribute : ExportAttribute
    {
        public uint CommandId { get; private set; }
        public CommandFlags Flags { get; set; }
        public String CommandGroupId { get; set; }
        public String PackageId { get; set; }

        public VsxCommandAttribute(String commandGroupId, uint commandId)
            : base(typeof(IVsxCommand))
        {
            CommandGroupId = commandGroupId;
            CommandId = commandId;
            PackageId = "";
            Flags = CommandFlags.Default;
        }

        public VsxCommandAttribute(uint commandId)
            : this("", commandId)
        {
        }
    }
}