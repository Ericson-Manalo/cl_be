namespace cl_be.Models.Auth
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsRevoked { get; set; }
        public int TotalRefreshes { get; set; }
    }
}
