using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Enums;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests view mode decisions in AvailabilityService.
/// </summary>
public class AvailabilityServiceViewModeTests
{
    /// <summary>
    /// Verifies shared mode is returned when more than one user is selected.
    /// </summary>
    [Fact]
    public void DetermineViewMode_Should_ReturnShared_WhenMoreThanOneUserAfterNormalization()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            otherUserId,
            otherUserId
        };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var mode = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Shared, mode);
    }

    /// <summary>
    /// Verifies personal mode is returned when only the current user remains.
    /// </summary>
    [Fact]
    public void DetermineViewMode_Should_ReturnPersonal_WhenOnlyCurrentUserAfterNormalization()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            currentUserId,
            currentUserId
        };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var mode = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Personal, mode);
    }

    /// <summary>
    /// Verifies personal mode is returned when selection is empty.
    /// </summary>
    [Fact]
    public void DetermineViewMode_Should_ReturnPersonal_WhenSelectionIsEmpty()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var selectedUsers = new List<Guid>();

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var mode = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Personal, mode);
    }
}