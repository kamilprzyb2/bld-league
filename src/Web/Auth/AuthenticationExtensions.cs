using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using BldLeague.Application.Queries.Users.GetById;
using BldLeague.Application.Queries.Users.GetUserDetailByWcaId;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BldLeague.Web.Auth;

public static class AuthenticationExtensions
{
    private static readonly TimeSpan ClaimsRefreshInterval = TimeSpan.FromMinutes(5);

    public static AuthenticationBuilder AddRefreshingCookie(this AuthenticationBuilder builder)
    {
        return builder.AddCookie(options =>
        {
            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = async context =>
                {
                    var refreshedAtValue = context.Principal?.FindFirstValue("claims_refreshed_at");
                    if (refreshedAtValue != null
                        && DateTime.TryParse(refreshedAtValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var lastRefresh)
                        && DateTime.UtcNow - lastRefresh < ClaimsRefreshInterval)
                    {
                        return;
                    }

                    var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userIdValue == null || !Guid.TryParse(userIdValue, out var userId))
                    {
                        context.RejectPrincipal();
                        return;
                    }

                    var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
                    var user = await mediator.Send(new GetUserByIdRequest { UserId = userId });
                    if (user == null)
                    {
                        context.RejectPrincipal();
                        return;
                    }

                    var thumbnail = context.Principal?.FindFirstValue("thumbnail") ?? string.Empty;
                    var newClaims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new(ClaimTypes.Name, user.FullName),
                        new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                        new("thumbnail", thumbnail),
                        new("claims_refreshed_at", DateTime.UtcNow.ToString("O"))
                    };

                    var newIdentity = new ClaimsIdentity(newClaims, context.Principal?.Identity?.AuthenticationType);
                    context.ReplacePrincipal(new ClaimsPrincipal(newIdentity));
                    context.ShouldRenew = true;
                }
            };
        });
    }

    public static AuthenticationBuilder AddWcaOAuth(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        return builder.AddOAuth("WCA", options =>
        {
            options.ClientId = configuration["ClientId"];
            options.ClientSecret = configuration["ClientSecret"];
            options.CallbackPath = new PathString("/signin-wca");

            options.AuthorizationEndpoint = "https://www.worldcubeassociation.org/oauth/authorize";
            options.TokenEndpoint = "https://www.worldcubeassociation.org/oauth/token";
            options.UserInformationEndpoint = "https://www.worldcubeassociation.org/api/v0/me";

            //options.SaveTokens = true;
            options.Scope.Add("public");

            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", context.AccessToken);

                    var response = await context.Backchannel.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var jsonString = await response.Content.ReadAsStringAsync();
                    var wcaResponse = JsonSerializer.Deserialize<WcaUserResponse>(jsonString);

                    if (wcaResponse == null)
                    {
                        context.Fail("Invalid wca response.");
                        return;
                    }

                    var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
                    var user = await mediator.Send(
                        new GetUserDetailByWcaIdRequest(wcaResponse.User.WcaId),
                        context.HttpContext.RequestAborted);

                    if (user == null)
                    {
                        context.Fail("User account not found.");
                        return;
                    }

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new(ClaimTypes.Name, user.FullName),
                        new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                        new("thumbnail", wcaResponse.User.Avatar.ThumbnailUrl),
                        new("claims_refreshed_at", DateTime.UtcNow.ToString("O"))
                    };

                    context.Principal = new ClaimsPrincipal(
                        new ClaimsIdentity(claims, context.Scheme.Name)
                    );
                },
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/AccessDenied");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });
    }
}