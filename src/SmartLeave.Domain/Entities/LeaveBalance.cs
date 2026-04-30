using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Domain.Entities
{
    public class LeaveBalance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EmployeeId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays => TotalDays - UsedDays; // computed
        public int Year { get; set; } = DateTime.UtcNow.Year;

        // Navigation
        public Employee Employee { get; set; } = null!;
        public LeaveType LeaveType { get; set; } = null!;
    }
}
