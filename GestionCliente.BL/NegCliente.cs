using GestionCliente.DAL;
using GestionCliente.Model;
using Microsoft.Extensions.Configuration;

namespace GestionCliente.BL
{
    public class NegCliente
    {
        private readonly IConfiguration configuration;
        
        public NegCliente(IConfiguration _configuration)
        {
            configuration = _configuration;
            
        }

        /// <summary>
        /// Descripción: Método que se comunica con capa de datos para obtener datos de cliente
        /// </summary>
        /// <param name="rutCliente"></param>
        /// <returns></returns>
        public async Task<Cliente> GetDatosCliente(int rutCliente) { 
            Cliente objCliente= new Cliente();
            DatCliente objDat = new DatCliente(configuration);

            objCliente = await objDat.GetDatosCliente(rutCliente);

            return objCliente;
        }
    }
}
