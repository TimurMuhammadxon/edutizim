using MediatR;

namespace OnlineTesting.Application.Crm.Groups.Commands.SetGroupSchedule;

public record SetGroupScheduleCommand(Guid GroupId, List<ScheduleSlotInput> Slots) : IRequest;

public record ScheduleSlotInput(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
