using System.Text.Json;
using Dapper;
using Heronest.Database;
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

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapGet(
                "/users",
                (HttpContext httpContext) =>
                {
                    var sql = "SELECT * FROM users";
                    var users = conn.Query<User>(sql).ToList();

                    return users;
                }
            )
            .WithName("GetUsers")
            .WithOpenApi();

        app.MapPost(
            "/users",
            (HttpContext httpContext) =>
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    var sql =
                        @"
                        INSERT INTO users (email, password, role) 
                        VALUES (@Email, @Password, @Role::role)
                        ";

                    var user = new
                    {
                        Email = "test@gmail.com",
                        Password = "password",
                        Role = "admin",
                    };

                    conn.Execute(sql, user);
                }
            }
        );

        app.Run();
    }
}
