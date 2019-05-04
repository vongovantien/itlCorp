using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL
{
    public static class RedisCacheHelper
    {
        // save 
        public static async Task SetObjectAsync<T>(IDistributedCache cache, string key, T value)
        {
            await cache.SetStringAsync(key, JsonConvert.SerializeObject(value));
        }
        public static void SetObject<T>(IDistributedCache cache, string key, T value)
        {
            cache.SetString(key, JsonConvert.SerializeObject(value));
        }
        // get
        public static async Task<T> GetObjectAsync<T>(IDistributedCache cache, string key)
        {
            var value = await cache.GetStringAsync(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        public static T GetObject<T>(IDistributedCache cache, string key)
        {
            var value = cache.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        // verify if an object exists
        public static async Task<bool> ExistObjectAsync<T>(IDistributedCache cache, string key)
        {
            var value = await cache.GetStringAsync(key);
            return value == null ? false : true;
        }
        public static bool ExistObject<T>(IDistributedCache cache, string key)
        {
            var value = cache.GetStringAsync(key);
            return value == null ? false : true;
        }

        public static IQueryable<T> Get<T>(IDistributedCache cache, string key)
        {
            var lstUnit = GetObject<List<T>>(cache, key);
            if (lstUnit != null)
            {
                return lstUnit.AsQueryable();
            }
            else
            {
                return null;
            }
        }
        public static List<T> GetList<T>(IDistributedCache cache, string key)
        {
            var lstUnit = GetObject<List<T>>(cache, key);
            if (lstUnit != null)
            {
                return lstUnit;
            }
            else
            {
                return null;
            }
        }
        public static void ChangeItemInList<T>(IDistributedCache cache, string key, T newItem, Func<T, bool> predicate)
        {
            var list = GetObject<List<T>>(cache, key);
            if(list != null)
            {
                int index = 0;
                foreach (var item in list)
                {
                    if (predicate(item))
                    {
                        list[index] = newItem;
                        break;
                    }
                    index++;
                }
                SetObject(cache, key, list);
            }
        }

        public static void RemoveItemInList<T>(IDistributedCache cache, string key, Func<T, bool> predicate)
        {
            var list = GetObject<List<T>>(cache, key);
            int index = 0;
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    list.RemoveAt(index);
                    break;
                }
                index++;
            }
            SetObject(cache, key, list);
        }
    }
}