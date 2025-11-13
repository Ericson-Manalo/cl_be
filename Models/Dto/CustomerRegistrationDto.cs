using System;

namespace Models.Dto{
    public class CustomerRegistrationDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress, MaxLength(50)]
        public string EmailAddress { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; }

        [Phone, MaxLength(25)]
        public string? Phone { get; set; }
    }

}