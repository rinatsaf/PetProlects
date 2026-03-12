using Microsoft.EntityFrameworkCore;
using NotificationNoise.Ingestion.Application;
using NotificationNoise.Ingestion.Domain;
using NotificationNoise.Ingestion.Infrastructure.Persistence;

namespace NotificationNoise.Ingestion.Infrastructure;

public sealed class GmailTokenStore : IGmailTokenStore
{
    private readonly IngestionDbContext _db;
    private readonly ITokenProtector _protector;

    public GmailTokenStore(IngestionDbContext db, ITokenProtector protector)
    {
        _db = db;
        _protector = protector;
    }

    public async Task<GmailTokenData?> GetAsync(string userId, CancellationToken ct)
    {
        var token = await _db.GmailTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == "gmail", ct);

        if (token is null) return null;

        return new GmailTokenData(
            UserId: token.UserId,
            AccessToken: _protector.Unprotect(token.AccessTokenEncrypted),
            RefreshToken: token.RefreshTokenEncrypted is null ? null : _protector.Unprotect(token.RefreshTokenEncrypted),
            ExpiresAt: token.ExpiresAt,
            Scope: token.Scope,
            TokenType: token.TokenType
        );
    }

    public async Task UpsertAsync(GmailTokenData data, CancellationToken ct)
    {
        var token = await _db.GmailTokens
            .FirstOrDefaultAsync(x => x.UserId == data.UserId && x.Provider == "gmail", ct);

        var encAccess = _protector.Protect(data.AccessToken);
        var encRefresh = data.RefreshToken is null ? null : _protector.Protect(data.RefreshToken);
        var now = DateTimeOffset.UtcNow;

        if (token is null)
        {
            token = new GmailToken(
                userId: data.UserId,
                provider: "gmail",
                accessTokenEncrypted: encAccess,
                refreshTokenEncrypted: encRefresh,
                expiresAt: data.ExpiresAt,
                scope: data.Scope,
                tokenType: data.TokenType,
                updatedAt: now);

            _db.GmailTokens.Add(token);
        }
        else
        {
            token.Update(encAccess, encRefresh, data.ExpiresAt, data.Scope, data.TokenType, now);
        }

        await _db.SaveChangesAsync(ct);
    }
}
