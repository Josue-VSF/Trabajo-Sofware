using SgikmMvp1.Api.Services;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://localhost:5187");
}

builder.Services.AddControllers();
builder.Services.AddSingleton<InventoryStore>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapGet("/api/salud", () => Results.Ok(new
{
    estado = "ok",
    servicio = "ISI-SGIKM-INT · MVP 1",
    hora = DateTimeOffset.Now
}));

app.Run();
