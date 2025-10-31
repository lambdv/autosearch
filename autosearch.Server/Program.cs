using autosearch.Data;
using Microsoft.EntityFrameworkCore;
using autosearch.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICarService, TradeMeService>();
// builder.Services.AddScoped<ICarService, TradeMeService>();


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
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
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

app.UseAuthorization();

app.MapControllers();

app.Run();

