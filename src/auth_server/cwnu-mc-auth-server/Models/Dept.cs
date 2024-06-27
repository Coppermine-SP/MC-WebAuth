using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MC_WebAuth.Models
{
    public class Dept
    {
        [Key]
        public int DeptId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
