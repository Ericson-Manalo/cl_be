using cl_be.Repositories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cl_be.Models
{
  [Table("LogErrorActivities")]
  public class LogErrorActivities : ILogEntity
  {

    [Key]
    public int LogId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;


    public int? CustomerID { get; set; }
    public string? Email { get; set; }
    public int Role { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public string EventType { get; set; } = "";
    public string? EventSource { get; set; }
    public string? ActionName { get; set; }

    public string Message { get; set; } = "";
    public string? ExceptionDetail { get; set; }
    public string? RequestData { get; set; }
    public string? Url { get; set; }
  }
}
