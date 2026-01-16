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
        var supabaseUrl = configuration["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase:Url not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Usar JWKS endpoint do Supabase para validar tokens ES256
            options.Authority = $"{supabaseUrl}/auth/v1";
            options.Audience = "authenticated";
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"{supabaseUrl}/auth/v1",
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            // Configurar para buscar JWKS automaticamente
            options.RequireHttpsMetadata = true;
            
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
