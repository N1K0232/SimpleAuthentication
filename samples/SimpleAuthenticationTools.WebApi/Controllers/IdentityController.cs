using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthenticationTools.JwtBearer;

namespace SimpleAuthenticationTools.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController : ControllerBase
{
    [HttpPost]
    public LoginResponse Login(LoginRequest loginRequest, [FromServices] IJwtBearerService jwtBearerService)
    {
        // Check for login rights...

        // Add custom claims (optional).
        var claims = new List<Claim>
        {
            new(ClaimTypes.GivenName, "Marco"),
            new(ClaimTypes.Surname, "Minerva")
        };

        var token = jwtBearerService.CreateToken(loginRequest.Username, claims);
        return new(token);
    }
}

public record class LoginRequest(string Username, string Password);

public record class LoginResponse(string Token);