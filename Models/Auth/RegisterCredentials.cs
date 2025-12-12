namespace cl_be.Models.Auth
{
    public class RegisterCredentials
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string? MiddleName { get; set; } // optional

        public string Email { get; set; }

        public string Password { get; set; }

        public string? Phone { get; set; } // optional
    }
}
