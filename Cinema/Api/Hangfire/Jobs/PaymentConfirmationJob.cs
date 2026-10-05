using Application.Abstractions.Services;

namespace Api.Hangfire.Jobs;

public sealed class PaymentConfirmationJob(
    IPaymentService paymentService,
    ILogger<PaymentConfirmationJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var count = await paymentService.SynchronizePendingPaymentsAsync(cancellationToken);

        if (count > 0)
        {
            logger.LogInformation("Payments synchronized: {Count}", count);
        }
    }
}
