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
            
            var tenantId = request.TenantId?.ToString() ?? Guid.NewGuid().ToString();
            
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
            var tenantId = Guid.NewGuid().ToString(); // Default
            string? businessName = null;
            if (result.GetProperty("user").TryGetProperty("user_metadata", out var metadata))
            {
                if (metadata.TryGetProperty("tenant_id", out var tidElement))
                {
                    tenantId = tidElement.GetString() ?? tenantId;
                }
                if (metadata.TryGetProperty("business_name", out var bnElement))
                {
                    businessName = bnElement.GetString();
                }
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
        var jwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")
            ?? _configuration["Supabase:JwtSecret"]
            ?? throw new InvalidOperationException("JWT Secret not configured");

        // Validate secret length to avoid cryptic runtime errors from token library
        if (jwtSecret.Length < 32)
        {
            _logger.LogError("JWT Secret too short ({Length} chars). It must be at least 32 characters.", jwtSecret.Length);
            throw new InvalidOperationException("JWT Secret is too short. Set SUPABASE_JWT_SECRET with at least 32 characters.");
        }

        var key = Encoding.ASCII.GetBytes(jwtSecret);
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
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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
