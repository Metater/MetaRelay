using System.Net;
using System.Net.Sockets;

namespace MetaRelay.API;

public class Relay
{
    private static readonly IPEndPoint BlankEndpoint = new(IPAddress.Any, 0);

    public readonly int port;

    private readonly Socket socket = SocketPool.Get();
    private readonly Memory<byte> buffer = GC.AllocateArray<byte>(length: 65527, pinned: true).AsMemory();

    // Client -> This: Encapsulate data with some sort of id that is made from the client's endpoint (ip and port)
    // MetaRelay.Local -> This: Grab id that is made from the client's endpoint (ip and port) and use to send data to client
    // Is encapsulation needed or can the relay just make a new socket every time a client needs to talk to the godot server????


    // Problem: How does MetaRelay.Local -> This communication work? yes it does, if MetaRelay.Local sends stuff back to the clients socket
    // This -> MetaRelay.Local works with current scheme
    // Client -> This works
    // This -> Client works

    public Relay()
    {
        port = ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await socket.ReceiveFromAsync(buffer, SocketFlags.None, BlankEndpoint, cancellationToken);
                ReadOnlyMemory<byte> receivedData = buffer[..result.ReceivedBytes];
            }
            catch { }
        }
    }
}

public class Client
{
    private const double TimeoutSeconds = 10;

    private readonly InterlockedStatistic lastRelayTime = new(DateTime.UtcNow.ToBinary());
    private readonly Socket socket = SocketPool.Get();
    private readonly CancellationTokenSource cts = new();

    public bool IsTimedOut => (DateTime.UtcNow - DateTime.FromBinary(lastRelayTime.Value)).TotalSeconds > TimeoutSeconds;

    public void Relay()
    {
        lastRelayTime.Set(DateTime.UtcNow.ToBinary());
    }

    public void Run()
    {
        CancellationToken cancellationToken = cts.Token;
        while (!cancellationToken.IsCancellationRequested)
        {

        }
        SocketPool.Return(socket);
    }

    // TODO Call from relay, or whatever manages clients, on timeout and shutdown
    public void Cancel()
    {
        cts.Cancel();
    }
}