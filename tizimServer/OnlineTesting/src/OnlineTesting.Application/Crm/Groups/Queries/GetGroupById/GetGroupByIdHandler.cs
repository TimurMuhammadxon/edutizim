using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Groups.Queries.GetGroupById;

public class GetGroupByIdHandler : IRequestHandler<GetGroupByIdQuery, GroupDetailsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetGroupByIdHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GroupDetailsDto> Handle(GetGroupByIdQuery request, CancellationToken ct)
    {
        var group = await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == request.Id, ct)
            ?? throw new NotFoundException($"Group '{request.Id}' not found.");

        // A Teacher may only view a group they're actually assigned to.
        if (_currentUser.Role == Role.Teacher && group.TeacherId != _currentUser.UserId)
            throw new NotFoundException($"Group '{request.Id}' not found.");

        string? teacherName = null;
        if (group.TeacherId.HasValue)
        {
            var teacher = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == group.TeacherId.Value, ct);
            if (teacher is not null)
                teacherName = ((teacher.FirstName ?? "") + (teacher.LastName != null ? " " + teacher.LastName : "")).Trim();
        }

        string? roomName = null;
        int? roomCapacity = null;
        if (group.RoomId.HasValue)
        {
            var room = await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == group.RoomId.Value, ct);
            if (room is not null)
            {
                roomName = room.Name;
                roomCapacity = room.Capacity;
            }
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var studentRows = await (
            from gs in _db.GroupStudents.AsNoTracking()
            join s in _db.Students.AsNoTracking() on gs.StudentId equals s.Id
            where gs.GroupId == request.Id
            select new { Membership = gs, Student = s }
        ).ToListAsync(ct);

        var payments = await _db.TuitionPayments.AsNoTracking()
            .Where(p => p.GroupId == request.Id)
            .Select(p => new { p.StudentId, p.ForMonth, p.Amount })
            .ToListAsync(ct);
        var paymentsByStudent = payments.ToLookup(p => p.StudentId);

        var students = studentRows.Select(x =>
        {
            var effectivePrice = x.Membership.EffectivePrice(group.Price, today);
            var studentPayments = paymentsByStudent[x.Student.Id];
            var totalPaid = studentPayments.Sum(p => p.Amount);
            var monthlyPaid = studentPayments.GroupBy(p => p.ForMonth).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            var asOfDate = x.Membership.BalanceAsOfDate(today);
            var (balance, nextDueDate) = BalanceCalculator.Compute(
                x.Membership.ActivatedAt, totalPaid,
                month => x.Membership.EffectivePriceForMonth(month, group.Price),
                month => monthlyPaid.GetValueOrDefault(month, 0m), asOfDate, today);
            var isDebtor = x.Membership.Status != GroupMembershipStatus.Trial && balance < 0;

            return new GroupStudentDto(
                x.Student.Id, x.Student.FullName, x.Student.Phone,
                x.Membership.Status, balance, nextDueDate,
                effectivePrice, isDebtor,
                x.Membership.DiscountedPrice, x.Membership.DiscountStartDate, x.Membership.DiscountEndDate);
        }).ToList();

        var schedule = await _db.GroupScheduleSlots.AsNoTracking()
            .Where(s => s.GroupId == request.Id)
            .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
            .Select(s => new GroupScheduleSlotDto(s.DayOfWeek, s.StartTime, s.EndTime))
            .ToListAsync(ct);

        return new GroupDetailsDto(
            group.Id, group.BranchId, group.Name, group.Description, group.Price,
            group.TeacherId, teacherName, group.RoomId, roomName, roomCapacity,
            group.IsActive, group.CreatedAt, students, schedule);
    }
}
