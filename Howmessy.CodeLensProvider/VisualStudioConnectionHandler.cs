#nullable enable

namespace Howmessy.CodeLensProvider;

using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using Howmessy.Shared;
using StreamJsonRpc;

public class VisualStudioConnectionHandler : IRemoteCodeLens, IDisposable
{
    private readonly CodeLensDataPoint owner;
    private readonly NamedPipeClientStream stream;
    private JsonRpc? rpc;

    public async static Task<VisualStudioConnectionHandler> CreateAsync(CodeLensDataPoint owner, int vspid)
    {
        var handler = new VisualStudioConnectionHandler(owner, vspid);
        await handler.ConnectAsync().Caf();
        return handler;
    }

    public VisualStudioConnectionHandler(CodeLensDataPoint owner, int vspid)
    {
        this.owner = owner;
        stream = new NamedPipeClientStream(
            serverName: ".",
            PipeName.Get(vspid),
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
    }

    public void Dispose() => stream.Dispose();

    public async Task ConnectAsync()
    {
        await stream.ConnectAsync().Caf();
        rpc = JsonRpc.Attach(stream, this);
        await rpc.InvokeAsync(nameof(IRemoteVisualStudio.RegisterCodeLensDataPoint), owner.id).Caf();
    }

    public void Refresh() => owner.Refresh();
}
