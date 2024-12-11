using Microsoft.EntityFrameworkCore;
using UserManagementData.Dtos;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;
using Microsoft.Extensions.Logging;

namespace UserManagementData.Repository
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AttendanceDbContext _context;
        private readonly ILogger<AttendanceRepository> _logger;

        public AttendanceRepository(AttendanceDbContext context, ILogger<AttendanceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<bool> MarkInTimeAsync(string userId)
        {
            try
            {
                var currentDate = DateTime.Now.Date;

                // Check if the user is deleted
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !(u.IsDeleted ?? false));
                if (user == null)
                {
                    return false; // User is deleted, cannot mark in-time
                }

                var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(a => a.UserId == userId && a.Date == currentDate && a.OutTime == null);

                if (existingAttendance != null)
                {
                    return false;
                }

                var attendance = new Attendance
                {
                    UserId = userId,
                    InTime = DateTimeOffset.Now.TimeOfDay,
                    OutTime = null,
                    WorkingHours = null,
                    IsOnLeave = false,
                    Date = currentDate
                };

                await _context.Attendances.AddAsync(attendance);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while marking in-time for user {userId}: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> MarkOutTimeAsync(string userId)
        {
            try
            {
                var currentDate = DateTime.Now.Date;

                // Check if the user is deleted
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !(u.IsDeleted ?? false));
                if (user == null)
                {
                    return false; // User is deleted, cannot mark out-time
                }

                var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(a => a.UserId == userId && a.Date == currentDate && a.OutTime == null);

                if (existingAttendance != null)
                {
                    existingAttendance.OutTime = DateTimeOffset.Now.TimeOfDay;
                    existingAttendance.WorkingHours = existingAttendance.OutTime.Value - existingAttendance.InTime.Value;

                    _context.Attendances.Update(existingAttendance);
                    return await _context.SaveChangesAsync() > 0;
                }
                else
                {
                    var inTimeRecord = await _context.Attendances.FirstOrDefaultAsync(a => a.UserId == userId && a.Date == currentDate && a.OutTime == null && a.InTime != null);

                    if (inTimeRecord != null)
                    {
                        inTimeRecord.OutTime = DateTimeOffset.Now.TimeOfDay;
                        inTimeRecord.WorkingHours = inTimeRecord.OutTime.Value - inTimeRecord.InTime.Value;

                        _context.Attendances.Update(inTimeRecord);
                        return await _context.SaveChangesAsync() > 0;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while marking out-time for user {userId}: {ex.Message}");
                throw;
            }
        }



        public async Task<bool> MarkLeaveAsync(string userId, DateTime leaveDate)
        {
            try
            {
                // Check if the user is deleted
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !(u.IsDeleted ?? false));
                if (user == null)
                {
                    return false; // User is deleted, cannot mark leave
                }

                // Check if there is already an attendance record with in-time, out-time, or working hours for the leave date
                var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.Date == leaveDate &&
                    (a.InTime != null || a.OutTime != null || a.WorkingHours != null));

                if (existingAttendance != null)
                {
                    return false; // Return false indicating leave cannot be marked as attendance is already marked
                }

                var existingLeave = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == leaveDate && (a.IsOnLeave == true || a.IsOnLeave == null));

                if (existingLeave != null)
                {
                    return false; // Return false indicating leave is already marked
                }

                var leave = new Attendance
                {
                    UserId = userId,
                    InTime = null,
                    OutTime = null,
                    WorkingHours = null,
                    IsOnLeave = true,
                    Date = leaveDate
                };

                await _context.Attendances.AddAsync(leave);
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while marking leave for user {userId} on {leaveDate}: {ex.Message}");
                return false;
            }
        }


        //public async Task<bool> EditAttendanceAsync(AttendanceDto attendanceDto)
        //{
        //    try
        //    {
        //        var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == attendanceDto.Id);
        //        if (existingAttendance != null)
        //        {
        //            // Preserve the original Date
        //            var originalDate = existingAttendance.Date;

        //            // Update existing attendance record
        //            existingAttendance.InTime = attendanceDto.InTime ?? existingAttendance.InTime; // Use provided or keep original if not provided
        //            existingAttendance.OutTime = attendanceDto.OutTime ?? existingAttendance.OutTime; // Use provided or keep original if not provided

        //            // Calculate WorkingHours based on provided or preserved times
        //            if (existingAttendance.InTime != default && existingAttendance.OutTime != default)
        //            {
        //                existingAttendance.WorkingHours = existingAttendance.OutTime - existingAttendance.InTime;
        //            }

        //            // Do not change the Date property here
        //            existingAttendance.Date = originalDate;

        //            _context.Attendances.Update(existingAttendance);
        //            return await _context.SaveChangesAsync() > 0;
        //        }
        //        else
        //        {
        //            var userId = attendanceDto.UserId; // Assuming UserId is part of AttendanceDto
        //            var currentDate = DateTime.Today;

        //            // Find the latest record for the user on the current date with no OutTime set
        //            var inTimeRecord = await _context.Attendances.FirstOrDefaultAsync(a =>
        //                a.UserId == userId && a.Date == currentDate && a.OutTime == null && a.InTime != null);

        //            if (inTimeRecord != null)
        //            {
        //                // Preserve the original Date
        //                var originalDate = inTimeRecord.Date;

        //                // Update the latest InTime record found
        //                inTimeRecord.InTime = attendanceDto.InTime ?? inTimeRecord.InTime; // Use provided or keep original if not provided
        //                inTimeRecord.OutTime = attendanceDto.OutTime ?? inTimeRecord.OutTime; // Use provided or keep original if not provided

        //                // Calculate WorkingHours based on provided or preserved times
        //                if (inTimeRecord.InTime != default && inTimeRecord.OutTime != default)
        //                {
        //                    inTimeRecord.WorkingHours = inTimeRecord.OutTime - inTimeRecord.InTime;
        //                }

        //                // Do not change the Date property here
        //                inTimeRecord.Date = originalDate;

        //                _context.Attendances.Update(inTimeRecord);
        //                return await _context.SaveChangesAsync() > 0;
        //            }
        //            else
        //            {
        //                // Handle case where no record is found
        //                return false; // No attendance record found to update
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Exception occurred while editing attendance for user {attendanceDto.UserId}: {ex.Message}");
        //        return false;
        //    }
        //}


        public async Task<bool> DeleteAttendanceAsync(int id)
        {
            try
            {
                var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id);
                if (attendance == null)
                {
                    return false; // Attendance not found
                }

                _context.Attendances.Remove(attendance);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while deleting attendance with ID {id}: {ex.Message}");
                return false;
            }
        }


        public async Task<AttendanceDto> GetAttendanceByIdAsync(int id, bool includeUser = false)
        {
            IQueryable<Attendance> query = _context.Attendances;

            if (includeUser)
            {
                query = query.Include(a => a.User);
            }

            var attendance = await query.FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return null;
            }

            return new AttendanceDto
            {
                Id = attendance.Id,
                UserId = attendance.UserId,
                InTime = attendance.InTime,
                OutTime = attendance.OutTime,
                WorkingHours = attendance.WorkingHours,
                Date = attendance.Date,
                IsOnLeave = attendance.IsOnLeave,
                FullName = includeUser ? (attendance.User.FirstName + " " + attendance.User.LastName) : null,
                EmployeeId = includeUser ? attendance.User.EmployeeId : null
            };
        }


        public async Task<IEnumerable<AttendanceDto>> GetAllAttendancesUsersAsync(bool includeUser = false)
        {
            var attendances = await _context.Attendances.ToListAsync();
            var users = includeUser ? await _context.Users.ToListAsync() : new List<ApplicationUser>();

            var query = from a in attendances
                        join u in users on a.UserId equals u.Id into attendanceUsers
                        from au in attendanceUsers.DefaultIfEmpty()
                        group a by a.UserId into grouped
                        select new
                        {
                            UserId = grouped.Key,
                            FullName = includeUser ? (grouped.FirstOrDefault()?.User.FirstName + " " + grouped.FirstOrDefault()?.User.LastName) : null,
                            EmployeeId = includeUser ? grouped.FirstOrDefault()?.User.EmployeeId : null,
                        };

            var result = new List<AttendanceDto>();
            foreach (var item in query)
            {
                var attendanceDto = new AttendanceDto
                {
                    UserId = item.UserId,
                    FullName = item.FullName,
                    EmployeeId = item.EmployeeId,
                };
                result.Add(attendanceDto);
            }

            return result;
        }


        public async Task<IEnumerable<AttendanceDto>> GetAllAttendanceByUserIdAsync(string userId, bool includeUser = false)
        {
            var users = await _context.Users.ToListAsync();
            var attendances = await _context.Attendances.Where(a => a.UserId == userId).ToListAsync();

            var query = from a in attendances
                        join u in users on a.UserId equals u.Id into attendanceUsers
                        from au in attendanceUsers.DefaultIfEmpty()
                        select new AttendanceDto
                        {
                            Id = a.Id,
                            UserId = a.UserId,
                            InTime = a.InTime,
                            OutTime = a.OutTime,
                            WorkingHours = a.WorkingHours,
                            Date = a.Date,
                            IsOnLeave = a.IsOnLeave,
                            FullName = au.FirstName + " " + au.LastName,
                            EmployeeId = au.EmployeeId,
                        };

            return query.ToList();
        }


        public async Task<IEnumerable<AttendanceDto>> GetAttendancesForDailyDateAsync(string customId)
        {
            var attendances = await _context.Attendances
                                            .Include(a => a.User)
                                            .Where(a => a.UserId == customId)
                                            .ToListAsync();

            var query = from a in attendances
                        group a by a.Date into grouped
                        select new
                        {
                            Date = grouped.Key,
                            UserId = customId,
                            FullName = $"{grouped.First().User.FirstName} {grouped.First().User.LastName}",
                            EmployeeId = grouped.First().User.EmployeeId,
                            Attendances = grouped.ToList()
                        };

            var result = new List<AttendanceDto>();
            foreach (var item in query)
            {
                var totalWorkingHours = await CalculateTotalWorkingHoursForDateAsync(customId, item.Date);
                var attendanceDto = new AttendanceDto
                {
                    UserId = item.UserId,
                    Date = item.Date,
                    WorkingHours = totalWorkingHours,
                    FullName = item.FullName,
                    EmployeeId = item.EmployeeId
                };
                result.Add(attendanceDto);
            }

            return result;
        }


        public async Task<TimeSpan> CalculateTotalWorkingHoursForDateAsync(string userId, DateTime date)
        {
            var attendances = await _context.Attendances
                                            .Where(a => a.UserId == userId && a.Date == date)
                                            .ToListAsync();

            TimeSpan totalWorkingHours = TimeSpan.Zero;

            foreach (var attendance in attendances)
            {
                if (attendance.InTime.HasValue && attendance.OutTime.HasValue)
                {
                    totalWorkingHours += attendance.OutTime.Value - attendance.InTime.Value;
                }
            }

            return totalWorkingHours;
        }


        public async Task<List<AttendanceDto>> GetPresentEmployeesDetailsAsync()
        {
            var today = DateTime.Today;

            // Get UserIds of users who have marked attendance today and are not deleted
            var userIds = await _context.Attendances
                .Where(a => a.InTime != null && a.Date == today
                         && (a.User.Role == "Employee" || a.User.Role == "HR")
                         && !(a.User.IsDeleted ?? false)) // Filter out deleted users
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            // Get details of users who have marked attendance today
            var presentEmployees = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new AttendanceDto
                {
                    UserId = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    EmployeeId = u.EmployeeId
                }).ToListAsync();

            return presentEmployees;
        }


        public async Task<List<AttendanceDto>> GetAbsentEmployeesDetailsAsync()
        {
            var today = DateTime.Today;

            // Get the UserIds of users who have marked their attendance today
            var presentUserIds = await _context.Attendances
                .Where(a => a.Date == today && a.InTime != null)
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            // Get details of users who have not marked attendance today and are not deleted
            var absentEmployees = await _context.Users
                .Where(u => (u.Role == "Employee" || u.Role == "HR")
                         && !presentUserIds.Contains(u.Id)
                         && !(u.IsDeleted ?? false)) // Filter out deleted users
                .Select(u => new AttendanceDto
                {
                    UserId = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    EmployeeId = u.EmployeeId,
                    Date = today
                }).ToListAsync();

            return absentEmployees;
        }


        public async Task<AttendanceDto> GetAttendanceByDateForBackgroundServiceAsync(string userId, DateTime date)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == date);

            if (attendance == null)
            {
                return null;
            }

            return new AttendanceDto
            {
                Id = attendance.Id,
                UserId = attendance.UserId,
                InTime = attendance.InTime,
                OutTime = attendance.OutTime,
                WorkingHours = attendance.WorkingHours,
                Date = attendance.Date,
                IsOnLeave = attendance.IsOnLeave,
                FullName = attendance.User?.FirstName + " " + attendance.User?.LastName,
                EmployeeId = attendance.User?.EmployeeId
            };
        }


        public async Task<List<DateTime>> GetAbsentDaysCountUpToTodayAsync(string userId)
        {
            var today = DateTime.Today;

            // Start date is the 1st day of the current month
            var startDate = new DateTime(today.Year, today.Month, 1);

            // End date is today's date
            var endDate = today;

            // Query for all days in the current month up to today
            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            // List to store absent dates
            var absentDates = new List<DateTime>();

            // Calculate absent days from the start of the month up to today's date, excluding weekends
            foreach (var attendance in attendances)
            {
                // Check if the day is a Saturday or Sunday
                if (attendance.Date.DayOfWeek != DayOfWeek.Saturday && attendance.Date.DayOfWeek != DayOfWeek.Sunday)
                {
                    // Check if the user was on leave (IsOnLeave is true) or if InTime is null (absent)
                    if (attendance.IsOnLeave == true || attendance.InTime == null)
                    {
                        absentDates.Add(attendance.Date.Date); // Add the date to the list of absent dates
                    }
                }
            }

            return absentDates;
        }


        public async Task<(int leavesTaken, List<DateTime> leaveDates, int allottedLeaves)> GetLeavesTakenInCurrentYearAsync(string userId)
        {
            var currentYear = DateTime.Now.Year;

            // Query to fetch leave dates and count the leaves taken by the user in the current year
            var leaveData = await _context.Attendances
                .Where(a => a.UserId == userId && a.Date.Year == currentYear && (a.IsOnLeave == true || a.IsOnLeave == null))
                .Select(a => new { a.Date })
                .ToListAsync();

            var leaveDates = leaveData.Select(ld => ld.Date).ToList();
            var leaveCount = leaveDates.Count;

            // Assuming 12 leaves per year are allotted
            var allottedLeaves = 12; // This can be fetched from a configuration or database

            // Return total leaves taken, leave dates, and allotted leaves
            return (leaveCount, leaveDates, allottedLeaves);
        }




        //public async Task<(int ShortLeavesTaken, int PaidShortLeaves)> CalculateShortLeavesAsync(string userId, int allottedShortLeavesPerMonth)
        //{
        //    try
        //    {
        //        // Get the current month and year
        //        var currentMonth = DateTime.Today.Month;
        //        var currentYear = DateTime.Today.Year;

        //        // Calculate the start and end dates for the current month
        //        var startDate = new DateTime(currentYear, currentMonth, 1);
        //        var endDate = startDate.AddMonths(1).AddDays(-1);

        //        // Fetch attendance records for the current user and current month
        //        var attendances = await _context.Attendances
        //            .Where(a => a.UserId == userId && a.Date >= startDate && a.Date <= endDate)
        //            .OrderBy(a => a.Date)
        //            .ToListAsync();

        //        // Define office hours (9:10 AM to 6:00 PM)
        //        var officeStartTime = new TimeSpan(9, 10, 0);
        //        var officeEndTime = new TimeSpan(18, 0, 0);

        //        // Calculate short leaves based on defined criteria
        //        int shortLeaveCount = 0;
        //        int paidShortLeaves = 0;

        //        foreach (var attendance in attendances)
        //        {
        //            if (attendance.InTime.HasValue && attendance.OutTime.HasValue)
        //            {
        //                // Initialize flags to avoid double-counting
        //                bool lateInHandled = false;
        //                bool earlyOutHandled = false;

        //                // Check if in time is after office start time
        //                if (attendance.InTime.Value > officeStartTime)
        //                {
        //                    // Calculate total duration from office start time to in time
        //                    var lateDuration = attendance.InTime.Value - officeStartTime;

        //                    // Calculate how many short leaves are required
        //                    int lateLeavesRequired = (int)Math.Ceiling(lateDuration.TotalHours / 3);

        //                    for (int i = 0; i < lateLeavesRequired; i++)
        //                    {
        //                        AddShortLeave(ref shortLeaveCount, ref paidShortLeaves, allottedShortLeavesPerMonth);
        //                    }

        //                    lateInHandled = true;
        //                }

        //                // Check if out time is before office end time
        //                if (attendance.OutTime.Value < officeEndTime)
        //                {
        //                    // Calculate total duration from out time to office end time
        //                    var earlyDuration = officeEndTime - attendance.OutTime.Value;

        //                    // Calculate how many short leaves are required
        //                    int earlyLeavesRequired = (int)Math.Ceiling(earlyDuration.TotalHours / 3);

        //                    for (int i = 0; i < earlyLeavesRequired; i++)
        //                    {
        //                        AddShortLeave(ref shortLeaveCount, ref paidShortLeaves, allottedShortLeavesPerMonth);
        //                    }

        //                    earlyOutHandled = true;
        //                }

        //                // Calculate total duration if both late in and early out
        //                if (!lateInHandled && !earlyOutHandled)
        //                {
        //                    var totalDuration = attendance.OutTime.Value - attendance.InTime.Value;

        //                    // Check if total duration is more than 3 hours
        //                    if (totalDuration.TotalHours > 3)
        //                    {
        //                        int extraLeaves = (int)Math.Ceiling(totalDuration.TotalHours / 3) - 1; // Subtract 1 because the first 3 hours count as the initial short leave

        //                        for (int i = 0; i < extraLeaves; i++)
        //                        {
        //                            AddShortLeave(ref shortLeaveCount, ref paidShortLeaves, allottedShortLeavesPerMonth);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        // Prepare response data
        //        return (ShortLeavesTaken: shortLeaveCount, PaidShortLeaves: paidShortLeaves);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException($"Error calculating short leaves: {ex.Message}");
        //    }
        //}

        //private void AddShortLeave(ref int shortLeaveCount, ref int paidShortLeaves, int allottedShortLeavesPerMonth)
        //{
        //    if (shortLeaveCount < allottedShortLeavesPerMonth)
        //    {
        //        shortLeaveCount++;
        //    }
        //    else
        //    {
        //        paidShortLeaves++;
        //    }
        //}


        public async Task<(int ShortLeavesTaken, int PaidShortLeaves, List<DateTime> ShortLeaveDates)> CalculateShortLeavesAsync(string userId, int allottedShortLeavesPerMonth)
        {
            try
            {
                // Get the current month and year
                var currentMonth = DateTime.Today.Month;
                var currentYear = DateTime.Today.Year;

                // Calculate the start and end dates for the current month
                var startDate = new DateTime(currentYear, currentMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Fetch attendance records for the current user and current month
                var attendances = await _context.Attendances
                    .Where(a => a.UserId == userId && a.Date >= startDate && a.Date <= endDate)
                    .OrderBy(a => a.Date)
                    .ToListAsync();

                // Define office hours (9:10 AM to 18:00 PM)
                var officeStartTime = new TimeSpan(9, 10, 0);
                var officeEndTime = new TimeSpan(18, 0, 0);

                // Calculate short leaves based on defined criteria
                int shortLeaveCount = 0;
                int paidShortLeaves = 0;
                var shortLeaveDates = new List<DateTime>();

                // Track which dates have already had a short leave marked
                var datesWithShortLeave = new HashSet<DateTime>();

                foreach (var attendance in attendances)
                {
                    if (attendance.InTime.HasValue && attendance.OutTime.HasValue)
                    {
                        var inTime = DateTime.Today + attendance.InTime.Value; // Convert TimeSpan to DateTime

                        // Check if in time is after office start time
                        if (inTime.TimeOfDay > officeStartTime)
                        {
                            // Check if this date already has a short leave marked
                            if (!datesWithShortLeave.Contains(attendance.Date))
                            {
                                datesWithShortLeave.Add(attendance.Date);
                                shortLeaveDates.Add(attendance.Date); // Add date to short leave dates

                                AddShortLeave(ref shortLeaveCount, ref paidShortLeaves, allottedShortLeavesPerMonth);
                            }
                        }

                        // Check if out time is before office end time
                        var outTime = DateTime.Today + attendance.OutTime.Value; // Convert TimeSpan to DateTime
                        if (outTime.TimeOfDay < officeEndTime)
                        {
                            // Check if this date already has a short leave marked
                            if (!datesWithShortLeave.Contains(attendance.Date))
                            {
                                datesWithShortLeave.Add(attendance.Date);
                                shortLeaveDates.Add(attendance.Date); // Add date to short leave dates

                                AddShortLeave(ref shortLeaveCount, ref paidShortLeaves, allottedShortLeavesPerMonth);
                            }
                        }

                        // Calculate total duration outside office hours
                        var duration = attendance.OutTime.Value - attendance.InTime.Value;

                        // Check if duration is more than 3 hours
                        if (duration.TotalHours > 3)
                        {
                            // Check if this date already has a short leave marked
                            if (!datesWithShortLeave.Contains(attendance.Date))
                            {
                                datesWithShortLeave.Add(attendance.Date);
                                shortLeaveDates.Add(attendance.Date); // Add date to short leave dates

                                int extraLeaves = (int)Math.Ceiling(duration.TotalHours / 3) - 1; // Subtract 1 because the first 3 hours count as the initial short leave
                                for (int i = 0; i < extraLeaves; i++)
                                {
                                    AddShortLeave(ref shortLeaveCount, ref paidShortLeaves, allottedShortLeavesPerMonth);
                                }
                            }
                        }
                    }
                }

                // Prepare response data
                return (ShortLeavesTaken: shortLeaveCount, PaidShortLeaves: paidShortLeaves, ShortLeaveDates: shortLeaveDates);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error calculating short leaves: {ex.Message}");
            }
        }

        private void AddShortLeave(ref int shortLeaveCount, ref int paidShortLeaves, int allottedShortLeavesPerMonth)
        {
            if (shortLeaveCount < allottedShortLeavesPerMonth)
            {
                shortLeaveCount++;
            }
            else
            {
                paidShortLeaves++;
            }
        }







    }
}

