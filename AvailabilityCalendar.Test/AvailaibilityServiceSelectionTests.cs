using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using Moq;

namespace AvailabilityCalendar.Tests;

/// <summary>
/// Tests selection normalization behaviors in AvailabilityService.
/// </summary>
public class AvailabilityServiceSelectionTests
{
    /// <summary>
    /// Verifies current user is added and duplicates are removed.
    /// </summary>
    [Fact]
    public void NormalizeSelection_Should_AddCurrentUser_And_RemoveDuplicates()
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
        var normalized = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(2, normalized.Count);
        Assert.Contains(currentUserId, normalized);
        Assert.Contains(otherUserId, normalized);
    }

    /// <summary>
    /// Verifies empty selections normalize to only the current user.
    /// </summary>
    [Fact]
    public void NormalizeSelection_Should_ReturnOnlyCurrentUser_WhenSelectionIsEmpty()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var selectedUsers = new List<Guid>();

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var normalized = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Single(normalized);
        Assert.Contains(currentUserId, normalized);
    }
}