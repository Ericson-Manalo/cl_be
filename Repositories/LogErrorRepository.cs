using cl_be.Models;

using Microsoft.EntityFrameworkCore;

using System.Linq.Dynamic.Core;

namespace cl_be.Repositories
{
  public class LogErrorRepository : ILogErrorRepository
  {
    private readonly ClcredsDbContext _ctx;
    public LogErrorRepository(ClcredsDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(LogErrorActivities entity, CancellationToken ct = default)
    {
      _ctx.LogErrorActivities.Add(entity);
      await _ctx.SaveChangesAsync(ct);
    }

    public async Task<Models.ErrorPagedResult<LogErrorActivities>> GetPagedAsync(int pageIndex, int pageSize, string? filterEventType = null, int? customerId = null, CancellationToken ct = default)
    {
      var query = _ctx.LogErrorActivities.AsNoTracking().AsQueryable();

      if (!string.IsNullOrEmpty(filterEventType))
        query = query.Where(x => x.EventType == filterEventType);

      //if (customerId.HasValue)
      //    query = query.Where(x => x.CustomerID == customerId.Value);
      if (customerId.HasValue)
      {
        // cast esplicito per evitare problemi di tipo con Dynamic LINQ / SQL
        query = query.Where(x => x.CustomerID.HasValue && x.CustomerID.Value == customerId.Value);
      }

      var total = await query.CountAsync(ct);
      var items = await query
          .OrderByDescending(x => x.Timestamp)
          .Skip((pageIndex - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync(ct);

      return new Models.ErrorPagedResult<LogErrorActivities>(items, total, pageIndex, pageSize);
    }


  }
}
