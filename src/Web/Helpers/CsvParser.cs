using System.Text;

namespace BldLeague.Web.Helpers;

public static class CsvParser
{
    /// <summary>
    /// Parses a CSV IFormFile into rows of fields (including the header row).
    /// Returns an empty list if the file is empty.
    /// </summary>
    public static async Task<List<string?[]>> ParseAsync(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
        var rows = new List<string?[]>();
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            rows.Add(ParseLine(line));
        }
        return rows;
    }

    private static string?[] ParseLine(string line)
    {
        var fields = new List<string?>();
        var i = 0;
        while (i <= line.Length)
        {
            if (i == line.Length)
            {
                fields.Add(null);
                break;
            }

            if (line[i] == '"')
            {
                // Quoted field
                i++;
                var sb = new StringBuilder();
                while (i < line.Length)
                {
                    if (line[i] == '"')
                    {
                        i++;
                        if (i < line.Length && line[i] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        sb.Append(line[i++]);
                    }
                }
                fields.Add(sb.Length > 0 ? sb.ToString() : null);
                if (i < line.Length && line[i] == ',')
                    i++;
            }
            else
            {
                // Unquoted field
                var end = line.IndexOf(',', i);
                if (end == -1)
                {
                    var value = line[i..];
                    fields.Add(string.IsNullOrEmpty(value) ? null : value);
                    break;
                }
                else
                {
                    var value = line[i..end];
                    fields.Add(string.IsNullOrEmpty(value) ? null : value);
                    i = end + 1;
                }
            }
        }
        return fields.ToArray();
    }
}
