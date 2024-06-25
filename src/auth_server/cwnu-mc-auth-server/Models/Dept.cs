using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace cwnu_mc_auth_server.Models
{
    public class Dept
    {
        [Key]
        public int DeptId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
