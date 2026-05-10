using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartLeave.Application.DTOs.LeaveRequest;
using SmartLeave.Domain.Entities;
using SmartLeave.Domain.Enums;
using SmartLeave.Infrastructure.Persistence;
using SmartLeave.Infrastructure.Services;

namespace SmartLeave.Tests.Services;

public class LeaveRequestServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

        return new AppDbContext(options);
    }

    private (AppDbContext context, LeaveRequestService service) Setup()
    {
        var context = CreateInMemoryContext();
        var service = new LeaveRequestService(context);
        return (context, service);
    }

    // TEST 1 — Happy path: valid request should be created
    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldCreateLeaveRequest()
    {
        // Arrange
        var (context, service) = Setup();

        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "Annual",
            DefaultDays = 21
        };

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Haytham Test",
            Email = "test@test.com",
            PasswordHash = "hash",
            Role = "Employee"
        };

        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            TotalDays = 21,
            UsedDays = 0,
            Year = DateTime.UtcNow.Year
        };

        context.LeaveTypes.Add(leaveType);
        context.Employees.Add(employee);
        context.LeaveBalances.Add(balance);
        await context.SaveChangesAsync();

        var dto = new CreateLeaveRequestDto
        {
            LeaveTypeId = leaveType.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            Reason = "Family trip"
        };

        // Act
        var result = await service.CreateAsync(employee.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.EmployeeName.Should().Be("Haytham Test");
        result.TotalDays.Should().Be(3);
        result.Status.Should().Be("Pending");
    }

    // TEST 2 — No balance exists → should throw
    [Fact]
    public async Task CreateAsync_NoBalance_ShouldThrowException()
    {
        // Arrange
        var (context, service) = Setup();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test2@test.com",
            PasswordHash = "hash",
            Role = "Employee"
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var dto = new CreateLeaveRequestDto
        {
            LeaveTypeId = Guid.NewGuid(), // no balance for this type
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            Reason = "Test"
        };

        // Act
        var act = async () => await service.CreateAsync(employee.Id, dto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*balance*");
    }

    // TEST 3 — Not enough days → should throw
    [Fact]
    public async Task CreateAsync_InsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var (context, service) = Setup();

        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "Annual",
            DefaultDays = 21
        };

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test3@test.com",
            PasswordHash = "hash",
            Role = "Employee"
        };

        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            TotalDays = 21,
            UsedDays = 20, // only 1 day remaining
            Year = DateTime.UtcNow.Year
        };

        context.LeaveTypes.Add(leaveType);
        context.Employees.Add(employee);
        context.LeaveBalances.Add(balance);
        await context.SaveChangesAsync();

        var dto = new CreateLeaveRequestDto
        {
            LeaveTypeId = leaveType.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5), // requesting 5 days
            Reason = "Test"
        };

        // Act
        var act = async () => await service.CreateAsync(employee.Id, dto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Insufficient*");
    }

    // TEST 4 — Approve request → balance should be deducted
    [Fact]
    public async Task ReviewAsync_ApproveRequest_ShouldDeductBalance()
    {
        // Arrange
        var (context, service) = Setup();

        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "Annual",
            DefaultDays = 21
        };

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test4@test.com",
            PasswordHash = "hash",
            Role = "Employee"
        };

        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            TotalDays = 21,
            UsedDays = 0,
            Year = DateTime.UtcNow.Year
        };

        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            TotalDays = 3,
            Status = LeaveStatus.Pending
        };

        context.LeaveTypes.Add(leaveType);
        context.Employees.Add(employee);
        context.LeaveBalances.Add(balance);
        context.LeaveRequests.Add(request);
        await context.SaveChangesAsync();

        var dto = new ReviewLeaveRequestDto
        {
            IsApproved = true,
            ManagerNote = "Approved"
        };

        var managerId = Guid.NewGuid();

        // Act
        await service.ReviewAsync(request.Id, managerId, dto);

        // Assert
        var updatedBalance = await context.LeaveBalances.FirstAsync();
        updatedBalance.UsedDays.Should().Be(3);

        var updatedRequest = await context.LeaveRequests.FirstAsync();
        updatedRequest.Status.Should().Be(LeaveStatus.Approved);
    }

    // TEST 5 — Cancel request → status should be Cancelled
    [Fact]
    public async Task CancelAsync_PendingRequest_ShouldBeCancelled()
    {
        // Arrange
        var (context, service) = Setup();

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test5@test.com",
            PasswordHash = "hash",
            Role = "Employee"
        };

        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LeaveTypeId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            TotalDays = 3,
            Status = LeaveStatus.Pending
        };

        context.Employees.Add(employee);
        context.LeaveRequests.Add(request);
        await context.SaveChangesAsync();

        // Act
        await service.CancelAsync(request.Id, employee.Id);

        // Assert
        var updated = await context.LeaveRequests.FirstAsync();
        updated.Status.Should().Be(LeaveStatus.Cancelled);
    }
}