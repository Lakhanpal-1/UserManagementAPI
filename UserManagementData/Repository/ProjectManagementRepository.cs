using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Claims;
using UserManagementData.Dtos;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;

namespace UserManagementData.Repository
{
    public class ProjectManagementRepository : IProjectManagementRepository
    {
        private readonly AttendanceDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectManagementRepository(AttendanceDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }


        public async Task<int> CreateProjectAsync(ProjectManagementDTO projectData)
        {
            if (projectData == null)
            {
                throw new ArgumentNullException(nameof(projectData));
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }

            var project = new ProjectManagement
            {
                ProjectName = projectData.ProjectName,
                TaskName = projectData.TaskName,
                TaskDescription = projectData.TaskDescription,
                Date = DateTime.Now,
                UserId = userId
            };

            _context.ProjectManagements.Add(project);
            await _context.SaveChangesAsync();
            return project.Id;
        }


        public async Task UpdateProjectAsync(int id, ProjectManagementDTO updatedProjectData)
        {
            if (updatedProjectData == null)
            {
                throw new ArgumentNullException(nameof(updatedProjectData));
            }

            var projectToUpdate = await _context.ProjectManagements.FindAsync(id);
            if (projectToUpdate == null)
            {
                throw new ArgumentException("Project not found.");
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || projectToUpdate.UserId != userId)
            {
                throw new UnauthorizedAccessException("User not authorized to update this project.");
            }

            // Update only the fields that are provided in updatedProjectData
            if (!string.IsNullOrWhiteSpace(updatedProjectData.ProjectName))
            {
                projectToUpdate.ProjectName = updatedProjectData.ProjectName;
            }

            if (!string.IsNullOrWhiteSpace(updatedProjectData.TaskName))
            {
                projectToUpdate.TaskName = updatedProjectData.TaskName;
            }

            if (!string.IsNullOrWhiteSpace(updatedProjectData.TaskDescription))
            {
                projectToUpdate.TaskDescription = updatedProjectData.TaskDescription;
            }

            await _context.SaveChangesAsync();
        }


        public async Task DeleteProjectAsync(int id)
        {
            var projectToDelete = await _context.ProjectManagements.FindAsync(id);
            if (projectToDelete == null)
            {
                throw new ArgumentException("Project not found.");
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || projectToDelete.UserId != userId)
            {
                throw new UnauthorizedAccessException("User not authorized to delete this project.");
            }

            _context.ProjectManagements.Remove(projectToDelete);
            await _context.SaveChangesAsync();
        }


        public async Task<ProjectManagementDTO> GetProjectAsync(int id)
        {
            var project = await _context.ProjectManagements.FindAsync(id);
            return project != null ? new ProjectManagementDTO
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                TaskName = project.TaskName,
                TaskDescription = project.TaskDescription,
                Date = project.Date,
                UserId = project.UserId
            } : null;
        }


        public async Task<IEnumerable<ProjectManagementDTO>> GetAllProjectsAsync()
        {
            var projects = await _context.ProjectManagements.ToListAsync();
            return projects.Select(project => new ProjectManagementDTO
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                TaskName = project.TaskName,
                TaskDescription = project.TaskDescription,
                Date = project.Date
            });
        }


        public async Task<ProjectManagement> GetByNameAsync(string name)
        {
            return await _context.ProjectManagements.FirstOrDefaultAsync(p => p.ProjectName == name);
        }


        public async Task<(IEnumerable<DashboardCountDto>, int)> GetTotalProjectsCount()
        {
            var query = from p in _context.ProjectManagements
                        join u in _context.Users on p.UserId equals u.Id
                        select new DashboardCountDto
                        {
                            EmployeeId = u.EmployeeId,
                            FullName = u.FirstName + " " + u.LastName,
                            ProjectName = p.ProjectName,
                            Count = 1 // This will be summed up later
                        };

            var result = await query.ToListAsync();
            int totalCount = result.Count;

            return (result, totalCount);
        }


        public async Task<MemoryStream> GenerateProjectManagementExcelFileAsync()
        {
            var projects = await _context.ProjectManagements.ToListAsync();

            // Create Excel package
            using (var package = new ExcelPackage())
            {
                // Add a worksheet named "Projects"
                var worksheet = package.Workbook.Worksheets.Add("Projects");

                // Set headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Project Name";
                worksheet.Cells[1, 3].Value = "Task Name";
                worksheet.Cells[1, 4].Value = "Task Description";
                worksheet.Cells[1, 5].Value = "Date";

                // Populate data
                int row = 2;
                foreach (var project in projects)
                {
                    worksheet.Cells[row, 1].Value = project.Id;
                    worksheet.Cells[row, 2].Value = project.ProjectName;
                    worksheet.Cells[row, 3].Value = project.TaskName;
                    worksheet.Cells[row, 4].Value = project.TaskDescription;
                    worksheet.Cells[row, 5].Value = project.Date.ToString("yyyy-MM-dd");

                    row++;
                }

                // Prepare Excel file as a MemoryStream
                var stream = new MemoryStream(package.GetAsByteArray());
                stream.Position = 0;

                return stream;
            }
        }


        //public async Task<IEnumerable<ProjectManagementDTO>> GetAllProjectsByUserIdAsync(string userId)
        //{
        //    var projects = await _context.ProjectManagements
        //        .Where(project => project.UserId == userId)
        //        .ToListAsync();

        //    return projects.Select(project => new ProjectManagementDTO
        //    {
        //        Id = project.Id,
        //        ProjectName = project.ProjectName,
        //        TaskName = project.TaskName,
        //        TaskDescription = project.TaskDescription,
        //        Date = project.Date,
        //        UserId = project.UserId
        //    });
        //}


    }
}
