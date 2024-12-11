using Microsoft.AspNetCore.Identity;

namespace UserManagementData.Entities
{
    public class ApplicationUser : IdentityUser
    {
   
        public string FirstName { get; set; }

        public string? LastName { get; set; }

        public override string Email { get; set; }

        public override string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        public string? EmployeeId { get; set; }

        public string? Designation { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool? IsDeleted { get; set; }

    }
}
