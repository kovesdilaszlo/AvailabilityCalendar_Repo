using Xunit;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Enums;
using Moq;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests view mode selection rules.
/// </summary>
public class ViewModeTests
{
    /// <summary>
    /// Verifies personal view mode is returned when only the current user is selected.
    /// </summary>
    [Fact]
    public void DetermineViewMode_ShouldReturnPersonal_WhenOnlyCurrentUserIsSelected()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var selectedUsers = new List<Guid> { currentUserId };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var result = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Personal, result);
    }

    /// <summary>
    /// Verifies shared view mode is returned when multiple users are selected.
    /// </summary>
    [Fact]
    public void DetermineViewMode_ShouldReturnShared_WhenMultipleUsersAreSelected()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid> { currentUserId, otherUserId };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var result = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Shared, result);
    }
}