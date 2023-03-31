using Catalyzr.Engine.Models.Client;
using Microsoft.EntityFrameworkCore;

namespace Open.Server
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Client> CZ_Clients { get; set; }
    }
}
