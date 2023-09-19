using System.Net;
using System.Net.Sockets;

namespace MetaRelay.API;

public static class SocketPool
{
    private const double MinimumPooledSocketAgeSeconds = 100;
    private static readonly IPEndPoint BlankEndpoint = new(IPAddress.Any, 0);

    private static readonly object poolLock = new();
    private static readonly Queue<PooledSocket> pool = new();

    public static Socket Get()
    {
        lock (poolLock)
        {
            if (pool.Count == 0)
            {
                return New();
            }
            else
            {
                DateTime firstPoolTime = pool.Peek().PoolTime;
                if ((DateTime.UtcNow - firstPoolTime).TotalSeconds > MinimumPooledSocketAgeSeconds)
                {
                    return pool.Dequeue().Socket;
                }

                return New();
            }
        }
    }

    public static void Return(Socket socket)
    {
        lock (poolLock)
        {
            pool.Enqueue(new(socket, DateTime.UtcNow));
        }
    }

    private static Socket New()
    {
        Socket socket = new(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)
        {
            DualMode = true,
        };

        socket.Bind(BlankEndpoint);
        return socket;
    }

    private record struct PooledSocket(Socket Socket, DateTime PoolTime);
}