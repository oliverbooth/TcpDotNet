using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace TcpDotNet;

/// <summary>
///     Represents a client on the TcpDotNet protocol.
/// </summary>
public sealed class ProtocolClient : BaseClientNode
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtocolClient" /> class.
    /// </summary>
    public ProtocolClient()
    {
        Aes.GenerateKey();
        Aes.GenerateIV();
    }

    /// <summary>
    ///     Establishes a connection to a remote host.
    /// </summary>
    /// <param name="host">The remote host to which this client should connect.</param>
    /// <param name="port">The remote port to which this client should connect.</param>
    /// <exception cref="ArgumentNullException"><paramref name="host" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="host" /> contains an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="port" /> is less than <see cref="IPEndPoint.MinPort" />. -or - <paramref name="port" /> is greater
    ///     than <see cref="IPEndPoint.MaxPort" />.
    /// </exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public Task ConnectAsync(string host, int port)
    {
        return ConnectAsync(new DnsEndPoint(host, port));
    }

    /// <summary>
    ///     Establishes a connection to a remote host.
    /// </summary>
    /// <param name="address">The remote <see cref="IPAddress" /> to which this client should connect.</param>
    /// <param name="port">The remote port to which this client should connect.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="port" /> is less than <see cref="IPEndPoint.MinPort" />. -or - <paramref name="port" /> is greater
    ///     than <see cref="IPEndPoint.MaxPort" />. -or- <paramref name="address" /> is less than 0 or greater than
    ///     0x00000000FFFFFFFF.
    /// </exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public Task ConnectAsync(IPAddress address, int port)
    {
        return ConnectAsync(new IPEndPoint(address, port));
    }

    /// <summary>
    ///     Establishes a connection to a remote host.
    /// </summary>
    /// <param name="remoteEP">An EndPoint that represents the remote device.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="remoteEP" /> is <see langword="null" />.</exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public async Task ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken = default)
    {
        if (remoteEP is null) throw new ArgumentNullException(nameof(remoteEP));
        BaseSocket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await Task.Run(() => BaseSocket.ConnectAsync(remoteEP), cancellationToken);
        IsConnected = true;
    }
}
