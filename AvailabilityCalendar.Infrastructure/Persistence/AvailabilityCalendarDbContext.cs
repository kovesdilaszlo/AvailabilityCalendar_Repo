using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityCalendar.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core context for availability calendar data and identity.
/// </summary>
public class AvailabilityCalendarDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AvailabilityCalendarDbContext(
        DbContextOptions<AvailabilityCalendarDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();

    /// <summary>
    /// Configures entity mappings for domain and identity models.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureEvent(modelBuilder);
        ConfigureEventParticipant(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasMany(x => x.CreatedEvents)
                .WithOne(x => x.CreatedByUser)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.EventParticipants)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Start)
                .IsRequired();

            entity.Property(x => x.End)
                .IsRequired();

            entity.Property(x => x.CreatedByUserId)
                .IsRequired();

            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedEvents)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Participants)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.Start);
            entity.HasIndex(x => x.End);
            entity.HasIndex(x => x.CreatedByUserId);
        });
    }

    private static void ConfigureEventParticipant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventParticipant>(entity =>
        {
            entity.ToTable("EventParticipants");

            entity.HasKey(x => new { x.EventId, x.UserId });

            entity.HasOne(x => x.Event)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(x => x.EventParticipants)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId);
        });
    }
}