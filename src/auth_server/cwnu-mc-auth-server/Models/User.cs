using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cwnu_mc_auth_server.Models
{
    public class User
    {
        [Key]
        public string Uuid { get; set; }

        [Required]
        [ForeignKey(nameof(Dept))]
        public int DeptId { get; set; }
    }
}
