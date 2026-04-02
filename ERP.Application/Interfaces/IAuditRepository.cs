using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    public interface IAuditRepository
    {
        Task AddSecurityEventAsync(int? userId, string eventType, string? email, string ip, string? userAgent, string? metadata);
        Task AddAuditLogAsync(string table, string action, string? oldData, string? newData, int? userId);
    }
}
