using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Entregas.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Models.Entrega> Entregas { get; set; }
    }
}
