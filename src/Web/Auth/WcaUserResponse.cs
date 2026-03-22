using System.Text.Json.Serialization;

namespace BldLeague.Web.Auth;

public class WcaUserResponse
{
    [JsonPropertyName("me")]
    public required WcaUser User { get; set; }
}