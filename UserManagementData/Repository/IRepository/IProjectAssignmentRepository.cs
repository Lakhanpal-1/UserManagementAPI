using UserManagementData.Dtos;
using UserManagementData.Entities;

namespace UserManagementData.Repository.IRepository
{
    public interface IProjectAssignmentRepository
    {
        Task AddAsync(ProjectAssignment projectAssignment);

        Task UpdateAsync(ProjectAssignment projectAssignment);

        Task DeleteAsync(int id);

        Task<ProjectAssignmentDTO> GetByIdAsync(int id);

        Task<IEnumerable<ProjectAssignmentDTO>> GetAllProjectAssignmentsAsync();

        Task<IEnumerable<ProjectAssignmentDTO>> GetAllProjectAssignmentsByUserIdAsync(string userId);

        Task<(IEnumerable<DashboardCountDto>, int)> GetTotalAssignedProjectsCount();

        Task<(int totalCount, List<ProjectAssignmentDTO> projectsAssigned)> GetProjectsAssignedToUserThisMonth(string userId);

        Task<byte[]> GenerateAllProjectAssignmentsExcelAsync();

    }
}
