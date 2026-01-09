using cl_be.Models;
using cl_be.Services;
using System.Linq.Dynamic.Core;
namespace cl_be.Repositories
{
  public interface ILogErrorRepository
  {
    Task AddAsync(LogErrorActivities entity, CancellationToken ct = default);
    Task<Models.ErrorPagedResult<LogErrorActivities>> GetPagedAsync(int pageIndex, int pageSize, string? filterEventType = null, int? customerId = null, CancellationToken ct = default);
  }
}
