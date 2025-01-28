using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public class ImprovedDatabaseAccess : IDisposable
{
    private readonly string _connectionString;
    private bool _disposed;

    public ImprovedDatabaseAccess(IConfiguration configuration)
    {
        // Get connection string from configuration
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    /// <summary>
    /// Creates a new database connection
    /// </summary>
    private SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Tests the database connection
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Executes a command that doesn't return data (INSERT, UPDATE, DELETE)
    /// </summary>
    public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters = null)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Executes a query and returns a single value
    /// </summary>
    public async Task<T> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters = null)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        
        if (result == DBNull.Value || result == null)
            return default;

        return (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>
    /// Executes a query and returns multiple rows
    /// </summary>
    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters = null)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        var results = new List<Dictionary<string, object>>();
        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.GetValue(i);
                row[reader.GetName(i)] = value == DBNull.Value ? null : value;
            }
            results.Add(row);
        }

        return results;
    }

    /// <summary>
    /// Executes a stored procedure
    /// </summary>
    public async Task<List<Dictionary<string, object>>> ExecuteStoredProcedureAsync(
        string procedureName, 
        Dictionary<string, object> parameters = null)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        var results = new List<Dictionary<string, object>>();
        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.GetValue(i);
                row[reader.GetName(i)] = value == DBNull.Value ? null : value;
            }
            results.Add(row);
        }

        return results;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Add proper cleanup of any remaining connections
            if (_connectionString != null)
            {
                using var connection = CreateConnection();
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Dispose();
            }
        }

        _disposed = true;
    }

    // Add a finalizer
    ~ImprovedDatabaseAccess()
    {
        Dispose(false);
    }

    // Add some helper methods for common scenarios
    /// <summary>
    /// Executes a query and returns results as a list of specified type
    /// </summary>
    public async Task<List<T>> ExecuteQueryAsync<T>(string sql, Dictionary<string, object> parameters = null) where T : class, new()
    {
        var results = new List<T>();
        var dictResults = await ExecuteQueryAsync(sql, parameters);

        foreach (var row in dictResults)
        {
            var item = new T();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                if (row.ContainsKey(prop.Name) && row[prop.Name] != null)
                {
                    prop.SetValue(item, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                }
            }

            results.Add(item);
        }

        return results;
    }

    /// <summary>
    /// Begins a transaction scope
    /// </summary>
    public async Task<SqlTransaction> BeginTransactionAsync()
    {
        var connection = CreateConnection();
        await connection.OpenAsync();
        return await Task.FromResult(connection.BeginTransaction());
    }
} 