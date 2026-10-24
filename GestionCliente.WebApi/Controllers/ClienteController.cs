using GestionCliente.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GestionClienteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("PorCliente")]
    public class ClienteController(NegCliente negCliente) : ControllerBase
    {
        [HttpGet("GetDatosCliente")]
        public async Task<IActionResult> GetDatosCliente([FromQuery] int rutCliente)
        {
            if (rutCliente <= 0)
            {
                return BadRequest(new { Mensaje = "El parámetro rutCliente debe ser un entero positivo." });
            }

            try
            {
                var cliente = await negCliente.GetDatosCliente(rutCliente);

                if (cliente is null)
                {
                    return NotFound(new { Mensaje = $"No se encontraron datos para el rut {rutCliente}." });
                }

                return Ok(cliente);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { Mensaje = "Error en proceso de obtención de datos de cliente." });
            }
        }
    }
}
