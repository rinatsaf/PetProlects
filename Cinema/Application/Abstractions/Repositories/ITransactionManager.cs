namespace Application.Abstractions.Repositories;

public interface ITransactionManager
{
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
