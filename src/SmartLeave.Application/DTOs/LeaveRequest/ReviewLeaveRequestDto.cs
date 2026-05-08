using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Application.DTOs.LeaveRequest
{
    public class ReviewLeaveRequestDto
    {
        public bool IsApproved { get; set; }
        public string? ManagerNote { get; set; }
    }
}
