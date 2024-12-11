using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementData.Dtos;
using UserManagementData.Repository.IRepository;
using Microsoft.Extensions.Logging;
using UserManagementData.Entities;
using OfficeOpenXml;


namespace UserManagementData.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRepository> _logger;
        private readonly AttendanceDbContext _context;

        public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserRepository> logger, AttendanceDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
        }

        public async Task<RequestResponse> AddEmployee(UserDto registration, string password)
        {
            var requestResponse = new RequestResponse();

            try
            {
                var userExists = await _userManager.FindByEmailAsync(registration.Email);
                if (userExists != null)
                {
                    requestResponse.Success = false;
                    requestResponse.Message = "Email already exists.";
                    return requestResponse;
                }

                // Use the first name and last name with a space in between as the base username
                var baseUserName = $"{registration.FirstName} {registration.LastName}";

                // Check if the base username is already taken
                var userName = baseUserName;
                var userWithSameUserName = await _userManager.FindByNameAsync(userName);
                int suffix = 1;

                // Append a suffix until a unique username is found
                while (userWithSameUserName != null)
                {
                    userName = $"{baseUserName}{suffix}";
                    userWithSameUserName = await _userManager.FindByNameAsync(userName);
                    suffix++;
                }

                var user = new ApplicationUser
                {
                    FirstName = registration.FirstName,
                    LastName = registration.LastName,
                    Email = registration.Email,
                    PhoneNumber = registration.PhoneNumber,
                    UserName = userName, // Use the unique username
                    Password = BCrypt.Net.BCrypt.HashPassword(registration.Password),
                    Role = registration.Role,
                    Designation = registration.Designation,
                    RegistrationDate = DateTime.Now,
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    requestResponse.Success = false;
                    requestResponse.Message = $"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    return requestResponse;
                }

                // Generate EmployeeId based on user's real Id
                user.EmployeeId = $"AT-000{user.Id.Substring(user.Id.Length - 2)}";
                await _userManager.UpdateAsync(user);

                // Assign role to the user
                if (!await _roleManager.RoleExistsAsync(registration.Role))
                {
                    requestResponse.Success = false;
                    requestResponse.Message = "Role does not exist.";
                    return requestResponse;
                }

                var roleResult = await _userManager.AddToRoleAsync(user, registration.Role);
                if (!roleResult.Succeeded)
                {
                    requestResponse.Success = false;
                    requestResponse.Message = $"Role assignment failed: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}";
                    return requestResponse;
                }

                requestResponse.Success = true;
                requestResponse.Message = "User registered and role assigned successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user: {Message}", ex.Message);
                requestResponse.Success = false;
                requestResponse.Message = "An error occurred while registering user. Please try again later.";
            }

            return requestResponse;
        }


        public async Task<RequestResponse> UpdateEmployee(string id, UserDto userDto)
        {
            var requestResponse = new RequestResponse();

            try
            {
                var existingUser = await _userManager.FindByIdAsync(id);
                if (existingUser == null)
                {
                    requestResponse.Success = false;
                    requestResponse.Message = "User not found.";
                    return requestResponse;
                }

                if (!string.IsNullOrWhiteSpace(userDto.FirstName))
                {
                    existingUser.FirstName = userDto.FirstName;
                }
                if (!string.IsNullOrWhiteSpace(userDto.LastName))
                {
                    existingUser.LastName = userDto.LastName;
                }

                // Update username combining updated FirstName and LastName
                var baseUserName = $"{existingUser.FirstName} {existingUser.LastName}";
                var userName = baseUserName;
                var userWithSameUserName = await _userManager.FindByNameAsync(userName);
                int suffix = 1;

                // Append a suffix until a unique username is found
                while (userWithSameUserName != null && userWithSameUserName.Id != existingUser.Id)
                {
                    userName = $"{baseUserName}{suffix}";
                    userWithSameUserName = await _userManager.FindByNameAsync(userName);
                    suffix++;
                }
                existingUser.UserName = userName;

                if (!string.IsNullOrWhiteSpace(userDto.Email))
                {
                    existingUser.Email = userDto.Email;
                }
                if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber))
                {
                    existingUser.PhoneNumber = userDto.PhoneNumber;
                }

                if (!string.IsNullOrWhiteSpace(userDto.Designation))
                {
                    existingUser.Designation = userDto.Designation;
                }

                // Update user's role in AspNetUsers table
                if (!string.IsNullOrWhiteSpace(userDto.Role))
                {
                    // Check if the role exists
                    if (!await _roleManager.RoleExistsAsync(userDto.Role))
                    {
                        requestResponse.Success = false;
                        requestResponse.Message = "Role does not exist.";
                        return requestResponse;
                    }

                    existingUser.Role = userDto.Role; // Update role in ApplicationUser

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (!result.Succeeded)
                    {
                        requestResponse.Success = false;
                        requestResponse.Message = $"User update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                        return requestResponse;
                    }
                }

              

                // Update user's role in AspNetUserRoles table
                var roles = await _userManager.GetRolesAsync(existingUser);
                var currentRole = roles.FirstOrDefault();

                if (currentRole != userDto.Role)
                {
                    // Remove current role
                    if (currentRole != null)
                    {
                        await _userManager.RemoveFromRoleAsync(existingUser, currentRole);
                    }

                    // Add new role
                    var roleResult = await _userManager.AddToRoleAsync(existingUser, userDto.Role);
                    if (!roleResult.Succeeded)
                    {
                        requestResponse.Success = false;
                        requestResponse.Message = $"Failed to update user role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}";
                        return requestResponse;
                    }
                }

                requestResponse.Success = true;
                requestResponse.Message = "User updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user: {Message}", ex.Message);
                requestResponse.Success = false;
                requestResponse.Message = "An error occurred while updating user. Please try again later.";
            }

            return requestResponse;
        }


        public async Task<RequestResponse> SoftDeleteUser(string id)
        {
            var requestResponse = new RequestResponse();

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    requestResponse.Success = false;
                    requestResponse.Message = "User not found.";
                    return requestResponse;
                }

                user.IsDeleted = true;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    requestResponse.Success = false;
                    requestResponse.Message = $"Soft delete failed: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    return requestResponse;
                }

                requestResponse.Success = true;
                requestResponse.Message = "User soft deleted successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while soft deleting user: {Message}", ex.Message);
                requestResponse.Success = false;
                requestResponse.Message = "An error occurred while soft deleting user. Please try again later.";
            }

            return requestResponse;
        }


        public async Task<UserDto> GetEmployeeById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            return new UserDto
            {
                Id = id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Password = user.Password,
                Role = user.Role,
                UserName = user.UserName,
                EmployeeId = user.EmployeeId,
                Designation = user.Designation,
                RegistrationDate = user.RegistrationDate,
                IsDeleted = user.IsDeleted
            };
        }


        public async Task<IEnumerable<UserDto>> GetAllEmployee()
        {
            try
            {
                // Retrieve users excluding deleted ones
                var users = await _context.Users
                                          .Where(u => u.IsDeleted != true)
                                          .ToListAsync();

                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    // Get roles of the user
                    var roles = await _userManager.GetRolesAsync(user);

                    // Check if user has HR or Employee role
                    if (roles.Contains("HR") || roles.Contains("Employee"))
                    {
                        userDtos.Add(new UserDto
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Role = user.Role,
                            UserName = user.UserName,
                            EmployeeId = user.EmployeeId,
                            Designation = user.Designation,
                            RegistrationDate = user.RegistrationDate,
                            IsDeleted = user.IsDeleted
                        });
                    }
                }

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users: {Message}", ex.Message);
                throw;
            }
        }


        public async Task<UserDto> GetEmployeeByName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || user.IsDeleted == true)
            {
                return null; // Return null if user is not found or is deleted
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                Role = user.Role,
                EmployeeId = user.EmployeeId,
                Designation = user.Designation,
                IsDeleted = user.IsDeleted ?? false // Handle nullable boolean correctly
            };
        }


        public async Task<(IEnumerable<DashboardCountDto>, int)> GetTotalEmployeesCount()
        {
            var users = await _context.Users
                .Where(u => (u.Role == "Employee" || u.Role == "HR") && (u.IsDeleted ?? false) == false) // Filter out deleted users correctly
                .Select(u => new DashboardCountDto
                {
                    EmployeeId = u.EmployeeId,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Count = 1 // Each user represents one count
                })
                .ToListAsync();

            int totalCount = users.Count();

            return (users, totalCount);
        }


        public async Task<MemoryStream> GenerateUsersExcelFileAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                var users = await _context.Users
         .Where(u => (u.Role == "Employee" || u.Role == "HR") && (u.IsDeleted ?? false) == false)
         .ToListAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Users");

                    worksheet.Cells[1, 1].Value = "Id";
                    worksheet.Cells[1, 2].Value = "First Name";
                    worksheet.Cells[1, 3].Value = "Last Name";
                    worksheet.Cells[1, 4].Value = "Email";
                    worksheet.Cells[1, 5].Value = "Phone Number";
                    worksheet.Cells[1, 6].Value = "Role";
                    worksheet.Cells[1, 7].Value = "UserName";
                    worksheet.Cells[1, 8].Value = "EmployeeId";
                    worksheet.Cells[1, 9].Value = "Designation";
                    worksheet.Cells[1, 10].Value = "IsDeleted";

                    int row = 2;
                    foreach (var user in users)
                    {
                        worksheet.Cells[row, 1].Value = user.Id;
                        worksheet.Cells[row, 2].Value = user.FirstName;
                        worksheet.Cells[row, 3].Value = user.LastName;
                        worksheet.Cells[row, 4].Value = user.Email;
                        worksheet.Cells[row, 5].Value = user.PhoneNumber;
                        worksheet.Cells[row, 6].Value = user.Role;
                        worksheet.Cells[row, 7].Value = user.UserName;
                        worksheet.Cells[row, 8].Value = user.EmployeeId;
                        worksheet.Cells[row, 9].Value = user.Designation;
                        worksheet.Cells[row, 10].Value = user.IsDeleted;

                        row++;
                    }

                    var stream = new MemoryStream(package.GetAsByteArray());
                    stream.Position = 0;
                    return stream;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the Excel file: {Message}", ex.Message);
                throw;
            }
        }


    }
}
