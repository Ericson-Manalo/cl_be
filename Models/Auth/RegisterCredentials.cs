namespace cl_be.Models.Auth
{
    public class RegisterCredentials
    {
        public string Name { get; set; }

        public string Surname { get; set; }
        
        public string? Middlename { get; set; } // optional

        public string Email { get; set; }

        public string Password { get; set; }

        public string? Phone { get; set; } // optional
    }
}
