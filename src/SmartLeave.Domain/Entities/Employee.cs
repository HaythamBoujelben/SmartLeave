using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // "Employee" or "Manager"
        public Guid? DepartmentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Department? Department { get; set; }
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
        public ICollection<LeaveBalance> LeaveBalances { get; set; } = [];
    }
}
