
using System.Security.Cryptography;
using System.Text;

namespace DogWalk_Domain.Common.ValueObjects;

public sealed record Password
    {
        public string Hash { get; }
        public string Salt { get; }
        
        private Password(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }
        
        public static Password Create(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));
                
            if (password.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres", nameof(password));
                
            // Generar salt aleatorio
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            
            string salt = Convert.ToBase64String(saltBytes);
            
            // Hashear contraseña con salt
            string hashedPassword = ComputeHash(password, salt);
            
            return new Password(hashedPassword, salt);
        }
        
        public bool Verify(string password)
        {
            return ComputeHash(password, Salt) == Hash;
        }
        
        private static string ComputeHash(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var passwordWithSalt = $"{password}{salt}";
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
            return Convert.ToBase64String(bytes);
        }
    }
