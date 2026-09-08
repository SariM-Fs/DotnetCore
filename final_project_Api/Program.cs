using final_project_API.Auth;
using final_project_API.BackgroundServices;
using final_project_API.Middlware;
using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Core.Interface;
using final_project_Core.Repositories;
using final_project_Data;
using final_project_Data.Repositories;
using final_project_Service.Mapping;
using final_project_Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog;
using NLog.Web;
using System.Text;


var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- NLog ---
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // --- Configuration-bound values (User Secrets in dev, env vars in prod) ---
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var jwtKey = builder.Configuration["Jwt:Key"]!;

    // --- EF Core ---
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // --- DI: repositories & services (interfaces live in Core, so callers never see Data/Service directly) ---
    builder.Services.AddScoped<ISpotRepository, SpotRepository>();
    builder.Services.AddScoped<IChargingSessionRepository, ChargingSessionRepository>();
    builder.Services.AddScoped<IDriverRepository, DriverRepository>();
    builder.Services.AddScoped<IChargingSpotService, ChargingSpotService>();
    builder.Services.AddScoped<IStationRepository, StationRepository>();
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
    builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
    builder.Services.AddScoped<IAmenityService, AmenityService>();
    builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
    builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<ISpotService, SpotService>();
    builder.Services.AddScoped<IStationService, StationService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

    // --- Session timeout sweep (auto-ends sessions left active too long) ---
    builder.Services.Configure<SessionTimeoutOptions>(
        builder.Configuration.GetSection(SessionTimeoutOptions.SectionName));
    builder.Services.AddHostedService<SessionTimeoutHostedService>();

    // --- AutoMapper ---
    builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
    // --- JWT auth ---
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
    builder.Services.AddAuthorization();

    // --- Controllers / Swagger ---
    // Serialize enums (SpotStatus, PaymentMethod) as their string names, both ways,
    // so request bodies from the client can keep sending "Available"/"CreditCard" etc.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste only the JWT (no 'Bearer ' prefix) — Swagger adds that for you."
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });
    });
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowClient", p =>
            p.WithOrigins("http://localhost:5173")
             .AllowAnyHeader()
             .AllowAnyMethod());
    });
    var app = builder.Build();
    app.UseCors("AllowClient");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Exception handling wraps everything downstream (including CorrelationId), so
    // any unhandled exception anywhere in the pipeline gets turned into uniform JSON.
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    // --- Seed data (dev convenience) ---
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Amenity wifi = null!, cafe = null!, restroom = null!, fastCharge = null!;
        if (!db.Amenities.Any())
        {
            wifi = new Amenity { Name = "WiFi" };
            cafe = new Amenity { Name = "Cafe" };
            restroom = new Amenity { Name = "Restroom" };
            fastCharge = new Amenity { Name = "Fast Charging" };
            db.Amenities.AddRange(wifi, cafe, restroom, fastCharge);
            db.SaveChanges();
        }
        else
        {
            wifi = db.Amenities.First(a => a.Name == "WiFi");
            cafe = db.Amenities.First(a => a.Name == "Cafe");
            restroom = db.Amenities.First(a => a.Name == "Restroom");
            fastCharge = db.Amenities.First(a => a.Name == "Fast Charging");
        }

        if (!db.ChargingStations.Any())
        {
            var stationDefs = new[]
            {
                new { Name = "Downtown Garage",    Location = "1 Main St",     Connector = "Type2", PowerKw = 50.0,  Amenities = new[] { wifi, cafe } },
                new { Name = "Mall Parking Lot",   Location = "22 Market Ave", Connector = "CCS",   PowerKw = 75.0,  Amenities = new[] { wifi, restroom } },
                new { Name = "Airport Terminal B", Location = "Airport Rd",    Connector = "CCS",   PowerKw = 120.0, Amenities = new[] { restroom, fastCharge } },
            };

            foreach (var def in stationDefs)
            {
                var station = new ChargingStation
                {
                    Name = def.Name,
                    Location = def.Location,
                    ConnectorType = def.Connector,
                    PowerKw = def.PowerKw
                };
                foreach (var amenity in def.Amenities)
                {
                    station.Amenities.Add(amenity);
                }

                // 6 spots per station; mostly Available, a couple Occupied/OutOfOrder
                // so the dashboard shows a realistic mix right away.
                for (int i = 1; i <= 6; i++)
                {
                    var status = i switch
                    {
                        2 => SpotStatus.Occupied,
                        5 => SpotStatus.OutOfOrder,
                        _ => SpotStatus.Available
                    };
                    station.Spots.Add(new ChargingSpot { SpotNumber = i, Status = status });
                }

                db.ChargingStations.Add(station);
            }

            db.SaveChanges();
        }
        var adminEmail = builder.Configuration["AdminSeed:Email"];
        var adminPassword = builder.Configuration["AdminSeed:Password"];

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword)
            && !db.Drivers.Any(d => d.Email == adminEmail))
        {
            db.Drivers.Add(new Driver
            {
                Name = "Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                LicensePlate = "",
                Role = "Admin"
            });
            db.SaveChanges();
        }
    }

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped because of an exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
