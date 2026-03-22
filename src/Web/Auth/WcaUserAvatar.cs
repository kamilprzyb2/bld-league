using System.Text.Json.Serialization;

namespace BldLeague.Web.Auth;

public class WcaUserAvatar
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
    
    [JsonPropertyName("thumb_url")]
    public required string ThumbnailUrl { get; set; }
}