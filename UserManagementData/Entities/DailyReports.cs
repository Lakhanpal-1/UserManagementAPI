using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementData.Entities
{
    public class DailyReports
    {

        [ForeignKey("UserId")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Project Name is required.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Project Name must be at least 4 characters long.")]
        public string ProjectName { get; set; }

        //public string? Task { get; set; }            

        public string TaskDescription { get; set; }

        [Required]
        public string Status { get; set; }          

        public DateTime? TodayDate { get; set; }

        public TimeSpan? TaskTime { get; set; }

        public string? ImageFilePath { get; set; }   

        public string UserId { get; set; }


        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
