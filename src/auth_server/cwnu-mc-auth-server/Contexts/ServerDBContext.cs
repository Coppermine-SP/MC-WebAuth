using cwnu_mc_auth_server.Models;
using Microsoft.EntityFrameworkCore;

namespace cwnu_mc_auth_server.Contexts
{
    public class ServerDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Dept> Depts { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data source=");
    }
}
