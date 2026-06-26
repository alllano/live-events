using App.Domain.Services;
using App.Infrastructure.Mapping;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();

// Services
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();