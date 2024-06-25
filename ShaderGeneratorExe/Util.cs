// See https://aka.ms/new-console-template for more information
namespace ShaderGeneratorExe;
internal static class Util
{
    internal static bool HasSpirvHeader(byte[] bytes)
    {
        return bytes.Length > 4
            && bytes[0] == 0x03
            && bytes[1] == 0x02
            && bytes[2] == 0x23
            && bytes[3] == 0x07;
    }
}