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
    public DbSet<Session> Sessions { get; set; }
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
        
        modelBuilder.Entity<Session>()
            .HasOne(s => s.Movie)
            .WithMany()
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Session>()
            .HasOne(s => s.Hall)    
            .WithMany()
            .HasForeignKey(s => s.HallId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Session)
            .WithMany()
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Seat)
            .WithMany()
            .HasForeignKey(r => r.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Reservation)
            .WithOne()
            .HasForeignKey<Ticket>(t => t.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.TicketNumber)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<Movie>()
            .Property(m => m.Title)
            .HasMaxLength(200)
            .IsRequired();

        // modelBuilder.Entity<Movie>()
        //     .Property(m => m.Genre)
        //     .HasMaxLength(100)
        //     .IsRequired();

        modelBuilder.Entity<Session>()
            .Property(s => s.BasePrice)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Seat>()
            .HasIndex(s => new
            {
                s.HallId,
                s.RowNumber,
                s.SeatNumber
            })
            .IsUnique();
        
        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new
            {
                r.SessionId,
                r.SeatId
            })
            .IsUnique()
            .HasFilter("[IsCancelled] = 0");
    }
}