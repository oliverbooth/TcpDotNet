using Chilkat;

namespace TcpDotNet;

internal static class CryptographyUtils
{
    public static Crypt2 GenerateAes(byte[] key)
    {
        return new Crypt2
        {
            CryptAlgorithm = "aes",
            CipherMode = "cfb",
            KeyLength = 128,
            PaddingScheme = 0,
            SecretKey = key[..],
            IV = key[..]
        };
    }
}
