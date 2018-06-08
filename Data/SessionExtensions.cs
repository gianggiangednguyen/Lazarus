using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Lazarus.Data
{
    public static class SessionExtensions
    {
        public static void SetSessionObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetSessionObject<T>(this ISession session, string key) where T : class
        {
            string var = session.GetString(key);

            return var == null ? default(T) : JsonConvert.DeserializeObject<T>(var);
        }
    }
}
