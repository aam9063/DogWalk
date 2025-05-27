namespace DogWalk_Application.Contracts.DTOs.Auth
{
    /// <summary>
    /// DTO para revocar un token de acceso.
    /// </summary>
    public class RevokeTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}