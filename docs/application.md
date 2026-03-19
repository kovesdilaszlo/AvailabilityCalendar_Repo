##  Application Layer

The Application layer orchestrates use cases. It sits between the Web layer and the Domain/Infrastructure layers.

### 1 Selection Logic

#### NormalizeSelection(selectedUsers, currentUserId)

Implemented in `AvailabilityService`.

**Purpose**
- removes duplicate identifiers
- ensures the current user is always present

**Effect**
- guarantees a valid selected user set before any mode or availability calculation

### Mode Logic

#### DetermineViewMode(selectedUsers, currentUserId)

Implemented in `AvailabilityService`.

**Purpose**
- resolves whether the interface should display personal events or shared free intervals

### Event Retrieval

#### GetEventsByUserAsync(userId, range)

Implemented in `EventService`.

**Purpose**
- retrieves the events relevant to one user within a time range
- returns them as `EventDto` objects for the Web layer

### Event Management

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

### Shared Availability Calculation

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

### Minimum Duration Filtering

The minimum duration filter is applied after free intervals are computed. This ensures that technically free but practically too short intervals are removed from the final result.

### Slot Generation for Shared Scheduling

After the minimum-duration filtered free intervals are computed, the controller layer may optionally split long intervals into smaller slots of a specified meeting length.

**Purpose**
- turn long common free intervals into concrete candidate meeting slots
- improve usability in shared mode
- allow direct event creation from a selected slot

This is not a replacement for the base availability algorithm. It is a post-processing step on already valid shared free intervals.

### Commands

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

### DTOs

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

### Interfaces

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

### Service Implementations

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