
using CinemaGo.API.Middleware;
using Microsoft.OpenApi;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using CinemaGo.Infrastructure.Services;
using CinemaGo.Infrastructure.BackgroundServices;

using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Services;
using CinemaGo.Infrastructure.Repositories;

using CinemaGo.Domain.Entities;
using Microsoft.AspNetCore.Identity;

using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register CinemaGo database context
builder.Services.AddDbContext<CinemaGoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// User Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();

// User Profile Service
builder.Services.AddScoped<IUserService, UserService>();

// Authentication
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Movies
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();

// Halls
builder.Services.AddScoped<IHallRepository, HallRepository>();
builder.Services.AddScoped<IHallService, HallService>();

// Seats
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ISeatService, SeatService>();

// Sessions
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionService, SessionService>();

// Reservations
builder.Services.AddScoped<ReservationRepository>();

builder.Services.AddScoped<IReservationRepository>(sp =>
    sp.GetRequiredService<ReservationRepository>());

builder.Services.AddScoped<IReservationService, ReservationService>();

// Background service for expired reservation holds
builder.Services.AddHostedService<ReservationExpirationService>();

// Tickets
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();

// JWT Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// JWT Configuration
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
                   ?? throw new InvalidOperationException(
                       "JWT Secret Key is not configured.");

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer is not configured.");

var jwtAudience = builder.Configuration["JwtSettings:Audience"]
                  ?? throw new InvalidOperationException(
                      "JWT Audience is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Global Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// OpenAPI and Swagger JWT configuration
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token."
            };

        document.Security ??= new List<OpenApiSecurityRequirement>();

        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Global Exception Handling Middleware
app.UseExceptionHandler();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "CinemaGo API v1"
        );
    });
}

// HTTPS redirection disabled for local development
// app.UseHttpsRedirection();

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// API Controllers
app.MapControllers();

// Start application
app.Run();
