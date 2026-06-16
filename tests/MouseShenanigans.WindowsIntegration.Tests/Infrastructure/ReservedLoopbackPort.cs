using System.Net;
using System.Net.Sockets;

namespace MouseShenanigans.WindowsIntegration.Tests.Infrastructure;

internal sealed class ReservedLoopbackPort : IDisposable
{
    private readonly TcpListener listener;
    private bool disposed;

    private ReservedLoopbackPort(TcpListener listener)
    {
        this.listener = listener;
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        BaseUri = new Uri($"http://127.0.0.1:{Port}");
    }

    public int Port { get; }

    public Uri BaseUri { get; }

    public static ReservedLoopbackPort Reserve()
    {
        var listener = new TcpListener(IPAddress.Loopback, port: 0);
        listener.Start();
        return new ReservedLoopbackPort(listener);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        listener.Stop();
        disposed = true;
    }
}
