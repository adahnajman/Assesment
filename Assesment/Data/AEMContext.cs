using Assesment.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assesment.Data
{
    public class AEMContext : DbContext
    {
        public AEMContext(DbContextOptions<AEMContext> options) : base(options) { }

        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Well> Wells { get; set; }

    }
}
