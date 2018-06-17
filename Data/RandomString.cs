using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lazarus.Models;

namespace Lazarus.Data
{
    public class RandomString
    {
        public static string GenerateRandomString(IQueryable<string> sourceToCheck)
        {
            string result = "";
            again:
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var ran = new Random();
            var e = Enumerable.Repeat(chars, 10).Select(s => s[ran.Next(0, s.Length)]).ToArray();
            result = new string(e);
            foreach (var item in sourceToCheck)
            {
                if (item == result)
                {
                    goto again;
                }
            }

            return result;
        }
    }
}
