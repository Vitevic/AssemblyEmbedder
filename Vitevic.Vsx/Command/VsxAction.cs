using System;
using System.ComponentModel.Design;

namespace Vitevic.Vsx.Command
{
    // TODO: add before query
    internal class VsxAction : VsxCommand
    {
        private readonly Action<IVsxCommand> _action;
        internal VsxAction(BasePackage package, CommandID commandId, Action<IVsxCommand> action)
        {
            Package = package;
            CommandID = commandId;
            this._action = action;
        }

        public override void Execute()
        {
            this._action.Invoke(this);
        }
    }
}
