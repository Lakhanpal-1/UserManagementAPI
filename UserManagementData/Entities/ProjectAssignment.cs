using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementData.Entities
{
    public class ProjectAssignment
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }
       
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        public string UserId { get; set; }

        public virtual ProjectManagement Project { get; set; }
    }
}
