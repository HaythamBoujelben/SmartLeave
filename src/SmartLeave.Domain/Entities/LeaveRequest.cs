using SmartLeave.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Domain.Entities
{
    public class LeaveRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EmployeeId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        public string? Reason { get; set; }
        public string? ManagerNote { get; set; }
        public Guid? ReviewedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Employee Employee { get; set; } = null!;
        public LeaveType LeaveType { get; set; } = null!;
    }
}
