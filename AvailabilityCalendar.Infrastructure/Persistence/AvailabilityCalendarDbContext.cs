using AvailabilityCalendar.Domain.Entities;
using AvailabilityCalendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityCalendar.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the application domain
/// and the ASP.NET Core Identity tables.
/// </summary>
public class AvailabilityCalendarDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    /// <summary>
    /// Creates a new database context instance with the configured options.
    /// </summary>
    public AvailabilityCalendarDbContext(
        DbContextOptions<AvailabilityCalendarDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Domain users used by the calendar system.
    /// </summary>
    public DbSet<User> DomainUsers => Set<User>();

    /// <summary>
    /// Calendar events stored by the application.
    /// </summary>
    public DbSet<Event> Events => Set<Event>();

    /// <summary>
    /// Join table entity between users and events.
    /// </summary>
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();

    /// <summary>
    /// Configures all entity mappings for both the domain and identity model.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureEvent(modelBuilder);
        ConfigureEventParticipant(modelBuilder);
    }

    /// <summary>
    /// Configures the <see cref="User"/> entity.
    /// </summary>
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            // A user participates in many events through the join entity.
            entity.HasMany(x => x.EventParticipants)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the <see cref="Event"/> entity.
    /// </summary>
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

            // Events are linked to users only through EventParticipant.
            entity.HasMany(x => x.Participants)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes supporting common time-based event queries.
            entity.HasIndex(x => x.Start);
            entity.HasIndex(x => x.End);
        });
    }

    /// <summary>
    /// Configures the <see cref="EventParticipant"/> join entity.
    /// </summary>
    private static void ConfigureEventParticipant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventParticipant>(entity =>
        {
            entity.ToTable("EventParticipants");

            // Composite primary key ensures the same user
            // cannot be added twice to the same event.
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