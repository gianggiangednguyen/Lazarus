using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lazarus.Data
{
    public class RandomString
    {
        public static string GenerateRandomString()
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var ran = new Random();
            var e = Enumerable.Repeat(chars, 10).Select(s => s[ran.Next(0, s.Length)]).ToArray();
            string result = new string(e);
            return result;
        }
    }
}
