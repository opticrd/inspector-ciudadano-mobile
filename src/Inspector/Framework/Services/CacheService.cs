using Akavache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Inspector.Framework.Services
{

    public class CacheService : ICacheService
    {
        public CacheService()
        {
            //BlobCache.ApplicationName = "Inspector";
            Registrations.Start("Inspector");
        }

        #region Local
        public async Task<T> GetLocalObject<T>(string key)
        {
            try
            {
                return await BlobCache.LocalMachine.GetObject<T>(key);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public async Task InsertLocalObject<T>(string key, T value)
        {
            await BlobCache.LocalMachine.InsertObject(key, value);
        }

        public async Task RemoveLocalObject(string key)
        {
            var keys = await BlobCache.LocalMachine.GetAllKeys();

            if (keys.Contains(key))
                await BlobCache.LocalMachine.Invalidate(key);
        }
        #endregion

        #region Secure
        public async Task<T> GetSecureObject<T>(string key)
        {
            try
            {
                return await BlobCache.Secure.GetObject<T>(key);
            }
            catch (KeyNotFoundException)
            {
                return default(T);
            }
        }

        public async Task InsertSecureObject<T>(string key, T value)
        {
            await BlobCache.Secure.InsertObject(key, value);
        }

        public async Task RemoveSecureObject(string key)
        {
            await BlobCache.Secure.Invalidate(key);
        }
        #endregion
    }

    public interface ICacheService : ILocalMachineCache//, ISecureCache
    {

    }

    public interface ILocalMachineCache
    {
        Task<T> GetLocalObject<T>(string key);

        Task InsertLocalObject<T>(string key, T value);

        Task RemoveLocalObject(string key);
    }

    public interface ISecureCache
    {
        Task<T> GetSecureObject<T>(string key);

        Task InsertSecureObject<T>(string key, T value);

        Task RemoveSecureObject(string key);
    }

}
