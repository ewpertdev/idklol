using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class DatabaseTest
{
    // This is where you put YOUR server and database name
    private string connectionString = @"Server=.\SQLEXPRESS;Database=master;Trusted_Connection=True;";

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            // Try to connect
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            // If anything goes wrong, show the error
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }
} 