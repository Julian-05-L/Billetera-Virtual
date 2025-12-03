using Microsoft.EntityFrameworkCore;

namespace BilleteraVirtual.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Transaccion> Transacciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaccion>()
                .Property(t => t.CryptoAmount)
                .HasPrecision(18,8);

            modelBuilder.Entity<Transaccion>()
                .Property(t => t.Money)
                .HasPrecision(18,2);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.SaldoPesos)
                .HasPrecision(18,8);
        }
    }
}
