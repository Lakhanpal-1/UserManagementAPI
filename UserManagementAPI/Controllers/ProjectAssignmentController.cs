using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementData.Dtos;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;

namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectAssignmentsController : ControllerBase
    {
        private readonly IProjectAssignmentRepository _projectAssignmentRepository;
        private readonly IProjectManagementRepository _projectManagementRepository;
        private readonly IUserRepository _userRepository;

        public ProjectAssignmentsController(
            IProjectAssignmentRepository projectAssignmentRepository,
            IProjectManagementRepository projectManagementRepository,
            IUserRepository userRepository)
        {
            _projectAssignmentRepository = projectAssignmentRepository;
            _projectManagementRepository = projectManagementRepository;
            _userRepository = userRepository;
        }




        [HttpPost("AssignProject")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CreateProjectAssignment([FromBody] ProjectAssignmentDTO projectAssignmentDto)
        {
            if (projectAssignmentDto == null)
            {
                return BadRequest("ProjectAssignmentDTO cannot be null.");
            }

            var user = await _userRepository.GetEmployeeByName(projectAssignmentDto.UserName);
            if (user == null)
            {
                return BadRequest("Invalid UserName.");
            }

            var project = await _projectManagementRepository.GetByNameAsync(projectAssignmentDto.ProjectName);
            if (project == null)
            {
                return BadRequest("Invalid ProjectName.");
            }

            var timeNow = DateTimeOffset.Now.TimeOfDay;
            var dateNow = DateTime.Now.Date;

            var projectAssignment = new ProjectAssignment
            {
                Date = dateNow,
                Time = timeNow,
                StartDate = projectAssignmentDto.StartDate,
                EndDate = projectAssignmentDto.EndDate,
                ProjectId = project.Id,
                UserId = user.Id
            };

            await _projectAssignmentRepository.AddAsync(projectAssignment);

            projectAssignmentDto.Time = timeNow;
            projectAssignmentDto.Date = dateNow;

            var projectDto = new ProjectManagementDTO
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                TaskName = project.TaskName,
                TaskDescription = project.TaskDescription,
                Date = project.Date,
                UserId = project.UserId
            };
            projectAssignmentDto.Project = projectDto;

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                Role = user.Role,
                EmployeeId = user.EmployeeId,
                Designation = user.Designation
            };
            projectAssignmentDto.User = userDto;

            return CreatedAtAction(nameof(GetProjectAssignment), new { id = projectAssignment.Id }, projectAssignmentDto);
        }



        [HttpPut("UpdateProjectAssignment/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UpdateProjectAssignment(int id, [FromBody] ProjectAssignmentDTO projectAssignmentDto)
        {
            if (projectAssignmentDto == null)
            {
                return BadRequest("ProjectAssignmentDTO cannot be null.");
            }

            try
            {
                var projectAssignment = new ProjectAssignment
                {
                    Id = id,
                    StartDate = projectAssignmentDto.StartDate,
                    EndDate = projectAssignmentDto.EndDate
                };

                if (!string.IsNullOrEmpty(projectAssignmentDto.UserName))
                {
                    var user = await _userRepository.GetEmployeeByName(projectAssignmentDto.UserName);
                    if (user == null)
                    {
                        return BadRequest("Invalid UserName.");
                    }
                    projectAssignment.UserId = user.Id;
                }

                if (!string.IsNullOrEmpty(projectAssignmentDto.ProjectName))
                {
                    var project = await _projectManagementRepository.GetByNameAsync(projectAssignmentDto.ProjectName);
                    if (project == null)
                    {
                        return BadRequest("Invalid ProjectName.");
                    }
                    projectAssignment.ProjectId = project.Id;
                }

                await _projectAssignmentRepository.UpdateAsync(projectAssignment);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpDelete("DeleteProjectAssignment/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteProjectAssignment(int id)
        {
            try
            {
                await _projectAssignmentRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpGet("GetProjectAssignmentById/{id}")]
        public async Task<IActionResult> GetProjectAssignment(int id)
        {
            var projectAssignment = await _projectAssignmentRepository.GetByIdAsync(id);

            if (projectAssignment == null)
            {
                return NotFound();
            }

            return Ok(projectAssignment);
        }



        [HttpGet("GetAllProjectAssignments")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllProjectAssignments()
        {
            var projectAssignments = await _projectAssignmentRepository.GetAllProjectAssignmentsAsync();
            return Ok(projectAssignments);
        }



        [HttpGet("GetAllProjectAssignmentsByUserId")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetAllProjectAssignmentsByUserId()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Unauthorized();
                }

                var projectAssignments = await _projectAssignmentRepository.GetAllProjectAssignmentsByUserIdAsync(userId);

                return Ok(projectAssignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("GetAllProjects")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projectManagementRepository = HttpContext.RequestServices.GetService(typeof(IProjectManagementRepository)) as IProjectManagementRepository;

                if (projectManagementRepository == null)
                {
                    return StatusCode(500, "ProjectManagementRepository is not configured correctly.");
                }

                var projects = await projectManagementRepository.GetAllProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpGet("GetAllEmployees")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _userRepository.GetAllEmployee();
                if (employees == null || !employees.Any())
                {
                    return NotFound("No employees found.");
                }

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving employees: {ex.Message}");
            }
        }



        [HttpGet("DownloadAllProjectAssignmentsExcel")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> DownloadAllProjectAssignmentsExcel()
        {
            try
            {
                var excelBytes = await _projectAssignmentRepository.GenerateAllProjectAssignmentsExcelAsync();
                var excelFileName = $"AllProjectAssignments_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



    }
}
