using SmartLeave.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace SmartLeave.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
        public DbSet<Department> Departments => Set<Department>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // LeaveBalance: computed column not stored in DB
            modelBuilder.Entity<LeaveBalance>()
                .Ignore(x => x.RemainingDays);

            // Seed default leave types
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), Name = "Annual", DefaultDays = 21 },
                new LeaveType { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), Name = "Sick", DefaultDays = 10 },
                new LeaveType { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), Name = "Emergency", DefaultDays = 3 }
            );
        }
    }
}
