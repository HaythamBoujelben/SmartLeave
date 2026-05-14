// LeaveRequestController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLeave.Application.DTOs.LeaveRequest;
using SmartLeave.Application.Interfaces;

namespace SmartLeave.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // ALL endpoints require JWT
public class LeaveRequestController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    // Helper — reads employeeId from JWT token
    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Employee: submit a leave request
    [HttpPost]
    public async Task<IActionResult> Create(CreateLeaveRequestDto dto, CancellationToken ct)
    {

        var result = await _leaveRequestService.CreateAsync(GetCurrentUserId(), dto, ct);
        return Ok(result);
    }

    // Employee: get my own requests
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests(CancellationToken ct)
    {
        var result = await _leaveRequestService.GetMyRequestsAsync(GetCurrentUserId(), ct);
        return Ok(result);
    }

    // Manager only: get all pending requests
    [HttpGet("pending")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _leaveRequestService.GetAllPendingAsync();
        return Ok(result);
    }

    // Manager only: approve or reject
    [HttpPut("{id}/review")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Review(Guid id, ReviewLeaveRequestDto dto)
    {

        await _leaveRequestService.ReviewAsync(id, GetCurrentUserId(), dto);
        return Ok(new { message = "Request reviewed successfully." });
    }

    // Employee: cancel own request
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {

        await _leaveRequestService.CancelAsync(id, GetCurrentUserId());
        return Ok(new { message = "Request cancelled." });

    }
}