using UserManagementData.Dtos;

namespace UserManagementData.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<RequestResponse> AddEmployee(UserDto registration, string password);

        Task<RequestResponse> UpdateEmployee(string id, UserDto userDto);

        Task<RequestResponse> SoftDeleteUser(string id);

        Task<UserDto> GetEmployeeById(string id);

        Task<IEnumerable<UserDto>> GetAllEmployee();

        Task<UserDto> GetEmployeeByName(string userName);

        Task<(IEnumerable<DashboardCountDto>, int)> GetTotalEmployeesCount();

        Task<MemoryStream> GenerateUsersExcelFileAsync();

    }
}
