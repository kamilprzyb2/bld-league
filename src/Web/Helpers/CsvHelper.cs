using System.Text;

namespace BldLeague.Web.Helpers;

public static class CsvHelper
{
    private static readonly Encoding Utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    public static byte[] BuildCsv(string[] headers, IEnumerable<string?[]> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeField)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeField)));
        return Utf8Bom.GetBytes(sb.ToString());
    }

    private static string EscapeField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
