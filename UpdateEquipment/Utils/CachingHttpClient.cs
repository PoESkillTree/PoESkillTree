using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateEquipment.Utils
{
    public class CachingHttpClient : IDisposable
    {
        private const int MaxParallelRequests = 4;

        private readonly SemaphoreSlim _taskSema = new SemaphoreSlim(MaxParallelRequests);

        private readonly ConcurrentDictionary<string, Task<string>> _stringCache =
            new ConcurrentDictionary<string, Task<string>>();

        private readonly ConcurrentDictionary<string, Task<byte[]>> _byteArrayCache =
            new ConcurrentDictionary<string, Task<byte[]>>();

        private readonly HttpClient _client = new HttpClient();

        public async Task<string> GetStringAsync(string requestUri)
        {
            return await _stringCache.GetOrAdd(requestUri, async s =>
            {
                var content = await GetAsync(s);
                return await content.ReadAsStringAsync();
            }).ConfigureAwait(false);
        }

        public async Task<byte[]> GetByteArrayAsync(string requestUri)
        {
            return await _byteArrayCache.GetOrAdd(requestUri, async s =>
            {
                var content = await GetAsync(s).ConfigureAwait(false);
                return await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private async Task<HttpContent> GetAsync(string requestUri)
        {
            await _taskSema.WaitAsync().ConfigureAwait(false);
            try
            {
                var response = await _client.GetAsync(requestUri).ConfigureAwait(false);
                return response.Content;
            }
            finally
            {
                _taskSema.Release();
            }
        }

        public void Dispose()
        {
            _client.Dispose();
            _taskSema.Dispose();
        }
    }
}