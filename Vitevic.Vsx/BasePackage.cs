using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Vitevic.Foundation.Extensions;
using Vitevic.Vsx.Command;
using Vitevic.Vsx.Service;

namespace Vitevic.Vsx
{
    /// <summary>
    /// There are two great classes to use in Packet attributes: 
    ///     Microsoft.VisualStudio.Shell.Interop.ToolWindowGuids & 
    ///     Microsoft.VisualStudio.Shell.Interop
    /// 
    /// Example:
    ///     [ProvideToolWindow(typeof(MyToolWindow), Window = ToolWindowGuids.Outputwindow)]
    ///     [ProvideAutoLoad(UIContextGuids.NoSolution)]
    ///     class MyPackage : BasePackage
    ///     {}
    /// 
    /// Both referenced from $(VSSDK)\VisualStudioIntegration\Common\Assemblies\v2.0\Microsoft.VisualStudio.Shell.Interop.8.0.dll
    /// </summary>
    [PackageRegistration] //  RegPkg.exe will look for additional attributes.
    [ComVisible(true)]
    public abstract class BasePackage : Package, IVsShellPropertyEvents
    {
        public bool IsAutoloaded { get; private set; }
        public string ProductName { get; private set; }
        public EnvDTE.DTE IDE { get; private set; }

        private List<VsxCommand> _commands = new List<VsxCommand>();
        public ReadOnlyCollection<VsxCommand> Commands { get; private set; }

        protected BasePackage()
        {
            IsAutoloaded = false;

            var autoLoadAttr = GetType().AttributesOfType<ProvideAutoLoadAttribute>().FirstOrDefault();
            if (autoLoadAttr != null && (0 == string.Compare(autoLoadAttr.LoadGuid.ToString("B"), UIContextGuids.NoSolution, StringComparison.CurrentCultureIgnoreCase)) )
                IsAutoloaded = true;

            var productNameAttr = GetType().AttributesOfType<InstalledProductRegistrationAttribute>().FirstOrDefault();
            if (productNameAttr != null)
            {
                ProductName = GetPackageResourceString(productNameAttr.ProductName, GetType().Assembly);
            }

            Commands = new ReadOnlyCollection<VsxCommand>(this._commands);
        }

        public static string GetPackageResourceString(string str, Assembly resourceAssembly)
        {
            if (string.IsNullOrEmpty(str) || str[0] != '#' || !char.IsDigit(str, 1))
                return str;

            try
            {
                var rm = new ResourceManager("VSPackage", resourceAssembly);
                string resId = str.Substring(1);
                return rm.GetString(resId);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Can not find package resource string '{0}' in '{1}' assembly: {2}", str, resourceAssembly.FullName, e.Message);
            }

            return string.Empty;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }
        public TInterface GetService<T, TInterface>()
        {
            return (TInterface)GetService(typeof(T));
        }

        public T DialogPage<T>() where T : DialogPage
        {
            return (T)GetDialogPage(typeof(T));
        }

        public static T GetGlobalService<T>()
        {
            return (T)GetGlobalService(typeof(T));
        }
        public static TInterface GetGlobalService<T, TInterface>()
        {
            return (TInterface)GetGlobalService(typeof(T));
        }

        public DialogResult ShowMessage(string text, string title = null,
                                        OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                        OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                        OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO,
                                        string helpFilePath = null, uint helpTopic = 0,
                                        bool systemModal = false)
        {
            Debug.Assert( buttons != OLEMSGBUTTON.OLEMSGBUTTON_YESALLNOCANCEL, "The shell does not support this" );

            var uiShell = GetService<SVsUIShell, IVsUIShell>();

            if (title == null)
                title = ProductName;

            var clsid = Guid.Empty;
            int result;
            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       title,
                       text,
                       helpFilePath,
                       helpTopic,
                       buttons,
                       defaultButton,
                       icon,
                       systemModal?1:0,
                       out result));

            var dialogResult = DialogResult.Fail;
            if (typeof (DialogResult).IsEnumDefined(result))
                dialogResult = (DialogResult) result;

            return dialogResult;
        }

        public void ShowToolwindow(Type type, int id, bool create)
        {
            var toolWindowPane = FindToolWindow(type, id, create);

            if (toolWindowPane != null)
            {
                var windowFrame = (IVsWindowFrame)toolWindowPane.Frame;
                if (windowFrame != null)
                    ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            else
                ErrorHandler.ThrowOnFailure(VSConstants.E_FAIL);
        }

        /// <summary>
        /// Called when the VSPackage is loaded by Visual Studio and the shell is not Zombied.
        /// Do not add global services here! Override BeforePackageInit instead
        /// </summary>
        protected virtual void PackageInitialize()
        {
        }
        // called before services are proffed
        protected virtual void BeforePackageInit()
        {
        }

        /// <summary>
        /// Called during package dispose
        /// </summary>
        protected virtual void PackageCleanup()
        {
        }

        #region internals

        protected sealed override void Initialize()
        {
            if( IsAutoloaded )
            {
                // Automatically loaded packages has to wait while the Shell gets active
                var shellService = GetService<SVsShell,IVsShell>();
                if( shellService != null )
                {
                    ErrorHandler.ThrowOnFailure(
                      shellService.AdviseShellPropertyChanges(this, out this._eventSinkCookie));
                }
            }
            else
            {
                InitializeInternal();
            }
        }

        /// <remarks>
        /// This method will be called by Visual Studio in reponse to a package close (disposing will be true in this case).
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Debug.Assert(this._eventSinkCookie == 0, "Dispose before the shell activates...");
                PackageCleanup();
                if (this._iocContainer != null)
                {
                    this._iocContainer.Dispose();
                    this._iocContainer = null;
                }
                if (this._packageAssemblyCatalog != null)
                {
                    this._packageAssemblyCatalog.Dispose();
                    this._packageAssemblyCatalog = null;
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeInternal()
        {
            // Setup helper properties bafore any object can query for one
            IDE = GetService<EnvDTE.DTE>();

            BeforePackageInit(); // give package dev a chance

            AddVsxObjects();

            base.Initialize(); // will proffer services

            PackageInitialize();
        }

        private AssemblyCatalog _packageAssemblyCatalog;
        private CompositionContainer _iocContainer;

        [ImportMany(typeof(IVsxService))]
        private IEnumerable<Lazy<IVsxService, IVsxServiceMetadata>> _serviceDefinitions = null;

        [ImportMany(typeof(IVsxCommand))]
        private IEnumerable<Lazy<IVsxCommand, IVsxCommandMetadata>> _commandDefinitions = null;

        [ImportMany(VsxCommandActionAttribute.VsxCommandExecuteContractName)]
        private IEnumerable<Lazy<Action<IVsxCommand>, IVsxCommandMetadata>> _commandActionDefinitions = null;

        private void AddVsxObjects()
        {
            try
            {
                //  TODO: add thisCatalog & container to disposing list
                this._packageAssemblyCatalog = new AssemblyCatalog(GetType().Assembly);
                this._iocContainer = new CompositionContainer(this._packageAssemblyCatalog);
                this._iocContainer.SatisfyImportsOnce(this);
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }

            var thisPackageId = GetType().GUID;
            Debug.WriteLine("Processing '{0}' Vsx Package", thisPackageId);
            Debug.WriteLine("\tProductName: '{0}', AutoLoaded: '{1}'", ProductName, IsAutoloaded);
            Debug.WriteLine("Vsx objects:");

            AddServices(thisPackageId);
            AddCommands(thisPackageId);
            AddCommandActions(thisPackageId);
        }

        private void AddCommandActions(Guid thisPackageId)
        {
            foreach( var action in this._commandActionDefinitions )
            {
                Debug.WriteLine("\tFound action. Group: '{0}', Id: '{1}'.", action.Metadata.CommandGroupId, action.Metadata.CommandId);
                Debug.WriteLineIf(action.Value.Target != null, "\t\tWARNIG: not a static method!");

                if( ShouldSkipCommandOrAction(action, thisPackageId) )
                    continue;

                var commandGroup = CommandGroupId(action);
                if (commandGroup == Guid.Empty)
                {
                    Debug.WriteLine("\t!!! VsxCommand with invalid CommandGroupId. Group: '{0}', Id: '{1}'", action.Metadata.CommandGroupId, action.Metadata.CommandId);
                    continue;
                }

                CreateVsxAction(action, commandGroup);
                Debug.WriteLine("\t\tRegistered as '{0}':'{1}'", commandGroup, action.Metadata.CommandId);
            }
        }

        private void CreateVsxAction(Lazy<Action<IVsxCommand>, IVsxCommandMetadata> obj, Guid commandGroup)
        {
            var commandId = new CommandID(commandGroup, (int)obj.Metadata.CommandId);
            var action = new VsxAction(this, commandId, obj.Value);
            action.Register(obj.Metadata.Flags);

            this._commands.Add(action);
        }

        #region Service registration

        private void AddServices(Guid packageId)
        {
            foreach (var service in this._serviceDefinitions)
            {
                Debug.WriteLine("\tFound '{0}' VsxService from '{1}' Packet. Flags: {2}.", service.Metadata.ServiceType.Name,
                                service.Metadata.PackageId, service.Metadata.Flags);

                if( ShouldSkipService(service, packageId) )
                    continue;

                var container = (IServiceContainer) this;
                var serviceType = service.Metadata.ServiceType;
                bool promote = service.Metadata.Flags.HasFlag(ServiceFlags.Global);
                bool autoCreate = service.Metadata.Flags.HasFlag(ServiceFlags.AutoCreate);

                if (autoCreate)
                    container.AddService(serviceType, CreateVsxServiceObject(service), promote);
                else
                    container.AddService(serviceType, OnCreateServiceInstance, promote);
            }
        }

        private object OnCreateServiceInstance(IServiceContainer container, Type serviceType)
        {
            // we know what to do with only this package services
            if (container != this)
                return null;

            var obj = this._serviceDefinitions.FirstOrDefault(x => x.Metadata.ServiceType == serviceType);

            return obj == null ? null : CreateVsxServiceObject(obj);
        }

        private object CreateVsxServiceObject(Lazy<IVsxService, IVsxServiceMetadata> obj)
        {
            var service = (VsxService)obj.Value;
            service.Package = this;

            return service;
        }

        private static bool ShouldSkipService(Lazy<IVsxService, IVsxServiceMetadata> service, Guid packageId)
        {
            bool manualBind = service.Metadata.Flags.HasFlag(ServiceFlags.ManualBind);
            if (manualBind)
                return true; // auto binding not required

            Guid servicePackageId;
            if (Guid.TryParse(service.Metadata.PackageId, out servicePackageId))
            {
                if (servicePackageId != packageId)
                    return true; // the service belongs to another packet
            }

            return false;
        }

        #endregion Service registration

        #region Commands registration

        private void AddCommands(Guid packageId)
        {
            foreach (var command in this._commandDefinitions)
            {
                Debug.WriteLine("\tFound VsxCommand. Group: '{0}', Id: '{1}', Package: '{2}'", command.Metadata.CommandGroupId, command.Metadata.CommandId, command.Metadata.PackageId);

                if( ShouldSkipCommandOrAction(command, packageId))
                    continue;

                var commandGroup = CommandGroupId(command);
                if (commandGroup == Guid.Empty)
                {
                    Debug.WriteLine("\t!!! VsxCommand with invalid CommandGroupId. Group: '{0}', Id: '{1}', Package: '{2}'", command.Metadata.CommandGroupId, command.Metadata.CommandId, command.Metadata.PackageId);
                    continue;
                }

                CreateVsxCommand(command, commandGroup);

                Debug.WriteLine("\t\tRegistered as '{0}':'{1}'", commandGroup, command.Metadata.CommandId);
            }
        }

        private void CreateVsxCommand(Lazy<IVsxCommand, IVsxCommandMetadata> obj, Guid commandGroup)
        {
            var command = (VsxCommand)obj.Value;
            command.CommandID = new CommandID(commandGroup, (int)obj.Metadata.CommandId);
            command.Package = this;
            command.Register( obj.Metadata.Flags );

            this._commands.Add(command);
        }

        private static bool ShouldSkipCommandOrAction<T>(Lazy<T, IVsxCommandMetadata> commandOrAction, Guid packageId)
        {
            if( commandOrAction.Metadata.Flags.HasFlag(CommandFlags.ManualBind) )
                return true;

            var commandPackageId = CommandPackageId(commandOrAction);

            if (commandPackageId != Guid.Empty && commandPackageId != packageId)
                return true;

            return false;
        }

        private static Guid CommandPackageId<T>(Lazy<T, IVsxCommandMetadata> command)
        {
            Guid packageId;
            if (Guid.TryParse(command.Metadata.PackageId, out packageId))
                return packageId;

            var parentCommandGroupAttr = FindParentCommandGroupAttr(command);
            if (parentCommandGroupAttr != null && Guid.TryParse(parentCommandGroupAttr.PackageId, out packageId))
                return packageId;

            return Guid.Empty;
        }

        private static Guid CommandGroupId<T>(Lazy<T, IVsxCommandMetadata> commandOrAction)
        {
            Guid groupId;
            if (Guid.TryParse(commandOrAction.Metadata.CommandGroupId, out groupId))
                return groupId;

            var parentCommandGroupAttr = FindParentCommandGroupAttr(commandOrAction);
            if (parentCommandGroupAttr != null && Guid.TryParse(parentCommandGroupAttr.CommandGroupId, out groupId) )
                return groupId;

            return Guid.Empty;
        }

        private static VsxCommandGroupAttribute FindParentCommandGroupAttr<T>(Lazy<T> commandOrAction)
        {
            var declaringType = commandOrAction.Value.GetType().DeclaringType;
            if (declaringType == null)
            {
                var action = commandOrAction.Value as Action<IVsxCommand>;
                if (action != null)
                {
                    declaringType = action.Method.DeclaringType;
                }
            }

            return declaringType != null ? declaringType.AttributesOfType<VsxCommandGroupAttribute>().FirstOrDefault() : null;
        }

        #endregion Commands registration

        #region IVsShellPropertyEvents

        private uint _eventSinkCookie;
        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object propValue)
        {
            // --- We handle the event if zombie state changes from true to false
            if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
            {
                if ((bool)propValue == false)
                {
                    InitializeInternal();

                    var shellService = GetService<SVsShell,IVsShell>();
                    if (shellService != null)
                    {
                        ErrorHandler.ThrowOnFailure( shellService.UnadviseShellPropertyChanges(this._eventSinkCookie) );
                    }

                    this._eventSinkCookie = 0;
                }
            }
            return VSConstants.S_OK;
        }

        #endregion IVsShellPropertyEvents

        #endregion internals
    }
}
