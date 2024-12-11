namespace UserManagementData.Dtos
{
    public class ProjectManagementDTO
    {
        public int Id { get; set; }

        public string? ProjectName { get; set; }

        public string? TaskName { get; set; }

        public string? TaskDescription { get; set; }

        public DateTime? Date { get; set; }

        public string? UserId { get; set; }

    }
}
