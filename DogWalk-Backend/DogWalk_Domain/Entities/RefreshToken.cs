// En DogWalk_Domain/Entities/RefreshToken.cs
using System;

namespace DogWalk_Domain.Entities
{
    public class RefreshToken : EntityBase
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; }
        public string JwtId { get; private set; }
        public bool IsUsed { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public string CreatedByIp { get; private set; }
        public string UserAgent { get; private set; }

        // Propiedades de relación
        public virtual Usuario Usuario { get; private set; }

        private RefreshToken() : base() { } // Para EF Core

        public RefreshToken(
            Guid id,
            Guid userId,
            string token,
            string jwtId,
            DateTime expiryDate,
            string createdByIp,
            string userAgent
        ) : base(id)
        {
            UserId = userId;
            Token = token;
            JwtId = jwtId;
            IsUsed = false;
            IsRevoked = false;
            ExpiryDate = expiryDate;
            CreatedByIp = createdByIp;
            UserAgent = userAgent;
        }

        public void UseToken()
        {
            IsUsed = true;
            ActualizarFechaModificacion();
        }

        public void RevokeToken()
        {
            IsRevoked = true;
            ActualizarFechaModificacion();
        }

        public bool IsActive => !IsRevoked && !IsUsed && ExpiryDate > DateTime.UtcNow;
    }
}
