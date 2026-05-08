using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Application.DTOs.LeaveRequest
{
    public class LeaveRequestDto
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? ManagerNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
