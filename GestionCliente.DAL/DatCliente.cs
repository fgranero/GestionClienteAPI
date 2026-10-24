using GestionCliente.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionCliente.DAL
{
    public class DatCliente(ApplicationDbPlanexContext contextoBD, ILogger<DatCliente> logger)
    {
        /// <summary>
        /// Descripción: Método que enlaza a BD para obtener datos de cliente.
        /// </summary>
        /// <param name="rutCliente"></param>
        /// <returns></returns>
        public async Task<Cliente?> GetDatosCliente(int rutCliente)
        {
            try
            {
                var queryCliente = await contextoBD.Cliente
                    .FromSqlInterpolated($"exec sp_consulta_cliente @rut_cliente={rutCliente}")
                    .ToListAsync();

                return queryCliente.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en la base de datos al consultar los datos del cliente {Rut}.", rutCliente);
                throw;
            }
        }
    }
}
