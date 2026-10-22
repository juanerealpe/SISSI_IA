using Microsoft.EntityFrameworkCore;
using SISSI_IA.Application.Interfaces;
using SISSI_IA.Domain.Entities;
namespace SISSI_IA.Infrastructure.Persistence;
public class ApplicationDbContext : DbContext, ISissiDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    public DbSet<Afiliado> Afiliados => Set<Afiliado>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Afiliado>().HasIndex(a => new { a.TipoDocumento, a.NumeroDocumento }).IsUnique();
        modelBuilder.Entity<Afiliado>().Property(a => a.Estado).HasConversion<int>();
    }
}