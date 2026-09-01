using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Groups.Queries.GetGroupById;

public record GetGroupByIdQuery(Guid Id) : IRequest<GroupDetailsDto>;

public record GroupDetailsDto(
    Guid Id,
    Guid BranchId,
    string Name,
    string? Description,
    decimal Price,
    Guid? TeacherId,
    string? TeacherName,
    Guid? RoomId,
    string? RoomName,
    int? RoomCapacity,
    bool IsActive,
    DateTime CreatedAt,
    List<GroupStudentDto> Students,
    List<GroupScheduleSlotDto> Schedule);

public record GroupStudentDto(
    Guid StudentId,
    string FullName,
    string Phone,
    GroupMembershipStatus Status,
    decimal Balance,
    DateOnly? NextPaymentDueDate,
    decimal EffectivePrice,
    bool IsDebtor,
    decimal? DiscountedPrice,
    DateOnly? DiscountStartDate,
    DateOnly? DiscountEndDate);

public record GroupScheduleSlotDto(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
