using cl_be.Models.Dto;
using cl_be.Services;
using System.Security.Claims;
namespace cl_be.Middleware
{
  public class ErrorLoggingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }



    public async Task Invoke(HttpContext context, ILogErrorService logService)
    {
      try
      {
        await _next(context);

        //  LOG 401 / 403 (authorization errors)
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
            context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
          var dto = new LogErrorActivitiesDto
          {
            Timestamp = DateTime.UtcNow,
            EventType = "AuthorizationError",
            EventSource = context.Request.Path,
            ActionName = context.Request.Method,
            Message = context.Response.StatusCode == 401
                  ? "Unauthorized access"
                  : "Forbidden access",
            Url = context.Request.Path + context.Request.QueryString,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString()
          };

          // Claims (se presenti)
          if (context.User?.Identity?.IsAuthenticated == true)
          {
            var custClaim = context.User.FindFirst("customerId");
            var emailClaim = context.User.FindFirst(ClaimTypes.Email);
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);

            if (custClaim != null && int.TryParse(custClaim.Value, out var cid))
              dto.CustomerID = cid;

            dto.Email = emailClaim?.Value;

            if (roleClaim != null && int.TryParse(roleClaim.Value, out var r))
              dto.Role = r;
            else
              dto.Role = 1;
          }
          else
          {
            dto.Role = 0; // non loggato
          }

          try
          {
            await logService.LogAsync(dto);
          }
          catch (Exception logEx)
          {
            _logger.LogError(logEx, "Failed to save authorization log");
          }
        }
      }
      catch (Exception ex)
      {
        //LOG 500 
        var dto = new LogErrorActivitiesDto
        {
          Timestamp = DateTime.UtcNow,
          EventType = "UnhandledException",
          EventSource = context.Request.Path,
          ActionName = context.Request.Method,
          Message = ex.Message,
          ExceptionDetail = ex.ToString(),
          Url = context.Request.Path + context.Request.QueryString,
          IpAddress = context.Connection.RemoteIpAddress?.ToString(),
          UserAgent = context.Request.Headers["User-Agent"].ToString()
        };

        if (context.User?.Identity?.IsAuthenticated == true)
        {
          var custClaim = context.User.FindFirst("customerId") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
          var emailClaim = context.User.FindFirst(ClaimTypes.Email);
          var roleClaim = context.User.FindFirst(ClaimTypes.Role);

          if (custClaim != null && int.TryParse(custClaim.Value, out var cid))
            dto.CustomerID = cid;

          dto.Email = emailClaim?.Value;

          if (roleClaim != null && int.TryParse(roleClaim.Value, out var r))
            dto.Role = r;
          else
            dto.Role = dto.CustomerID.HasValue ? 1 : 0;
        }
        else
        {
          dto.Role = 0;
        }

        try
        {
          await logService.LogAsync(dto);
        }
        catch (Exception logEx)
        {
          _logger.LogError(logEx, "Failed to save error log");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("An error occurred.");
      }
    }


  }
}
