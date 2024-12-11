using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserManagementData.Dtos;
using UserManagementData.Repository.IRepository;

namespace UserManagementData.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleRepository> _logger;
        private readonly RoleManager<IdentityRole> roleManager;

        public RoleRepository(RoleManager<IdentityRole> roleManager, ILogger<RoleRepository> logger, RoleManager<IdentityRole> _roleManager)
        {
            _roleManager = roleManager;
            _logger = logger;
            roleManager = _roleManager;
        }

        public async Task<RequestResponse> CreateRole(string roleName)
        {
            var requestResponse = new RequestResponse();

            try
            {
                // Ensure only predefined roles can be created
                var predefinedRoles = new[] { "Admin", "HR", "Employee" };
                if (!predefinedRoles.Contains(roleName))
                {
                    requestResponse.Success = false;
                    requestResponse.Message = "Invalid role specified.";
                    return requestResponse;
                }

                // Check if role exists, if not create it
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var newRole = new IdentityRole(roleName);
                    var roleResult = await _roleManager.CreateAsync(newRole);
                    if (!roleResult.Succeeded)
                    {
                        requestResponse.Success = false;
                        requestResponse.Message = "Role creation failed.";
                        return requestResponse;
                    }

                    requestResponse.Success = true;
                    requestResponse.Message = "Role created successfully";
                }
                else
                {
                    requestResponse.Success = false;
                    requestResponse.Message = "Role already exists.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while creating role.");

                requestResponse.Success = false;
                requestResponse.Message = "An error occurred while creating role. Please try again later.";
            }

            return requestResponse;
        }


    }
}
