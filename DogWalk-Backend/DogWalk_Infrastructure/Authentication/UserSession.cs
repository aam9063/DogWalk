using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DogWalk_Infrastructure.Authentication
{
    public class UserSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public UserSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public Guid UserId
        {
            get
            {
                var subClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                
                if (subClaim != null && Guid.TryParse(subClaim.Value, out Guid userId))
                {
                    return userId;
                }
                
                return Guid.Empty;
            }
        }
        
        public string UserEmail
        {
            get
            {
                return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            }
        }
        
        public string UserRole
        {
            get
            {
                return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
            }
        }
        
        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
            }
        }
        
        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext.User.IsInRole(role);
        }
    }
}