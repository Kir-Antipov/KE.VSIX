using System;
using System.Linq;
using KE.VSIX.Commands;
using System.Threading;
using System.Reflection;
using Microsoft.VisualStudio;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;

namespace KE.VSIX.Packages
{
    [ProvideBindingPath]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.CodeWindow_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.FirstLaunchSetup_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.BackgroundProjectLoad_string, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public class BasePackage<TPackage> : AsyncPackage where TPackage: BasePackage<TPackage>
    {
        #region Var
        public static Guid Guid { get; }
        public static PathContainer PathData { get; }
        public static TPackage Instance { get; private set; }
        #endregion

        #region Init
        static BasePackage()
        {
            Type t = typeof(TPackage);
            PathData = PackageHelper.Initialize<TPackage>(t.GetCustomAttribute<ProvideRoslynCompilersAttribute>()?.Provide ?? false);
            string guidStr = t.GetCustomAttribute<GuidAttribute>()?.Value;
            Guid = string.IsNullOrEmpty(guidStr) ? t.GUID : new Guid(guidStr);
        }
        #endregion

        #region Functions
        protected override async Task InitializeAsync(CancellationToken CancellationToken, IProgress<ServiceProgressData> Progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken);

            Instance = (TPackage)this;

            string commandsNamespace = $"{typeof(TPackage).Namespace}.Commands";
            IEnumerable<Type> commands =
                typeof(TPackage).Assembly
                    .GetTypes()
                    .Where(x => x.Namespace == commandsNamespace)
                    .Where(BaseCommand.IsCommand)
                    .Select(type => (type, set: type.GetProperty("CommandSet"), id: type.GetProperty("CommandID")))
                    .OrderBy(x => (Guid)x.set.GetValue(null))
                    .ThenBy(x => (int)x.id.GetValue(null))
                    .Select(x => x.type);

            foreach (Type cmdType in commands)
            {
                MethodInfo method = cmdType.GetMethod("InitializeAsync");
                if (method is null)
                    method = cmdType.GetMethod("Initialize");
                if (method is null || !method.IsStatic)
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                object[] arguments = new object[parameters.Length];
                if (parameters.Length == 0)
                    arguments = new object[0];
                else if (parameters.Length == 1 && typeof(AsyncPackage).IsAssignableFrom(parameters[0].ParameterType))
                    arguments = new object[] { this };
                else
                    continue;

                object result = method.Invoke(null, arguments);
                if (result is Task task)
                    await task;
            }
        }

        public TService GetService<TService>()
        {
            object service = GetService(typeof(TService));
            return service is null ? (default) : (TService)service;
        }

        public async Task<TService> GetServiceAsync<TService>()
        {
            object service = await GetServiceAsync(typeof(TService));
            return service is null ? (default) : (TService)service;
        }

        public TPage GetDialogPage<TPage>() where TPage : DialogPage => GetDialogPage(typeof(TPage)) as TPage;

        public void ShowToolWindow<TWindow>() where TWindow : ToolWindowPane 
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Type t = typeof(TWindow);
            IVsWindowFrame frame = FindToolWindow(t, 0, true)?.Frame as IVsWindowFrame;
            if (frame is null)
                throw new NotSupportedException($"Can't create {t.Name}'s tool window");
            ErrorHandler.ThrowOnFailure(frame.Show());
        }
        #endregion
    }
}
