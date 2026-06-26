using App.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<City> Cities => Set<City>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<EventStatus> EventStatuses => Set<EventStatus>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ReservationStatus> ReservationStatuses => Set<ReservationStatus>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<LogType> LogTypes => Set<LogType>();
    public DbSet<Log> Logs => Set<Log>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
