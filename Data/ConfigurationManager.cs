using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data.Sql;
using UMC.Data.Entities;

namespace UMC.Data
{
    /// <summary>
    /// 数据缓存代理
    /// </summary>
    /// <param name="key"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public delegate System.Collections.Hashtable DataCacheCallback(Guid key, string name, System.Collections.Hashtable cache);
    /// <summary>
    /// 配置管理
    /// </summary>
    public sealed class ConfigurationManager
    {


        public static System.Collections.Hashtable DataCache(Guid key, string name, int timeout, DataCacheCallback callback)
        {
            var cache = DataFactory.Instance().Cache(key, name);
            if (cache != null)
            {
                var oldValue = JSON.Deserialize<System.Collections.Hashtable>(cache.CacheData);
                if (cache.ExpiredTime.Value < DateTime.Now)
                {
                    var data = callback(key, name, oldValue);
                    if (data != null)
                    {
                        cache = new Cache()
                        {
                            Id = key,
                            CacheKey = name,
                            BuildDate = DateTime.Now,
                            ExpiredTime = DateTime.Now.AddSeconds(timeout),
                            CacheData = JSON.Serialize(data)
                        };
                        DataFactory.Instance().Put(cache);
                        return data;
                    }
                    else
                    {
                        DataFactory.Instance().Put(new Cache()
                        {
                            Id = key,
                            CacheKey = name,
                            BuildDate = DateTime.Now,
                            ExpiredTime = DateTime.Now.AddSeconds(timeout)
                        });
                    }
                }
                return oldValue;
            }
            else
            {
                var data = callback(key, name, null);


                cache = new Cache()
                {
                    Id = key,
                    CacheKey = name,
                    BuildDate = DateTime.Now,
                    ExpiredTime = DateTime.Now.AddSeconds(timeout),
                    CacheData = JSON.Serialize(data, "ts")
                };
                DataFactory.Instance().Put(cache);

                return data;
            }

        }


        /// <summary>
        /// 清除数据缓存
        /// </summary>
        /// <param name="cacheId"></param>
        /// <param name="CacheKey"></param>
        public static void ClearCache(Guid cacheId, string CacheKey)
        {
            DataFactory.Instance().Delete(new Cache { CacheKey = CacheKey, Id = cacheId });

        }
    }

}
