using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;

namespace CrawlFB_PW._1._0.Service
{
    public class PostCacheItem
    {
        public List<PostPage> Data { get; set; }
        public DateTime ExpireAt { get; set; }
    }

    public static class PostCacheService
    {
        private static readonly Dictionary<string, PostCacheItem> _cache
            = new Dictionary<string, PostCacheItem>();

        private static readonly TimeSpan TTL = TimeSpan.FromMinutes(5);

        public static List<PostPage> Get(string pageId)
        {
            if (_cache.ContainsKey(pageId))
            {
                var item = _cache[pageId];

                if (DateTime.Now < item.ExpireAt)
                    return item.Data;

                // hết hạn
                _cache.Remove(pageId);
            }

            // load lại
            var data = SQLDAO.Instance.GetPostsByPage(pageId);

            _cache[pageId] = new PostCacheItem
            {
                Data = data,
                ExpireAt = DateTime.Now.Add(TTL)
            };

            return data;
        }

        public static void Clear(string pageId)
        {
            if (_cache.ContainsKey(pageId))
                _cache.Remove(pageId);
        }
    }
}
