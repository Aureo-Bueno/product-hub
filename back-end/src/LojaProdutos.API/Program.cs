using LojaProdutos.Infrastructure;
using LojaProdutos.Infrastructure.Data;
using LojaProdutos.Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Ensuring database and schema are created...");
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Database ready.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not initialize database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapGet("/swagger", async (HttpContext ctx) =>
    {
        ctx.Response.Redirect("/swagger/index.html");
        await Task.CompletedTask;
    });

    app.MapGet("/swagger/index.html", async (HttpContext ctx) =>
    {
        ctx.Response.ContentType = "text/html";
        await ctx.Response.WriteAsync("""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>Loja Produtos API - Swagger UI</title>
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css" />
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
  <script>
    SwaggerUIBundle({ url: '/openapi/v1.json', dom_id: '#swagger-ui' });
  </script>
</body>
</html>
""");
    });
}

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
