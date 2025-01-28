using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public class ExampleUsage
{
    private readonly ImprovedDatabaseAccess _db;

    public ExampleUsage(IConfiguration configuration)
    {
        _db = new ImprovedDatabaseAccess(configuration);
    }

    public async Task Examples()
    {
        // Test connection
        if (!await _db.TestConnectionAsync())
        {
            Console.WriteLine("Failed to connect!");
            return;
        }

        // Insert data
        var insertParams = new Dictionary<string, object>
        {
            ["@name"] = "John Doe",
            ["@email"] = "john@example.com"
        };
        
        await _db.ExecuteNonQueryAsync(
            "INSERT INTO Users (Name, Email) VALUES (@name, @email)",
            insertParams
        );

        // Get count
        int userCount = await _db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Users"
        );

        // Get all users
        var users = await _db.ExecuteQueryAsync(
            "SELECT * FROM Users WHERE Active = @active",
            new Dictionary<string, object> { ["@active"] = true }
        );

        // Call stored procedure
        var procParams = new Dictionary<string, object>
        {
            ["@startDate"] = DateTime.Today,
            ["@endDate"] = DateTime.Today.AddDays(7)
        };
        
        var report = await _db.ExecuteStoredProcedureAsync(
            "GenerateReport",
            procParams
        );
    }
} 