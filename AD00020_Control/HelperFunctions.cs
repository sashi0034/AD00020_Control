#nullable enable

namespace AD00020_Control;

public static class HelperFunctions
{
    public static void ParseHexString(string hexString, out byte[] bytes, out int length)
    {
        if (string.IsNullOrEmpty(hexString))
        {
            bytes = Array.Empty<byte>();
            length = 0;
            return;
        }

        if (hexString.Length % 2 != 0)
            throw new ArgumentException("Hex string must have an even length.", nameof(hexString));

        int byteCount = hexString.Length / 2;
        bytes = new byte[byteCount];
        for (int i = 0; i < byteCount; i++)
        {
            string byteStr = hexString.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(byteStr, 16);
        }

        length = byteCount;
    }
}