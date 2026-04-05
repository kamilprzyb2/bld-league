using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using BldLeague.Application;
using BldLeague.Application.Queries.Users.GetUserDetailByWcaId;
using BldLeague.Infrastructure;
using BldLeague.Infrastructure.Context;
using BldLeague.Infrastructure.Helpers;
using BldLeague.Web.Auth;
using BldLeague.Web.Options;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

// Set culture
var culture = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddBldLeagueInfrastructure(builder.Configuration.GetConnectionString("Default") ?? string.Empty);
builder.Services.AddBldLeagueApplication();

builder.Services.Configure<EnvironmentBadgeOptions>(builder.Configuration.GetSection("EnvironmentBadge"));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "WCA";
    })
    .AddRefreshingCookie()
    .AddWcaOAuth(builder.Configuration.GetSection("WCA"));

var app = builder.Build();

// Ensure the database is migrated
await EnsureMigratedHelper.EnsureMigratedAsync<AppDbContext>(app.Services);

// Seed super admin user if configured
await SeedSuperAdminHelper.SeedAsync<AppDbContext>(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/ServerError");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/Error/NotFound");

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();