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