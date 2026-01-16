using Npgsql;
using System.Data;

namespace AstraFuture.Infrastructure.Persistence;

/// <summary>
/// Contexto de conexão com Supabase PostgreSQL usando Dapper
/// Gerencia conexões e executa queries com Row-Level Security (RLS)
/// </summary>
public class SupabaseContext : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public SupabaseContext(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Obtém conexão ativa (cria se não existir)
    /// </summary>
    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }
    }

    /// <summary>
    /// Define o tenant_id para Row-Level Security
    /// CRÍTICO: Deve ser chamado antes de qualquer query para multi-tenancy funcionar
    /// </summary>
    public async Task SetTenantContextAsync(Guid tenantId)
    {
        var command = (NpgsqlCommand)Connection.CreateCommand();
        // SET LOCAL não aceita parâmetros, mas GUID é seguro pois já vem validado
        command.CommandText = $"SET LOCAL app.tenant_id = '{tenantId}'";

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Inicia uma transação
    /// </summary>
    public IDbTransaction BeginTransaction()
    {
        _transaction = Connection.BeginTransaction();
        return _transaction;
    }

    /// <summary>
    /// Commit da transação ativa
    /// </summary>
    public void Commit()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    /// <summary>
    /// Rollback da transação ativa
    /// </summary>
    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
        _transaction = null;
        _connection = null;
    }
}
