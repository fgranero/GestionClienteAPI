using System.Text;
using System.Threading.RateLimiting;
using GestionCliente.BL;
using GestionCliente.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT emitido por /api/Auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbPlanexContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSistemaCliente")
        ?? throw new InvalidOperationException(
            "La cadena de conexión 'ConexionSistemaCliente' no está configurada (user-secrets en desarrollo, variable de entorno ConnectionStrings__ConexionSistemaCliente en producción).")));

builder.Services.AddScoped<DatCliente>();
builder.Services.AddScoped<NegCliente>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtIssuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer no está configurado.");
var jwtAudience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("JwtSettings:Audience no está configurado.");
var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException(
        "JwtSettings:Key no está configurada (user-secrets en desarrollo, variable de entorno JwtSettings__Key en producción).");

if (Encoding.UTF8.GetBytes(jwtKey).Length < 48)
{
    throw new InvalidOperationException(
        "JwtSettings:Key debe tener al menos 48 caracteres (384 bits) para firmar con HMAC-SHA256.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PorCliente", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0
            }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
