namespace cl_be.Models.Auth
{
    public class LoginCredentials
    {
        // Per la verificare esistenza
        public string Email { get; set; } = null!;

        // Con questo andremo a comparare con l'hash e il salt
        public string Password { get; set; }
    }
}
