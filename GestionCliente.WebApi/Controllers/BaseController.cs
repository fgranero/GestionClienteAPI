using Microsoft.AspNetCore.Mvc;

namespace Zurich.Cotizador.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public BaseController(IConfiguration iConfiguration)
        {
            Configuration = iConfiguration;
        }

        public IConfiguration Configuration { get; set; }
    }
}