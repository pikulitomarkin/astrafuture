using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AstraFuture.Api.Auth;

public static class SupabaseAuthExtensions
{
    public static IServiceCollection AddSupabaseJwtAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSecret = configuration["Supabase:JwtSecret"] 
            ?? Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")
            ?? throw new InvalidOperationException("JWT Secret not configured");

        var key = Encoding.ASCII.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Permite HTTP em desenvolvimento
            options.SaveToken = true;
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "AstraFuture",
                ValidateAudience = true,
                ValidAudience = "AstraFuture",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) 
                              ?? context.Principal?.FindFirstValue("sub");
                    logger.LogInformation("JWT validated for user: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("tenant_id");
            });
        });

        return services;
    }
}
