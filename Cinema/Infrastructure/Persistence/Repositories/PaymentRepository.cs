using Application;
using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PaymentRepository(CinemaDbContext context) : IPaymentRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<Payment?> GetByIdAsync(long id, CurrentUserInfo info, CancellationToken cancellationToken = default)
    {
        return await BuildAccessiblePaymentsQuery(info, asNoTracking: false)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CurrentUserInfo info, CancellationToken cancellationToken = default)
    {
        return await BuildAccessiblePaymentsQuery(info, asNoTracking: false)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);
    }

    public async Task<Payment?> GetByExternalIdUnsafeAsync(string externalPaymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(x => x.Order)
                .ThenInclude(o => o.Tickets)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(x => x.Order)
                .ThenInclude(o => o.Tickets)
                .ThenInclude(t => t.Session)
            .Where(p => p.Status == Domain.Enums.PaymentStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetAccessibleAsync(CurrentUserInfo info, CancellationToken cancellationToken = default)
    {
        return await BuildAccessiblePaymentsQuery(info, asNoTracking: true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    private IQueryable<Payment> BuildAccessiblePaymentsQuery(CurrentUserInfo info, bool asNoTracking)
    {
        if (!info.IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        IQueryable<Payment> query = _context.Payments
            .Include(x => x.Order);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (info.IsAdmin)
        {
            return query;
        }

        if (info.IsCashier)
        {
            return query.Where(x =>
                x.Order.UserId == info.UserId ||
                x.Order.Tickets.Any(t => t.Session.CreatedByUserId == info.UserId));
        }

        return query.Where(x => x.Order.UserId == info.UserId);
    }
}


