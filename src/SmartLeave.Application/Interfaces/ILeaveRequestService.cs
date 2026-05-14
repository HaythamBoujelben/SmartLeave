using SmartLeave.Application.DTOs.LeaveRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Application.Interfaces
{

    public interface ILeaveRequestService
    {
        Task<LeaveRequestDto> CreateAsync(Guid employeeId, CreateLeaveRequestDto dto, CancellationToken ct = default);
        Task<List<LeaveRequestDto>> GetMyRequestsAsync(Guid employeeId, CancellationToken ct = default);
        Task<List<LeaveRequestDto>> GetAllPendingAsync(CancellationToken ct = default);
        Task ReviewAsync(Guid requestId, Guid managerId, ReviewLeaveRequestDto dto, CancellationToken ct = default);
        Task CancelAsync(Guid requestId, Guid employeeId, CancellationToken ct = default);
    }
}
