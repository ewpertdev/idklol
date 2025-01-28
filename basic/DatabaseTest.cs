using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class DatabaseTest
{
    // Template connection string - replace with your values
    private string connectionString = @"Server=.\SQLEXPRESS;Database=YourDatabase;Trusted_Connection=True;";

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Successfully connected!");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }
} 