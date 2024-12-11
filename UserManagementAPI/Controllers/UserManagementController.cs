using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementData.Dtos;
using UserManagementData.Repository.IRepository;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserManagementController(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;   
            _roleRepository = roleRepository;
        }



        [HttpPost("AddEmployee")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> AddEmployee([FromBody] UserDto userDto)
        {
            if (userDto == null)
                return BadRequest(new { Success = false, Message = "Invalid registration data" });

            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Invalid model state" });

            try
            {
                var result = await _userRepository.AddEmployee(userDto, userDto.Password);
                if (!result.Success)
                    return Ok(new { Success = false, Message = result.Message });

                return Ok(new { Success = true, Message = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = $"User registration failed: {ex.Message}" });
            }
        }



        [HttpPut("UpdateEmployee/{id}")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] UserDto userDto)
        {
            if (userDto == null) return BadRequest("Invalid user data");

            try
            {
                var result = await _userRepository.UpdateEmployee(id, userDto);
                if (!result.Success) return StatusCode(500, result.Message);

                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"User update failed: {ex.Message}");
            }
        }



        [HttpDelete("SoftDeleteUser/{id}")]
        [Authorize(Roles = "Admin, HR")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            var response = await _userRepository.SoftDeleteUser(id);

            if (!response.Success)
                return BadRequest(response.Message);

            return Ok(response.Message);
        }



        [HttpGet("GetEmployeeById/{id}")]
        public async Task<IActionResult> GetEmployeeById(string id)
        {
            try
            {
                var user = await _userRepository.GetEmployeeById(id);
                if (user == null) return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving user: {ex.Message}");
            }
        }



        [HttpGet("GetAllEmployee")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> GetAllEmployee()
        {
            try
            {
                var users = await _userRepository.GetAllEmployee();
                if (users == null || !users.Any()) return NotFound("No users found.");

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving users: {ex.Message}");
            }
        }



        [HttpPost("CreateRole")]
        //[Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return BadRequest("Role name is required for role creation.");

            try
            {
                var result = await _roleRepository.CreateRole(roleName);
                if (!result.Success) return StatusCode(500, result.Message);

                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Role creation failed: {ex.Message}");
            }
        }



        [HttpGet("DownloadEmployeeExcel")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> DownloadUsersExcel()
        {
            try
            {
                var stream = await _userRepository.GenerateUsersExcelFileAsync();
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while downloading user data: {ex.Message}");
            }
        }


    }
}
