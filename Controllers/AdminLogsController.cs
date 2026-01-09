using cl_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace cl_be.Controllers
{
  [Route("api/admin/logs")]
  [Authorize(Roles = "Admin")]
  [ApiController]
  public class AdminLogsController : ControllerBase
  {
    private readonly ILogErrorService _logService;
    public AdminLogsController(ILogErrorService logService) => _logService = logService;

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, [FromQuery] string? eventType = null, [FromQuery] int? customerId = null)
    {
      var paged = await _logService.GetPagedAsync(pageIndex, pageSize, eventType, customerId);
      return Ok(paged);
    }

  }
}
