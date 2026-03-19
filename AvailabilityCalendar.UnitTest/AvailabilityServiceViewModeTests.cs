using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using AvailabilityCalendar.Domain.Enums;
using Moq;
using Xunit;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests view mode selection rules in AvailabilityService.
/// </summary>
public class AvailabilityServiceViewModeTests
{
    private static AvailabilityService CreateService()
    {
        return new AvailabilityService(new Mock<IEventRepository>().Object);
    }

    [Fact]
    public void DetermineViewMode_ShouldReturnPersonal_WhenOnlyCurrentUserIsSelected()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var selectedUsers = new List<Guid> { currentUserId };

        var service = CreateService();

        // Act
        var result = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Personal, result);
    }

    [Fact]
    public void DetermineViewMode_ShouldReturnShared_WhenMultipleUsersAreSelected()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid> { currentUserId, otherUserId };

        var service = CreateService();

        // Act
        var result = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Shared, result);
    }

    [Fact]
    public void DetermineViewMode_ShouldReturnShared_WhenOneOtherUserRemainsAfterNormalization()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            otherUserId,
            otherUserId
        };

        var service = CreateService();

        // Act
        var result = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Shared, result);
    }

    [Fact]
    public void DetermineViewMode_ShouldReturnPersonal_WhenOnlyCurrentUserRemainsAfterNormalization()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            currentUserId,
            currentUserId
        };

        var service = CreateService();

        // Act
        var result = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Personal, result);
    }

    [Fact]
    public void DetermineViewMode_ShouldReturnPersonal_WhenSelectionIsEmpty()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var selectedUsers = new List<Guid>();

        var service = CreateService();

        // Act
        var result = service.DetermineViewMode(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(ViewMode.Personal, result);
    }
}