namespace ERP.Application.Interfaces;

public interface IDataProtectorService
{
    string Protect(string plainText);
    string Unprotect(string cipherText);
}