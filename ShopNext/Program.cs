using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using ShopNext.Data;
using ShopNext.Models;
using ShopNext.Services;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Response Compression for Ultra-Fast HTML, JSON, CSS, and JS transfer
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "text/html",
        "text/css",
        "application/javascript",
        "application/json",
        "text/json",
        "image/svg+xml"
    });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// In-Memory Caching for performance optimization (Point 30)
builder.Services.AddMemoryCache();

// Antiforgery configuration (Point 29)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 1. SQL Server connection string registration
builder.Services.AddDbContext<ShopNextDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

// 2. Register Services (SOLID - Dependency Inversion & Interface Segregation)
builder.Services.AddScoped<ShopNextService>();
builder.Services.AddScoped<IShopNextService>(sp => sp.GetRequiredService<ShopNextService>());
builder.Services.AddScoped<ICustomerService>(sp => sp.GetRequiredService<ShopNextService>());
builder.Services.AddScoped<IProductService>(sp => sp.GetRequiredService<ShopNextService>());
builder.Services.AddScoped<IOrderService>(sp => sp.GetRequiredService<ShopNextService>());
builder.Services.AddScoped<IVendorService>(sp => sp.GetRequiredService<ShopNextService>());
builder.Services.AddScoped<IRiderService>(sp => sp.GetRequiredService<ShopNextService>());
builder.Services.AddScoped<IAdminReportService>(sp => sp.GetRequiredService<ShopNextService>());
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICustomerRiskService, CustomerRiskService>();
builder.Services.AddScoped<IAddressRiskService, AddressRiskService>();
builder.Services.AddScoped<IMultiAccountDetectionService, MultiAccountDetectionService>();
builder.Services.AddScoped<ICodAbuseService, CodAbuseService>();
builder.Services.AddScoped<ISaleCampaignService, SaleCampaignService>();
builder.Services.AddScoped<IFlashSaleService, FlashSaleService>();
builder.Services.AddScoped<IInventoryProtectionService, InventoryProtectionService>();
builder.Services.AddScoped<ISaleFraudMonitoringService, SaleFraudMonitoringService>();
builder.Services.AddScoped<IAdvancedEcommerceService, AdvancedEcommerceService>();
builder.Services.AddScoped<ISystemMonitoringService, SystemMonitoringService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddHostedService<CampaignSchedulerBackgroundService>();

// 3. Point 46 & 29: Security & Role-Based Authentication Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SellerOnly", policy => policy.RequireRole("Seller", "Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer", "Admin"));
    options.AddPolicy("RiderOnly", policy => policy.RequireRole("Rider", "Admin"));
});

var app = builder.Build();

// Seed Database automatically from SQL Server / EF Core (Point 33, 34, 35)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ShopNextDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Point 28: Global Exception & Status Code Handler
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseResponseCompression();
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseStaticFiles();

app.UseRouting();

// Authentication Middleware must precede Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();