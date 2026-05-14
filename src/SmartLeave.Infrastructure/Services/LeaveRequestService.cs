// LeaveRequestService.cs
using Microsoft.EntityFrameworkCore;
using SmartLeave.Application.DTOs.LeaveRequest;
using SmartLeave.Application.Interfaces;
using SmartLeave.Domain.Entities;
using SmartLeave.Domain.Enums;
using SmartLeave.Infrastructure.Persistence;

namespace SmartLeave.Infrastructure.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly AppDbContext _context;

    public LeaveRequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequestDto> CreateAsync(Guid employeeId, CreateLeaveRequestDto dto,CancellationToken ct = default)
    {
        // Calculate total days
        var totalDays = (int)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;

        // Check leave balance
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == employeeId &&
                b.LeaveTypeId == dto.LeaveTypeId &&
                b.Year == DateTime.UtcNow.Year, ct);

        if (balance == null)
            throw new KeyNotFoundException("No leave balance found for this leave type.");

        if (balance.RemainingDays < totalDays)
            throw new ArgumentException("Insufficient leave balance. You have {balance.RemainingDays} days remaining.");

        // Check for overlapping requests
        var hasOverlap = await _context.LeaveRequests
            .AnyAsync(r =>
                r.EmployeeId == employeeId &&
                r.Status == LeaveStatus.Pending || r.Status == LeaveStatus.Approved &&
                r.StartDate <= dto.EndDate &&
                r.EndDate >= dto.StartDate);

        if (hasOverlap)
            throw new Exception("You already have a leave request for this period.");

        var request = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending
        };

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync(ct);

        return await MapToDto(request.Id,ct);
    }

    public async Task<List<LeaveRequestDto>> GetMyRequestsAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _context.LeaveRequests
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.LeaveType)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new LeaveRequestDto
            {
                Id = r.Id,
                EmployeeName = r.Employee.FullName,
                LeaveTypeName = r.LeaveType.Name,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TotalDays = r.TotalDays,
                Status = r.Status.ToString(),
                Reason = r.Reason,
                ManagerNote = r.ManagerNote,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<List<LeaveRequestDto>> GetAllPendingAsync(CancellationToken ct = default)
    {
        return await _context.LeaveRequests
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.LeaveType)
            .Where(r => r.Status == LeaveStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new LeaveRequestDto
            {
                Id = r.Id,
                EmployeeName = r.Employee.FullName,
                LeaveTypeName = r.LeaveType.Name,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TotalDays = r.TotalDays,
                Status = r.Status.ToString(),
                Reason = r.Reason,
                ManagerNote = r.ManagerNote,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task ReviewAsync(Guid requestId, Guid managerId, ReviewLeaveRequestDto dto,CancellationToken ct = default)
    {
        var request = await _context.LeaveRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request == null)
            throw new KeyNotFoundException("Leave request not found.");

        if (request.Status != LeaveStatus.Pending)
            throw new ArgumentException("This request has already been reviewed.");

        request.Status = dto.IsApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        request.ManagerNote = dto.ManagerNote;
        request.ReviewedById = managerId;
        request.UpdatedAt = DateTime.UtcNow;

        // If approved — deduct from balance
        if (dto.IsApproved)
        {
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(b =>
                    b.EmployeeId == request.EmployeeId &&
                    b.LeaveTypeId == request.LeaveTypeId &&
                    b.Year == DateTime.UtcNow.Year, ct); 

            if (balance != null)
                balance.UsedDays += request.TotalDays;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Guid requestId, Guid employeeId, CancellationToken ct = default)
    {
        var request = await _context.LeaveRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.EmployeeId == employeeId,ct);

        if (request == null)
            throw new Exception("Leave request not found.");

        if (request.Status != LeaveStatus.Pending)
            throw new ArgumentException("Only pending requests can be cancelled.");

        request.Status = LeaveStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    private async Task<LeaveRequestDto> MapToDto(Guid requestId, CancellationToken ct = default)
    {
        var r = await _context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .FirstAsync(x => x.Id == requestId, ct);

        return new LeaveRequestDto
        {
            Id = r.Id,
            EmployeeName = r.Employee.FullName,
            LeaveTypeName = r.LeaveType.Name,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            TotalDays = r.TotalDays,
            Status = r.Status.ToString(),
            Reason = r.Reason,
            ManagerNote = r.ManagerNote,
            CreatedAt = r.CreatedAt
        };
    }
}