using System.Text.Json.Serialization;

namespace BldLeague.Web.Auth;

public class WcaUser
{
    [JsonPropertyName("wca_id")]
    public required string WcaId { get; set; }
    
    [JsonPropertyName("name")]
    public required string FullName { get; set; }
    
    [JsonPropertyName("avatar")]
    public required WcaUserAvatar Avatar { get; set; }
}