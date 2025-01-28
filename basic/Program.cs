using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Database Connection Tester ===\n");
        
        var test = new DatabaseTest();
        
        if (await test.TestConnectionAsync())
        {
            Console.WriteLine("Connection successful!");
        }
        else
        {
            Console.WriteLine("Connection failed!");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
} 