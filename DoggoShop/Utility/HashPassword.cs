using System.Security.Cryptography;
using System.Text;

namespace DoggoShopAPI.Utility
{
    public static class HashPassword
    {
        public static string Hash(string password)
        {
            var sha = SHA256.Create();
            var byteArray = Encoding.Default.GetBytes(password);
            var hashedPassword = sha.ComputeHash(byteArray);
            return Convert.ToBase64String(hashedPassword);
        }
    }
}
