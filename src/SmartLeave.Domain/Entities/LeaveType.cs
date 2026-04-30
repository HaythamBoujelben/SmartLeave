using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLeave.Domain.Entities
{
    public class LeaveType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // Annual, Sick, Emergency
        public int DefaultDays { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
