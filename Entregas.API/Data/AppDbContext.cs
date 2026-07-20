using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Entregas.Shared;

namespace Entregas.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EntregaModel> Entregas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Le decimos a EF que ignore la lista porque tú la manejas como JSON
            modelBuilder.Entity<EntregaModel>()
                .Ignore(e => e.Productos);
        }
    }

}
