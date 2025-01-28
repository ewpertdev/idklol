# SQL Server Code Templates

Simple, reusable templates for SQL Server database access in C#.

## Quick Start

1. Clone this repository
2. Open solution in Visual Studio
3. Install required NuGet packages:
   - System.Data.SqlClient
   - Microsoft.Extensions.Configuration

## Connection String Setup

1. Find your SQL Server name in SSMS
2. Update connection string in either:
   - DatabaseTest.cs
   - App.config
   - ImprovedDatabaseAccess.cs

## Common Issues

1. "Cannot connect to server"
   - Check if SQL Server is running
   - Verify server name
   - Check Windows Services

2. "Login failed"
   - Verify Windows Authentication is enabled
   - Check user permissions

## Folder Structure

- /basic - Simple connection examples
- /advanced - More complex database operations
- /config - Configuration templates
- /docs - Additional documentation

## Usage Examples

See /docs/getting-started.md for detailed examples. 