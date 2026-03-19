## 1. Web Layer and UI Behavior

The Web layer contains controllers, page-level view models, Razor views, and UI helpers.

### 1.1 Controllers

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

### 1.2 Web Enums

#### CalendarViewType

**Members**
- `Day = 0`
- `Week = 1`
- `Month = 2`

**Role**
- determines how the calendar should be rendered and which date range should be used

### 1.3 Web ViewModels

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

### 1.4 Web Extensions

#### UserExtensions

**Methods**
- `Guid GetUserId(this ClaimsPrincipal user)`

**Role**
- extracts and validates the authenticated GUID user identifier from the claims principal

### 1.5 Razor Views

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

### 1.6 UI Rendering Model

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

### 1.7 24-hour UI Specification

For the extended UI design, day and week views use a **00:00–24:00** scale.

**Expected hourly labeling**
- 0 through 24

**Expected rendering principle**
- blocks positioned proportionally based on minute offset from midnight
- full day duration basis: `1440` minutes

**Block calculation model**
```
TopPercent = (startMinutes / 1440) * 100
HeightPercent = (durationMinutes / 1440) * 100
```

#### Navigation expectation
- Day view: previous/next day
- Week view: previous/next week
- Month view: previous/next month

### 1.8 Selection UI

The visible user selector uses a chip-style interface:

- each selectable user is displayed as a pill-shaped chip
- unchecked chips are white
- checked chips are blue
- the active user is hidden from the visible list because they are always implicitly selected

Technically, the chip interface is still based on checkbox inputs, but rendered with a custom visual style.

### 1.9 Auto-submit Behavior

The selection form updates automatically when the user changes:

- the current view type
- the minimum duration
- the slot length
- the selected users

This is handled client-side through JavaScript by listening to `change` events and submitting the surrounding form automatically.

### 1.10 Modal Interaction Model

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

### 1.11 Redirect Preservation

After update, delete, or shared-slot event creation, the controller preserves:

- current date
- current view
- minimum duration
- slot length
- selected users

This keeps the user in the same UI context instead of forcing a reset to a default state.

### 1.12 Web Layer Inventory

```
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


