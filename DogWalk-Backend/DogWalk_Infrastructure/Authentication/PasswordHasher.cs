using System.Security.Cryptography;
using System.Text;

namespace DogWalk_Infrastructure.Authentication
{
    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;
        
        public static (string Hash, string Salt) HashPassword(string password)
        {
            // Generar un salt aleatorio
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            string saltString = Convert.ToBase64String(salt);
            
            // Generar el hash de la contraseña
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);
                
            string hashString = Convert.ToBase64String(hash);
            
            return (hashString, saltString);
        }
        
        public static bool VerifyPassword(string password, string hash, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            
            byte[] hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);
                
            string hashString = Convert.ToBase64String(hashToCompare);
            
            return hashString == hash;
        }
    }
}
