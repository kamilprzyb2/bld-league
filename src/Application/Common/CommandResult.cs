namespace BldLeague.Application.Common;

public class CommandResult
{
    /// <summary>
    /// Indicates whether the command succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// The message to display. For field-specific errors, this is the error text. 
    /// For general errors or success, this is the general message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// The name of the field that caused the error, if applicable.
    /// Null or empty if this is a general error or a success message.
    /// </summary>
    public string? Field { get; }

    /// <summary>
    /// Returns true if this is a general error (not tied to a specific field).
    /// </summary>
    public bool IsGeneralError => !Success && string.IsNullOrEmpty(Field);

    private CommandResult(bool success, string? field, string message)
    {
        Success = success;
        Field = field;
        Message = message;
    }

    /// <summary>
    /// Creates a successful result with an optional success message.
    /// </summary>
    public static CommandResult Ok(string message = "")
        => new(true, null, message);

    /// <summary>
    /// Creates a failure result tied to a specific field.
    /// </summary>
    public static CommandResult Fail(string field, string message)
        => new(false, field ?? throw new ArgumentNullException(nameof(field)), message);

    /// <summary>
    /// Creates a general failure result not tied to any specific field.
    /// </summary>
    public static CommandResult FailGeneral(string message)
        => new(false, null, message ?? throw new ArgumentNullException(nameof(message)));
}
