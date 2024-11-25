using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Internal.Api;
using Heronest.Internal.Auth;
using Heronest.Internal.Database;
using Heronest.Internal.Event;
using Heronest.Internal.Seat;
using Heronest.Internal.Ticket;
using Heronest.Internal.User;
using Heronest.Internal.Venue;
using Microsoft.AspNetCore.Http.Json;
using Npgsql;

namespace Heronest;

public class Program
{
    public static async Task Main(string[] args)
    {
        // TODO: Put this in Configuration
        var connectionString =
            "Server=localhost;Port=5432;UserId=gihyun;Password=password;Database=heronest";
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.MapEnum<Role>();
        dataSourceBuilder.MapEnum<Sex>();

        await using var dataSource = dataSourceBuilder.Build();
        var conn = await dataSource.OpenConnectionAsync();

        var builder = WebApplication.CreateBuilder(args);


        SqlMapperConfig.ConfigureMappers(Assembly.GetExecutingAssembly());

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower)
            );
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

        var authController = new AuthController(new AuthRepository(conn));

        app.MapPost("/api/register", ApiHandler.Handle(authController.Register))
            .WithName("Register")
            .WithOpenApi();
        app.MapPost("/api/login", ApiHandler.Handle(authController.Login))
            .WithName("Login")
            .WithOpenApi();

        var userController = new UserController(new UserRepository(conn));

        app.MapGet("/api/users/{userId}", ApiHandler.Handle(userController.GetById))
            .WithName("GetUser")
            .WithOpenApi();
        app.MapPost("/api/users/{userId}/details", ApiHandler.Handle(userController.CreateDetails))
            .WithName("CreateUserDetail")
            .WithOpenApi();

        var venueController = new VenueController(new VenueRepository(conn));

        app.MapGet("/api/venues", ApiHandler.Handle(venueController.Get))
            .WithName("GetVenues")
            .WithOpenApi();
        app.MapPost("/api/venues", ApiHandler.Handle(venueController.Create))
            .WithName("CreateVenue")
            .WithOpenApi();

        var ticketController = new TicketController(new TicketRepository(conn));

        app.MapPost("/api/tickets", ApiHandler.Handle(ticketController.Create))
            .WithName("CreateTicket")
            .WithOpenApi();
        app.MapPatch("/api/tickets/{ticketId}", ApiHandler.Handle(ticketController.Update))
            .WithName("UpdateTicket")
            .WithOpenApi();

        var seatController = new SeatController(new SeatRepository(conn));

        app.MapPost("/api/seats", ApiHandler.Handle(seatController.Create))
            .WithName("CreateSeat")
            .WithOpenApi();

        var seatSectionController = new SeatSectionController(new SeatSectionRepository(conn));

        app.MapPost("/api/seat-section", ApiHandler.Handle(seatSectionController.Create))
            .WithName("CreateSeatSection")
            .WithOpenApi();

        var eventController = new EventController(new EventRepository(conn));

        app.MapGet("/api/events", ApiHandler.Handle(eventController.Get))
            .WithName("GetEvents")
            .WithOpenApi();
        app.MapPost("/api/events", ApiHandler.Handle(eventController.Create))
            .WithName("CreateEvent")
            .WithOpenApi();

        app.Run();
    }
}
