using cl_be.Models.Pagination;
using cl_be.Models;
using cl_be.Models.Dto;
using cl_be.Services;

namespace cl_be.Services
{
  public interface ILogErrorService
  {
    Task LogAsync(LogErrorActivitiesDto dto, CancellationToken ct = default);
    Task<ErrorPagedResult<LogErrorActivitiesDto>> GetPagedAsync(int pageIndex, int pageSize, string? eventType = null, int? customerId = null, CancellationToken ct = default);
  }
}
