using cl_be.Models.Dto;

namespace cl_be.Models.Dto
{
  public class LogErrorActivitiesDto
  {

    public int? CustomerID { get; set; }         // null => user non registrato
    public string? Email { get; set; }           // email se presente
    public int Role { get; set; }            // 0 = non registrato, 1 = registrato, 2 = admin

    public DateTime Timestamp { get; set; }      // quando è avvenuto (server side)
    public string EventType { get; set; } = "";  // Error, Info, LoginFailed, PasswordReset, ApiException...
    public string? EventSource { get; set; }     // es. ProductController, AuthService
    public string? ActionName { get; set; }      // azione / metodo
    public string Message { get; set; } = "";    // descrizione breve
    public string? ExceptionDetail { get; set; } // stacktrace o details
    public string? RequestData { get; set; }     // JSON string dei dati di request (attenzione a PII)
    public string? Url { get; set; }             // endpoint chiamato
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
  }
}
