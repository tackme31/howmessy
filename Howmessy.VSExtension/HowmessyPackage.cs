#nullable enable

namespace Howmessy.VSExtension
{
    using Howmessy.Shared;
    using Howmessy.VSExtension.Options;

    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using static Howmessy.Shared.Logging;

    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [Guid(PackageGuids.PackageIdString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(DialogPageProvider.General), Vsix.Name, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.CognitiveComplexity), Vsix.Name, "Cognitive Complexity", 0, 0, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.CyclomaticComplexity), Vsix.Name, "Cyclomatic Complexity", 0, 0, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.MaintainabilityIndex), Vsix.Name, "Maintainability Index", 0, 0, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.LinesOfCode), Vsix.Name, "Lines of Code", 0, 0, true)]
    [ProvideProfile(typeof(DialogPageProvider.General), Vsix.Name, "General", 0, 0, true)]
    [ProvideProfile(typeof(DialogPageProvider.CognitiveComplexity), Vsix.Name, "Cognitive Complexity", 0, 0, true)]
    [ProvideProfile(typeof(DialogPageProvider.CyclomaticComplexity), Vsix.Name, "Cyclomatic Complexity", 0, 0, true)]
    [ProvideProfile(typeof(DialogPageProvider.MaintainabilityIndex), Vsix.Name, "Maintainability Index", 0, 0, true)]
    [ProvideProfile(typeof(DialogPageProvider.LinesOfCode), Vsix.Name, "Lines of Code", 0, 0, true)]
    public sealed class HowmessyPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken ct, IProgress<ServiceProgressData> progress)
        {
            try
            {
                await base.InitializeAsync(ct, progress);
                await JoinableTaskFactory.SwitchToMainThreadAsync(ct);
                _ = CodeLensConnectionHandler.AcceptCodeLensConnections();

                GeneralOptions.Saved += RefreshAllCodeLensDataPoints;
                CognitiveComplexityOptions.Saved += RefreshAllCodeLensDataPoints;
                CyclomaticComplexityOptions.Saved += RefreshAllCodeLensDataPoints;
                MaintainabilityIndexOptions.Saved += RefreshAllCodeLensDataPoints;
                LinesOfCodeOptions.Saved += RefreshAllCodeLensDataPoints;
            }
            catch (Exception ex)
            {
                LogVS(ex);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            GeneralOptions.Saved -= RefreshAllCodeLensDataPoints;
            CognitiveComplexityOptions.Saved -= RefreshAllCodeLensDataPoints;
            CyclomaticComplexityOptions.Saved -= RefreshAllCodeLensDataPoints;
            MaintainabilityIndexOptions.Saved -= RefreshAllCodeLensDataPoints;
            LinesOfCodeOptions.Saved -= RefreshAllCodeLensDataPoints;
            base.Dispose(disposing);
        }

        private void RefreshAllCodeLensDataPoints(object sender, EventArgs arg)
        {
            _ = CodeLensConnectionHandler.RefreshAllCodeLensDataPoints().Caf();
        }
    }
}
