using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementData.Repository.IRepository;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceController(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }


        [HttpPost("markInTime")]
        [Authorize(Policy = "HRorEmployee")]
        public async Task<IActionResult> MarkInTime()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401, "Unauthorized");
            }

            var result = await _attendanceRepository.MarkInTimeAsync(userId);
            if (result)
            {
                return Ok("InTime marked successfully");
            }
            else
            {
                return StatusCode(400, "You cannot mark attendance. User may have been deleted or you need to fill out previous OutTime first.");
            }
        }


        [HttpPost("markOutTime")]
        [Authorize(Policy = "HRorEmployee")]
        public async Task<IActionResult> MarkOutTime()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _attendanceRepository.MarkOutTimeAsync(userId);
                if (result)
                {
                    return Ok("OutTime marked successfully");
                }
                else
                {
                    return BadRequest("Failed to mark OutTime. User may have been deleted or you need to fill out InTime first.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions without logging
                return StatusCode(500, "Internal server error");
            }
        }



        //[HttpPost("markLeave")]
        //[Authorize(Policy = "HRorEmployee")]
        //public async Task<IActionResult> MarkLeave()
        //{
        //    var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (userId == null)
        //    {
        //        return StatusCode(401, "Unauthorized");
        //    }

        //    try
        //    {
        //        var currentDate = DateTime.Now.Date;
        //        var result = await _attendanceRepository.MarkLeaveAsync(userId, currentDate);
        //        if (result)
        //        {
        //            return Ok("Leave marked successfully");
        //        }
        //        else
        //        {
        //            return StatusCode(400, "Failed to mark leave. User may have been deleted or today's attendance is already submitted for this user.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}



        //[HttpPut("EditAttendance/{id}")]
        //[Authorize(Policy = "AdminOrHR")]
        //public async Task<IActionResult> EditAttendance(int id, [FromBody] AttendanceDto attendanceDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        // Set the ID from the route parameter into the DTO
        //        attendanceDto.Id = id;

        //        var result = await _attendanceRepository.EditAttendanceAsync(attendanceDto);
        //        if (result)
        //        {
        //            return Ok("Attendance edited successfully");
        //        }
        //        else
        //        {
        //            return StatusCode(500, "An error occurred while editing the attendance");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while editing the attendance: {ex.Message}");
        //    }
        //}



        [HttpDelete("DeleteAttendance/{id}")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var result = await _attendanceRepository.DeleteAttendanceAsync(id);
            if (result)
            {
                return Ok("Attendance deleted successfully");
            }
            else
            {
                return StatusCode(500, "An error occurred while deleting the attendance");
            }
        }



        [HttpGet("GetAttendanceById/{id}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id, includeUser: true);
            if (attendance == null)
            {
                return NotFound();
            }
            return Ok(attendance);
        }



        [HttpGet("GetAllAttendances")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllAttendancesUsers()
        {
            var attendances = await _attendanceRepository.GetAllAttendancesUsersAsync(includeUser: true);
            return Ok(attendances);
        }



        [HttpGet("GetAllAttendanceByCustomUserId")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllAttendanceByCustomUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required.");
                }

                var attendances = await _attendanceRepository.GetAllAttendanceByUserIdAsync(userId);
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("GetAllAttendancesForDailyDate")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllAttendancesForDailyDate(string customId)
        {
            var attendances = await _attendanceRepository.GetAttendancesForDailyDateAsync(customId);
            return Ok(attendances);
        }



        [HttpGet("GetAllAttendanceByUserId")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetAllAttendanceByUserId()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized();
                }

                var attendances = await _attendanceRepository.GetAllAttendanceByUserIdAsync(userId);
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("GetAllAttendanceDetailsByUserId")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetAllAttendanceDetailsByUserId()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var attendanceDetails = await _attendanceRepository.GetAttendancesForDailyDateAsync(userId);
            if (attendanceDetails == null || !attendanceDetails.Any())
            {
                return NotFound("Attendance details not found.");
            }

            return Ok(attendanceDetails);
        }


       
    }
}
