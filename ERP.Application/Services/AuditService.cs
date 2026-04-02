using ERP.Application.Interfaces;

namespace ERP.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _repo;

    public AuditService(IAuditRepository repo)
    {
        _repo = repo;
    }

    public Task WriteSecurityEventAsync(
        int? userId,
        string eventType,
        string? email,
        string ip,
        string? userAgent,
        string? metadata = null)
    {
        return _repo.AddSecurityEventAsync(userId, eventType, email, ip, userAgent, metadata);
    }

    public Task WriteAuditLogAsync(
        string table,
        string action,
        string? oldData,
        string? newData,
        int? userId)
    {
        return _repo.AddAuditLogAsync(table, action, oldData, newData, userId);
    }
}