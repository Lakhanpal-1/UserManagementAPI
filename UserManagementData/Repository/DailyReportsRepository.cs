using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using UserManagementData.Dtos;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;

namespace UserManagementData.Repository
{
    public class DailyReportsRepository : IDailyReportsRepository
    {
        private readonly AttendanceDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DailyReportsRepository(AttendanceDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> CreateReportAsync(DailyReports report)
        {
            _context.DailyReports.Add(report);
            await _context.SaveChangesAsync();
            return report.Id; // Assuming Id is auto-generated
        }


        public string ExtractPlainTextFromJson(string jsonText)
        {
            var jsonData = JObject.Parse(jsonText);
            var blocks = jsonData["blocks"] as JArray;
            if (blocks != null && blocks.Count > 0)
            {
                // Combine all text blocks into a single string
                var plainText = string.Join("\n", blocks.Select(b => b["text"].ToString()));

                return plainText.Trim(); // Return plain text content
            }
            return string.Empty;
        }

        //public async Task UpdateReportByIdAsync(DailyReports updatedReport)
        //{
        //    var reportToUpdate = await _context.DailyReports.FindAsync(updatedReport.Id);

        //    if (reportToUpdate == null)
        //    {
        //        throw new ArgumentException("Daily report not found.");
        //    }

        //    // Authorization check (if needed)
        //    // var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    // Add authorization logic here if required

        //    // Update the report fields that are not null
        //    if (updatedReport.ProjectName != null)
        //    {
        //        reportToUpdate.ProjectName = updatedReport.ProjectName;
        //    }
        //    if (updatedReport.Task != null)
        //    {
        //        reportToUpdate.Task = updatedReport.Task;
        //    }
        //    if (updatedReport.TaskDescription != null)
        //    {
        //        reportToUpdate.TaskDescription = updatedReport.TaskDescription;
        //    }
        //    if (updatedReport.Status != null)
        //    {
        //        reportToUpdate.Status = updatedReport.Status;
        //    }
        //    if (updatedReport.TodayDate != null)
        //    {
        //        reportToUpdate.TodayDate = updatedReport.TodayDate.Value; // Ensure to access the underlying value
        //    }
        //    if (updatedReport.TaskTime != null)
        //    {
        //        reportToUpdate.TaskTime = updatedReport.TaskTime.Value; // Ensure to access the underlying value
        //    }

        //    _context.Entry(reportToUpdate).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteReportAsync(int id)
        //{
        //    var reportToDelete = await _context.DailyReports.FindAsync(id);
        //    if (reportToDelete == null)
        //    {
        //        throw new ArgumentException("Daily report not found.");
        //    }

        //    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userId) || reportToDelete.UserId != userId)
        //    {
        //        throw new UnauthorizedAccessException("User not authorized to delete this daily report.");
        //    }

        //    _context.DailyReports.Remove(reportToDelete);
        //    await _context.SaveChangesAsync();
        //}


        public async Task<DailyReportDTO> GetReportAsync(int id)
        {
            var reports = await _context.DailyReports.ToListAsync();
            var users = await _context.Users.ToListAsync();

            var query = (from d in reports
                         join u in users on d.UserId equals u.Id
                         where d.Id == id
                         select new DailyReportDTO
                         {
                             Id = d.Id,
                             UserId = d.UserId,
                             ProjectName = d.ProjectName,
                             //Task = d.Task,
                             TaskDescription = d.TaskDescription,
                             Status = d.Status,
                             TodayDate = d.TodayDate,
                             TaskTime = d.TaskTime,
                             FullName = u.FirstName + " " + u.LastName,
                             EmployeeId = u.EmployeeId,
                             ImageFile = ConvertToFile(d.ImageFilePath),
                             ImageFilePath = d.ImageFilePath
                         }).FirstOrDefault();

            return query;
        }


        public async Task<List<DailyReportDTO>> GetAllReportsAsync(bool includeUser = false)
        {
            var reports = await _context.DailyReports.ToListAsync();
            var users = includeUser ? await _context.Users.ToListAsync() : new List<ApplicationUser>();

            var query = from d in reports
                        join u in users on d.UserId equals u.Id into reportUsers
                        from ru in reportUsers.DefaultIfEmpty()
                        group d by d.UserId into groupedReports
                        select new DailyReportDTO
                        {
                            UserId = groupedReports.Key,
                            FullName = includeUser ? (groupedReports.FirstOrDefault()?.User.FirstName + " " + groupedReports.FirstOrDefault()?.User.LastName) : null,
                            EmployeeId = includeUser ? groupedReports.FirstOrDefault()?.User.EmployeeId : null
                        };

            return query.ToList();
        }


        public async Task<List<DailyReportDTO>> GetAllReportsByUserIdAsync(string userId)
        {
            var reports = await _context.DailyReports.Where(r => r.UserId == userId).ToListAsync();
            var users = await _context.Users.ToListAsync();

            var query = (from d in reports
                         join u in users on d.UserId equals u.Id
                         select new DailyReportDTO
                         {
                             Id = d.Id,
                             UserId = d.UserId,
                             ProjectName = d.ProjectName,
                             //Task = d.Task,
                             TaskDescription = d.TaskDescription,
                             Status = d.Status,
                             TodayDate = d.TodayDate,
                             TaskTime = d.TaskTime,
                             FullName = u.FirstName + " " + u.LastName,
                             EmployeeId = u.EmployeeId,
                             ImageFile = ConvertToFile(d.ImageFilePath),
                             ImageFilePath = d.ImageFilePath
                         }).ToList();

            return query;
        }


        private IFormFile ConvertToFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            try
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return new FormFile(fileStream, 0, fileStream.Length, null, Path.GetFileName(filePath))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/octet-stream"
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                return null;
            }
        }


        public async Task<MemoryStream> GenerateDailyReportsExcelFileAsync(string customUserId)
        {
            var reports = await GetAllReportsByUserIdAsync(customUserId);

            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Daily Reports");

                worksheet.Cells[1, 1].Value = "Full Name";
                worksheet.Cells[1, 2].Value = "Employee Id";
                worksheet.Cells[1, 3].Value = "Project Name";
                //worksheet.Cells[1, 4].Value = "Task";
                worksheet.Cells[1, 4].Value = "Task Description";
                worksheet.Cells[1, 5].Value = "Status";
                worksheet.Cells[1, 6].Value = "Today Date";
                worksheet.Cells[1, 7].Value = "Task Time";

                int row = 2;
                foreach (var report in reports)
                {
                    worksheet.Cells[row, 1].Value = report.FullName;
                    worksheet.Cells[row, 2].Value = report.EmployeeId;
                    worksheet.Cells[row, 3].Value = report.ProjectName;
                    //worksheet.Cells[row, 4].Value = report.Task;
                    worksheet.Cells[row, 4].Value = report.TaskDescription;
                    worksheet.Cells[row, 5].Value = report.Status;

                    // Format TodayDate
                    worksheet.Cells[row, 6].Value = report.TodayDate?.ToString("dd/MM/yyyy");

                    // Format TaskTime
                    worksheet.Cells[row, 7].Value = report.TaskTime?.ToString(@"hh\:mm");

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

                var stream = new MemoryStream(package.GetAsByteArray());
                stream.Position = 0;
                return stream;
            }
        }




        public async Task<(int totalCount, List<(string fullName, string employeeId)> markedUsers)> GetMarkedUsersAsync()
        {
            var today = DateTime.Today;

            // Get details of users who have submitted daily reports today and are in Employee or HR roles
            var markedUsers = await _context.DailyReports
                .Where(r => r.TodayDate == today && (r.User.Role == "Employee" || r.User.Role == "HR"))
                .Include(r => r.User) // Include the related User entity
                .Select(r => new
                {
                    FullName = r.User.FirstName + " " + r.User.LastName,
                    EmployeeId = r.User.EmployeeId
                })
                .ToListAsync();

            var totalCount = markedUsers.Count;

            // Prepare the response directly as a tuple
            var markedUsersList = markedUsers
                .Select(mu => (fullName: mu.FullName, employeeId: mu.EmployeeId))
                .ToList();

            return (totalCount, markedUsersList);
        }


        public async Task<(int totalCount, List<(string fullName, string employeeId)> pendingUsers)> GetPendingUsersAsync()
        {
            var today = DateTime.Today;

            // Get details of users who haven't submitted daily reports today and are in Employee or HR roles and not deleted
            var pendingUsers = await _context.Users
                .Where(u => !_context.DailyReports.Any(r => r.UserId == u.Id && r.TodayDate == today)
                         && (u.Role == "Employee" || u.Role == "HR")
                         && !(u.IsDeleted ?? false)) // Filter out deleted users
                .Select(u => new
                {
                    FullName = u.FirstName + " " + u.LastName,
                    EmployeeId = u.EmployeeId
                })
                .ToListAsync();

            var totalCount = pendingUsers.Count;

            // Prepare the response directly as a tuple
            var pendingUsersList = pendingUsers
                .Select(pu => (fullName: pu.FullName, employeeId: pu.EmployeeId))
                .ToList();

            return (totalCount, pendingUsersList);
        }

        

    }
}
