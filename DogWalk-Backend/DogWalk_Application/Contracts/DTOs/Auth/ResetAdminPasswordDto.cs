using System;

namespace DogWalk_Application.Contracts.DTOs.Auth;

public class ResetAdminPasswordDto
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; }
    public string SecurityKey { get; set; } // ResetPasswordKey123
}
