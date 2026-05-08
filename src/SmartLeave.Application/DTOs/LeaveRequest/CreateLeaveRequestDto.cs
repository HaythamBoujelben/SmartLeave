using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Application.DTOs.LeaveRequest
{
    public class CreateLeaveRequestDto
    {
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
