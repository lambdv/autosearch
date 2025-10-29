using autosearch.Data;
using Microsoft.EntityFrameworkCore;
using autosearch.Services;
using Microsoft.AspNetCore.SpaServices.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICarService, TradeMeService>();
builder.Services.AddSpaStaticFiles(options =>
{
    //store built spa assets in wwwroot
    options.RootPath = "wwwroot";
});

//db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

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
app.UseSpaStaticFiles();

app.UseAuthorization();

app.MapControllers();

//spa fallback and dev proxy
app.UseSpa(spa =>
{
    //point to client source for dev
    spa.Options.SourcePath = Path.Combine(app.Environment.ContentRootPath, "..", "autosearch.client");

    if (app.Environment.IsDevelopment())
    {
        //proxy to vite/react-router dev server
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    }
});

app.Run();

