using GestionCliente.BL;
using GestionCliente.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zurich.Cotizador.Controllers;

namespace GestionClienteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController(IConfiguration _configuration) : BaseController(_configuration)
    {
        private readonly IConfiguration configuration = _configuration;

        [HttpGet]
        [Route("GetDatosCliente")]
        public async Task<IActionResult> GetDatosCliente(int rutCliente) { 
            Cliente objCliente= new Cliente();
            NegCliente objNeg= new NegCliente(configuration);

            try { 
                objCliente= await objNeg.GetDatosCliente(rutCliente);
                return Ok(objCliente);
            } 
            catch {
                return BadRequest(error: new { Mensaje = "Error en proceso de obtención de datos de cliente." });
            }
        }
       
    }
}
