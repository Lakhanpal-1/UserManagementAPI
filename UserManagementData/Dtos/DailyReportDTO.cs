using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UserManagementData.Dtos
{
    public class DailyReportDTO
    {
        public int?  Id { get; set; }

        [Required(ErrorMessage = "Project Name is required.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Project Name must be between 4 and 50 characters long.")]
        public string ProjectName { get; set; }


        //public string? Task { get; set; }         

        public string TaskDescription { get; set; }

        [Required]
        public string Status { get; set; }

        public TimeSpan? TaskTime { get; set; }

        public DateTime? TodayDate { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ImageFilePath { get; set; } 

        public string? UserId { get; set; }

        public string? FullName { get; set; }

        public string? EmployeeId { get; set; }

        public string? Role { get; set; }

    }
}
