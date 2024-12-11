using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class AttendanceScheduler : BackgroundService
{
    private readonly ILogger<AttendanceScheduler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AttendanceScheduler(ILogger<AttendanceScheduler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Attendance Scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var attendanceRepository = scope.ServiceProvider.GetRequiredService<IAttendanceRepository>();

                    // Fetch all users
                    var users = await userManager.Users.ToListAsync();

                    foreach (var user in users)
                    {
                        // Ensure the user has a valid registration date
                        if (user.RegistrationDate == null)
                        {
                            _logger.LogWarning($"User {user.Id} ({user.UserName}) does not have a valid registration date.");
                            continue;
                        }

                        // Check if the user has the roles "Employee" or "HR"
                        var roles = await userManager.GetRolesAsync(user);
                        if (!roles.Contains("Employee") && !roles.Contains("HR"))
                        {
                            continue; // Skip users who are not "Employee" or "HR"
                        }

                        var currentDate = DateTime.Now.Date;

                        // Check if currentDate is not a Saturday or Sunday
                        if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                        {
                            // Only mark leave for dates before today and after registration date
                            var startDate = user.RegistrationDate.Value.Date;
                            for (DateTime date = startDate; date < currentDate; date = date.AddDays(1))
                            {
                                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    var attendance = await attendanceRepository.GetAttendanceByDateForBackgroundServiceAsync(user.Id, date);

                                    if (attendance == null)
                                    {
                                        // Mark leave for the date
                                        await attendanceRepository.MarkLeaveAsync(user.Id, date);
                                        _logger.LogInformation($"Automatically marked leave for user {user.UserName} (ID: {user.Id}) for {date}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Attendance Scheduler: {Message}", ex.Message);
            }

            // Wait for 24 hours before running the scheduler again
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }

        _logger.LogInformation("Attendance Scheduler stopped.");
    }
}
