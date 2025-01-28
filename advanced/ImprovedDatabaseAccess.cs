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

    // Constructor that takes connection string from configuration
    public ImprovedDatabaseAccess(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string not found in configuration");
    }

    // Basic connection test
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Execute INSERT, UPDATE, DELETE commands
    public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = CreateCommand(connection, sql, parameters);
        
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    // Get a single value (COUNT, SUM, etc.)
    public async Task<T> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = CreateCommand(connection, sql, parameters);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        
        if (result == DBNull.Value || result == null)
            return default;

        return (T)Convert.ChangeType(result, typeof(T));
    }

    // Get multiple rows of data
    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(
        string sql, 
        Dictionary<string, object> parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = CreateCommand(connection, sql, parameters);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        
        return await ReadAllRowsAsync(reader);
    }

    // Execute a stored procedure
    public async Task<List<Dictionary<string, object>>> ExecuteStoredProcedureAsync(
        string procedureName, 
        Dictionary<string, object> parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
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

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        
        return await ReadAllRowsAsync(reader);
    }

    // Helper method to create parameterized commands
    private static SqlCommand CreateCommand(
        SqlConnection connection, 
        string sql, 
        Dictionary<string, object> parameters)
    {
        var command = new SqlCommand(sql, connection);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        return command;
    }

    // Helper method to read all rows from a DataReader
    private static async Task<List<Dictionary<string, object>>> ReadAllRowsAsync(SqlDataReader reader)
    {
        var results = new List<Dictionary<string, object>>();

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

    // Implement IDisposable pattern
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            // Clean up managed resources if needed
        }
        _disposed = true;
    }
} 