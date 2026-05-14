using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLeave.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add the ones that don't exist yet
            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Status",
                table: "LeaveRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_Year",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "Year" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_LeaveRequests_Status", "LeaveRequests");
            migrationBuilder.DropIndex("IX_LeaveBalances_EmployeeId_Year", "LeaveBalances");
        }
    }
}
