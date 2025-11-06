using autosearch.Data;
using Microsoft.EntityFrameworkCore;
using autosearch.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// builder.Services.AddScoped<ICarService, TradeInClearanceCarsService>();


builder.Services.AddScoped<HttpClient>(_ =>
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
    return client;
});


//db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

//redis
// var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
// try
// {
//     // Test Redis connection
//     var options = ConfigurationOptions.Parse(redisConnectionString);
//     options.AbortOnConnectFail = false;
//     var mux = ConnectionMultiplexer.Connect(options);
//     var db = mux.GetDatabase();
//     await db.PingAsync(); // This will throw if Redis is not available
//     builder.Services.AddSingleton<ICacheService>(_ => new RedisCacheService(redisConnectionString));
// }
// catch
// {
    // builder.Services.AddScoped<ICacheService, SqliteCacheService>();
    builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// }

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


//db scope
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

