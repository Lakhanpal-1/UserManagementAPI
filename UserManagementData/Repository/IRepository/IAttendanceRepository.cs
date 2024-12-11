using UserManagementData.Dtos;

namespace UserManagementData.Repository.IRepository
{
    public interface IAttendanceRepository
    {

        Task<bool> MarkInTimeAsync(string userId);

        Task<bool> MarkOutTimeAsync(string userId);

        Task<bool> MarkLeaveAsync(string userId, DateTime leaveDate);

        //Task<bool> EditAttendanceAsync(AttendanceDto attendanceDto);

        Task<bool> DeleteAttendanceAsync(int id);

        Task<AttendanceDto> GetAttendanceByIdAsync(int id, bool includeUser = false);

        //For Admin and HR 
        Task<IEnumerable<AttendanceDto>> GetAllAttendancesUsersAsync(bool includeUser);

        //For Get DailyWorking Hours 
        Task<IEnumerable<AttendanceDto>> GetAttendancesForDailyDateAsync(string customId);

        //For  Admin & HR & Employee
        Task<IEnumerable<AttendanceDto>> GetAllAttendanceByUserIdAsync(string userId, bool includeUser = false);

        
        Task<TimeSpan> CalculateTotalWorkingHoursForDateAsync(string userId, DateTime date);

        Task<List<AttendanceDto>> GetPresentEmployeesDetailsAsync();

        Task<List<AttendanceDto>> GetAbsentEmployeesDetailsAsync();

        //For Background Leave Service
        Task<AttendanceDto> GetAttendanceByDateForBackgroundServiceAsync(string userId, DateTime date);

        //For Employees to see the Absent of 1 month
        Task<List<DateTime>> GetAbsentDaysCountUpToTodayAsync(string userId);


        Task<(int leavesTaken, List<DateTime> leaveDates, int allottedLeaves)> GetLeavesTakenInCurrentYearAsync(string userId);


        //Task<(int ShortLeavesTaken, int PaidShortLeaves)> CalculateShortLeavesAsync(string userId, int allottedShortLeavesPerMonth);

        Task<(int ShortLeavesTaken, int PaidShortLeaves, List<DateTime> ShortLeaveDates)> CalculateShortLeavesAsync(string userId, int allottedShortLeavesPerMonth);

    }
}
