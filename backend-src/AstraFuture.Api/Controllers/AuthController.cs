using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY");

            var payload = new
            {
                email = request.Email,
                password = request.Password,
                data = new
                {
                    full_name = request.FullName,
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
            
            _logger.LogInformation("User registered: {Email}", request.Email);

            return Ok(new AuthResponse
            {
                AccessToken = result.TryGetProperty("access_token", out var at) ? at.GetString() : null,
                RefreshToken = result.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                ExpiresIn = result.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : 0,
                User = new UserInfo
                {
                    Id = result.TryGetProperty("user", out var user) && user.TryGetProperty("id", out var id) 
                        ? id.GetString() : null,
                    Email = request.Email
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
            var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY");

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
            
            _logger.LogInformation("User logged in: {Email}", request.Email);

            return Ok(new AuthResponse
            {
                AccessToken = result.GetProperty("access_token").GetString(),
                RefreshToken = result.GetProperty("refresh_token").GetString(),
                ExpiresIn = result.GetProperty("expires_in").GetInt32(),
                User = new UserInfo
                {
                    Id = result.GetProperty("user").GetProperty("id").GetString(),
                    Email = result.GetProperty("user").GetProperty("email").GetString()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "Erro interno no login" });
        }
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
            var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY");

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
}
