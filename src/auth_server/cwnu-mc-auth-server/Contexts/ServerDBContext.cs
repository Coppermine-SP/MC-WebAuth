using System.Configuration;
using cwnu_mc_auth_server.Models;
using Microsoft.EntityFrameworkCore;

namespace cwnu_mc_auth_server.Contexts
{
    public class ServerDBContext : DbContext
    {
        private IConfiguration _config;
        public ServerDBContext(IConfiguration config) => _config = config;
        

        public DbSet<User> Users { get; set; }
        public DbSet<Dept> Depts { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySQL(_config.GetConnectionString("ServerDBContext"));
    }
}
