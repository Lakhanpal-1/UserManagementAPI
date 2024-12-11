using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementData.Dtos;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DailyReportsController : ControllerBase
    {
        private readonly IDailyReportsRepository _repository;
        private readonly string _imagesPath;

        public DailyReportsController(IDailyReportsRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(_imagesPath))
            {
                Directory.CreateDirectory(_imagesPath);
            }
        }

        [HttpPost("DailyReport")]
        [Authorize(Policy = "HRorEmployee")]
        public async Task<ActionResult<int>> CreateReport([FromBody] DailyReportDTO dailyReport)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401, "Unauthorized");
            }

            if (dailyReport == null)
            {
                return BadRequest("Invalid request. Report data is required.");
            }

            //// Convert the JSON formatted TaskDescription to plain text
            dailyReport.TaskDescription = _repository.ExtractPlainTextFromJson(dailyReport.TaskDescription);

            string imagePath = null;
            if (dailyReport.ImageFile != null)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(dailyReport.ImageFile.FileName)}";
                imagePath = Path.Combine(_imagesPath, fileName);
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await dailyReport.ImageFile.CopyToAsync(fileStream);
                }
            }

            var report = new DailyReports
            {
                ProjectName = dailyReport.ProjectName,
                TaskDescription = dailyReport.TaskDescription,
                Status = dailyReport.Status,
                TodayDate = DateTime.Now.Date,
                TaskTime = DateTimeOffset.Now.TimeOfDay,
                ImageFilePath = imagePath,
                UserId = userId
            };

            var newReportId = await _repository.CreateReportAsync(report);
            return CreatedAtAction(nameof(GetReport), new { id = newReportId }, newReportId);
        }


        //[HttpPut("UpdateReportById/{id}")]
        //public async Task<IActionResult> UpdateReportById(int id, [FromBody] DailyReportDTO updatedReportDto)
        //{
        //    try
        //    {
        //        if (updatedReportDto == null || id <= 0)
        //        {
        //            return BadRequest("Invalid request. Report data or ID is missing.");
        //        }

        //        // Optionally map DTO to entity if needed
        //        var updatedReportEntity = new DailyReports
        //        {
        //            Id = id,
        //            ProjectName = updatedReportDto.ProjectName,
        //            Task = updatedReportDto.Task,
        //            TaskDescription = updatedReportDto.TaskDescription,
        //            Status = updatedReportDto.Status,
        //            TodayDate = updatedReportDto.TodayDate ?? DateTime.Now, // Example of default value
        //            TaskTime = updatedReportDto.TaskTime ?? TimeSpan.Zero, // Example of default value
        //            UserId = updatedReportDto.UserId,
        //            // Map other properties as needed
        //        };

        //        await _repository.UpdateReportByIdAsync(updatedReportEntity);
        //        return NoContent(); // 204 No Content
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return NotFound(ex.Message); // 404 Not Found
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Unauthorized(); // 401 Unauthorized
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message }); // 500 Internal Server Error
        //    }
        //}



        //[HttpDelete("DeleteReport/{id}")]
        ////[Authorize(Policy = "HRorEmployee")]
        //public async Task<IActionResult> DeleteReport(int id)
        //{
        //    try
        //    {
        //        await _repository.DeleteReportAsync(id);
        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception here
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        //    }
        //}



        [HttpGet("GetReportById/{id}")]
        public async Task<ActionResult<DailyReports>> GetReport(int id)
        {
            var report = await _repository.GetReportAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }



        [HttpGet("GetAllReports")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<ActionResult<IEnumerable<DailyReportDTO>>> GetAllReports()
        {
            var reports = await _repository.GetAllReportsAsync(includeUser: true);
            return Ok(reports);
        }



        [HttpGet("GetAllReportsByUserId")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetAllReportsByUserId()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized();
                }

                var reports = await _repository.GetAllReportsByUserIdAsync(userId);
                return Ok(reports);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }



        [Authorize(Policy = "AdminOrHR")]
        [HttpGet("GetAllReportsByCustomUserId/{customUserId}")]
        public async Task<IActionResult> GetAllReportsByCustomUserId(string customUserId)
        {
            try
            {
                var reports = await _repository.GetAllReportsByUserIdAsync(customUserId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }



        [HttpGet("DownloadDailyReportsExcelForUser/{customUserId}")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> DownloadDailyReportsExcel(string customUserId)
        {
            try
            {
                var stream = await _repository.GenerateDailyReportsExcelFileAsync(customUserId);

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DailyReports.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while downloading daily reports: {ex.Message}");
            }
        }



    }
}
