using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing database connection...");
        
        // Create the test class
        var test = new DatabaseTest();
        
        // Try to connect
        if (await test.TestConnectionAsync())
        {
            Console.WriteLine("It worked! We connected!");
        }
        else
        {
            Console.WriteLine("Connection failed!");
        }
        
        // Keep window open
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
} 