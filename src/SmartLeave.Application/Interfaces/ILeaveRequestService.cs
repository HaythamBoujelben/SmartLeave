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
        Task<LeaveRequestDto> CreateAsync(Guid employeeId, CreateLeaveRequestDto dto);
        Task<List<LeaveRequestDto>> GetMyRequestsAsync(Guid employeeId);
        Task<List<LeaveRequestDto>> GetAllPendingAsync();
        Task ReviewAsync(Guid requestId, Guid managerId, ReviewLeaveRequestDto dto);
        Task CancelAsync(Guid requestId, Guid employeeId);
    }
}
