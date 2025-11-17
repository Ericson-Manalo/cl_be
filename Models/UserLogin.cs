using System;
using System.Collections.Generic;

namespace cl_be.Models;

public partial class UserLogin
{
    public int CustomerId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public byte Role { get; set; }

    public bool AsUpdated { get; set; }
}
