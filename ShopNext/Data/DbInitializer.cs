using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNext.Models;

namespace ShopNext.Data
{
    /// <summary>
    /// Database Initializer for ShopNext Platform
    /// Database schema, seed data, and stored procedures are managed centrally 
    /// via the master SQL file (Database/ShopNext_Complete_Schema_And_Stored_Procedures.sql).
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedAsync(ShopNextDbContext context)
        {
            // Ensure Database schema exists via EF Core
            await context.Database.EnsureCreatedAsync();
        }
    }
}
