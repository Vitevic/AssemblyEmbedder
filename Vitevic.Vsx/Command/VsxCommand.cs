using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace Vitevic.Vsx.Command
{
    public abstract class VsxCommand : BaseVsxObject, IVsxCommand
    {
        public CommandID CommandID { get; internal set; }
        public MenuCommand MenuCommmand{ get; private set; }

        public abstract void Execute();
        protected virtual void BeforeQueryStatus(OleMenuCommand cmd)
        {
        }

        internal protected virtual void Register(CommandFlags flags)
        {
            var mcs = Package.GetService<IMenuCommandService, OleMenuCommandService>();
            if( flags.HasFlag(CommandFlags.OleCommand) )
            {
                var oleMenuCommand = new OleMenuCommand(CommandCallback, CommandID);
                if (!flags.HasFlag(CommandFlags.NoBeforeQuery))
                {
                    oleMenuCommand.BeforeQueryStatus += (BeforeQueryStatusCallback);
                }

                MenuCommmand = oleMenuCommand;
            }
            else
            {
                MenuCommmand = new MenuCommand(CommandCallback, CommandID);
            }

            mcs.AddCommand(MenuCommmand);
        }

        private void BeforeQueryStatusCallback(object sender, EventArgs e)
        {
            BeforeQueryStatus((OleMenuCommand)sender);
        }

        private void CommandCallback(object sender, EventArgs e)
        {
            Execute();
        }
    }
}