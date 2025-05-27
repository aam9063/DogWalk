namespace DogWalk_Application.Contracts.DTOs.Auth
{
    /// <summary>
    /// DTO para solicitar un nuevo token de acceso.
    /// </summary>
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}