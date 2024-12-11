using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementData.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public TimeSpan? InTime { get; set; }

        public TimeSpan? OutTime { get; set; }

        public TimeSpan? WorkingHours { get; set; }

        public bool? IsOnLeave { get; set; }

        public DateTime Date { get; set; }


        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

    }
}
