using PdfChunkService;
using PdfChunkService.Configuration;
using PdfChunkService.Data;
using Microsoft.EntityFrameworkCore;

// Get or create configuration on startup
var config = ConfigurationManager.GetOrCreateConfiguration();

var builder = Host.CreateApplicationBuilder(args);

// Add database context
builder.Services.AddDbContext<PdfChunkDbContext>(options =>
    options.UseSqlite($"Data Source={config.DatabasePath}"));

// Add configuration as singleton
builder.Services.AddSingleton(config);

// Add worker service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PdfChunkDbContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine($"Database initialized at: {config.DatabasePath}");
}

host.Run();
