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
                ValidateIssuer = false, // Supabase usa issuer dinâmico baseado na URL do projeto
                ValidateAudience = false, // Supabase usa "authenticated" como audience
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                TryAllIssuerSigningKeys = true // Tenta todas as chaves disponíveis
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    var token = context.Token;
                    logger.LogInformation("[JWT] Token received: {HasToken}, Path: {Path}", !string.IsNullOrEmpty(token), context.Request.Path);
                    if (!string.IsNullOrEmpty(token))
                    {
                        logger.LogInformation("[JWT] Token preview: {TokenPreview}", token.Substring(0, Math.Min(50, token.Length)));
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogError("[JWT] Authentication FAILED for {Path}: {Error}", context.Request.Path, context.Exception.Message);
                    logger.LogError("[JWT] Exception details: {Exception}", context.Exception.ToString());
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) 
                              ?? context.Principal?.FindFirstValue("sub");
                    var tenantId = context.Principal?.FindFirstValue("tenant_id");
                    logger.LogInformation("[JWT] Token VALIDATED successfully for user: {UserId}, tenant: {TenantId}", userId, tenantId);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogWarning("[JWT] Challenge triggered for {Path}: {Error}, {ErrorDescription}", 
                        context.Request.Path, context.Error, context.ErrorDescription);
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
