using System.Security.Cryptography;
using System.Text;

namespace cwnu_mc_auth_server.Services
{
    public static class Util
    {
        public static string GetSHA256Hash(string x)
        {
            var sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(x));
                for (int i = 0; i < result.Length; i++)
                    sb.Append(result[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
