using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Infrastructure.Persistence;
using AstraFuture.Infrastructure.Repositories;
using AstraFuture.Api.Auth;
using FluentValidation;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONFIGURATION
// =====================================================

// Carrega .env.local (desenvolvimento) - com null-safety para produção
var parentDir = Directory.GetParent(builder.Environment.ContentRootPath);
if (parentDir?.Parent != null)
{
    var envFile = Path.Combine(parentDir.Parent.FullName, ".env.local");
    if (File.Exists(envFile))
    {
        foreach (var line in File.ReadAllLines(envFile))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }
    }
}

var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") 
    ?? throw new InvalidOperationException("SUPABASE_URL não configurada");

var jwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET");
var useAuth = !string.IsNullOrEmpty(jwtSecret);

// Connection string do Supabase PostgreSQL
var connectionString = "Host=db.alxtzjmtclopraayehfg.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=JGa8QltC28m9zBzP;SSL Mode=Require;Trust Server Certificate=true";

// Adiciona configurações ao IConfiguration para uso posterior
builder.Configuration["Supabase:Url"] = supabaseUrl;
if (useAuth)
{
    builder.Configuration["Supabase:JwtSecret"] = jwtSecret;
}

// =====================================================
// SERVICES
// =====================================================

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Desabilita validação automática de ModelState para controlar manualmente
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; // MUDOU PARA TRUE
});

// MediatR (CQRS)
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(Assembly.Load("AstraFuture.Application")));

// Infrastructure - Repositories & UnitOfWork
builder.Services.AddScoped(sp => new SupabaseContext(connectionString));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();

// CORS (desenvolvimento)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication (Supabase) - só ativa se JWT secret estiver configurado
if (useAuth)
{
    builder.Services.AddSupabaseJwtAuthentication(builder.Configuration);
    Console.WriteLine("[AUTH] JWT Authentication ENABLED");
}
else
{
    // Sem autenticação real em desenvolvimento
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    Console.WriteLine("[AUTH] JWT Authentication DISABLED (SUPABASE_JWT_SECRET not set)");
}

// HttpClient para chamadas ao Supabase Auth
builder.Services.AddHttpClient("Supabase", client =>
{
    client.BaseAddress = new Uri(supabaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AstraFuture API",
        Version = "v1",
        Description = "API de Agendamentos Multi-tenant com Supabase",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "AstraFuture",
            Email = "contato@astrafuture.com"
        }
    });

    // Configuração JWT no Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Token JWT do Supabase Auth. Formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =====================================================
// MIDDLEWARE PIPELINE
// =====================================================

// Middleware para logar body raw (debug)
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;
    
    if (!string.IsNullOrEmpty(body))
    {
        Console.WriteLine($"[REQUEST BODY] {body}");
    }
    
    await next();
});

app.UseCors();

// Swagger UI (disponível em qualquer ambiente)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AstraFuture API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantMiddleware();

app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithTags("System");

app.Run();
