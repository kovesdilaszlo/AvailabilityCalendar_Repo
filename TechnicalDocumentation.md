# Availability Calendar – Technical Documentation (Full Extended Version)

## 1. Overview

AvailabilityCalendar is an ASP.NET Core application designed for personal event management and multi-user time coordination.

The system operates in two distinct modes:

- **Personal Mode** – displays the current user's own events
- **Shared Availability Mode** – displays only the common free time intervals of multiple selected users

The system also supports a **minimum duration filter**, allowing the user to request only those shared free intervals that are long enough for practical use. For example, if the user is interested only in one-hour meeting slots, shorter intervals are excluded from the result.

In the refined version of the system, shared mode also supports an optional **slot generation parameter**. Instead of hiding long shared free intervals, the application can split them into smaller, practically bookable meeting slots of a user-defined length. This makes the coordination view more useful for actual meeting creation.

From an architectural perspective, the project is structured into the following main layers:

- **Web** – controllers, Razor views, page-level UI models, authentication entry points
- **Application** – use-case orchestration, commands, DTOs, service interfaces and service implementations
- **Domain** – core entities, value objects, and enums
- **Infrastructure** – Entity Framework Core persistence, repositories, database schema and seed data
- **Tests** – automated verification of business and application rules

The application uses ASP.NET Core MVC, ASP.NET Core Identity, and Entity Framework Core with SQL Server LocalDB configuration.

---

## 2. Core Interaction Model

### 2.1 Default Behavior

When the user opens the application:

- only the currently logged-in user is selected
- the application displays the current user's own events
- the current mode is **Personal Mode**

This is the default interaction pattern and represents normal personal calendar usage.

### 2.2 Multi-user Selection

When the user selects one or more additional users:

- the application switches from **Personal Mode** to **Shared Availability Mode**
- event details are no longer displayed
- only the common free intervals of the selected users are shown

This design enforces a privacy-oriented coordination model. The user does not see the contents of other users’ events. Instead, the interface is focused on the scheduling problem itself: finding valid shared time slots.

### 2.3 Returning to Personal Mode

If the selection is reduced back to the current user only:

- the application switches back to **Personal Mode**
- the user's own events are displayed again

This creates a simple and deterministic model:

- one selected user → personal calendar
- multiple selected users → shared free-time coordination

### 2.4 Selection Constraints

The system enforces the following selection rules:

- the logged-in user is always part of the selection
- the selection cannot become empty
- duplicate user identifiers are removed
- invalid edge cases are normalized before the actual mode logic runs

This guarantees that:

- mode resolution always has a valid input
- controller logic remains simple
- the application never has to handle “no selected user” states

### 2.5 Visible Selection Behavior

In the refined UI, the current user is **not displayed** in the selectable user chip list. This is an intentional interaction decision:

- the active user is always implicitly included
- only additional selectable users are shown
- the visible selection area stays cleaner and easier to understand

### 2.6 Mode Determination Logic

The mode is determined using the normalized selected user list:

```text
if selectedUsers.Count == 1 → Personal Mode
else → Shared Availability Mode
```

Because the current user is always present, the system never has to handle selections that exclude the active user.

---

## 3. Domain Layer

The Domain layer contains the core business concepts and the scheduling-related rules of the system.

### 3.1 Entities

#### User

Represents a domain-level calendar user.

**Properties**
- `Guid Id`
- `string Name`
- `ICollection<EventParticipant> EventParticipants`
- `ICollection<Event> CreatedEvents`

**Role in the system**
- identifies a participant in the scheduling domain
- acts as the creator of events
- participates in events through the join entity

---

#### Event

Represents a scheduled busy interval.

**Properties**
- `Guid Id`
- `string Title`
- `DateTime Start`
- `DateTime End`
- `Guid CreatedByUserId`
- `User? CreatedByUser`
- `ICollection<EventParticipant> Participants`

**Methods**
- `void AddParticipant(Guid userId)`
- `void RemoveParticipant(Guid userId)`
- `bool HasParticipant(Guid userId)`
- `void UpdateTime(DateTime start, DateTime end)`
- `bool IsCreatedBy(Guid userId)`

**Role in the system**
- stores the title and time boundaries of a busy period
- keeps the creator information
- keeps the participant list
- enforces basic participant and time-related logic

---

#### EventParticipant

Represents the many-to-many relationship between users and events.

**Properties**
- `Guid EventId`
- `Event? Event`
- `Guid UserId`
- `User? User`

**Role in the system**
- links one event to one participant
- allows one event to have multiple users
- allows one user to participate in multiple events

---

### 3.2 Value Objects

#### TimeInterval

Represents a generic time range and is the mathematical core of the scheduling algorithm.

**Properties**
- `DateTime Start`
- `DateTime End`

**Methods**
- `bool OverlapsWith(TimeInterval other)`
- `TimeInterval MergeWith(TimeInterval other)`
- `TimeSpan Duration()`

**Role in the system**
- used to represent busy intervals
- used to represent free intervals
- used to merge overlaps
- used to measure minimum duration constraints

The constructor validates that `End` is not earlier than `Start`.

---

### 3.3 Domain Enums

#### ViewMode

Represents the business-level mode of the calendar.

**Members**
- `Personal = 0`
- `Shared = 1`

**Role in the system**
- separates own-event rendering from shared-free-time rendering

---

### 3.4 Domain Class Inventory

```text
AvailabilityCalendar.Domain
├── Entities
│   ├── User
│   ├── Event
│   └── EventParticipant
├── ValueObjects
│   └── TimeInterval
└── Enums
    └── ViewMode
```

---

## 4. Application Layer

The Application layer orchestrates use cases. It sits between the Web layer and the Domain/Infrastructure layers.

### 4.1 Selection Logic

#### NormalizeSelection(selectedUsers, currentUserId)

Implemented in `AvailabilityService`.

**Purpose**
- removes duplicate identifiers
- ensures the current user is always present

**Effect**
- guarantees a valid selected user set before any mode or availability calculation

### 4.2 Mode Logic

#### DetermineViewMode(selectedUsers, currentUserId)

Implemented in `AvailabilityService`.

**Purpose**
- resolves whether the interface should display personal events or shared free intervals

### 4.3 Event Retrieval

#### GetEventsByUserAsync(userId, range)

Implemented in `EventService`.

**Purpose**
- retrieves the events relevant to one user within a time range
- returns them as `EventDto` objects for the Web layer

### 4.4 Event Management

#### CreateEventAsync(command)

Implemented in `EventService`.

**Purpose**
- creates a new event
- trims the title
- validates the time range
- automatically adds the creator as participant
- adds distinct extra participants if provided

#### UpdateEventAsync(command)

Implemented in `EventService`.

**Purpose**
- updates an existing event
- verifies creator ownership
- validates title and time range

#### DeleteEventAsync(eventId, currentUserId)

Implemented in `EventService`.

**Purpose**
- deletes an event
- verifies creator ownership

### 4.5 Shared Availability Calculation

#### GetCommonFreeTimeAsync(userIds, range, minimumDuration)

Implemented in `AvailabilityService`.

**Workflow**
1. normalize user IDs
2. load busy events through the repository
3. convert events to `TimeInterval`
4. clip intervals to the requested range
5. merge overlapping or directly touching intervals
6. compute remaining gaps
7. keep only intervals with `Duration >= minimumDuration`

This is the central scheduling function of the application.

### 4.6 Minimum Duration Filtering

The minimum duration filter is applied after free intervals are computed. This ensures that technically free but practically too short intervals are removed from the final result.

### 4.7 Slot Generation for Shared Scheduling

After the minimum-duration filtered free intervals are computed, the controller layer may optionally split long intervals into smaller slots of a specified meeting length.

**Purpose**
- turn long common free intervals into concrete candidate meeting slots
- improve usability in shared mode
- allow direct event creation from a selected slot

This is not a replacement for the base availability algorithm. It is a post-processing step on already valid shared free intervals.

### 4.8 Commands

#### CreateEventCommand

**Properties**
- `string Title`
- `DateTime Start`
- `DateTime End`
- `Guid CurrentUserId`
- `List<Guid> ParticipantIds`

**Purpose**
- input model for event creation

#### UpdateEventCommand

**Properties**
- `Guid EventId`
- `string Title`
- `DateTime Start`
- `DateTime End`
- `Guid CurrentUserId`

**Purpose**
- input model for event update

### 4.9 DTOs

#### EventDto

**Properties**
- `Guid Id`
- `string Title`
- `DateTime Start`
- `DateTime End`
- `Guid CreatedByUserId`
- `List<Guid> ParticipantIds`

**Purpose**
- transports event data from the Application layer to the Web layer

#### FreeTimeIntervalDto

**Properties**
- `DateTime Start`
- `DateTime End`
- `double DurationInMinutes`

**Purpose**
- prepared DTO type for free interval transport and presentation

### 4.10 Interfaces

#### IAvailabilityService

**Methods**
- `ViewMode DetermineViewMode(List<Guid> selectedUsers, Guid currentUserId)`
- `List<Guid> NormalizeSelection(List<Guid> selectedUsers, Guid currentUserId)`
- `List<TimeInterval> MergeIntervals(List<TimeInterval> intervals)`
- `Task<List<TimeInterval>> GetCommonFreeTimeAsync(List<Guid> userIds, TimeInterval range, TimeSpan minimumDuration)`

#### IEventService

**Methods**
- `Task<Guid> CreateEventAsync(CreateEventCommand command)`
- `Task UpdateEventAsync(UpdateEventCommand command)`
- `Task DeleteEventAsync(Guid eventId, Guid currentUserId)`
- `Task<List<EventDto>> GetEventsByUserAsync(Guid userId, TimeInterval range)`

#### IEventRepository

**Methods**
- `Task<Event?> GetByIdAsync(Guid id)`
- `Task<List<Event>> GetByUsersAsync(List<Guid> userIds, TimeInterval range)`
- `Task AddAsync(Event entity)`
- `Task UpdateAsync(Event entity)`
- `Task DeleteAsync(Guid id)`

#### IUserRepository

**Methods**
- `Task<User?> GetByIdAsync(Guid id)`
- `Task<List<User>> GetByIdsAsync(List<Guid> ids)`
- `Task<List<User>> GetAllAsync()`

### 4.11 Service Implementations

#### AvailabilityService

**Fields**
- `IEventRepository _eventRepository`

**Methods**
- constructor
- `DetermineViewMode(...)`
- `NormalizeSelection(...)`
- `MergeIntervals(...)`
- `GetCommonFreeTimeAsync(...)`

**Role**
- owns the selection normalization and shared-availability logic

#### EventService

**Fields**
- `IEventRepository _eventRepository`

**Methods**
- constructor
- `CreateEventAsync(...)`
- `GetEventsByUserAsync(...)`
- `UpdateEventAsync(...)`
- `DeleteEventAsync(...)`

**Role**
- owns event-level use cases and authorization enforcement

### 4.12 Application Class Inventory

```text
AvailabilityCalendar.Application
├── Commands
│   ├── CreateEventCommand
│   └── UpdateEventCommand
├── DTOs
│   ├── EventDto
│   └── FreeTimeIntervalDto
├── Interfaces
│   ├── IAvailabilityService
│   ├── IEventRepository
│   ├── IEventService
│   └── IUserRepository
└── Services
    ├── AvailabilityService
    └── EventService
```

---

## 5. Infrastructure Layer

The Infrastructure layer contains persistence, database schema definitions, repository implementations, Identity integration, and seed data.

### 5.1 Persistence Context

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

### 5.2 Entity Configuration Details

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

### 5.3 Repository Implementations

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

### 5.4 Identity Integration

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

### 5.7 Application Startup and Configuration

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

### 5.8 Infrastructure Class Inventory

```text
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
```

---

## 6. Core Functionality

### 6.1 Personal Mode

In personal mode, the system behaves as a personal calendar application.

**Main features**
- display own events
- create events
- update events
- delete events
- validate date/time input
- show the current period in the selected view

### 6.2 Shared Availability Mode

In shared mode, the system behaves as a coordination tool.

**Main features**
- resolve selected users
- calculate the common free intervals
- apply the minimum duration filter
- optionally split free intervals into meeting-sized slots
- display only valid shared free time
- create common events directly from a shared free slot

### 6.3 Privacy Principle

The system intentionally does not expose other users’ event details in shared mode. It displays only whether time is free and which intervals are suitable.

### 6.4 Event Authorization

Update and delete operations are restricted to the creator of the event.

### 6.5 Input Validation

Validation exists at multiple levels:

- view model validation for UI form input
- service-level validation for empty titles
- domain-level validation for invalid time ranges
- controller-level validation for end time not being earlier than start time

### 6.6 Calendar Range Resolution

The application computes different time ranges depending on the selected view:

- **Day** → one calendar day
- **Week** → Monday-based 7-day interval
- **Month** → first day of month until next month

These ranges are used both for event retrieval and free-time calculation.

### 6.7 Cell Generation

The system generates UI cells based on the selected view:

- **Day** → 1 cell
- **Week** → 7 cells
- **Month** → 42 cells

The month view uses a fixed 6×7 grid.

### 6.8 Month Grid Rationale

The month view contains 42 cells because:

- a month may begin late in a week
- a month may require six visual rows
- a fixed 6×7 grid keeps the layout stable

### 6.9 Month View Interval Splitting

In month view, free intervals that span across day boundaries are visually split into day-specific textual fragments.

Example:
- shared free interval: `2026-03-18 18:00 -> 2026-03-19 14:00`

Displayed as:
- `03.18` → `18:00-23:59`
- `03.19` → `00:00-14:00`

This prevents misleading rendering where a multi-day interval would otherwise appear only on its start date.

---

## 7. Web Layer and UI Behavior

The Web layer contains controllers, page-level view models, Razor views, and UI helpers.

### 7.1 Controllers

#### CalendarController

**Fields**
- `IEventService _eventService`
- `IAvailabilityService _availabilityService`
- `IUserRepository _userRepository`

**Actions**
- `Task<IActionResult> Index(DateTime? date, CalendarViewType view = CalendarViewType.Day, List<Guid>? selectedUsers = null, int minimumDurationMinutes = 60, int? slotLengthMinutes = null)`
- `IActionResult Create(DateTime? date)`
- `Task<IActionResult> Create(CreateEventViewModel model)`
- `Task<IActionResult> CreateFromFreeSlot(...)`
- `Task<IActionResult> Update(UpdateEventViewModel model, ...)`
- `Task<IActionResult> Delete(Guid id, ...)`

**Private methods**
- `BuildRange(CalendarViewType view, DateTime currentDate)`
- `BuildCells(CalendarViewType view, DateTime currentDate)`
- `BuildDayCells(DateTime currentDate)`
- `BuildWeekCells(DateTime currentDate)`
- `BuildMonthCells(DateTime currentDate)`
- `GetPreviousDate(...)`
- `GetNextDate(...)`
- `SplitIntervalsIntoSlots(...)`
- `BuildBlocks(...)`
- `BuildEventBlocks(...)`
- `BuildFreeTimeBlocks(...)`
- `SplitIntoDailyBlocks(...)`
- `BuildBlock(...)`
- `ClipToVisibleHours(...)`
- `GetStartOfWeek(DateTime date)`

**Role**
- central web endpoint of the calendar
- resolves current date and selected view
- builds the page view model
- loads either events or shared free intervals
- handles event creation
- handles slot-based common event creation
- handles event update and deletion
- preserves redirect context after modal actions

#### HomeController

**Actions**
- `IActionResult Index()`
- `IActionResult Landing()`

**Role**
- redirects authenticated users to the calendar
- redirects anonymous users to the login flow

#### AccountController

**Field**
- `SignInManager<ApplicationUser> _signInManager`

**Actions**
- `IActionResult Login()`
- `Task<IActionResult> Login(string email, string password)`
- `Task<IActionResult> Logout()`

**Role**
- handles login/logout flow through ASP.NET Core Identity

### 7.2 Web Enums

#### CalendarViewType

**Members**
- `Day = 0`
- `Week = 1`
- `Month = 2`

**Role**
- determines how the calendar should be rendered and which date range should be used

### 7.3 Web ViewModels

#### CalendarPageViewModel

**Properties**
- `DateTime CurrentDate`
- `CalendarViewType ViewType`
- `ViewMode Mode`
- `int MinimumDurationMinutes`
- `int? SlotLengthMinutes`
- `Guid CurrentUserId`
- `List<Guid> SelectedUserIds`
- `List<SelectableUserViewModel> AvailableUsers`
- `List<CalendarCellViewModel> Cells`
- `List<EventDto> Events`
- `List<TimeInterval> FreeIntervals`
- `List<CalendarBlockViewModel> Blocks`
- computed property: `string ModeLabel`

**Role**
- aggregated page-level model of the main calendar page

#### CalendarBlockViewModel

**Properties**
- `Guid? EventId`
- `Guid? CreatedByUserId`
- `DateTime Date`
- `DateTime Start`
- `DateTime End`
- `string Title`
- `bool IsFreeTime`
- `double TopPercent`
- `double HeightPercent`
- `int ColumnIndex`
- computed property: `string TimeLabel`

**Role**
- represents one visual event or free-time block in day and week rendering
- stores modal-related metadata for interactive actions

#### CreateEventViewModel

**Properties**
- `string Title`
- `DateTime Start`
- `DateTime End`

**Attributes**
- required validation attributes
- display labels

**Role**
- input model of the event creation form

#### UpdateEventViewModel

**Properties**
- `Guid Id`
- `string Title`
- `DateTime Start`
- `DateTime End`

**Role**
- input model of modal-based event editing

#### SelectableUserViewModel

**Properties**
- `Guid Id`
- `string Name`
- `bool IsSelected`
- `bool IsCurrentUser`

**Role**
- represents a user in the selectable user list

#### CalendarCellViewModel

**Properties**
- `DateTime Date`
- `bool IsCurrentPeriod`
- `bool IsToday`
- `bool IsSelectedDate`

**Role**
- represents one rendered date cell in day, week, or month layouts

### 7.4 Web Extensions

#### UserExtensions

**Methods**
- `Guid GetUserId(this ClaimsPrincipal user)`

**Role**
- extracts and validates the authenticated GUID user identifier from the claims principal

### 7.5 Razor Views

#### Home/Index.cshtml
- simple landing view with navigation buttons to calendar or login

#### Calendar/Create.cshtml
- form for new event creation
- includes anti-forgery token
- uses validation summary and field-level validation

#### Calendar/Index.cshtml
- main interactive calendar page
- contains personal/shared mode rendering
- contains user chip selection UI
- contains modal-based event update/delete
- contains modal-based common event creation from shared slots

#### Account/Login.cshtml
- login form
- includes anti-forgery token
- email/password input fields
- posts to `AccountController.Login`

#### Shared layout files
- `_Layout.cshtml`
- `_ViewStart.cshtml`
- `_ViewImports.cshtml`

**Layout responsibilities**
- Bootstrap inclusion
- shared page shell
- navigation bar
- login/logout links
- global calendar cell card styling

### 7.6 UI Rendering Model

The UI concept is based on three views:

- **Day View**
- **Week View**
- **Month View**

#### Day View
- one day cell
- full 24-hour hourly subdivision
- events or free intervals rendered proportionally in vertical blocks

#### Week View
- seven day cells from Monday to Sunday
- hourly subdivision and proportional time-slot rendering

#### Month View
- 42-cell fixed grid
- text-based display of events or split free intervals inside day boxes

### 7.7 24-hour UI Specification

For the extended UI design, day and week views use a **00:00–24:00** scale.

**Expected hourly labeling**
- 0 through 24

**Expected rendering principle**
- blocks positioned proportionally based on minute offset from midnight
- full day duration basis: `1440` minutes

**Block calculation model**
```text
TopPercent = (startMinutes / 1440) * 100
HeightPercent = (durationMinutes / 1440) * 100
```

#### Navigation expectation
- Day view: previous/next day
- Week view: previous/next week
- Month view: previous/next month

### 7.8 Selection UI

The visible user selector uses a chip-style interface:

- each selectable user is displayed as a pill-shaped chip
- unchecked chips are white
- checked chips are blue
- the active user is hidden from the visible list because they are always implicitly selected

Technically, the chip interface is still based on checkbox inputs, but rendered with a custom visual style.

### 7.9 Auto-submit Behavior

The selection form updates automatically when the user changes:

- the current view type
- the minimum duration
- the slot length
- the selected users

This is handled client-side through JavaScript by listening to `change` events and submitting the surrounding form automatically.

### 7.10 Modal Interaction Model

The application uses Bootstrap modal dialogs for interactive actions.

#### Event modal
Available in personal mode for event blocks and month items.

**Supported actions**
- modify event title
- modify start/end time
- delete event after explicit confirmation

#### Free-slot modal
Available in shared mode for shared free intervals and slot blocks.

**Supported actions**
- display selected free slot boundaries
- set the title of the new common event
- confirm creation
- create an event that includes all currently selected users as participants

### 7.11 Redirect Preservation

After update, delete, or shared-slot event creation, the controller preserves:

- current date
- current view
- minimum duration
- slot length
- selected users

This keeps the user in the same UI context instead of forcing a reset to a default state.

### 7.12 Web Layer Inventory

```text
AvailabilityCalendar.Web
├── Controllers
│   ├── CalendarController
│   ├── HomeController
│   └── AccountController
├── Extensions
│   └── UserExtensions
├── ViewModels
│   └── Calendar
│       ├── CalendarPageViewModel
│       ├── CalendarBlockViewModel
│       ├── CalendarViewType
│       ├── CreateEventViewModel
│       ├── UpdateEventViewModel
│       ├── SelectableUserViewModel
│       └── CalendarCellViewModel
├── Views
│   ├── Home/Index.cshtml
│   ├── Calendar/Index.cshtml
│   ├── Calendar/Create.cshtml
│   ├── Account/Login.cshtml
│   ├── Shared/_Layout.cshtml
│   ├── Shared/_ViewStart.cshtml
│   └── Shared/_ViewImports.cshtml
└── wwwroot
    ├── css/calendar.css
    ├── css/site.css
    └── js/calendar.js
```

---

## 8. Testing Strategy

The system is designed for automated testing at multiple levels.

### 8.1 Domain Tests

Covered behaviors:
- `TimeInterval` overlap detection
- `TimeInterval` merge behavior
- `TimeInterval` duration calculation
- `Event` participant management
- `Event` ownership check
- `Event` time validation

### 8.2 Application Tests

Covered behaviors:
- selection normalization
- mode switching
- interval merging
- shared availability calculation
- minimum duration filtering
- slot post-processing logic
- authorization checks for update/delete

### 8.3 Important Test Scenarios

#### TimeInterval
- intervals overlap
- intervals do not overlap
- intervals only touch at boundary
- multiple overlaps merge correctly

#### Selection
- duplicates removed
- current user restored if missing
- empty selection normalized
- current user hidden from the visible chip list but still implicitly selected

#### Mode determination
- one selected user → personal
- multiple selected users → shared

#### Shared free time
- no busy intervals → full range free
- full overlap → no free result
- merged overlaps before gap computation
- minimum-duration edge cases
- long free intervals correctly split into slots when slot length is configured

#### Modal actions
- own event can be edited from day view
- own event can be edited from week view
- own event can be edited from month view
- delete requires explicit confirmation
- shared free slot creates a new event with all selected users as participants

#### Authorization
- only creator may update
- only creator may delete

---

## 9. Project Structure and Class Hierarchy

### 9.1 Logical Project Structure

```text
AvailabilityCalendar
├── Web
│   ├── Controllers
│   ├── Extensions
│   ├── ViewModels
│   ├── Views
│   └── wwwroot
├── Application
│   ├── Commands
│   ├── DTOs
│   ├── Interfaces
│   └── Services
├── Domain
│   ├── Entities
│   ├── Enums
│   └── ValueObjects
├── Infrastructure
│   ├── Identity
│   ├── Persistence
│   ├── Repositories
│   └── Migrations
└── Tests
```

### 9.2 Inheritance and Interface Hierarchy

```text
IdentityUser<Guid>
└── ApplicationUser

IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
└── AvailabilityCalendarDbContext

IEventRepository
└── EventRepository

IUserRepository
└── UserRepository

IEventService
└── EventService

IAvailabilityService
└── AvailabilityService

Controller
├── CalendarController
├── HomeController
└── AccountController
```

### 9.3 Dependency Flow

```text
Web Controllers
    ↓
Application Services / Interfaces
    ↓
Infrastructure Repositories
    ↓
EF Core DbContext
    ↓
SQL Database
```

### 9.4 Relationship Overview

```text
User 1 ─── * CreatedEvents
User 1 ─── * EventParticipants
Event 1 ─── * EventParticipants
EventParticipant * ─── 1 User
EventParticipant * ─── 1 Event
```

---

## 10. Problem Model

The scheduling problem can be described as a constraint-oriented time coordination problem.

### Goal
Find valid time intervals that satisfy all selected users simultaneously.

### Constraints
- users may already be busy because of existing events
- all returned results must remain inside the selected search range
- all returned results must satisfy the requested minimum duration
- the current user must be included in the selected user set

The implemented solution does not use a generic external CSP solver. Instead, it solves the practical problem using deterministic interval processing.

---

## 11. Mathematical Model

Let:

- `T` = searched time range
- `Busy(u)` = busy intervals of user `u`
- `Free(u)` = `T - Busy(u)`

For multiple users, the common free time can be conceptualized as:

```text
CommonFree(U) = ⋂ Free(u)
```

In implementation terms, this is computed indirectly:

1. gather busy intervals of all selected users
2. merge overlapping/touching busy intervals
3. compute remaining free gaps inside `T`

Then apply the duration filter:

```text
Result = { interval ∈ CommonFree(U) | Duration(interval) >= minimumDuration }
```

Optional slot generation then transforms the result into smaller equally sized candidate meeting intervals:

```text
Slots = Split(Result, slotLength)
```

with the additional rule that no produced slot may violate the configured minimum duration.

This model corresponds to the actual `AvailabilityService` implementation plus controller-level shared-slot post-processing.

---

## 12. Complexity

The dominant work of the scheduling algorithm comes from:

- collecting intervals
- sorting intervals
- merging overlaps
- scanning merged intervals to compute gaps

For `n` busy intervals, the dominant time complexity is approximately:

```text
O(n log n)
```

This is primarily due to sorting.

The optional slot splitting step is linear in the number of produced slots and is therefore smaller-order compared to the main merge-and-gap computation in normal usage.

### Storage considerations
- events are stored in relational tables
- event-user membership is represented through a join table
- indices exist on frequently filtered fields such as start date, end date, and creator ID

This is sufficient for the intended educational and medium-scale practical scenarios.

---

## 13. Future Improvements

Possible future improvements include:

### 13.1 Domain and Application
- richer event metadata such as description, location, and tags
- participant editing during event update
- conflict detection during event creation/update
- stronger domain invariants
- dedicated DTO usage for free interval UI transport everywhere

### 13.2 Web and UI
- dark mode / light mode theme switching
- language selection
- mobile-first rendering improvements
- richer slot recommendation logic
- visual overlap handling for dense event layouts
- partial page refresh instead of full form submit

### 13.3 Infrastructure and Security
- role-based access control
- better production configuration separation
- external integrations such as Google Calendar or Outlook
- notifications and reminders
- concurrency handling
- performance tuning for larger data volumes

### 13.4 Scheduling Extensions
- ranked slot recommendation
- preference-aware scheduling
- optimization of preferred hours
- recurring meeting assistance
- optional optimization criteria, not only valid interval finding

---

## Appendix A – Full Class and File Checklist

### Domain
- `User.cs`
- `Event.cs`
- `EventParticipant.cs`
- `TimeInterval.cs`
- `ViewMode.cs`

### Application
- `CreateEventCommand.cs`
- `UpdateEventCommand.cs`
- `EventDto.cs`
- `FreeTimeIntervalDto.cs`
- `IAvailabilityService.cs`
- `IEventRepository.cs`
- `IEventService.cs`
- `IUserRepository.cs`
- `AvailabilityService.cs`
- `EventService.cs`

### Infrastructure
- `ApplicationUser.cs`
- `AvailabilityCalendarDbContext.cs`
- `DbSeeder.cs`
- `EventRepository.cs`
- `UserRepository.cs`
- `InitialFullSchema.cs`
- `InitialFullSchema.Designer.cs`
- `AvailabilityCalendarDbContextModelSnapshot.cs`
- `Program.cs`
- `appsettings.json`

### Web
- `CalendarController.cs`
- `HomeController.cs`
- `AccountController.cs`
- `CalendarPageViewModel.cs`
- `CalendarBlockViewModel.cs`
- `CalendarViewType.cs`
- `CreateEventViewModel.cs`
- `UpdateEventViewModel.cs`
- `SelectableUserViewModel.cs`
- `CalendarCellViewModel.cs`
- `UserExtensions.cs`
- `Index.cshtml`
- `Create.cshtml`
- `Login.cshtml`
- `_Layout.cshtml`
- `_ViewStart.cshtml`
- `_ViewImports.cshtml`
- `calendar.css`
- `site.css`
- `calendar.js`

---

## 14. Explicit System Hierarchy and Execution Flow

### 14.1 End-to-End Request Flow

```text
HTTP Request (Browser)
    ↓
CalendarController (Web Layer)
    ↓
IEventService / IAvailabilityService (Application Layer)
    ↓
EventService / AvailabilityService (Implementation)
    ↓
IEventRepository / IUserRepository
    ↓
EventRepository / UserRepository (Infrastructure)
    ↓
AvailabilityCalendarDbContext (EF Core)
    ↓
SQL Database
```

### 14.2 UI → ViewModel → Data Flow

```text
Index.cshtml
    ↓
CalendarPageViewModel
    ├── Cells
    ├── Events
    ├── FreeIntervals
    ├── Blocks
    ├── SelectedUserIds
    ├── AvailableUsers
    ├── MinimumDurationMinutes
    └── SlotLengthMinutes
```

### 14.3 Domain Relationships

```text
User
 ├── CreatedEvents (1:N)
 └── EventParticipants (1:N)

Event
 ├── CreatedByUser (N:1)
 └── EventParticipants (1:N)

EventParticipant
 ├── User (N:1)
 └── Event (N:1)
```

### 14.4 Service Dependencies

```text
CalendarController
    ↓
IEventService → EventService
IAvailabilityService → AvailabilityService

EventService
    ↓
IEventRepository → EventRepository

AvailabilityService
    ↓
IEventRepository → EventRepository
```

### 14.5 Layer Rule

```text
Web → Application → Infrastructure → Database
```

Rules:
- Web never directly accesses the database
- Application depends only on interfaces
- Infrastructure provides implementations
- Domain remains independent

### 14.6 UI Rendering Hierarchy

```text
Calendar (Index.cshtml)
 ├── Day View
 ├── Week View
 └── Month View
```

Each view:
- uses the same page model
- renders different structures
- uses different visualization logic
- can open the same modal interaction flow for relevant items

### 14.7 Shared Slot Creation Flow

```text
Shared free interval rendered in UI
    ↓
User clicks free slot
    ↓
Free-slot modal opens
    ↓
User confirms creation and enters title
    ↓
CalendarController.CreateFromFreeSlot(...)
    ↓
EventService.CreateEventAsync(...)
    ↓
Event + EventParticipants saved
    ↓
Redirect back to current filtered calendar view
```


---

## 15. Detailed File Responsibilities + Layer Dependencies

### 15.1 Project Dependency Structure

```text
AvailabilityCalendar.Web
    → references AvailabilityCalendar.Application

AvailabilityCalendar.Application
    → references AvailabilityCalendar.Domain

AvailabilityCalendar.Infrastructure
    → references AvailabilityCalendar.Application
    → references AvailabilityCalendar.Domain

AvailabilityCalendar.Domain
    → no dependencies (pure domain layer)
```

### 15.2 File-Level Responsibilities

#### Controllers

```text
CalendarController.cs
    - main orchestration of calendar logic
    - handles view switching (day/week/month)
    - handles personal vs shared mode
    - handles event CRUD operations
    - handles slot-based event creation

AccountController.cs
    - authentication (login/logout)

HomeController.cs
    - entry routing
```

#### Services

```text
AvailabilityService.cs
    - selection normalization
    - mode determination
    - interval merging
    - shared free time calculation

EventService.cs
    - event creation/update/delete
    - authorization enforcement
```

#### Repositories

```text
EventRepository.cs
    - event persistence
    - querying by users and time range

UserRepository.cs
    - user retrieval
```

#### Domain

```text
Event.cs
    - core event entity logic

User.cs
    - domain user

EventParticipant.cs
    - many-to-many relation

TimeInterval.cs
    - interval math
```

---

## 16. UI ↔ Backend Interaction Flow

### 16.1 Selection Change Flow

```text
User changes checkbox (chip)
    ↓
Form auto-submit (JS)
    ↓
CalendarController.Index
    ↓
AvailabilityService.NormalizeSelection
    ↓
AvailabilityService.DetermineViewMode
    ↓
Render updated view
```

### 16.2 Free Slot Creation Flow

```text
User clicks free slot
    ↓
Modal opens
    ↓
User confirms
    ↓
POST CreateFromFreeSlot
    ↓
EventService.CreateEventAsync
    ↓
Database save
    ↓
Redirect to same view
```

### 16.3 Event Update Flow

```text
User clicks event
    ↓
Modal opens
    ↓
User edits data
    ↓
POST Update
    ↓
EventService.UpdateEventAsync
    ↓
Redirect back
```

### 16.4 Event Delete Flow

```text
User clicks delete
    ↓
Confirmation modal
    ↓
POST Delete
    ↓
EventService.DeleteEventAsync
    ↓
Redirect back
```

---

## 17. Validation Layers

Validation is implemented across multiple layers:

### 17.1 UI Layer
- required fields
- date inputs
- client-side validation

### 17.2 Controller Layer
- start < end check
- model state validation

### 17.3 Application Layer
- title trimming
- authorization checks
- command validation

### 17.4 Domain Layer
- TimeInterval validity (End >= Start)

---

## 18. Sequence Overview (Simplified)

### 18.1 Calendar Load

```text
User → Controller → Service → Repository → DB → back to UI
```

### 18.2 Shared Availability

```text
Users selected
    ↓
Load events
    ↓
Merge intervals
    ↓
Compute gaps
    ↓
Filter by duration
    ↓
Render
```

### 18.3 Slot Generation

```text
Free intervals
    ↓
Split into slots
    ↓
Render clickable blocks
```

