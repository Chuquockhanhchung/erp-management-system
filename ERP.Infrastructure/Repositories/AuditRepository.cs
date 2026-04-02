using ERP.Application.Interfaces;
using ERP.Infrastructure.Persistence.Entities;

namespace ERP.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _db;

    public AuditRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddSecurityEventAsync(int? userId, string eventType, string? email, string ip, string? userAgent, string? metadata)
    {
        _db.SecurityEvents.Add(new SecurityEvent
        {
            UserId = userId,
            EventType = eventType,
            Email = email,
            IpAddress = ip,
            UserAgent = userAgent,
            Metadata = metadata,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task AddAuditLogAsync(string table, string action, string? oldData, string? newData, int? userId)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            TableName = table,
            Action = action,
            OldData = oldData,
            NewData = newData,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}