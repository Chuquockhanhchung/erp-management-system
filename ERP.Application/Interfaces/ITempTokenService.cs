using System.Security.Claims;

namespace ERP.Application.Interfaces;

public interface ITempTokenService
{
    string GenerateMfaTempToken(int userId, string email, int ttlMinutes = 5);
    ClaimsPrincipal? ValidateMfaTempTokenRaw(string token);
}