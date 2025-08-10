using FluentValidation;
using Scheduler.BLL.DTOs;

namespace Scheduler.BLL.Validators;

public class ScheduleRequestValidator : AbstractValidator<ScheduleRequestDto>
{
    public ScheduleRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Continue;

        RuleFor(x => x.ParticipantIds)
            .NotEmpty()
            .WithMessage("At least one participant is required.");

        
        RuleFor(x => x.ParticipantIds)
            .Must(list => list == null || list.Distinct().Count() == list.Count)
            .WithMessage("ParticipantIds must not contain duplicates.");
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than 0.");

        RuleFor(x => x.EarliestStart)
            .Custom((value, context) =>
            {
                if (value.TimeOfDay < TimeSpan.FromHours(9))
                {
                    context.AddFailure("EarliestStart", "EarliestStart must be at or after 09:00 UTC.");
                }
            });

        RuleFor(x => x.LatestEnd)
            .Custom((value, context) =>
            {
                if (value.TimeOfDay > TimeSpan.FromHours(17))
                {
                    context.AddFailure("LatestEnd", "LatestEnd must be at or before 17:00 UTC.");
                }
            });

        RuleFor(x => x)
            .Must(x => x.LatestEnd > x.EarliestStart)
            .WithMessage("LatestEnd must be after EarliestStart.")
            .OverridePropertyName(nameof(ScheduleRequestDto.LatestEnd));

        RuleFor(x => (x.LatestEnd - x.EarliestStart).TotalMinutes)
            .GreaterThanOrEqualTo(x => x.DurationMinutes)
            .WithMessage("Time window must be long enough to fit the meeting duration.")
            .OverridePropertyName(nameof(ScheduleRequestDto.DurationMinutes));
    }
}