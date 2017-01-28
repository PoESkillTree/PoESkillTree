using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using log4net;
using static POESKillTree.Utils.WikiApi.ItemRdfPredicates;
using static POESKillTree.Utils.WikiApi.WikiApiUtils;

namespace POESKillTree.Utils.WikiApi
{
    public class PoolingImageLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PoolingImageLoader));

        private const int MaxBatchSize = 10;
        private static readonly TimeSpan MaxWaitTime = TimeSpan.FromMilliseconds(100);

        private readonly HttpClient _httpClient;
        private readonly ApiAccessor _apiAccessor;

        private readonly BufferBlock<PoolItem> _queue =
            new BufferBlock<PoolItem>(new DataflowBlockOptions {BoundedCapacity = MaxBatchSize});

        public PoolingImageLoader(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiAccessor = new ApiAccessor(httpClient);
            ConsumeAsync();
        }

        public async Task ProduceAsync(string itemName, string fileName)
        {
            var tcs = new TaskCompletionSource<string>();
            var item = new PoolItem(itemName, fileName, tcs);
            await _queue.SendAsync(item).ConfigureAwait(false);
            await tcs.Task.ConfigureAwait(false);
        }

        private async void ConsumeAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var items = new List<PoolItem>();
            while (await _queue.OutputAvailableAsync().ConfigureAwait(false))
            {
                var canceled = false;
                try
                {
                    var timeout = MaxWaitTime - stopwatch.Elapsed;
                    if (timeout.Milliseconds < 0)
                    {
                        canceled = true;
                    }
                    else
                    {
                        var task = _queue.ReceiveAsync(MaxWaitTime - stopwatch.Elapsed);
                        items.Add(await task.ConfigureAwait(false));
                    }
                }
                catch (TaskCanceledException)
                {
                    canceled = true;
                }
                if (canceled || items.Count >= MaxBatchSize)
                {
                    await LoadPoolAsync(items).ConfigureAwait(false);
                    stopwatch.Restart();
                    items.Clear();
                }
            }
            await LoadPoolAsync(items).ConfigureAwait(false);
        }

        private async Task LoadPoolAsync(IReadOnlyList<PoolItem> pool)
        {
            if (!pool.Any())
            {
                return;
            }
            try
            {
                var nameToItem = pool.ToDictionary(p => p.ItemName);
                var conditions = new ConditionBuilder
                {
                    {RdfName, string.Join("||", pool.Select(p => p.ItemName))}
                };
                var printouts = new[] {RdfName, RdfIcon};

                var results = (from ps in await _apiAccessor.Ask(conditions, printouts).ConfigureAwait(false)
                               let title = ps[RdfIcon].First.Value<string>("fulltext")
                               let name = SingularValue<string>(ps, RdfName)
                               select new { name, title }).ToList();
                var titleToItem = results.ToDictionary(x => x.title, x => nameToItem[x.name]);

                var task = _apiAccessor.QueryImageInfoUrls(results.Select(t => t.title));
                var imageInfo =
                    from tuple in await task.ConfigureAwait(false)
                    select new {Item = titleToItem[tuple.Item1], Url = tuple.Item2};
                var saveTasks = new List<Task>();
                var missing = new HashSet<PoolItem>(pool);
                foreach (var x in imageInfo)
                {
                    // Don't load duplicates (e.g. for Two-Stone Ring or items with weapon skins)
                    if (missing.Remove(x.Item))
                    {
                        saveTasks.Add(LoadSingleAsync(x.Item, x.Url));
                    }
                }
                await Task.WhenAll(saveTasks);
                foreach (var item in missing)
                {
                    item.Tcs.SetException(new Exception($"Item image for {item.ItemName} was not downloaded"));
                }
            }
            catch (Exception e)
            {
                var first = pool[0];
                first.Tcs.SetException(e);
                foreach (var item in pool.Skip(1))
                {
                    item.Tcs.SetCanceled();
                }
            }
        }

        private async Task LoadSingleAsync(PoolItem item, string url)
        {
            try
            {
                var imgData = await _httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
                using (var outputStream = File.Create(item.FileName))
                {
                    SaveImage(imgData, outputStream, true);
                }
                Log.Info($"Downloaded item image for {item.ItemName} to the file system.");
                item.Tcs.SetResult(item.ItemName);
            }
            catch (Exception e)
            {
                item.Tcs.SetException(e);
            }
        }


        private class PoolItem
        {
            public string ItemName { get; }
            public string FileName { get; }
            public TaskCompletionSource<string> Tcs { get; }

            public PoolItem(string itemName, string fileName, TaskCompletionSource<string> tcs)
            {
                ItemName = itemName;
                FileName = fileName;
                Tcs = tcs;
            }
        }
    }
}