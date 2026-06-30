using System.Text;

namespace FlappyBird.Utils;

/// <summary>
/// Minimal TTF/OTF name-table reader. Only extracts the font family name (nameID=1).
/// Returns null for files that are invalid, too large, or unreadable.
/// </summary>
public static class TtfReader
{
    private const long MaxBytes = 30L * 1024 * 1024; // 30 MB cap

    private static readonly HashSet<string> _validExts =
        [".ttf", ".otf"];

    /// <summary>
    /// Returns the font family name (nameID=1) from a TTF or OTF file,
    /// or null if the file is not a valid/supported font.
    /// </summary>
    public static string? ReadFamilyName(string path)
    {
        try
        {
            if (!_validExts.Contains(Path.GetExtension(path).ToLowerInvariant()))
                return null;

            var fi = new FileInfo(path);
            if (!fi.Exists || fi.Length < 16 || fi.Length > MaxBytes)
                return null;

            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);

            // Validate magic / sfVersion
            uint version = ReadU32(br);
            if (version != 0x00010000 && // TrueType v1.0
                version != 0x4F54544F && // OpenType CFF ('OTTO')
                version != 0x74727565)   // Apple TrueType ('true')
                return null;

            ushort numTables = ReadU16(br);
            if (numTables == 0 || numTables > 256) return null;
            br.ReadBytes(6); // searchRange, entrySelector, rangeShift

            // Walk the table directory looking for 'name' (0x6E616D65)
            for (int t = 0; t < numTables; t++)
            {
                uint tag    = ReadU32(br);
                br.ReadBytes(4);           // checkSum
                uint offset = ReadU32(br);
                uint length = ReadU32(br);

                if (tag != 0x6E616D65) continue; // not 'name'
                if (offset + length > (ulong)fi.Length) return null;

                fs.Seek(offset, SeekOrigin.Begin);
                return ParseNameTable(br, offset);
            }
            return null;
        }
        catch { return null; }
    }

    private static string? ParseNameTable(BinaryReader br, uint tableBase)
    {
        ushort format       = ReadU16(br);
        if (format > 1)     return null;
        ushort count        = ReadU16(br);
        ushort stringOffset = ReadU16(br);

        if (count == 0 || count > 1024) return null;

        string? winName = null;
        string? macName = null;

        for (int i = 0; i < count; i++)
        {
            ushort platformID = ReadU16(br);
            ushort encodingID = ReadU16(br);
            ushort languageID = ReadU16(br);
            ushort nameID     = ReadU16(br);
            ushort length     = ReadU16(br);
            ushort strOff     = ReadU16(br);

            if (nameID != 1) continue; // only Font Family name

            long saved  = br.BaseStream.Position;
            long strPos = tableBase + stringOffset + strOff;

            if (strPos < 0 || strPos + length > br.BaseStream.Length)
            {
                br.BaseStream.Seek(saved, SeekOrigin.Begin);
                continue;
            }

            br.BaseStream.Seek(strPos, SeekOrigin.Begin);
            byte[] bytes = br.ReadBytes(length);
            br.BaseStream.Seek(saved, SeekOrigin.Begin);

            if (platformID == 3 && encodingID == 1) // Windows Unicode
            {
                string candidate = Encoding.BigEndianUnicode.GetString(bytes).Trim();
                // prefer English (0x0409); accept any Windows entry if none found yet
                if (languageID == 0x0409) return candidate;
                winName ??= candidate;
            }
            else if (platformID == 1 && macName is null) // Mac Latin-1
            {
                macName = Encoding.Latin1.GetString(bytes).Trim();
            }
        }

        return winName ?? macName;
    }

    private static uint   ReadU32(BinaryReader r) { var b = r.ReadBytes(4); return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3]; }
    private static ushort ReadU16(BinaryReader r) { var b = r.ReadBytes(2); return (ushort)((b[0] << 8) | b[1]); }
}
