using MimeKit;
using MailKit.Net.Smtp;
using TouristRoutes.Interfaces.Services;

namespace TouristRoutes.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfiguration _config = configuration;

    public async Task Send2FaCodeAsync(string email, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Tourist Routes", _config["Email:From"]));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Your 2FA Verification Code";

        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is: {code}"
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _config["Email:Host"],
            int.Parse(_config["Email:Port"]),
            MailKit.Security.SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            _config["Email:User"],
            _config["Email:Password"]
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}