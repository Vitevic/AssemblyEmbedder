using System.ComponentModel.Design;

namespace Vitevic.Vsx.Command
{
    public interface IVsxCommand
    {
        CommandID CommandID { get; }
        BasePackage Package { get; }

        void Execute();
    }
}