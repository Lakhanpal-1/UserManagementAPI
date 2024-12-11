using System.ComponentModel.DataAnnotations.Schema;
using UserManagementData.Entities;

public class ProjectManagement
{
    public int Id { get; set; }

    public string ProjectName { get; set; }

    public string TaskName { get; set; }

    public string TaskDescription { get; set; }

    public DateTime Date { get; set; }

    public string UserId { get; set; }


    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }

}
