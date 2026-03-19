using AvailabilityCalendar.Application.Interfaces;
using AvailabilityCalendar.Application.Services;
using Moq;
using Xunit;

namespace AvailabilityCalendar.Tests.ApplicationTests;

/// <summary>
/// Tests selection normalization behavior in AvailabilityService.
/// </summary>
public class AvailabilityServiceSelectionTests
{
    private static AvailabilityService CreateService()
    {
        return new AvailabilityService(new Mock<IEventRepository>().Object);
    }

    [Fact]
    public void NormalizeSelection_ShouldAddCurrentUser_WhenMissingFromSelection()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid> { otherUserId };
        var service = CreateService();

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(currentUserId, result);
        Assert.Contains(otherUserId, result);
    }

    [Fact]
    public void NormalizeSelection_ShouldKeepOnlyOneInstanceOfCurrentUser_WhenAlreadyPresentMultipleTimes()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        var selectedUsers = new List<Guid>
        {
            currentUserId,
            currentUserId,
            currentUserId
        };

        var service = CreateService();

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Single(result);
        Assert.Equal(currentUserId, result[0]);
    }

    [Fact]
    public void NormalizeSelection_ShouldRemoveDuplicates_WhenSelectionContainsRepeatedUsers()
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

        var service = CreateService();

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result.Count(x => x == currentUserId));
        Assert.Equal(1, result.Count(x => x == otherUserId));
    }

    [Fact]
    public void NormalizeSelection_ShouldKeepCurrentUserAndOthers_WhenSelectionIsAlreadyValid()
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

        var service = CreateService();

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(currentUserId, result);
        Assert.Contains(otherUser1, result);
        Assert.Contains(otherUser2, result);
    }

    [Fact]
    public void NormalizeSelection_ShouldNeverReturnEmptySelection()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var selectedUsers = new List<Guid>();

        var service = CreateService();

        // Act
        var result = service.NormalizeSelection(selectedUsers, currentUserId);

        // Assert
        Assert.Single(result);
        Assert.Contains(currentUserId, result);
    }
}