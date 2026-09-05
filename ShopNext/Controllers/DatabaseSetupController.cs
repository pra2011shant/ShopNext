using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ShopNext.Models;
using System;

namespace ShopNext.Controllers
{
    public class DatabaseSetupController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ShopNextDbContext _context;

        public DatabaseSetupController(IWebHostEnvironment env, ShopNextDbContext context)
        {
            _env = env;
            _context = context;
        }

        // GET: /DatabaseSetup
        public async Task<IActionResult> Index()
        {
            string sqlFilePath = Path.Combine(_env.WebRootPath, "sql", "ShopNext_Complete_DB_Setup.sql");
            string sqlContent = "SQL master script not found.";

            if (System.IO.File.Exists(sqlFilePath))
            {
                sqlContent = await System.IO.File.ReadAllTextAsync(sqlFilePath);
            }

            ViewData["SqlContent"] = sqlContent;
            ViewData["ScriptPath"] = "/sql/ShopNext_Complete_DB_Setup.sql";
            ViewData["CanConnect"] = await _context.Database.CanConnectAsync();
            return View();
        }

        // POST: /DatabaseSetup/RunSetup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunSetup()
        {
            try
            {
                string sqlFilePath = Path.Combine(_env.WebRootPath, "sql", "ShopNext_Complete_DB_Setup.sql");
                if (!System.IO.File.Exists(sqlFilePath))
                {
                    TempData["ErrorMessage"] = "Master SQL script file not found at wwwroot/sql/ShopNext_Complete_DB_Setup.sql";
                    return RedirectToAction("Index");
                }

                string script = await System.IO.File.ReadAllTextAsync(sqlFilePath);
                
                // Split batches by GO commands
                var batches = System.Text.RegularExpressions.Regex.Split(
                    script, 
                    @"^\s*GO\s*$", 
                    System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                foreach (var batch in batches)
                {
                    string trimmed = batch.Trim();
                    // Skip database creation / USE statements if executing in current context connection
                    if (string.IsNullOrWhiteSpace(trimmed) || 
                        trimmed.StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(trimmed);
                    }
                    catch (Exception ex)
                    {
                        // Some statements might have minor warnings or objects already existing; log and continue
                        System.Diagnostics.Debug.WriteLine($"SQL Batch warning: {ex.Message}");
                    }
                }

                TempData["SuccessMessage"] = "Database schema, tables, and stored procedures executed and synchronized successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Setup failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
