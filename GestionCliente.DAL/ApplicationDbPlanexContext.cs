using GestionCliente.Model;
using Microsoft.EntityFrameworkCore;

namespace GestionCliente.DAL
{
    public class ApplicationDbPlanexContext(DbContextOptions<ApplicationDbPlanexContext> options)
        : DbContext(options)
    {
        public DbSet<Cliente> Cliente { get; set; }
    }
}
