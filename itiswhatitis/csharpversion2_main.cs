using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Database Access Examples");
        Console.WriteLine("=======================\n");

        try
        {
            // Create database access instance
            var db = new DatabaseAccess();

            // 1. Test Connection
            Console.WriteLine("1. Testing Connection...");
            if (db.CheckConnection())
            {
                Console.WriteLine("✓ Connection successful!\n");
            }
            else
            {
                throw new Exception("Connection failed!");
            }

            // 2. Execute Non-Query (INSERT)
            Console.WriteLine("2. Inserting Data...");
            bool insertSuccess = db.ExecuteNonQuery(
                "INSERT INTO Users (Name, Email) VALUES ('John Doe', 'john@example.com')"
            );
            Console.WriteLine(insertSuccess ? "✓ Insert successful!\n" : "× Insert failed!\n");

            // 3. Execute Scalar (COUNT)
            Console.WriteLine("3. Counting Records...");
            int userCount = db.ExecuteScalar(
                "SELECT COUNT(*) FROM Users"
            );
            Console.WriteLine($"✓ User count: {userCount}\n");

            // 4. Execute Query (SELECT)
            Console.WriteLine("4. Querying Data...");
            DataTable users = db.ExecuteQuery(
                "SELECT * FROM Users WHERE Name LIKE 'John%'"
            );
            Console.WriteLine("✓ Users found:");
            foreach (DataRow row in users.Rows)
            {
                Console.WriteLine($"   - {row["Name"]} ({row["Email"]})");
            }
            Console.WriteLine();

            // 5. Execute Stored Procedure
            Console.WriteLine("5. Executing Stored Procedure...");
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", DateTime.Today.AddDays(-30)),
                new SqlParameter("@EndDate", DateTime.Today)
            };
            DataTable procResults = db.ExecuteStoredProcedure("GetUsersByDateRange", parameters);
            Console.WriteLine("✓ Procedure executed successfully!\n");

            // 6. Bulk Copy
            Console.WriteLine("6. Performing Bulk Copy...");
            DataTable newData = new DataTable();
            newData.Columns.Add("Name", typeof(string));
            newData.Columns.Add("Email", typeof(string));
            newData.Rows.Add("Alice Smith", "alice@example.com");
            newData.Rows.Add("Bob Johnson", "bob@example.com");
            bool bulkCopySuccess = db.BulkCopy("Users");
            Console.WriteLine(bulkCopySuccess ? "✓ Bulk copy successful!\n" : "× Bulk copy failed!\n");

            // 7. Execute Transaction
            Console.WriteLine("7. Executing Transaction...");
            bool transactionSuccess = db.ExecuteTransaction(
                "INSERT INTO Users (Name) VALUES ('Transaction User 1')",
                "UPDATE Users SET Email = 'updated@example.com' WHERE Name = 'Transaction User 1'",
                "INSERT INTO Logs (Message) VALUES ('Transaction completed')"
            );
            Console.WriteLine(transactionSuccess ? "✓ Transaction successful!\n" : "× Transaction failed!\n");

        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    // Helper method to display DataTable contents
    static void DisplayTable(DataTable table)
    {
        foreach (DataColumn col in table.Columns)
        {
            Console.Write($"{col.ColumnName,-20}");
        }
        Console.WriteLine("\n" + new string('-', 80));

        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn col in table.Columns)
            {
                Console.Write($"{row[col],-20}");
            }
            Console.WriteLine();
        }
    }
}
