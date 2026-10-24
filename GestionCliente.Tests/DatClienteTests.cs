using GestionCliente.DAL;
using GestionCliente.Model;
using GestionCliente.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionCliente.Tests;

public class DatClienteTests
{
    [Fact]
    public async Task GetDatosCliente_RetornaCliente_CuandoStoredProcedureDevuelveDatos()
    {
        var esperado = new Cliente { rut = 98765432, dv = "K", nombre = "Maria", apellido_paterno = "Lopez" };
        var contexto = TestContextHelper.CreateContext([esperado]);
        var logger = new TestLogger<DatCliente>();
        var datCliente = new DatCliente(contexto, logger);

        var resultado = await datCliente.GetDatosCliente(98765432);

        Assert.NotNull(resultado);
        Assert.Equal(98765432, resultado!.rut);
        Assert.Equal("K", resultado.dv);
        Assert.Equal("Maria", resultado.nombre);
        Assert.Equal("Lopez", resultado.apellido_paterno);
    }

    [Fact]
    public async Task GetDatosCliente_RetornaNull_CuandoStoredProcedureNoDevuelveFilas()
    {
        var contexto = TestContextHelper.CreateContext([]);
        var datCliente = new DatCliente(contexto, new TestLogger<DatCliente>());

        var resultado = await datCliente.GetDatosCliente(1);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetDatosCliente_RegistraLogErrorYReenvia_CuandoBaseDatosFalla()
    {
        var contexto = TestContextHelper.CreateContextInMemoryReal();
        var logger = new TestLogger<DatCliente>();
        var datCliente = new DatCliente(contexto, logger);

        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => datCliente.GetDatosCliente(1));

        Assert.IsType<InvalidOperationException>(excepcion);

        var entradasError = logger.Entradas.Where(e => e.Level == LogLevel.Error).ToList();
        Assert.Single(entradasError);
        Assert.Contains("Error en la base de datos al consultar los datos del cliente 1", entradasError[0].Message);
        Assert.NotNull(entradasError[0].Exception);
    }

    [Fact]
    public async Task GetDatosCliente_ReenviaExcepcionOriginal_CuandoBaseDatosFalla()
    {
        var contexto = TestContextHelper.CreateContextInMemoryReal();
        var datCliente = new DatCliente(contexto, new TestLogger<DatCliente>());

        var excepcion = await Assert.ThrowsAnyAsync<Exception>(() => datCliente.GetDatosCliente(42));

        Assert.NotNull(excepcion);
    }
}
