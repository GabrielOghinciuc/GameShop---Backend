using Gamestore.Data;
using Gamestore.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// services used in controllers
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddDbContext<GamestoreContext>(options =>
    options.UseSqlServer("Server=DESKTOP-BV4NA5H;Database=Gamestore;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("GamestorePolicy",
        policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
                       .WithMethods(builder.Configuration.GetSection("Cors:AllowedMethods").Get<string[]>())
                       .WithHeaders(builder.Configuration.GetSection("Cors:AllowedHeaders").Get<string[]>()));
});

// Swagger - keep for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Output caching - used in GamesController
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

// Development tools - Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Essential middleware
app.UseCors("GamestorePolicy");
app.UseStaticFiles();
app.UseOutputCache();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure games directory exists (used by FileStorage service)
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games"));

app.Run();