using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionCliente.Model
{
    [Table("Cliente", Schema = "dbo")]
    [PrimaryKey(nameof(rut))]
    public class Cliente
    {
        public int rut { get; set; }
        public string dv { get; set; }
        public string nombre { get; set; }
        public string apellido_paterno { get; set; }
        public string apellido_materno {  get; set; }

        public Cliente() { 
            rut = 0;
            dv = string.Empty;
            nombre = string.Empty;
            apellido_paterno = string.Empty;
            apellido_materno = string.Empty;
        }
    }
}
