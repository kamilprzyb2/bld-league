using System.Globalization;

namespace BldLeague.Domain.ValueObjects;

/// <summary>
/// Represents a result of a solve that can be a timed result, DNF or DNS.
/// </summary>
public readonly record struct SolveResult
{
    private SolveResult(int value)
    {
        Centiseconds = value;
    }

    /// <summary>
    /// Creates a new <see cref="SolveResult"/> being a timed result.
    /// </summary>
    /// <param name="cs">Time in centiseconds.</param>
    /// <returns></returns>
    public static SolveResult FromCentiseconds(int cs) => new SolveResult(cs);

    /// <summary>
    /// Creates a new <see cref="SolveResult"/> representing a DNF.
    /// </summary>
    /// <returns></returns>
    public static SolveResult Dnf() => new SolveResult(-1);

    /// <summary>
    /// Creates a new <see cref="SolveResult"/> representing a DNS.
    /// </summary>
    /// <returns></returns>
    public static SolveResult Dns() => new SolveResult(-2);

    /// <summary>
    /// Gets the underlying time in centiseconds.
    /// </summary>
    public int Centiseconds { get; }

    /// <summary>
    /// Returns true if the time represents a "Did Not Finish" (DNF) result.
    /// </summary>
    public bool IsDnf => Centiseconds == -1;

    /// <summary>
    /// Returns true if the time represents a "Did Not Start" (DNS) result.
    /// </summary>
    public bool IsDns => Centiseconds == -2;

    /// <summary>
    /// Returns true if the time is a valid measured time (not DNF or DNS).
    /// </summary>
    public bool IsValid => Centiseconds >= 0;

    /// <summary>
    /// Implicit conversion to int.
    /// </summary>
    /// <param name="t">Solve time.</param>
    /// <returns></returns>
    public static implicit operator int(SolveResult t) => t.Centiseconds;

    /// <summary>
    /// Implicit conversion from int.
    /// </summary>
    /// <param name="cs">Value in centiseconds.</param>
    /// <returns></returns>
    public static implicit operator SolveResult(int cs) => FromCentiseconds(cs);

    /// <summary>
    /// Factory method that creates a SolveResult from a string.
    /// Rules:
    /// - Empty string → DNS
    /// - "DNF" (case-insensitive) → DNF
    /// - "DNS" (case-insensitive) → DNS
    /// - Time formats: s.ss, mm:ss.ss → centiseconds
    /// </summary>
    public static SolveResult FromString(string? input)
    {
        input = input?.Trim().ToUpper() ?? "";

        return input switch
        {
            "" or "DNS" => Dns(),
            "DNF" => Dnf(),
            _ => new SolveResult(ParseTimeToCentiseconds(input))
        };
    }

    private static int ParseTimeToCentiseconds(string input)
    {
        try
        {
            if (input.Contains(':'))
            {
                // mm:ss.ss format
                var parts = input.Split(':');
                if (parts.Length != 2)
                    throw new FormatException();

                int minutes = int.Parse(parts[0]);
                double seconds = double.Parse(parts[1], CultureInfo.InvariantCulture);
                return (int)Math.Round((minutes * 60 + seconds) * 100);
            }
            else
            {
                // s.ss format
                double seconds = double.Parse(input, CultureInfo.InvariantCulture);
                return (int)Math.Round(seconds * 100);
            }
        }
        catch
        {
            throw new ArgumentException($"Invalid solve result format: '{input}'");
        }
    }

    /// <summary>
    /// Returns a display string for aggregate results (Best/Ao5).
    /// DNS is shown as "-" rather than "DNS" since a walkover has no meaningful result.
    /// </summary>
    public string ToSummaryString()
    {
        if (IsDnf) return "DNF";
        if (IsDns) return "-";
        return ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsDnf) return "DNF";
        if (IsDns) return "DNS";

        int totalCentiseconds = Centiseconds;
        int minutes = totalCentiseconds / 6000;
        int seconds = (totalCentiseconds % 6000) / 100;
        int centiseconds = totalCentiseconds % 100;

        double secFraction = seconds + centiseconds / 100.0;

        // Always use '.' and always keep two decimal places
        if (minutes > 0)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}:{1:00.00}",
                minutes,
                secFraction
            );
        }
        else
        {
            return secFraction.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
