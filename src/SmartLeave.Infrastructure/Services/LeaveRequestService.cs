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

    public async Task<LeaveRequestDto> CreateAsync(Guid employeeId, CreateLeaveRequestDto dto)
    {
        // Calculate total days
        var totalDays = (int)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;

        // Check leave balance
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == employeeId &&
                b.LeaveTypeId == dto.LeaveTypeId &&
                b.Year == DateTime.UtcNow.Year);

        if (balance == null)
            throw new Exception("No leave balance found for this leave type.");

        if (balance.RemainingDays < totalDays)
            throw new Exception($"Insufficient leave balance. You have {balance.RemainingDays} days remaining.");

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
        await _context.SaveChangesAsync();

        return await MapToDto(request.Id);
    }

    public async Task<List<LeaveRequestDto>> GetMyRequestsAsync(Guid employeeId)
    {
        return await _context.LeaveRequests
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
            .ToListAsync();
    }

    public async Task<List<LeaveRequestDto>> GetAllPendingAsync()
    {
        return await _context.LeaveRequests
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
            .ToListAsync();
    }

    public async Task ReviewAsync(Guid requestId, Guid managerId, ReviewLeaveRequestDto dto)
    {
        var request = await _context.LeaveRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new Exception("Leave request not found.");

        if (request.Status != LeaveStatus.Pending)
            throw new Exception("This request has already been reviewed.");

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
                    b.Year == DateTime.UtcNow.Year);

            if (balance != null)
                balance.UsedDays += request.TotalDays;
        }

        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid requestId, Guid employeeId)
    {
        var request = await _context.LeaveRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.EmployeeId == employeeId);

        if (request == null)
            throw new Exception("Leave request not found.");

        if (request.Status != LeaveStatus.Pending)
            throw new Exception("Only pending requests can be cancelled.");

        request.Status = LeaveStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task<LeaveRequestDto> MapToDto(Guid requestId)
    {
        var r = await _context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .FirstAsync(x => x.Id == requestId);

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