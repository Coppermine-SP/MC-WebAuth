using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cwnu_mc_auth_server.Models
{
    [Index(nameof(StudentId), IsUnique = true)]
    public class User
    {
        [Key]
        public string Uuid { get; set; }

        [Required]
        [ForeignKey(nameof(Dept))]
        public int DeptId { get; set; }

        public string StudentId { get; set; }
    }
}
