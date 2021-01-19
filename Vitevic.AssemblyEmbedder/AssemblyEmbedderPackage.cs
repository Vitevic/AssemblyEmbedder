using System.Diagnostics;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Vitevic.Vsx;
using Vitevic.Vsx.Command;

namespace Vitevic.AssemblyEmbedder
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0.4", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.PkgGuidString)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [VsxCommandGroup(GuidList.CmdSetGuidString)]
    public sealed class AssemblyEmbedderPackage : BasePackage
    {
        private const int EmptyCookie = -1;
        private int _extenderRegistrationCSharpCookie = EmptyCookie;

        protected override void PackageInitialize()
        {
            var objectExtenders = GetService<ObjectExtenders>();
            Debug.Assert(objectExtenders != null);

            var solution = GetGlobalService<SVsSolution,IVsSolution>();
            var extenderProvider = new ReferenceExtenderProvider(IDE, solution);
            this._extenderRegistrationCSharpCookie = objectExtenders.RegisterExtenderProvider(VSLangProj.PrjBrowseObjectCATID.prjCATIDCSharpReferenceBrowseObject,
                ReferenceExtenderProvider.ExtenderName, extenderProvider);
        }

        protected override void PackageCleanup()
        {
            var objectExtenders = GetService<ObjectExtenders>();

            if (this._extenderRegistrationCSharpCookie != EmptyCookie && objectExtenders != null)
            {
                objectExtenders.UnregisterExtenderProvider(this._extenderRegistrationCSharpCookie);
                this._extenderRegistrationCSharpCookie = EmptyCookie;
            }
        }

        [VsxCommandAction(PkgCmdIDList.cmdidEmbed)]
        private static void Embed(IVsxCommand cmd)
        {
            
        }
    }
}
