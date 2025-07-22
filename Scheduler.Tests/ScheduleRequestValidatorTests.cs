using FluentValidation.TestHelper;
using Scheduler.BLL.DTOs;

namespace Scheduler.Tests;

public class ScheduleRequestValidatorTests
{
    private readonly ScheduleRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_ParticipantIds_Is_Empty()
    {
        var model = new ScheduleRequestDto
        {
            ParticipantIds = [],
            DurationMinutes = 1,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:00:00Z"),
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ParticipantIds);
        Assert.Equal("At least one participant is required.", result.Errors[0].ErrorMessage);
        Assert.Equal("LatestEnd must be at or before 17:00 UTC.", result.Errors[1].ErrorMessage);
    }

    [Fact]
    public void Should_Have_Error_When_Duration_Is_Zero()
    {
        var model = new ScheduleRequestDto
        {
            ParticipantIds = [1],
            DurationMinutes = 0,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:00:00Z")
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
        Assert.Equal("Duration must be greater than 0.", result.Errors[0].ErrorMessage);
        Assert.Equal("LatestEnd must be at or before 17:00 UTC.", result.Errors[1].ErrorMessage);
    }

    [Fact]
    public void Should_Have_Error_When_LatestEnd_After_5PM()
    {
        var model = new ScheduleRequestDto
        {
            ParticipantIds = [1],
            DurationMinutes = 30,
            EarliestStart = DateTime.Parse("2025-06-20T10:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:01:00Z")
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.LatestEnd);
        Assert.Equal("LatestEnd must be at or before 17:00 UTC.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Should_Have_Error_When_LatestEnd_Before_EarliestStart()
    {
        var model = new ScheduleRequestDto
        {
            ParticipantIds = [1],
            DurationMinutes = 0,
            EarliestStart = DateTime.Parse("2025-06-20T15:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T14:59:00Z")
        };

        var result = _validator.TestValidate(model);
        Assert.Equal("LatestEnd must be after EarliestStart.", result.Errors[1].ErrorMessage);
        Assert.Equal("Time window must be long enough to fit the meeting duration.", result.Errors[2].ErrorMessage);
    }

    [Fact]
    public void Should_Have_Error_When_Window_Shorter_Than_Duration()
    {
        var model = new ScheduleRequestDto
        {
            ParticipantIds = [1],
            DurationMinutes = 61,
            EarliestStart = DateTime.Parse("2025-06-20T10:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T11:00:00Z")
        };

        var result = _validator.TestValidate(model);
        Assert.Equal("Time window must be long enough to fit the meeting duration.", result.Errors[0].ErrorMessage);
    }
}
