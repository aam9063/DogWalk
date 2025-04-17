using System;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    public class TokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public string UserType { get; set; } // "Usuario", "Paseador", "Admin"
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
