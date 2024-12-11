using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using UserManagementData.Dtos;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;

namespace UserManagementData.Repository
{
    public class ProjectAssignmentRepository : IProjectAssignmentRepository
    {
        private readonly AttendanceDbContext _context;

        public ProjectAssignmentRepository(AttendanceDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(ProjectAssignment projectAssignment)
        {
            _context.Add(projectAssignment);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateAsync(ProjectAssignment projectAssignment)
        {
            var existingAssignment = await _context.ProjectAssignments.FindAsync(projectAssignment.Id);
            if (existingAssignment == null)
            {
                throw new ArgumentException("ProjectAssignment not found.");
            }

            // Update only the fields that are provided in projectAssignment
            if (projectAssignment.StartDate != default)
            {
                existingAssignment.StartDate = projectAssignment.StartDate;
            }

            if (projectAssignment.EndDate != default)
            {
                existingAssignment.EndDate = projectAssignment.EndDate;
            }

            // Update ProjectId if provided
            if (projectAssignment.ProjectId != 0)
            {
                existingAssignment.ProjectId = projectAssignment.ProjectId;
            }

            // Update UserId if provided
            if (!string.IsNullOrEmpty(projectAssignment.UserId))
            {
                existingAssignment.UserId = projectAssignment.UserId;
            }

            _context.ProjectAssignments.Update(existingAssignment);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var projectAssignment = await _context.ProjectAssignments.FindAsync(id);
            if (projectAssignment != null)
            {
                _context.ProjectAssignments.Remove(projectAssignment);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<ProjectAssignmentDTO> GetByIdAsync(int id)
        {
            var query = from pa in _context.ProjectAssignments
                        join u in _context.Users on pa.UserId equals u.Id
                        join p in _context.ProjectManagements on pa.ProjectId equals p.Id
                        where pa.Id == id
                        select new ProjectAssignmentDTO
                        {
                            Id = p.Id,
                            StartDate = pa.StartDate,
                            EndDate = pa.EndDate,
                            Time = pa.Time,
                            Date = pa.Date,
                            UserName = u.FirstName + " " + u.LastName,
                            ProjectName = p.ProjectName,
                            TaskName = p.TaskName,
                            TaskDescription = p.TaskDescription
                        };

            return await query.SingleOrDefaultAsync();
        }


        public async Task<IEnumerable<ProjectAssignmentDTO>> GetAllProjectAssignmentsAsync()
        {
            var query = from pa in _context.ProjectAssignments
                        join u in _context.Users on pa.UserId equals u.Id
                        join p in _context.ProjectManagements on pa.ProjectId equals p.Id
                        select new ProjectAssignmentDTO
                        {
                            Id = p.Id,
                            StartDate = pa.StartDate,
                            EndDate = pa.EndDate,
                            Time = pa.Time,
                            Date = pa.Date,
                            UserName = u.FirstName + " " + u.LastName,
                            ProjectName = p.ProjectName,
                            TaskName = p.TaskName,
                            TaskDescription = p.TaskDescription
                        };

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<ProjectAssignmentDTO>> GetAllProjectAssignmentsByUserIdAsync(string userId)
        {
            var query = from pa in _context.ProjectAssignments
                        join u in _context.Users on pa.UserId equals u.Id
                        join p in _context.ProjectManagements on pa.ProjectId equals p.Id
                        where pa.UserId == userId
                        select new ProjectAssignmentDTO
                        {
                            Id = p.Id,
                            StartDate = pa.StartDate,
                            EndDate = pa.EndDate,
                            Time = pa.Time,
                            Date = pa.Date,
                            UserName = u.FirstName + " " + u.LastName,
                            ProjectName = p.ProjectName,
                            TaskName = p.TaskName,
                            TaskDescription = p.TaskDescription
                        };

            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<DashboardCountDto>, int)> GetTotalAssignedProjectsCount()
        {
            var query = from pa in _context.ProjectAssignments
                        join u in _context.Users on pa.UserId equals u.Id
                        join p in _context.ProjectManagements on pa.ProjectId equals p.Id
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


        public async Task<(int totalCount, List<ProjectAssignmentDTO> projectsAssigned)> GetProjectsAssignedToUserThisMonth(string userId)
        {
            DateTime today = DateTime.Today;
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);

            var projectsAssignedThisMonth = await _context.ProjectAssignments
                .Include(pa => pa.Project) // Include to fetch related Project entity
                .Where(pa => pa.UserId == userId && pa.Date >= startOfMonth && pa.Date <= today)
                .Select(pa => new ProjectAssignmentDTO
                {
                    ProjectName = pa.Project.ProjectName, // Assuming Project entity has a Name property
                    Date = pa.Date
                })
                .ToListAsync();

            int totalCount = projectsAssignedThisMonth.Count;

            return (totalCount, projectsAssignedThisMonth);
        } 


        public async Task<byte[]> GenerateAllProjectAssignmentsExcelAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var projectAssignments = await GetAllProjectAssignmentsAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ProjectAssignments");

                worksheet.Cells[1, 1].Value = "StartDate";
                worksheet.Cells[1, 2].Value = "EndDate";
                worksheet.Cells[1, 3].Value = "Time";
                worksheet.Cells[1, 4].Value = "Date";
                worksheet.Cells[1, 5].Value = "UserName";
                worksheet.Cells[1, 6].Value = "ProjectName";
                worksheet.Cells[1, 7].Value = "TaskName";
                worksheet.Cells[1, 8].Value = "TaskDescription";

                int row = 2;
                foreach (var projectAssignment in projectAssignments)
                {
                    // Format StartDate
                    worksheet.Cells[row, 1].Value = projectAssignment.StartDate.ToString("dd/MM/yyyy");

                    // Format EndDate
                    worksheet.Cells[row, 2].Value = projectAssignment.EndDate.ToString("dd/MM/yyyy");

                    // Format Time as TimeSpan
                    worksheet.Cells[row, 3].Value = projectAssignment.Time != null ? projectAssignment.Time.Value.ToString(@"hh\:mm") : string.Empty;

                    // Format Date
                    worksheet.Cells[row, 4].Value = projectAssignment.Date?.ToString("dd/MM/yyyy");

                    worksheet.Cells[row, 5].Value = projectAssignment.UserName;
                    worksheet.Cells[row, 6].Value = projectAssignment.ProjectName;
                    worksheet.Cells[row, 7].Value = projectAssignment.TaskName;
                    worksheet.Cells[row, 8].Value = projectAssignment.TaskDescription;
                    row++;
                }



                // Adjust column widths to fit the content
                worksheet.Cells.AutoFitColumns();

                // Adjust row heights (only if needed, based on content)
                for (int r = 1; r <= worksheet.Dimension.End.Row; r++)
                {
                    // Assuming a default row height of 20 (adjust as needed)
                    worksheet.Row(r).Height = 20;
                }

                return package.GetAsByteArray();
            }
        }

    }
}
