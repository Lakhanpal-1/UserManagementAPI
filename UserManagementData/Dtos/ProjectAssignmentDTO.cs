using UserManagementData.Dtos;

public class ProjectAssignmentDTO
{
    public int? Id { get; set; }
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan? Time { get; set; }

    public DateTime? Date { get; set; }

    public UserDto? User { get; set; }

    public ProjectManagementDTO? Project { get; set; }

    public string? UserName { get; set; }

    public string? ProjectName { get; set; }

    public string? TaskDescription { get; set; }

    public string? TaskName { get; set; }

}
