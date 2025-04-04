using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using NATS.Net;

namespace SOA_example.Routes
{
    public static class LoginRoutes
    {
        async public static void StartListener()
        {
            await using var client = new NatsClient("nats");

            await foreach (var msg in client.SubscribeAsync<string>("login"))
            {
                Console.WriteLine($"Received message: {msg}");

                if (msg.Data == "exit")
                {
                    Console.WriteLine("Not listening anymore");
                    break;
                }
            }
        }

        public static void MapLoginRoutes(this WebApplication app)
        {
            app.MapPost("v1/login", (string? username, string? pwd) =>
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pwd))
                {
                    return Results.BadRequest("Username and password are required.");
                }
                // Simulate a login process
                if (username == "admin" && pwd == "password")
                {
                    return Results.Ok(GenerateJwtToken(username));
                }
                else
                {
                    return Results.Unauthorized();
                }
            })
        .WithName("Login Post")
        .WithOpenApi();
        }

        private static string GenerateJwtToken(string username)
        {
            byte[] key = System.Text.Encoding.UTF8.GetBytes("cunnycunnycunnycunnycunnycunnycunnycunnycunnycunnycunnycunnycunn");

            SymmetricSecurityKey secretKey = new SymmetricSecurityKey(key);
            SigningCredentials creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            Dictionary<string, object> claims = new Dictionary<string, object>
            {
                [ClaimTypes.Name] = username,
                [ClaimTypes.Role] = "admin",
                [ClaimTypes.Sid] = "1234567890"
            };

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Issuer = "https://localhost:5001 Login API",
                Audience = "https://localhost:5001 Everything",
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = creds,
                Claims = claims
            };

            var handler = new JsonWebTokenHandler();
            handler.SetDefaultTimesOnTokenCreation = false;

            return handler.CreateToken(descriptor);
        }

        

    }
}