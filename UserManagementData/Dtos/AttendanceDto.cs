namespace UserManagementData.Dtos
{
    public class AttendanceDto
    {
        public int? Id { get; set; }

        public string? UserId { get; set; }

        public TimeSpan? InTime { get; set; }

        public TimeSpan? OutTime { get; set; }

        public TimeSpan? WorkingHours { get; set; }

        public DateTime Date { get; set; }

        public bool? IsOnLeave { get; set; }

        public string? FullName { get; set; }

        public string? EmployeeId { get; set; }

    }
}
