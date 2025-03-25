using Gamestore;
using Microsoft.EntityFrameworkCore;
using Gamestore.Data;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy using configuration from appsettings.json
builder.Services.AddCors(options =>
{
    options.AddPolicy("GamestorePolicy",
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
                  .WithMethods(builder.Configuration.GetSection("Cors:AllowedMethods").Get<string[]>())
                  .WithHeaders(builder.Configuration.GetSection("Cors:AllowedHeaders").Get<string[]>());
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

// Add DbContext configuration
builder.Services.AddDbContext<GamestoreContext>(options =>
    options.UseSqlServer("Server=DESKTOP-BV4NA5H;Database=Gamestore;Trusted_Connection=True;TrustServerCertificate=True;"));

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GamestoreContext>();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Apply any pending migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw; // Re-throw to prevent the application from starting with an invalid database
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS before routing and endpoints
app.UseCors("GamestorePolicy");

app.UseOutputCache();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();