using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace GestionClienteAPI.Controllers
{
    public record LoginRequest(string Username, string Password);

    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("PorCliente")]
    public class AuthController(IConfiguration configuration, ILogger<AuthController> logger) : ControllerBase
    {
        private sealed record Credencial(string Username, string Password);

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Mensaje = "Usuario y contraseña son obligatorios." });
            }

            var credenciales = configuration.GetSection("AuthUsers").Get<List<Credencial>>();
            if (credenciales is null || credenciales.Count == 0)
            {
                logger.LogError("No hay usuarios configurados en 'AuthUsers' (user-secrets en desarrollo, variables de entorno AuthUsers__0__Username en producción).");
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new { Mensaje = "Servicio de autenticación no disponible." });
            }

            var credencial = credenciales.FirstOrDefault(c =>
                string.Equals(c.Username, request.Username, StringComparison.Ordinal));

            if (credencial is null || !Coinciden(request.Password, credencial.Password))
            {
                return Unauthorized(new { Mensaje = "Credenciales inválidas." });
            }

            var jwtSettings = configuration.GetSection("JwtSettings");
            var expiraMinutos = jwtSettings.GetValue("ExpiraMinutos", 30);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: [new Claim(ClaimTypes.Name, request.Username)],
                expires: DateTime.UtcNow.AddMinutes(expiraMinutos),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                    SecurityAlgorithms.HmacSha256));

            logger.LogInformation("Token JWT emitido para {Usuario}.", request.Username);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expira = DateTime.UtcNow.AddMinutes(expiraMinutos)
            });
        }

        private static bool Coinciden(string recibida, string configurada)
        {
            return CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(recibida)),
                SHA256.HashData(Encoding.UTF8.GetBytes(configurada)));
        }
    }
}
