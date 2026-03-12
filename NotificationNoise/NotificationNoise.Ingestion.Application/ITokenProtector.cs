namespace NotificationNoise.Ingestion.Application;

public interface ITokenProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedText);
}
