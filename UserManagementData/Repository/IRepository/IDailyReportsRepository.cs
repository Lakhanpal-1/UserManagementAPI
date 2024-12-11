using UserManagementData.Dtos;
using UserManagementData.Entities;

namespace UserManagementData.Repository.IRepository
{
    public interface IDailyReportsRepository
    {

        Task<int> CreateReportAsync(DailyReports report);

        string ExtractPlainTextFromJson(string jsonText);

        //Task UpdateReportByIdAsync(DailyReports updatedReport);

        //Task DeleteReportAsync(int id);

        Task<DailyReportDTO> GetReportAsync(int id);

        Task<List<DailyReportDTO>> GetAllReportsAsync(bool includeUser = false);

        Task<List<DailyReportDTO>> GetAllReportsByUserIdAsync(string userId);

        Task<MemoryStream> GenerateDailyReportsExcelFileAsync(string customUserId);

        Task<(int totalCount, List<(string fullName, string employeeId)> markedUsers)> GetMarkedUsersAsync();

        Task<(int totalCount, List<(string fullName, string employeeId)> pendingUsers)> GetPendingUsersAsync();

    }
}