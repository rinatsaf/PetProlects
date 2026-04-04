using Application.DTOs.Tickets;
using FluentValidation;

namespace Application.Validation;

public sealed class IssueTicketsRequestValidator : AbstractValidator<IssueTicketsRequest>
{
    public IssueTicketsRequestValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Seats).NotEmpty();
        RuleForEach(x => x.Seats).ChildRules(seat =>
        {
            seat.RuleFor(s => s.SeatId).GreaterThan(0);
            seat.RuleFor(s => s.TicketCode).NotEmpty();
        });
    }
}
