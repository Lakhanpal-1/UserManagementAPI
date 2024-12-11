using UserManagementData.Entities;

namespace UserManagementData.Repository.IRepository
{
    public interface ILoginRepository
    {
        Task<ApplicationUser> GetUserByEmailAndPassword(string email, string password);

    }
}
