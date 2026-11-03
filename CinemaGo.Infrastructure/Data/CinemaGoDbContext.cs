using CinemaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Data;

public class CinemaGoDbContext : DbContext
{
    public CinemaGoDbContext(
        DbContextOptions<CinemaGoDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Session>  Sessions { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Hall)
            .WithMany(h => h.Seats)
            .HasForeignKey(s => s.HallId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}