using GestionCliente.BL;
using GestionCliente.DAL;
using GestionCliente.Model;
using GestionCliente.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionCliente.Tests;

public class NegClienteTests
{
    [Fact]
    public async Task GetDatosCliente_DelegaEnCapaDeDatosYRetornaCliente()
    {
        var esperado = new Cliente { rut = 12345678, dv = "9", nombre = "Juan" };
        var contexto = TestContextHelper.CreateContext([esperado]);
        var negCliente = new NegCliente(new DatCliente(contexto, NullLogger<DatCliente>.Instance));

        var resultado = await negCliente.GetDatosCliente(12345678);

        Assert.NotNull(resultado);
        Assert.Equal(esperado.rut, resultado!.rut);
        Assert.Equal(esperado.nombre, resultado.nombre);
    }

    [Fact]
    public async Task GetDatosCliente_RetornaNull_CuandoNoHayRegistros()
    {
        var contexto = TestContextHelper.CreateContext([]);
        var negCliente = new NegCliente(new DatCliente(contexto, NullLogger<DatCliente>.Instance));

        var resultado = await negCliente.GetDatosCliente(1);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetDatosCliente_RetornaNull_CuandoHayMultiplesRegistros()
    {
        var primero = new Cliente { rut = 1, nombre = "Primero" };
        var segundo = new Cliente { rut = 1, nombre = "Segundo" };
        var contexto = TestContextHelper.CreateContext([primero, segundo]);
        var negCliente = new NegCliente(new DatCliente(contexto, NullLogger<DatCliente>.Instance));

        var resultado = await negCliente.GetDatosCliente(1);

        Assert.NotNull(resultado);
        Assert.Equal("Primero", resultado!.nombre);
    }
}
