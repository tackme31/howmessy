#nullable enable

namespace Howmessy.VSExtension;

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using static Howmessy.Shared.Logging;

[Export(typeof(ICommandHandler))]
[Name(nameof(HowmessySaveCommandHandler))]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
public class HowmessySaveCommandHandler : ICommandHandler<SaveCommandArgs>
{
    public string DisplayName => nameof(HowmessySaveCommandHandler);

    public bool ExecuteCommand(SaveCommandArgs args, CommandExecutionContext ctx)
    {
        LogCL();
        LogVS();

        try
        {
            if (args == null)
            {
                return false;
            }
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                // CodeLenses usually only live as long as the document is open so we just refresh all the connected ones.
                await CodeLensConnectionHandler.RefreshAllCodeLensDataPointsAsync());

            return true;
        }
        catch (Exception ex)
        {
            LogVS(ex);
            throw;
        }
    }

    public CommandState GetCommandState(SaveCommandArgs args)
    {
        LogCL();
        LogVS();
        return CommandState.Available;
    }
}
