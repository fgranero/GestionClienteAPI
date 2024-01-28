using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GestionCliente.Model;

namespace GestionCliente.DAL
{
    public class ApplicationDbPlanexContext: DbContext
    {
        protected readonly IConfiguration Configuration;

        public ApplicationDbPlanexContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                //se obtiene conexión a BD Planex
                string conexionPlanex = Configuration.GetConnectionString("ConexionSistemaCliente");

                //enlace a BD
                optionsBuilder.UseSqlServer(conexionPlanex);
                base.OnConfiguring(optionsBuilder);

            }

        }

        //SP
        public DbSet<Cliente> Cliente { get; set; }
    }
}
