using Microsoft.AspNetCore.Identity;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;

namespace UserManagementData.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }



        public async Task<ApplicationUser> GetUserByEmailAndPassword(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                return user;
            }

            return null;
        }



    }
}
