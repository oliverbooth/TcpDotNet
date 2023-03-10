using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace TcpDotNet.Protocol;

/// <summary>
///     Represents a class that can write protocol-compliant messages to a stream.
/// </summary>
public sealed class ProtocolWriter : BinaryWriter
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtocolWriter" /> class.
    /// </summary>
    /// <param name="output">The input stream.</param>
    /// <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="output" /> is <see langword="null" />.</exception>
    public ProtocolWriter(Stream output) : base(output, Encoding.UTF8, false)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtocolReader" /> class.
    /// </summary>
    /// <param name="output">The input stream.</param>
    /// <param name="leaveOpen">
    ///     <see langword="true" /> to leave the stream open after the <see cref="BinaryReader" /> object is disposed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="output" /> is <see langword="null" />.</exception>
    public ProtocolWriter(Stream output, bool leaveOpen) : base(output, Encoding.UTF8, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override void Write(double value)
    {
        Span<byte> buffer = stackalloc byte[8];
        MemoryMarshal.TryWrite(buffer, ref value);

        if (BitConverter.IsLittleEndian) buffer.Reverse();
        Write(buffer);
    }

    /// <summary>
    ///     Writes a <see cref="Guid" /> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="guid">The <see cref="Guid" /> value to write.</param>
    public void Write(Guid guid)
    {
        Span<byte> buffer = stackalloc byte[16];
        guid.TryWriteBytes(buffer);
        Write(buffer);
    }

    /// <inheritdoc />
    public override void Write(short value)
    {
        base.Write(IPAddress.HostToNetworkOrder(value));
    }

    /// <inheritdoc />
    public override void Write(int value)
    {
        base.Write(IPAddress.HostToNetworkOrder(value));
    }

    /// <inheritdoc />
    public override void Write(long value)
    {
        base.Write(IPAddress.HostToNetworkOrder(value));
    }

    /// <summary>
    ///     Writes a <see cref="Quaternion" /> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="value">The <see cref="Quaternion" /> value to write.</param>
    public void Write(Quaternion value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
        Write(value.W);
    }

    /// <inheritdoc />
    public override void Write(float value)
    {
        Span<byte> buffer = stackalloc byte[4];
        MemoryMarshal.TryWrite(buffer, ref value);

        if (BitConverter.IsLittleEndian) buffer.Reverse();
        Write(buffer);
    }

    /// <inheritdoc />
    [CLSCompliant(false)]
    public override void Write(ushort value)
    {
        base.Write((ushort) IPAddress.HostToNetworkOrder((short) value));
    }

    /// <inheritdoc />
    [CLSCompliant(false)]
    public override void Write(uint value)
    {
        base.Write((uint) IPAddress.HostToNetworkOrder((int) value));
    }

    /// <inheritdoc />
    [CLSCompliant(false)]
    public override void Write(ulong value)
    {
        base.Write((ulong) IPAddress.HostToNetworkOrder((long) value));
    }

    /// <summary>
    ///     Writes a <see cref="Vector2" /> value to the current stream and advances the stream position by eight bytes.
    /// </summary>
    /// <param name="value">The <see cref="Vector2" /> value to write.</param>
    public void Write(Vector2 value)
    {
        Write(value.X);
        Write(value.Y);
    }

    /// <summary>
    ///     Writes a <see cref="Vector3" /> value to the current stream and advances the stream position by twelve bytes.
    /// </summary>
    /// <param name="value">The <see cref="Vector3" /> value to write.</param>
    public void Write(Vector3 value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
    }

    /// <summary>
    ///     Writes a <see cref="Vector4" /> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="value">The <see cref="Vector4" /> value to write.</param>
    public void Write(Vector4 value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
        Write(value.W);
    }

    /// <summary>
    ///     Writes a 32-bit integer in a compressed format.
    /// </summary>
    /// <param name="value">The 32-bit integer to be written.</param>
    public void Write7BitEncodedInt32(int value)
    {
        var uValue = (uint) value;

        // Write out an int 7 bits at a time. The high bit of the byte,
        // when on, tells reader to continue reading more bytes.
        //
        // Using the constants 0x7F and ~0x7F below offers smaller
        // codegen than using the constant 0x80.

        while (uValue > 0x7Fu)
        {
            Write((byte) (uValue | ~0x7Fu));
            uValue >>= 7;
        }

        Write((byte) uValue);
    }

    /// <summary>
    ///     Writes a 64-bit integer in a compressed format.
    /// </summary>
    /// <param name="value">The 64-bit integer to be written.</param>
    public void Write7BitEncodedInt64(long value)
    {
        var uValue = (ulong) value;

        // Write out an int 7 bits at a time. The high bit of the byte,
        // when on, tells reader to continue reading more bytes.
        //
        // Using the constants 0x7F and ~0x7F below offers smaller
        // codegen than using the constant 0x80.

        while (uValue > 0x7Fu)
        {
            Write((byte) ((uint) uValue | ~0x7Fu));
            uValue >>= 7;
        }

        Write((byte) uValue);
    }
}
