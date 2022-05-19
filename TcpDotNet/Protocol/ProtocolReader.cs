using System.Net;
using System.Numerics;
using System.Text;

namespace TcpDotNet.Protocol;

/// <summary>
///     Represents a class that can read protocol-compliant messages from a stream.
/// </summary>
public sealed class ProtocolReader : BinaryReader
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtocolReader" /> class.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <exception cref="ArgumentException">
    ///     The stream does not support reading, is <see langword="null" />, or is already closed.
    /// </exception>
    public ProtocolReader(Stream input) : base(input, Encoding.UTF8, false)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtocolReader" /> class.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="leaveOpen">
    ///     <see langword="true" /> to leave the stream open after the <see cref="BinaryReader" /> object is disposed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     The stream does not support reading, is <see langword="null" />, or is already closed.
    /// </exception>
    public ProtocolReader(Stream input, bool leaveOpen) : base(input, Encoding.UTF8, leaveOpen)
    {
    }

    /// <summary>
    ///     Reads a <see cref="Guid" /> value from the current stream and advances the current position of the stream by sixteen
    ///     bytes.
    /// </summary>
    /// <returns>A <see cref="Guid" /> value.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
    public Guid ReadGuid()
    {
        Span<byte> buffer = stackalloc byte[16];
        int read = Read(buffer);
        if (read != 16) throw new EndOfStreamException();
        return new Guid(buffer);
    }

    /// <inheritdoc />
    public override short ReadInt16()
    {
        return IPAddress.NetworkToHostOrder(base.ReadInt16());
    }

    /// <inheritdoc />
    public override int ReadInt32()
    {
        return IPAddress.NetworkToHostOrder(base.ReadInt32());
    }

    /// <inheritdoc />
    public override long ReadInt64()
    {
        return IPAddress.NetworkToHostOrder(base.ReadInt64());
    }

    /// <inheritdoc />
    [CLSCompliant(false)]
    public override ushort ReadUInt16()
    {
        return (ushort) IPAddress.NetworkToHostOrder((short) base.ReadUInt16());
    }

    /// <inheritdoc />
    [CLSCompliant(false)]
    public override uint ReadUInt32()
    {
        return (uint) IPAddress.NetworkToHostOrder((int) base.ReadUInt32());
    }

    /// <inheritdoc />
    [CLSCompliant(false)]
    public override ulong ReadUInt64()
    {
        return (ulong) IPAddress.NetworkToHostOrder((long) base.ReadUInt64());
    }

    /// <summary>
    ///     Reads a <see cref="Quaternion" /> value from the current stream and advances the current position of the stream by
    ///     sixteen bytes.
    /// </summary>
    /// <returns>A <see cref="Quaternion" /> value read from the current stream.</returns>
    public Quaternion ReadQuaternion()
    {
        float x = ReadSingle();
        float y = ReadSingle();
        float z = ReadSingle();
        float w = ReadSingle();
        return new Quaternion(x, y, z, w);
    }

    /// <summary>
    ///     Reads in a 32-bit integer in compressed format.
    /// </summary>
    /// <returns>A 32-bit integer in compressed format.</returns>
    /// <exception cref="FormatException">The stream is corrupted.</exception>
    public int Read7BitEncodedInt32()
    {
        // Unlike writing, we can't delegate to the 64-bit read on
        // 64-bit platforms. The reason for this is that we want to
        // stop consuming bytes if we encounter an integer overflow.

        uint result = 0;
        byte byteReadJustNow;

        // Read the integer 7 bits at a time. The high bit
        // of the byte when on means to continue reading more bytes.
        //
        // There are two failure cases: we've read more than 5 bytes,
        // or the fifth byte is about to cause integer overflow.
        // This means that we can read the first 4 bytes without
        // worrying about integer overflow.

        const int maxBytesWithoutOverflow = 4;
        for (var shift = 0; shift < maxBytesWithoutOverflow * 7; shift += 7)
        {
            // ReadByte handles end of stream cases for us.
            byteReadJustNow = ReadByte();
            result |= (byteReadJustNow & 0x7Fu) << shift;

            if (byteReadJustNow <= 0x7Fu)
                return (int) result; // early exit
        }

        // Read the 5th byte. Since we already read 28 bits,
        // the value of this byte must fit within 4 bits (32 - 28),
        // and it must not have the high bit set.

        byteReadJustNow = ReadByte();
        if (byteReadJustNow > 0b_1111u)
            throw new FormatException();

        result |= (uint) byteReadJustNow << (maxBytesWithoutOverflow * 7);
        return (int) result;
    }

    /// <summary>
    ///     Reads in a 64-bit integer in compressed format.
    /// </summary>
    /// <returns>A 64-bit integer in compressed format.</returns>
    /// <exception cref="FormatException">The stream is corrupted.</exception>
    public long Read7BitEncodedInt64()
    {
        ulong result = 0;
        byte byteReadJustNow;

        // Read the integer 7 bits at a time. The high bit
        // of the byte when on means to continue reading more bytes.
        //
        // There are two failure cases: we've read more than 10 bytes,
        // or the tenth byte is about to cause integer overflow.
        // This means that we can read the first 9 bytes without
        // worrying about integer overflow.

        const int maxBytesWithoutOverflow = 9;
        for (var shift = 0; shift < maxBytesWithoutOverflow * 7; shift += 7)
        {
            // ReadByte handles end of stream cases for us.
            byteReadJustNow = ReadByte();
            result |= (byteReadJustNow & 0x7Ful) << shift;

            if (byteReadJustNow <= 0x7Fu)
                return (long) result; // early exit
        }

        // Read the 10th byte. Since we already read 63 bits,
        // the value of this byte must fit within 1 bit (64 - 63),
        // and it must not have the high bit set.

        byteReadJustNow = ReadByte();
        if (byteReadJustNow > 0b_1u)
            throw new FormatException();

        result |= (ulong) byteReadJustNow << (maxBytesWithoutOverflow * 7);
        return (long) result;
    }

    /// <summary>
    ///     Reads a <see cref="Vector2" /> value from the current stream and advances the current position of the stream by eight
    ///     bytes.
    /// </summary>
    /// <returns>A <see cref="Vector2" /> value read from the current stream.</returns>
    public Vector2 ReadVector2()
    {
        float x = ReadSingle();
        float y = ReadSingle();
        return new Vector2(x, y);
    }

    /// <summary>
    ///     Reads a <see cref="Vector3" /> value  from the current stream and advances the current position of the stream by
    ///     twelve bytes.
    /// </summary>
    /// <returns>A <see cref="Vector3" /> value read from the current stream.</returns>
    public Vector3 ReadVector3()
    {
        float x = ReadSingle();
        float y = ReadSingle();
        float z = ReadSingle();
        return new Vector3(x, y, z);
    }

    /// <summary>
    ///     Reads a <see cref="Vector4" /> value  from the current stream and advances the current position of the stream by
    ///     sixteen bytes.
    /// </summary>
    /// <returns>A <see cref="Vector4" /> value read from the current stream.</returns>
    public Vector4 ReadVector4()
    {
        float x = ReadSingle();
        float y = ReadSingle();
        float z = ReadSingle();
        float w = ReadSingle();
        return new Vector4(x, y, z, w);
    }
}
