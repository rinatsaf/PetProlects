using System.Net;
using System.Net.Mail;
using System.Text;
using Application.Abstractions.Services;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class TicketEmailService(IOptions<SmtpOptions> options) : ITicketEmailService
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendTicketsAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(order.User?.Email))
        {
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = $"Your tickets for order #{order.Id}",
            Body = BuildBody(order),
            IsBodyHtml = false
        };
        message.To.Add(order.User.Email);

        using var smtp = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            smtp.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        await smtp.SendMailAsync(message, cancellationToken);
    }

    private static string BuildBody(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Thank you for your purchase. Order #{order.Id}");
        sb.AppendLine($"Paid at: {order.PaidAt:yyyy-MM-dd HH:mm:ss zzz}");
        sb.AppendLine();
        sb.AppendLine("Tickets:");

        foreach (var ticket in order.Tickets.OrderBy(x => x.Id))
        {
            sb.AppendLine($"- TicketCode: {ticket.TicketCode}; Price: {ticket.Price:0.00}; SessionId: {ticket.SessionId}; SeatId: {ticket.SeatId}");
        }

        sb.AppendLine();
        sb.AppendLine("Enjoy your movie!");
        return sb.ToString();
    }
}
