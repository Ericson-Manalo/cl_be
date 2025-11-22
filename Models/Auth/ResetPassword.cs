namespace cl_be.Models.Auth
{
    public class ResetPassword
    {
        // Per la verificare esistenza
        public string Email { get; set; } = null!;

        // Con questo andremo a realizzare l'hash e il salt
        public string NewPassword { get; set; }
    }
}
