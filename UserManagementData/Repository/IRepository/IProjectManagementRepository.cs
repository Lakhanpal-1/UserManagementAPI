using UserManagementData.Dtos;

namespace UserManagementData.Repository.IRepository
{
    public interface IProjectManagementRepository
    {

        Task<int> CreateProjectAsync(ProjectManagementDTO projectData);

        Task UpdateProjectAsync(int id, ProjectManagementDTO updatedProjectData);

        Task DeleteProjectAsync(int id);

        Task<ProjectManagementDTO> GetProjectAsync(int id);

        Task<IEnumerable<ProjectManagementDTO>> GetAllProjectsAsync();

        //Task<IEnumerable<ProjectManagementDTO>> GetAllProjectsByUserIdAsync(string userId);

        Task<ProjectManagement> GetByNameAsync(string name);

        Task<(IEnumerable<DashboardCountDto>, int)> GetTotalProjectsCount();

        Task<MemoryStream> GenerateProjectManagementExcelFileAsync();
        
    }
}
