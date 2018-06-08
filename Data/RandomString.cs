using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lazarus.Data
{
    public class RandomString
    {
        public static string CreateRandomString
        {
            get
            {
                return GenerateRandomString();
            }
        }

        private static string GenerateRandomString()
        {
            string result;
            const string randoms = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var ran = new Random();
            var e = Enumerable.Repeat(randoms, 10).Select(s => s[ran.Next(0, s.Length)]);
            result = e.ToString();
            return result;
        }
    }
}
