# Future Improvements

This section outlines possible future extensions of the system, focusing on usability, scalability, and advanced scheduling capabilities.

---

## Web and UI

- **Theme support (dark mode as default, light mode optional)**
  - the system should support multiple visual themes
  - dark theme should be the default for better usability and reduced eye strain
  - users should be able to switch themes dynamically

- **Language selection (localization support)**
  - support for multiple languages (e.g., English, Hungarian)
  - dynamic language switching in the UI
  - preparation for full internationalization (i18n)

- **Mobile-first and responsive design**
  - improved layout for smaller screens
  - better touch interaction support

- **Improved interaction and rendering**
  - partial page updates instead of full reloads
  - smoother UX when creating or editing events

- **Advanced visualization**
  - better handling of overlapping events
  - clearer representation of dense schedules

---

## Domain and Application

- **Richer event metadata**
  - description, location, tags
  - extended domain model for more complex use cases

- **Participant management**
  - editing participants during event updates
  - improved validation and handling of participant lists

- **Conflict detection**
  - validation during event creation and update
  - prevention of overlapping events for the same user

- **Stronger domain invariants**
  - more explicit validation rules
  - better error handling and domain consistency

- **DTO standardization**
  - consistent use of DTOs for UI communication
  - separation of domain and presentation models

---

## Infrastructure and Security

- **Role-based access control**
  - different permission levels (e.g., admin, user)

- **Configuration management**
  - better separation of development and production environments

- **External integrations**
  - Google Calendar
  - Microsoft Outlook

- **Notifications and reminders**
  - email or in-app notifications
  - event reminders

- **Concurrency handling**
  - conflict resolution for simultaneous updates

- **Performance optimization**
  - handling larger datasets efficiently
  - improved query performance

- **Role-based access control with visibility rules**
  - users can have one or more roles (e.g., TL, TPE)
  - visibility of other users depends on shared roles
  - users can only see other users who share at least one role with them
  - example:
    - Balázs (TL, TPE) → sees János (TPE) and Péter (TL)
    - János (TPE) → does not see Péter (TL)
  - improves data isolation and reduces unnecessary data exposure
  - supports scalable team-based access control
---

## Scheduling Extensions

- **Ranked slot recommendation**
  - prioritizing better time slots instead of listing all valid ones

- **Preference-aware scheduling**
  - user preferences (e.g., morning vs evening)

- **Optimization strategies**
  - selecting the most optimal time instead of any valid time

- **Recurring events**
  - support for repeating meetings

- **Advanced constraints**
  - optional constraints beyond basic availability