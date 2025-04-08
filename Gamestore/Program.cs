using Gamestore;
using Microsoft.EntityFrameworkCore;
using Gamestore.Data;
using Gamestore.Services;  // Add this line

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

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

// Replace the Kestrel configuration with this:
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
});

// Add HostOptions configuration
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Add global error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred while processing request.");
        throw;
    }
});

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
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS before routing and endpoints
app.UseCors("GamestorePolicy");

// Create wwwroot/games directory if it doesn't exist
if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games")))
{
    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games"));
}

// Configure static files with no caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
        ctx.Context.Response.Headers.Append("Expires", "-1");
    }
});

app.UseStaticFiles();

app.UseOutputCache();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Înlocuiește partea de final cu:
app.Run();