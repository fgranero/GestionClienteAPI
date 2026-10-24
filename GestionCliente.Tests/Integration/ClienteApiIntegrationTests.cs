using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionClienteAPI.Tests;

public class ClienteApiFactory : WebApplicationFactory<Program>
{
    private const string ClaveJwt = "ClaveDeIntegracionDeMinimo48CaracteresParaFirmarJwtHmac!!!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:ConexionSistemaCliente", "Server=127.0.0.1,1;Database=test;User Id=sa;Password=xx;");
        builder.UseSetting("JwtSettings:Issuer", "emisor-test");
        builder.UseSetting("JwtSettings:Audience", "audiencia-test");
        builder.UseSetting("JwtSettings:Key", ClaveJwt);
        builder.UseSetting("JwtSettings:ExpiraMinutos", "30");
        builder.UseSetting("AuthUsers:0:Username", "admin");
        builder.UseSetting("AuthUsers:0:Password", "Secreta123");
        builder.UseSetting("Logging:LogLevel:Microsoft.AspNetCore.Authentication", "Debug");
    }
}

public class ClienteApiIntegrationTests : IClassFixture<ClienteApiFactory>
{
    private readonly ClienteApiFactory _factory;

    public ClienteApiIntegrationTests(ClienteApiFactory factory) => _factory = factory;

    private async Task<(HttpClient cliente, string token)> ObtenerClienteConTokenAsync()
    {
        var cliente = _factory.CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/Auth/login", new { Username = "admin", Password = "Secreta123" });
        respuesta.EnsureSuccessStatusCode();

        var json = await respuesta.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(json).RootElement.GetProperty("token").GetString()!;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (cliente, token);
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_RetornaTokenYExpira()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/Auth/login", new { Username = "admin", Password = "Secreta123" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var json = await respuesta.Content.ReadAsStringAsync();
        var documento = JsonDocument.Parse(json);
        var token = documento.RootElement.GetProperty("token").GetString()!;
        Assert.False(string.IsNullOrWhiteSpace(token));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("emisor-test", jwt.Issuer);
    }

    [Fact]
    public async Task Login_ConCredencialesInvalidas_Retorna401()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/Auth/login", new { Username = "admin", Password = "ClaveInvalida" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetDatosCliente_SinToken_Retorna401()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/Cliente/GetDatosCliente?rutCliente=1");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetDatosCliente_RutNoPositivo_Retorna400()
    {
        var (cliente, _) = await ObtenerClienteConTokenAsync();

        var respuesta = await cliente.GetAsync("/api/Cliente/GetDatosCliente?rutCliente=0");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var json = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("entero positivo", json);
    }

    [Fact]
    public async Task GetDatosCliente_ErrorBaseDatos_Retorna500()
    {
        var (cliente, _) = await ObtenerClienteConTokenAsync();

        var respuesta = await cliente.GetAsync("/api/Cliente/GetDatosCliente?rutCliente=777");

        Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
    }

    [Fact]
    public async Task RateLimiter_QuedarseSinCuotas_Retorna429()
    {
        using var factoryLocal = new ClienteApiFactory();
        var cliente = factoryLocal.CreateClient();

        HttpStatusCode? ultimo = null;
        for (var i = 0; i < 12; i++)
        {
            var respuesta = await cliente.PostAsJsonAsync("/api/Auth/login", new { Username = "admin", Password = "ClaveInvalida" });
            ultimo = respuesta.StatusCode;

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
            {
                break;
            }
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, ultimo);
    }
}
