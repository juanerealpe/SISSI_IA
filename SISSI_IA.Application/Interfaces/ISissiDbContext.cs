using Microsoft.EntityFrameworkCore;
using SISSI_IA.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SISSI_IA.Application.Interfaces;

public interface ISissiDbContext
{
    DbSet<Afiliado> Afiliados { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
