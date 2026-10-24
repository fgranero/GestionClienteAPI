using System.Text.Json;
using GestionCliente.BL;
using GestionCliente.DAL;
using GestionCliente.Model;
using GestionCliente.Tests.Helpers;
using GestionClienteAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionClienteAPI.Tests;

public class ClienteControllerTests
{
    private static ClienteController CrearController(IList<Cliente> clientes)
        => new(new NegCliente(new DatCliente(
            TestContextHelper.CreateContext(clientes),
            NullLogger<DatCliente>.Instance)));

    private static string Serializar(object? valor)
        => JsonSerializer.Serialize(valor, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999999)]
    public async Task GetDatosCliente_RutNoPositivo_RetornaBadRequest(int rutCliente)
    {
        var controller = CrearController([]);

        var resultado = await controller.GetDatosCliente(rutCliente);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        var json = Serializar(badRequest.Value);
        Assert.Contains("El parámetro rutCliente debe ser un entero positivo.", json);
    }

    [Fact]
    public async Task GetDatosCliente_ClienteExiste_RetornaOkConDatos()
    {
        var cliente = new Cliente { rut = 12345678, dv = "9", nombre = "Juan", apellido_paterno = "Perez" };
        var controller = CrearController([cliente]);

        var resultado = await controller.GetDatosCliente(12345678);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var recibido = Assert.IsType<Cliente>(ok.Value);
        Assert.Equal(12345678, recibido.rut);
        Assert.Equal("9", recibido.dv);
        Assert.Equal("Juan", recibido.nombre);
        Assert.Equal("Perez", recibido.apellido_paterno);
    }

    [Fact]
    public async Task GetDatosCliente_ClienteNoExiste_RetornaNotFound()
    {
        var controller = CrearController([]);

        var resultado = await controller.GetDatosCliente(99999);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        var json = Serializar(notFound.Value);
        Assert.Contains("99999", json);
        Assert.Contains("No se encontraron datos", json);
    }

    [Fact]
    public async Task GetDatosCliente_ErrorBaseDatos_Retorna500()
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbPlanexContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contextoFragil = new ApplicationDbPlanexContext(opciones);
        var controller = new ClienteController(new NegCliente(new DatCliente(contextoFragil, NullLogger<DatCliente>.Instance)));

        var resultado = await controller.GetDatosCliente(100);

        var error = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
        var json = Serializar(error.Value);
        Assert.Contains("Error en proceso de obtención de datos de cliente.", json);
    }

    [Fact]
    public async Task GetDatosCliente_RutMinimoPositivo_RetornaNotFoundOCorrecto()
    {
        var controller = CrearController([]);

        var resultado = await controller.GetDatosCliente(1);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
