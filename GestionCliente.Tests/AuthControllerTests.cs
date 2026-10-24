using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using GestionClienteAPI.Controllers;
using GestionCliente.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GestionClienteAPI.Tests;

public class AuthControllerTests
{
    private const string ClaveJwt = "ClaveDePruebaSuperLargaDeMinimo48CaracteresParaHmac!!!";

    private static IConfiguration CrearConfiguracion(Action<IDictionary<string, string?>>? override_ = null)
    {
        var config = new Dictionary<string, string?>
        {
            ["AuthUsers:0:Username"] = "admin",
            ["AuthUsers:0:Password"] = "Secreta123",
            ["JwtSettings:Issuer"] = "emisor-test",
            ["JwtSettings:Audience"] = "audiencia-test",
            ["JwtSettings:Key"] = ClaveJwt,
            ["JwtSettings:ExpiraMinutos"] = "15"
        };
        override_?.Invoke(config);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();
    }

    private static AuthController CrearController(IConfiguration configuracion, out TestLogger<AuthController> logger)
    {
        logger = new TestLogger<AuthController>();
        return new AuthController(configuracion, logger);
    }

    private static string Serializar(object? valor)
        => valor is null ? string.Empty : JsonSerializer.Serialize(valor, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

    [Fact]
    public void Login_UsuarioYContrasenaVacios_RetornaBadRequest()
    {
        var controller = CrearController(CrearConfiguracion(), out _);

        var resultado = controller.Login(new LoginRequest("", ""));

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Contains("obligatorios", Serializar(badRequest.Value));
    }

    [Fact]
    public void Login_SinAuthUsersConfigurados_Retorna503()
    {
        var configuracion = CrearConfiguracion(config =>
        {
            config.Remove("AuthUsers:0:Username");
            config.Remove("AuthUsers:0:Password");
        });
        var controller = CrearController(configuracion, out var logger);

        var resultado = controller.Login(new LoginRequest("admin", "Secreta123"));

        var error = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, error.StatusCode);
        Assert.Contains("Servicio de autenticación no disponible.", Serializar(error.Value));
        Assert.Contains(logger.Entradas, e => e.Message.Contains("No hay usuarios configurados en 'AuthUsers'"));
    }

    [Fact]
    public void Login_ConUsuariosAdicionales_SePuedeAccederConSegundoUsuario()
    {
        var configuracion = CrearConfiguracion(config =>
        {
            config["AuthUsers:1:Username"] = "operador";
            config["AuthUsers:1:Password"] = "ClaveOperador99";
        });
        var controller = CrearController(configuracion, out _);

        var resultado = controller.Login(new LoginRequest("operador", "ClaveOperador99"));

        Assert.IsType<OkObjectResult>(resultado);
    }

    [Fact]
    public void Login_UsuarioNoExiste_RetornaUnauthorized()
    {
        var controller = CrearController(CrearConfiguracion(), out _);

        var resultado = controller.Login(new LoginRequest("noexiste", "Secreta123"));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado);
        Assert.Contains("inválidas", Serializar(unauthorized.Value));
    }

    [Fact]
    public void Login_ContrasenaIncorrecta_RetornaUnauthorized()
    {
        var controller = CrearController(CrearConfiguracion(), out _);

        var resultado = controller.Login(new LoginRequest("admin", "OtraClave"));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado);
        Assert.Contains("Credenciales inválidas.", Serializar(unauthorized.Value));
    }

    [Fact]
    public void Login_CredencialesValidas_RetornaTokenYExpira()
    {
        var controller = CrearController(CrearConfiguracion(), out var logger);

        var resultado = controller.Login(new LoginRequest("admin", "Secreta123"));

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var json = Serializar(ok.Value)!;

        var token = new JwtSecurityTokenHandler();
        Assert.Contains("token", json);

        var documento = JsonDocument.Parse(json);
        var tokenTexto = documento.RootElement.GetProperty("token").GetString()!;
        Assert.False(string.IsNullOrWhiteSpace(tokenTexto));

        var jwt = token.ReadJwtToken(tokenTexto);
        Assert.Equal("emisor-test", jwt.Issuer);
        Assert.Contains("audiencia-test", jwt.Audiences);
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
        Assert.Contains(jwt.Claims, c => c.Value == "admin");

        Assert.Contains(logger.Entradas, e => e.Level == LogLevel.Information);
    }

    [Fact]
    public void Login_Whitespace_CamposEnBlanco_RetornaBadRequest()
    {
        var controller = CrearController(CrearConfiguracion(), out _);

        var resultado = controller.Login(new LoginRequest("   ", "   "));

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Login_CredencialesSensibles_CasiIguales_FallaPorDistintoCasen()
    {
        var controller = CrearController(CrearConfiguracion(), out _);

        var conEspacio = controller.Login(new LoginRequest("admin", "Secreta123 "));
        Assert.IsType<UnauthorizedObjectResult>(conEspacio);

        var enMinusculas = controller.Login(new LoginRequest("Admin", "Secreta123"));
        Assert.IsType<UnauthorizedObjectResult>(enMinusculas);
    }
}
