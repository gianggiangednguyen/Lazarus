using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Lazarus.Data
{
    public static class TempDataExtensions
    {
        public static void SetTempDataObject<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T GetTempDataObject<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object obj = new object();
            tempData.TryGetValue(key, out obj);

            return obj == null ? default(T) : JsonConvert.DeserializeObject<T>((string)obj);
        }
    }
}
