using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementData.Dtos;
using UserManagementData.Repository.IRepository;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectManagementController : ControllerBase
    {
        private readonly IProjectManagementRepository _repository;

        public ProjectManagementController(IProjectManagementRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        [HttpPost("CreateProject")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectManagementDTO projectData)
        {
            try
            {
                var newProjectId = await _repository.CreateProjectAsync(projectData);
                return CreatedAtAction(nameof(GetProject), new { id = newProjectId }, newProjectId);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPut("UpdateProject/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectManagementDTO updatedProjectData)
        {
            try
            {
                await _repository.UpdateProjectAsync(id, updatedProjectData);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpDelete("DeleteProject/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                await _repository.DeleteProjectAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpGet("GeProjectById/{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            try
            {
                var project = await _repository.GetProjectAsync(id);
                return project != null ? Ok(project) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpGet("GetAllProjects")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await _repository.GetAllProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

  

        [HttpGet("DownloadExcelForAllProjects")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> DownloadExcelForAll()
        {
            try
            {
                // Set EPPlus license context to NonCommercial
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                // Generate Excel file stream
                var stream = await _repository.GenerateProjectManagementExcelFileAsync();

                // Return Excel file as a FileResult
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProjectManagement.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while downloading project management data: {ex.Message}");
            }
        }



        //[HttpGet("GetAllProjectsByUserId")]
        //[Authorize(Policy = "HRorEmployee")]
        //public async Task<IActionResult> GetAllProjectsByUserId()
        //{
        //    try
        //    {
        //        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (userId == null)
        //        {
        //            return Unauthorized();
        //        }

        //        var projects = await _repository.GetAllProjectsByUserIdAsync(userId);
        //        return Ok(projects);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}



    }
}

