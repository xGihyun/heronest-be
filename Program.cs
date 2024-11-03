using System.Text.Json;
using Dapper;
using Heronest.Internal.Auth;
using Heronest.Internal.Database;
using Heronest.Internal.User;
using Microsoft.AspNetCore.Http.Json;
using Npgsql;

namespace Heronest;

public class Program
{
    public static async Task Main(string[] args)
    {
        var connectionString =
            "Server=localhost;Port=5432;User Id=gihyun;Password=password;Database=heronest";
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        await using var dataSource = dataSourceBuilder.Build();
        var conn = await dataSource.OpenConnectionAsync();

        var builder = WebApplication.CreateBuilder(args);

        SqlMapper.SetTypeMap(typeof(User), new SnakeCaseColumnNameMapper(typeof(User)));
        SqlMapper.SetTypeMap(typeof(UserDetail), new SnakeCaseColumnNameMapper(typeof(UserDetail)));
        SqlMapper.SetTypeMap(
            typeof(UserResponse),
            new SnakeCaseColumnNameMapper(typeof(UserResponse))
        );

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        });

        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                MyAllowSpecificOrigins,
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                }
            );
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.UseHttpsRedirection();
        app.UseAuthorization();

        var userService = new UserService(new UserRepository(conn));
        var authService = new AuthService(new AuthRepository(conn));

        app.MapGet("/api/users/{userId}", userService.HandleGetById)
            .WithName("GetUser")
            .WithOpenApi();
        app.MapPost("/api/register", authService.HandleRegister).WithName("Register").WithOpenApi();
        app.MapPost("/api/login", authService.HandleLogin).WithName("Login").WithOpenApi();

        app.Run();
    }
}
