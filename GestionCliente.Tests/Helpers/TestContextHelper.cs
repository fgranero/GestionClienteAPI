using GestionCliente.DAL;
using GestionCliente.Model;
using GestionCliente.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace GestionCliente.Tests.Helpers;

public static class TestContextHelper
{
    private static IEntityType? _tipoEntidadCliente;

    private static IEntityType TipoEntidadCliente()
    {
        if (_tipoEntidadCliente is null)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbPlanexContext>()
                .UseInMemoryDatabase("modelo-compartido")
                .Options;

            using var contexto = new ApplicationDbPlanexContext(opciones);
            _tipoEntidadCliente = contexto.Model.FindEntityType(typeof(Cliente))!;
        }

        return _tipoEntidadCliente;
    }

    public static ApplicationDbPlanexContext CreateContext(IList<Cliente> clientes)
    {
        var expandidos = clientes.ToList();
        var proveedor = new TestAsyncQueryProvider<Cliente>(expandidos.AsQueryable());

        var dbSetMock = new Mock<DbSet<Cliente>>();

        dbSetMock.As<IQueryable<Cliente>>()
            .Setup(m => m.Provider)
            .Returns(proveedor);

        dbSetMock.As<IQueryable<Cliente>>()
            .Setup(m => m.Expression)
            .Returns(new EntityQueryRootExpression(proveedor, TipoEntidadCliente()));

        dbSetMock.As<IQueryable<Cliente>>()
            .Setup(m => m.ElementType)
            .Returns(typeof(Cliente));

        dbSetMock.As<IQueryable<Cliente>>()
            .Setup(m => m.GetEnumerator())
            .Returns(() => expandidos.AsQueryable().GetEnumerator());

        dbSetMock.As<IAsyncEnumerable<Cliente>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Cliente>(expandidos.AsQueryable().GetEnumerator()));

        var opciones = new DbContextOptionsBuilder<ApplicationDbPlanexContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contexto = new ApplicationDbPlanexContext(opciones)
        {
            Cliente = dbSetMock.Object
        };

        return contexto;
    }

    public static ApplicationDbPlanexContext CreateContextInMemoryReal()
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbPlanexContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbPlanexContext(opciones);
    }
}
