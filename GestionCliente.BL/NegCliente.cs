using GestionCliente.DAL;
using GestionCliente.Model;

namespace GestionCliente.BL
{
    public class NegCliente(DatCliente datCliente)
    {
        /// <summary>
        /// Descripción: Método que se comunica con capa de datos para obtener datos de cliente
        /// </summary>
        /// <param name="rutCliente"></param>
        /// <returns></returns>
        public Task<Cliente?> GetDatosCliente(int rutCliente)
        {
            return datCliente.GetDatosCliente(rutCliente);
        }
    }
}
