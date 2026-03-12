namespace NotificationNoise.DigestSender.Application;

public interface IDigestDelivery
{
    Task SendAsync(string userId, string period, string payloadJson, CancellationToken ct);
}
