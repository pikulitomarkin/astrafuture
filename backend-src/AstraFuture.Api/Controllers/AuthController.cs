using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AstraFuture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly HttpClient _httpClient;

    public AuthController(
        IConfiguration configuration, 
        ILogger<AuthController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Supabase");
    }

    // Helper: Slugify simples
    private static string Slugify(string input)
    {
        var s = input.ToLowerInvariant();
        var sb = new System.Text.StringBuilder();
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || ch == '-') sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '_') sb.Append('-');
        }
        var slug = sb.ToString();
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        if (slug.Length == 0) slug = "tenant";
        return slug.Length > 50 ? slug.Substring(0, 50) : slug;
    }

    // Lê a Supabase anon key aceitando diferentes convenções de nome
    private string GetSupabaseAnonKey()
    {
        return Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
            ?? Environment.GetEnvironmentVariable("Supabase__AnonKey")
            ?? _configuration["Supabase:AnonKey"]
            ?? string.Empty;
    }

    /// <summary>
    /// Registra um novo usuário via Supabase Auth
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var supabaseUrl = _configuration["Supabase:Url"];
            var anonKey = GetSupabaseAnonKey();
            if (string.IsNullOrEmpty(anonKey))
            {
                _logger.LogError("Supabase anon key is not configured. Set SUPABASE_ANON_KEY or Supabase__AnonKey in environment variables.");
                return StatusCode(500, new { error = "Supabase API key not configured" });
            }

            var payload = new
            {
                email = request.Email,
                password = request.Password,
                data = new
                {
                    full_name = request.FullName,
                    business_name = request.BusinessName,
                    tenant_id = request.TenantId
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", anonKey);

            var response = await _httpClient.PostAsync(
                $"{supabaseUrl}/auth/v1/signup", 
                content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Supabase signup failed: {Response}", responseBody);
                return BadRequest(new { error = "Falha no registro", details = responseBody });
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            // Extrair user ID e email do resultado do Supabase
            var userId = result.TryGetProperty("user", out var user) && user.TryGetProperty("id", out var id) 
                ? id.GetString() : Guid.NewGuid().ToString();
            
            // Verificar flag de autocriação de tenant
            var autoCreate = false;
            var autoCreateCfg = Environment.GetEnvironmentVariable("AUTO_CREATE_TENANT") ?? _configuration["AUTO_CREATE_TENANT"];
            if (!string.IsNullOrEmpty(autoCreateCfg) && bool.TryParse(autoCreateCfg, out var ac)) autoCreate = ac;

            string tenantId;

            if (request.TenantId.HasValue)
            {
                tenantId = request.TenantId.Value.ToString();
            }
            else if (autoCreate)
            {
                // Criar tenant automaticamente
                var tenantGuid = Guid.NewGuid();
                var tenantName = string.IsNullOrEmpty(request.BusinessName) ? $"Tenant de {request.Email}" : request.BusinessName;
                var slugBase = Slugify(tenantName);
                var slug = slugBase;

                var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
                await using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Garantir slug único
                    var exists = await conn.ExecuteScalarAsync<bool>("SELECT EXISTS(SELECT 1 FROM tenants WHERE slug = @Slug)", new { Slug = slug });
                    if (exists)
                    {
                        slug = slug + "-" + tenantGuid.ToString().Substring(0, 8);
                    }

                    await conn.ExecuteAsync(@"INSERT INTO tenants (id, name, slug, tenant_type, subscription_tier, created_at, updated_at)
                                                VALUES (@Id, @Name, @Slug, @Type, @Tier, @Now, @Now)",
                        new { Id = tenantGuid, Name = tenantName, Slug = slug, Type = "general", Tier = "free", Now = DateTime.UtcNow });

                    // Criar usuário na tabela app.users vinculado ao tenant (owner)
                    var appUserId = Guid.NewGuid();
                    await conn.ExecuteAsync(@"INSERT INTO users (id, tenant_id, auth_user_id, email, full_name, role, is_active, email_verified_at, created_at, updated_at)
                                              VALUES (@Id, @TenantId, @AuthUserId, @Email, @FullName, @Role, true, NOW(), NOW(), NOW())",
                        new { Id = appUserId, TenantId = tenantGuid, AuthUserId = Guid.Parse(userId), Email = request.Email, FullName = request.FullName ?? request.Email, Role = "owner" });
                }

                tenantId = tenantGuid.ToString();

                // Atualizar user_metadata no Supabase via service role key, se disponível
                var serviceKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY") ?? _configuration["Supabase:ServiceRoleKey"];
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    try
                    {
                        var adminPayload = JsonSerializer.Serialize(new { user_metadata = new { tenant_id = tenantId } });
                        var adminReq = new HttpRequestMessage(new HttpMethod("PATCH"), $"{supabaseUrl}/auth/v1/admin/users/{userId}")
                        {
                            Content = new StringContent(adminPayload, Encoding.UTF8, "application/json")
                        };
                        adminReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
                        adminReq.Headers.Add("apikey", serviceKey);

                        var adminResp = await _httpClient.SendAsync(adminReq);
                        if (!adminResp.IsSuccessStatusCode)
                        {
                            var body = await adminResp.Content.ReadAsStringAsync();
                            _logger.LogWarning("Failed to update user_metadata for {UserId} via admin API: {Status}, {Body}", userId, adminResp.StatusCode, body);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error updating Supabase user metadata for {UserId}", userId);
                    }
                }
            }
            else
            {
                // Sem tenant informado e sem autocriação -> erro cliente
                _logger.LogWarning("Attempt to register user without tenant: {Email}", request.Email);
                return BadRequest(new { error = "Tenant not provided. Contact administrator or provide tenant_id." });
            }

            // Gerar nosso próprio JWT
            var token = GenerateJwtToken(userId!, request.Email, tenantId);
            
            _logger.LogInformation("User registered: {Email}", request.Email);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                RefreshToken = null,
                ExpiresIn = 86400, // 24 horas
                User = new UserInfo
                {
                    Id = userId,
                    Email = request.Email,
                    TenantId = Guid.TryParse(tenantId, out var tid) ? tid : null,
                    BusinessName = request.BusinessName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "Erro interno no registro" });
        }
    }

    /// <summary>
    /// Login de usuário via Supabase Auth
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var supabaseUrl = _configuration["Supabase:Url"];
            var anonKey = GetSupabaseAnonKey();
            if (string.IsNullOrEmpty(anonKey))
            {
                _logger.LogError("Supabase anon key is not configured. Set SUPABASE_ANON_KEY or Supabase__AnonKey in environment variables.");
                return StatusCode(500, new { error = "Supabase API key not configured" });
            }

            var payload = new
            {
                email = request.Email,
                password = request.Password
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", anonKey);

            var response = await _httpClient.PostAsync(
                $"{supabaseUrl}/auth/v1/token?grant_type=password", 
                content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Supabase login failed for {Email}: {Response}", request.Email, responseBody);
                return Unauthorized(new { error = "Credenciais inválidas" });
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            // Extrair informações do usuário
            var userId = result.GetProperty("user").GetProperty("id").GetString()!;
            var email = result.GetProperty("user").GetProperty("email").GetString()!;
            
            // Tentar pegar tenant_id e business_name dos metadados do usuário
            string? tenantId = null;
            string? businessName = null;
            if (result.GetProperty("user").TryGetProperty("user_metadata", out var metadata))
            {
                if (metadata.TryGetProperty("tenant_id", out var tidElement))
                {
                    tenantId = tidElement.GetString();
                }
                if (metadata.TryGetProperty("business_name", out var bnElement))
                {
                    businessName = bnElement.GetString();
                }
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                _logger.LogWarning("Login attempt for user {Email} without tenant_id in user_metadata", request.Email);
                return BadRequest(new { error = "Usuário não está associado a um tenant. Contate o administrador." });
            }
            
            // Gerar nosso próprio JWT
            var token = GenerateJwtToken(userId, email, tenantId);
            
            _logger.LogInformation("User logged in: {Email}", request.Email);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                RefreshToken = null,
                ExpiresIn = 86400, // 24 horas
                User = new UserInfo
                {
                    Id = userId,
                    Email = email,
                    TenantId = Guid.TryParse(tenantId, out var tidGuid) ? tidGuid : null,
                    BusinessName = businessName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "Erro interno no login" });
        }
    }

    private string GenerateJwtToken(string userId, string email, string tenantId)
    {
        // Use the Supabase:JwtSecret value from IConfiguration (set at startup) to ensure consistency
        var jwtSecret = _configuration["Supabase:JwtSecret"]
            ?? Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")
            ?? throw new InvalidOperationException("JWT Secret not configured");

        var key = Encoding.ASCII.GetBytes(jwtSecret);
        var securityKey = new SymmetricSecurityKey(key);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim("sub", userId),
                new Claim("email", email),
                new Claim("tenant_id", tenantId)
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = "AstraFuture",
            Audience = "AstraFuture",
            SigningCredentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Log a short hash of the secret at token creation time for correlation with validation logs
        try
        {
            var secretToHash = jwtSecret;
            if (!string.IsNullOrEmpty(secretToHash))
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secretToHash));
                var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                _logger.LogInformation("[AUTH] JWT secret SHA256 prefix when signing: {Hash}", hex.Substring(0, 8));
            }
        }
        catch { /* ignore */ }

        return tokenString;
    }

    /// <summary>
    /// Refresh do token JWT
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var supabaseUrl = _configuration["Supabase:Url"];
            var anonKey = GetSupabaseAnonKey();
            if (string.IsNullOrEmpty(anonKey))
            {
                _logger.LogError("Supabase anon key is not configured. Set SUPABASE_ANON_KEY or Supabase__AnonKey in environment variables.");
                return StatusCode(500, new { error = "Supabase API key not configured" });
            }

            var payload = new { refresh_token = request.RefreshToken };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", anonKey);

            var response = await _httpClient.PostAsync(
                $"{supabaseUrl}/auth/v1/token?grant_type=refresh_token", 
                content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized(new { error = "Token inválido ou expirado" });
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

            return Ok(new AuthResponse
            {
                AccessToken = result.GetProperty("access_token").GetString(),
                RefreshToken = result.GetProperty("refresh_token").GetString(),
                ExpiresIn = result.GetProperty("expires_in").GetInt32()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "Erro interno" });
        }
    }

    /// <summary>
    /// Logout (revoga o token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var supabaseUrl = _configuration["Supabase:Url"];
            var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            await _httpClient.PostAsync($"{supabaseUrl}/auth/v1/logout", null);

            _logger.LogInformation("User logged out");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "Erro interno" });
        }
    }

    /// <summary>
    /// Retorna informações do usuário autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        var tenantId = HttpContext.Items["TenantId"] as Guid?;

        return Ok(new UserInfo
        {
            Id = userId,
            Email = email,
            TenantId = tenantId
        });
    }
}

// DTOs
public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public string? BusinessName { get; init; }
    public Guid? TenantId { get; init; }
}

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public record AuthResponse
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public int ExpiresIn { get; init; }
    public UserInfo? User { get; init; }
}

public record UserInfo
{
    public string? Id { get; init; }
    public string? Email { get; init; }
    public Guid? TenantId { get; init; }
    public string? BusinessName { get; init; }
}
