using Xunit;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using Moq;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests selection normalization behaviors.
/// </summary>
public class SelectionTests
{
    /// <summary>
    /// Verifies the current user is added when missing from the selection.
    /// </summary>
    [Fact]
    public void NormalizeSelection_ShouldAddCurrentUser_WhenMissingFromSelection()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid> { otherUserId };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Contains(currentUserId, result);
    }

    /// <summary>
    /// Verifies only one instance of the current user is kept when duplicates exist.
    /// </summary>
    [Fact]
    public void NormalizeSelection_ShouldKeepOnlyOneInstanceOfCurrentUser_WhenAlreadyPresent()
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
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(1, result.Count(x => x == currentUserId));
    }

    /// <summary>
    /// Verifies normalize selection never returns an empty set.
    /// </summary>
    [Fact]
    public void NormalizeSelection_ShouldNeverReturnEmptySelection()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var selectedUsers = new List<Guid>();

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(currentUserId, result);
    }
}