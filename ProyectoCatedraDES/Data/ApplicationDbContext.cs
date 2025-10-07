using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyectoCatedraDES.Models;

namespace ProyectoCatedraDES.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
        }
    }
}
