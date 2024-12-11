namespace UserManagementData.Dtos
{
    public class UserDto
    {
        public string? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Password { get; set; }

        public string? UserName { get; set; }

        public string? Role { get; set; }

        public string? EmployeeId { get; set; }

        public string? Designation { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool? IsDeleted { get; set; }

    }
}
