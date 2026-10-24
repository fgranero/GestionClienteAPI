using GestionCliente.Model;

namespace GestionCliente.Tests;

public class ClienteTests
{
    [Fact]
    public void Constructor_InicializaPropiedadesPorDefecto()
    {
        var cliente = new Cliente();

        Assert.Equal(0, cliente.rut);
        Assert.Equal(string.Empty, cliente.dv);
        Assert.Equal(string.Empty, cliente.nombre);
        Assert.Equal(string.Empty, cliente.apellido_paterno);
        Assert.Equal(string.Empty, cliente.apellido_materno);
    }

    [Fact]
    public void Propiedades_AsiganYLeenValores()
    {
        var cliente = new Cliente
        {
            rut = 12345678,
            dv = "9",
            nombre = "Juan",
            apellido_paterno = "Perez",
            apellido_materno = "Soto"
        };

        Assert.Equal(12345678, cliente.rut);
        Assert.Equal("9", cliente.dv);
        Assert.Equal("Juan", cliente.nombre);
        Assert.Equal("Perez", cliente.apellido_paterno);
        Assert.Equal("Soto", cliente.apellido_materno);
    }
}
