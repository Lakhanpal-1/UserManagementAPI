using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementData.Repository.IRepository;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardStatsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectManagementRepository _projectManagementRepository;
        private readonly IProjectAssignmentRepository _projectAssignmentRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IDailyReportsRepository _dailyReportRepository;

        public DashboardStatsController(
            IUserRepository userRepository,
            IProjectManagementRepository projectManagementRepository,
            IProjectAssignmentRepository projectAssignmentRepository,
            IAttendanceRepository attendanceRepository,
            IDailyReportsRepository dailyReportRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _projectManagementRepository = projectManagementRepository ?? throw new ArgumentNullException(nameof(projectManagementRepository));
            _projectAssignmentRepository = projectAssignmentRepository ?? throw new ArgumentNullException(nameof(projectAssignmentRepository));
            _attendanceRepository = attendanceRepository ?? throw new ArgumentNullException(nameof(attendanceRepository));
            _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
        }



        [HttpGet("GetTotalEmployeesCount")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetTotalEmployeesCount()
        {
            try
            {
                var (employees, totalCount) = await _userRepository.GetTotalEmployeesCount();
                var data = new
                {
                    TotalCount = totalCount,
                    Employees = employees

                };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving total employees count: {ex.Message}");
            }
        }



        [HttpGet("GetTotalPresentEmployees")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetTotalPresentEmployees()
        {
            try
            {
                var presentEmployees = await _attendanceRepository.GetPresentEmployeesDetailsAsync();
                var data = new
                {
                    Total = presentEmployees.Count,
                    Employees = presentEmployees
                };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving present employees: {ex.Message}");
            }
        }



        [HttpGet("GetTotalAbsentEmployees")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetTotalAbsentEmployees()
        {
            try
            {
                var absentEmployees = await _attendanceRepository.GetAbsentEmployeesDetailsAsync();
                var data = new
                {
                    Total = absentEmployees.Count,
                    Employees = absentEmployees
                };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving absent employees: {ex.Message}");
            }
        }



        [HttpGet("GetTotalProjectsCount")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetTotalProjectsCount()
        {
            var (projects, totalCount) = await _projectManagementRepository.GetTotalProjectsCount();
            var data = new
            {
                TotalCount = totalCount,
                Projects = projects.Select(p => new { p.ProjectName }).ToList()
            };
            return Ok(data);
        }



        [HttpGet("GetTotalAssignedProjectsCount")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetTotalAssignedProjectsCount()
        {
            var (assignments, totalCount) = await _projectAssignmentRepository.GetTotalAssignedProjectsCount();

            // Transform the data to include FullName and EmployeeId for each assignment
            var data = assignments.GroupBy(a => a.ProjectName)
                                  .Select(group => new
                                  {
                                      ProjectName = group.Key,
                                      Assignments = group.Select(a => new
                                      {
                                          FullName = a.FullName,
                                          EmployeeId = a.EmployeeId
                                      }).ToList(),
                                      Count = group.Sum(a => a.Count)
                                  });

            return Ok(new { TotalCount = totalCount, Projects = data });
        }



        //[HttpGet("GetTotalAssignedProjectsCount")]
        //[Authorize(Policy = "AdminOrHR")]
        //public async Task<IActionResult> GetTotalAssignedProjectsCount()
        //{
        //    try
        //    {
        //        var (assignments, totalCount) = await _projectAssignmentRepository.GetTotalAssignedProjectsCount();

        //        // Transform the data to include FullName and EmployeeId for each assignment
        //        var data = assignments.GroupBy(a => a.ProjectName)
        //                              .Select(group => new
        //                              {
        //                                  ProjectName = group.Key,
        //                                  Assignments = group.Select(a => new
        //                                  {
        //                                      FullName = a.FullName,
        //                                      EmployeeId = a.EmployeeId
        //                                  }).ToList(),
        //                                  Count = group.Sum(a => a.Count)
        //                              });

        //        return Ok(new { TotalCount = totalCount, Projects = data });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving assigned projects count: {ex.Message}");
        //    }
        //}



        [HttpGet("GetMarkedUsersOfTodayDailyReport")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetMarkedUsers()
        {
            try
            {
                var (totalCount, markedUsers) = await _dailyReportRepository.GetMarkedUsersAsync();

                var data = new
                {
                    TotalCount = totalCount,
                    MarkedUsers = markedUsers.Select(mu => new { FullName = mu.fullName, EmployeeId = mu.employeeId }).ToList()
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving marked users: {ex.Message}");
            }
        }



        [HttpGet("GetPendingUsersOfTodayDailyReport")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetPendingUsers()
        {
            try
            {
                var (totalCount, pendingUsers) = await _dailyReportRepository.GetPendingUsersAsync();

                var data = new
                {
                    TotalCount = totalCount,
                    PendingUsers = pendingUsers.Select(pu => new { FullName = pu.fullName, EmployeeId = pu.employeeId }).ToList()
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving pending users: {ex.Message}");
            }
        }



        [HttpGet("EmployeeAbsentDaysCountUpToThisMonth")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> EmployeeAbsentDaysCountUpToThisMonth()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Fetch absent dates count from repository 
                var absentDates = await _attendanceRepository.GetAbsentDaysCountUpToTodayAsync(userId);

                // Prepare response data
                var responseData = new
                {
                    TotalAbsent = absentDates.Count, // Total count of absent dates
                    AbsentDates = absentDates.Select(date => date.ToString("yyyy-MM-dd")).ToList() // Format dates as strings
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("EmployeeProjectsAssignedToCurrentUserThisMonth")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetProjectsAssignedToCurrentUserThisMonth()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Fetch projects assigned this month from repository
                var (totalCount, projectsAssignedThisMonth) = await _projectAssignmentRepository.GetProjectsAssignedToUserThisMonth(userId);

                // Prepare response data
                var responseData = new
                {
                    TotalProjects = totalCount, // Total count of projects assigned this month
                    Projects = projectsAssignedThisMonth.Select(project => new { ProjectName = project.ProjectName }).ToList()
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("GetLeaveCount")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetLeaveCount()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Fetch leave counts, leave dates, and allotted leaves from repository
                var (totalLeavesTaken, leaveDates, allottedLeaves) = await _attendanceRepository.GetLeavesTakenInCurrentYearAsync(userId);

                // Format leave dates in "dd//MM//yyyy" format
                var formattedLeaveDates = leaveDates.Select(date => date.ToString("dd//MM//yyyy")).ToList();

                // Calculate remaining leaves (always set to 0 if negative)
                var remainingLeaves = Math.Max(allottedLeaves - totalLeavesTaken, 0);

                // Calculate paid leaves taken if total leaves exceed allotted leaves
                var totalPaidLeavesTaken = totalLeavesTaken > allottedLeaves ? totalLeavesTaken - allottedLeaves : 0;

                // Fetch allotted leaves for the current year
                var allottedLeavesThisYear = allottedLeaves; // You can fetch this from a configuration or database

                // Prepare response data
                var responseData = new
                {
                    AllottedLeavesThisYear = allottedLeavesThisYear,
                    RemainingFromAllotted = remainingLeaves,
                    PaidLeavesTaken = totalPaidLeavesTaken,
                    TotalLeavesTaken = totalLeavesTaken , 
                    LeaveDates = formattedLeaveDates,
                    Count = formattedLeaveDates.Count// Use formatted leave dates
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }



        //[HttpGet("CalculateShortLeaves")]
        //[Authorize(Policy = "Employee")]
        //public async Task<IActionResult> CalculateShortLeaves()
        //{
        //    try
        //    {
        //        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            return Unauthorized();
        //        }

        //        // Fetch allotted short leaves per month from configuration or settings
        //        var allottedShortLeavesPerMonth = 4; // Example value, replace with actual logic to fetch from configuration

        //        // Calculate short leaves taken by the current user
        //        var (shortLeavesCount, paidShortLeaves) = await _attendanceRepository.CalculateShortLeavesAsync(userId, allottedShortLeavesPerMonth);

        //        // Determine remaining short leaves
        //        var remainingShortLeaves = allottedShortLeavesPerMonth - shortLeavesCount;

        //        // Prepare response data
        //        var responseData = new
        //        {
        //            AllottedShortLeavesThisMonth = allottedShortLeavesPerMonth,
        //            ShortLeavesTaken = shortLeavesCount,
        //            RemainingShortLeaves = remainingShortLeaves,
        //            PaidShortLeaves = paidShortLeaves
        //        };

        //        return Ok(responseData);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        //    }
        //}



        [HttpGet("CalculateShortLeaves")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> CalculateShortLeaves()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Fetch allotted short leaves per month from configuration or settings
                var allottedShortLeavesPerMonth = 4; // Example value, replace with actual logic to fetch from configuration

                // Calculate short leaves taken by the current user
                var (shortLeavesCount, paidShortLeaves, shortLeaveDates) = await _attendanceRepository.CalculateShortLeavesAsync(userId, allottedShortLeavesPerMonth);

                // Determine remaining short leaves
                var remainingShortLeaves = allottedShortLeavesPerMonth - shortLeavesCount;

                // Calculate total short leaves taken (allotted + paid)
                var totalShortLeavesTaken = shortLeavesCount + paidShortLeaves;

                // Format short leave dates in dd//mm//yyyy format
                var formattedShortLeaveDates = shortLeaveDates.Select(date => date.ToString("dd//MM//yyyy")).ToList();

                // Prepare response data
                var responseData = new
                {
                    AllottedShortLeavesThisMonth = allottedShortLeavesPerMonth,
                    RemainingFromAllotted = remainingShortLeaves,
                    PaidShortLeaves = paidShortLeaves,
                    TotalShortLeavesTaken = totalShortLeavesTaken,
                    ShortLeaveDates = formattedShortLeaveDates,
                    Count = formattedShortLeaveDates.Count
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }



    }
}