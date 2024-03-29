﻿#nullable enable

namespace CodeCleanupOnSave;

using Howmessy.VSExtension;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

using System;
using System.ComponentModel.Composition;

using static Howmessy.Shared.Logging;

[Export(typeof(ICommandHandler))]
[Name(nameof(SaveCommandHandler))]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
public class SaveCommandHandler : ICommandHandler<SaveCommandArgs>
{
    public string DisplayName => nameof(SaveCommandHandler);

    public bool ExecuteCommand(SaveCommandArgs args, CommandExecutionContext ctx)
    {
        try
        {
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                // CodeLenses usually only live as long as the document is open so we just refresh all the connected ones.
                await CodeLensConnectionHandler.RefreshAllCodeLensDataPoints());

            return true;
        }
        catch (Exception ex)
        {
            LogVS(ex);
            throw;
        }
    }

    public CommandState GetCommandState(SaveCommandArgs args) => CommandState.Available;
}
