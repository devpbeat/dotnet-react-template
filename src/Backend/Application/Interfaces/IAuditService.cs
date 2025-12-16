namespace Backend.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string resource, string details, Guid? userId = null, string ipAddress = "", string userAgent = "");
}
