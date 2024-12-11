using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserManagementData.Entities
{
    public class AttendanceDbContext : IdentityDbContext<ApplicationUser>
    {
        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {

        }

        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<DailyReports> DailyReports { get; set; }

        public DbSet<ProjectManagement> ProjectManagements { get; set; }

        public DbSet<ProjectAssignment> ProjectAssignments { get; set; }

    }
}
