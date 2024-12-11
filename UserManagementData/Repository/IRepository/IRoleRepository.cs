using UserManagementData.Dtos;

namespace UserManagementData.Repository.IRepository
{
    public interface IRoleRepository
    {
        Task<RequestResponse> CreateRole(string roleName);

    }
}
