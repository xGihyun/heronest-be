namespace Heronest.Infrastructure.UnitOfWork;

using Npgsql;

// NOTE: This whole thing isn't used since transactions just won't work idk why
public interface IUnitOfWork
{
    NpgsqlConnection Connection { get; }
    NpgsqlTransaction? Transaction { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly NpgsqlDataSource DataSource;
    public NpgsqlConnection? Connection { get; private set; }
    public NpgsqlTransaction? Transaction { get; private set; }

    public UnitOfWork(NpgsqlDataSource dataSource)
    {
        this.DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task BeginTransactionAsync()
    {
        if (this.Connection is not null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        this.Connection = await this.DataSource.OpenConnectionAsync();
        this.Transaction = await this.Connection.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (this.Transaction is null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        await this.Transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (this.Transaction is null)
        {
            throw new InvalidOperationException("No active transaction to rollback.");
        }

        await this.Transaction.RollbackAsync();
    }

    private async Task DisposeTransactionAndConnection()
    {
        if (this.Transaction is not null)
        {
            await Transaction.DisposeAsync();
            Transaction = null;
        }

        if (this.Connection is not null)
        {
            await Connection.DisposeAsync();
            Connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAndConnection();
    }
}
