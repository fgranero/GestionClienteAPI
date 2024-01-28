using GestionCliente.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GestionCliente.DAL
{
    public class DatCliente
    {
        private readonly IConfiguration configuration;

        public DatCliente(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        /// <summary>
        /// Descripción: Método que enlaza a BD para obtener datos de cliente.
        /// </summary>
        /// <param name="rutCliente"></param>
        /// <returns></returns>
        public async Task<Cliente> GetDatosCliente(int rutCliente) { 
            Cliente? objCliente = new Cliente();

            try
            {
                using (var contextoBD = new ApplicationDbPlanexContext(configuration))
                {
                    //conexion con sp que obtiene datos de cliente
                    var queryCliente = await contextoBD.Cliente.FromSqlInterpolated($"exec sp_consulta_cliente @rut_cliente={rutCliente}").ToListAsync();
                    
                    objCliente=queryCliente.FirstOrDefault();
                }
            }
            catch(Exception e) {
                objCliente = null;
            }
            return objCliente;
        }
    }
}
