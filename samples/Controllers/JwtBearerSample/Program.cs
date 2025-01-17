using System.Security.Claims;
using JwtBearerSample.Authentication;
using JwtBearerSample.Controllers;
using Microsoft.AspNetCore.Authentication;
using SimpleAuthentication;
using SimpleAuthentication.Permissions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Add authentication services.
builder.Services.AddSimpleAuthentication(builder.Configuration);

// Enable permission-based authorization.
builder.Services.AddScopePermissions(); // This is equivalent to builder.Services.AddPermissions<ScopeClaimPermissionHandler>();

// Define a custom handler for permission handling.
//builder.Services.AddPermissions<CustomPermissionHandler>();

builder.Services.AddAuthorization(options =>
{
    // Define permissions using a policy.
    options.AddPolicy("PeopleRead", builder => builder.RequirePermission(Permissions.PeopleRead, Permissions.PeopleAdmin));

    //options.FallbackPolicy = options.DefaultPolicy = new AuthorizationPolicyBuilder()
    //                            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    //                            .RequireAuthenticatedUser()
    //                            .Build();

    //options.AddPolicy("Bearer", policy => policy
    //                            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    //                            .RequireAuthenticatedUser());
});

// Uncomment the following line if you have multiple authentication schemes and
// you need to determine the authentication scheme at runtime (for example, you don't want to use the default authentication scheme).
//builder.Services.AddSingleton<IAuthenticationSchemeProvider, ApplicationAuthenticationSchemeProvider>();

builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.AddSimpleAuthentication(builder.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class CustomPermissionHandler : IPermissionHandler
{
    public Task<bool> IsGrantedAsync(ClaimsPrincipal user, IEnumerable<string> permissions)
    {
        bool isGranted;

        if (!permissions?.Any() ?? true)
        {
            isGranted = true;
        }
        else
        {
            var permissionClaim = user.FindFirstValue("permissions");
            var userPermissions = permissionClaim?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Enumerable.Empty<string>();

            isGranted = userPermissions.Intersect(permissions!).Any();
        }

        return Task.FromResult(isGranted);
    }
}
