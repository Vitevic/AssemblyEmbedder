using System;
using System.ComponentModel.Composition;

namespace Vitevic.Vsx.Command
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class VsxCommandActionAttribute : ExportAttribute
    {
        public const String VsxCommandExecuteContractName = "commandExecuteMethod_{0A47CCAC-7CD2-42F1-95D8-5EFA6182D8AA}";
        
        public uint CommandId { get; private set; }
        public CommandFlags Flags { get; set; }
        public String CommandGroupId { get; private set; }
        public String PackageId { get; set; }

        public VsxCommandActionAttribute(String commandGroupId, uint commandId)
            : base(VsxCommandExecuteContractName)
        {
            CommandGroupId = commandGroupId;
            CommandId = commandId;
            Flags = CommandFlags.Default;
        }

        public VsxCommandActionAttribute(uint commandId)
            : this("", commandId)
        {}
    }
}