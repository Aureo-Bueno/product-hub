using LojaProdutos.Application.Interfaces;
using LojaProdutos.Application.Services;
using LojaProdutos.Domain.Interfaces;
using LojaProdutos.Infrastructure.Data;
using LojaProdutos.Infrastructure.Data.Repositories;
using LojaProdutos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LojaProdutos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost\\SQLEXPRESS;Database=ProjectWeb;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryLogRepository, CategoryLogRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddHttpClient<IGeminiService, GeminiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler();

        return services;
    }
}
