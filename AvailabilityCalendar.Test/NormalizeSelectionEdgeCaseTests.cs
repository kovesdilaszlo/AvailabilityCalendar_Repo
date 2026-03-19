using Xunit;
using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using Moq;

namespace AvailabilityCalendar.Tests.ApplicationTests;

public class NormalizeSelectionEdgeCaseTests
{
    /// <summary>
    /// Verifies duplicate selections are removed.
    /// </summary>
    [Fact]
    public void NormalizeSelection_ShouldRemoveDuplicates()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            currentUserId,
            otherUserId,
            otherUserId
        };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result.Count(x => x == otherUserId));
    }

    /// <summary>
    /// Verifies only the current user is returned when input contains only duplicates of them.
    /// </summary>
    [Fact]
    public void NormalizeSelection_ShouldReturnOnlyCurrentUser_WhenInputContainsOnlyDuplicatesOfCurrentUser()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            currentUserId,
            currentUserId,
            currentUserId
        };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Single(result);
        Assert.Contains(currentUserId, result);
    }

    /// <summary>
    /// Verifies valid selections keep current user and other users.
    /// </summary>
    [Fact]
    public void NormalizeSelection_ShouldKeepCurrentUserAndOthers_WhenSelectionIsValid()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUser1 = Guid.NewGuid();
        var otherUser2 = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            currentUserId,
            otherUser1,
            otherUser2
        };

        var service = new AvailabilityService(new Mock<IEventRepository>().Object);

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(currentUserId, result);
        Assert.Contains(otherUser1, result);
        Assert.Contains(otherUser2, result);
    }
}