using System.Security.Claims;

namespace AstraFuture.Api.Auth;

/// <summary>
/// Middleware para extrair tenant_id do token JWT e adicioná-lo aos headers
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Tenta extrair tenant_id do JWT (custom claim do Supabase)
            var tenantClaim = context.User.FindFirst("tenant_id") 
                ?? context.User.FindFirst("app_metadata.tenant_id");

            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                // Adiciona ao contexto
                context.Items["TenantId"] = tenantId;
                
                // Adiciona ao header se não existir
                if (!context.Request.Headers.ContainsKey("X-Tenant-Id"))
                {
                    context.Request.Headers.Append("X-Tenant-Id", tenantId.ToString());
                }
                
                _logger.LogDebug("Tenant {TenantId} extracted from JWT", tenantId);
            }
            else
            {
                // Se não tiver tenant_id no token, tenta usar o header
                if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId) 
                    && Guid.TryParse(headerTenantId, out var parsedTenantId))
                {
                    context.Items["TenantId"] = parsedTenantId;
                }
            }

            // Extrai user_id (sub claim)
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) 
                ?? context.User.FindFirst("sub");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Items["UserId"] = userId;
            }
        }

        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
