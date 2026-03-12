using Microsoft.AspNetCore.DataProtection;
using NotificationNoise.Ingestion.Application;

namespace NotificationNoise.Ingestion.Infrastructure;

public sealed class TokenProtector : ITokenProtector
{
    private readonly IDataProtector _protector;

    public TokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("NotificationNoise.Ingestion.GmailTokens");
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedText) => _protector.Unprotect(protectedText);
}
