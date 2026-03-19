## Infrastructure Layer

The Infrastructure layer contains persistence, database schema definitions, repository implementations, Identity integration, and seed data.

### Persistence Context

#### AvailabilityCalendarDbContext

Inherits from:

```text
IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

**DbSets**
- `DbSet<User> DomainUsers`
- `DbSet<Event> Events`
- `DbSet<EventParticipant> EventParticipants`

**Methods**
- constructor
- `OnModelCreating(ModelBuilder modelBuilder)`
- `ConfigureUser(ModelBuilder modelBuilder)`
- `ConfigureEvent(ModelBuilder modelBuilder)`
- `ConfigureEventParticipant(ModelBuilder modelBuilder)`

**Role**
- central EF Core database context
- stores both Identity and domain-level persistence
- defines table names, relationships, keys, and indexes

### Entity Configuration Details

#### User mapping
- table: `Users`
- key: `Id`
- `Name` required, max length 100
- one-to-many with `CreatedEvents`
- one-to-many with `EventParticipants`

#### Event mapping
- table: `Events`
- key: `Id`
- `Title` required, max length 200
- `Start` required
- `End` required
- `CreatedByUserId` required
- index on `Start`
- index on `End`
- index on `CreatedByUserId`

#### EventParticipant mapping
- table: `EventParticipants`
- composite key: `{ EventId, UserId }`
- index on `UserId`

### Repository Implementations

#### EventRepository

**Field**
- `AvailabilityCalendarDbContext _dbContext`

**Methods**
- constructor
- `GetByIdAsync(Guid id)`
- `GetByUsersAsync(List<Guid> userIds, TimeInterval range)`
- `AddAsync(Event entity)`
- `UpdateAsync(Event entity)`
- `DeleteAsync(Guid id)`

**Role**
- loads and persists event data
- applies interval overlap filtering at database level where possible
- includes participants during reads

#### UserRepository

**Field**
- `AvailabilityCalendarDbContext _dbContext`

**Methods**
- constructor
- `GetByIdAsync(Guid id)`
- `GetByIdsAsync(List<Guid> ids)`
- `GetAllAsync()`

**Role**
- loads domain users for selection and lookup

### Identity Integration

#### ApplicationUser

Inherits from:

```text
IdentityUser<Guid>
```

**Additional members**
- no extra fields are currently added

**Role**
- authentication-level user model used by ASP.NET Core Identity

### 5.5 Database Seeder

#### DbSeeder

**Methods**
- `Task SeedAsync(AvailabilityCalendarDbContext context, UserManager<ApplicationUser> userManager)`

**Behavior**
- applies pending migrations
- checks separately for identity users, domain users, and events
- creates 10 test users if the database is empty
- creates a matching domain user entry for each identity user
- creates 30 random events per user if no events exist
- adds the creator as the participant of each generated event

**Seed users**
- `user1@test.com`
- ...
- `user10@test.com`

**Seed password**
- `Password123!`

**Generated event characteristics**
- date range: `DateTime.Today` + 0 to 29 days
- hour range: roughly 08:00 to 18:00
- duration: 1 to 2 hours

The refined seeding logic prevents the earlier failure case where the presence of users incorrectly blocked event generation.

### 5.6 Database Schema Files

#### InitialFullSchema migration
Contains the first explicit relational schema creation.

#### InitialFullSchema.Designer
Contains EF-generated target model metadata for the migration.

#### AvailabilityCalendarDbContextModelSnapshot
Contains the current EF model snapshot.

These files are infrastructure-level generated schema assets and document the exact relational structure of the database.

### Application Startup and Configuration

#### Program.cs

**Main responsibilities**
- register EF Core with SQL Server
- register Identity
- configure password rules
- configure authentication cookie paths
- register repositories and services in DI
- enable MVC and Razor Pages
- build the request pipeline
- seed the database during startup

**Registered services**
- `IEventRepository -> EventRepository`
- `IUserRepository -> UserRepository`
- `IEventService -> EventService`
- `IAvailabilityService -> AvailabilityService`

#### appsettings.json

**Contains**
- `ConnectionStrings.DefaultConnection`

**Default connection**
- SQL Server LocalDB based development configuration

### Infrastructure Class Inventory

AvailabilityCalendar.Infrastructure
├── Identity
│   └── ApplicationUser : IdentityUser<Guid>
├── Persistence
│   ├── AvailabilityCalendarDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
│   └── Seed
│       └── DbSeeder
├── Repositories
│   ├── EventRepository : IEventRepository
│   └── UserRepository : IUserRepository
└── Migrations
    ├── InitialFullSchema
    ├── InitialFullSchema.Designer
    └── AvailabilityCalendarDbContextModelSnapshot