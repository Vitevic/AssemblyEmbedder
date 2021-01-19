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
        public string CommandGroupId { get; set; }
        public string PackageId { get; set; }

        public VsxCommandAttribute(string commandGroupId, uint commandId)
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
