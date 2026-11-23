using Apicafe.Models;
using Microsoft.EntityFrameworkCore;

namespace Apicafe.Data;

// Contexto do EF Core: ponte entre a entidade CafeEntity e o banco SQLite.
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public DbSet<CafeEntity> Cafes => Set<CafeEntity>();
}
