using cl_be.Models.Pagination;
using cl_be.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using cl_be.Models;
using cl_be.Models.Dto;


namespace cl_be.Services
{
  public class LogErrorService : ILogErrorService
  {
    private readonly ILogErrorRepository _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LogErrorService> _logger;

    public LogErrorService(ILogErrorRepository repo,
                           IHttpContextAccessor httpContextAccessor,
                           ILogger<LogErrorService> logger)
    {
      _repo = repo;
      _httpContextAccessor = httpContextAccessor;
      _logger = logger;
    }

    public async Task LogAsync(LogErrorActivitiesDto dto, CancellationToken ct = default)
    {
      var http = _httpContextAccessor.HttpContext;

      var entity = new LogErrorActivities
      {
        // Timestamp: se non fornito dal caller, metti UTC now
        Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp,

        // User info (da claims se presenti) - preferisci claim se dto.CustomerID null
        CustomerID = dto.CustomerID ?? GetCustomerIdFromClaims(http),
        Email = dto.Email ?? GetEmailFromClaims(http),
        Role = dto.Role, // se vuoi puoi derivarlo da claims: int.Parse(GetRoleFromClaims(http) ?? "0")

        // Request / env info
        IpAddress = dto.IpAddress ?? http?.Connection?.RemoteIpAddress?.ToString(),
        UserAgent = dto.UserAgent ?? http?.Request?.Headers["User-Agent"].ToString(),
        Url = dto.Url ?? (http != null ? (http.Request.Path + http.Request.QueryString) : null),
        ActionName = dto.ActionName ?? http?.GetEndpoint()?.DisplayName,
        EventSource = dto.EventSource ?? "Application",

        // Event data
        EventType = dto.EventType,
        Message = dto.Message,
        ExceptionDetail = dto.ExceptionDetail,
        RequestData = dto.RequestData
      };

      try
      {
        await _repo.AddAsync(entity, ct);

        Console.WriteLine($"[TEST] Log salvato: {entity.EventType} per CustomerID={entity.CustomerID}, Timestamp={entity.Timestamp}");
        _logger.LogInformation("Log salvato nel DB: EventType={EventType}, CustomerID={CustomerID}", entity.EventType, entity.CustomerID);
      }
      catch (DbUpdateException dbEx) when (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 547)
      {
        // FK violation: prob. CustomerID non esiste — fallback a anonimo e riprova
        _logger.LogWarning(dbEx, "FK violation inserendo log per CustomerID={CustomerID}, riprovo come anonimo", dto.CustomerID);
        try
        {
          entity.CustomerID = null;
          await _repo.AddAsync(entity, ct);
          _logger.LogInformation("Log salvato come anonimo dopo FK violation: EventType={EventType}", entity.EventType);
        }
        catch (Exception ex2)
        {
          _logger.LogError(ex2, "Fallita anche la scrittura fallback anonimo");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to persist log to DB");
      }
    }

    public async Task<ErrorPagedResult<LogErrorActivitiesDto>> GetPagedAsync(int pageIndex, int pageSize, string? eventType = null, int? customerId = null, CancellationToken ct = default)
    {
      var paged = await _repo.GetPagedAsync(pageIndex, pageSize, eventType, customerId, ct);

      var items = paged.Items.Select(e => new LogErrorActivitiesDto
      {

        CustomerID = e.CustomerID,
        Email = e.Email,
        Role = e.Role,
        Timestamp = e.Timestamp,
        EventType = e.EventType,
        EventSource = e.EventSource,
        ActionName = e.ActionName,
        Message = e.Message,
        ExceptionDetail = e.ExceptionDetail,
        RequestData = e.RequestData,
        Url = e.Url,
        IpAddress = e.IpAddress,
        UserAgent = e.UserAgent
      }).ToList();

      return new ErrorPagedResult<LogErrorActivitiesDto>(paged.Items, paged.TotalItems, paged.PageIndex, paged.PageSize);
    }

    // -------------------------
    // helper privati
    // -------------------------

    private int? GetCustomerIdFromClaims(HttpContext? http)
    {
      var id = http?.User?.FindFirst("CustomerID")?.Value;
      if (string.IsNullOrEmpty(id)) return null;
      return int.TryParse(id, out var c) ? c : (int?)null;
    }

    private string? GetEmailFromClaims(HttpContext? http)
    {
      return http?.User?.FindFirst("email")?.Value;
    }
  }
}
