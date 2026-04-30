using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Helper
{
    public class RecentPostCache
    {
        private readonly int _max;
        private readonly Queue<string> _queue = new Queue<string>();
        private readonly HashSet<string> _set = new HashSet<string>();

        public RecentPostCache(int max = 20)
        {
            _max = max;
        }

        public bool Contains(string link)
        {
            return _set.Contains(link);
        }

        public void Add(string link)
        {
            if (_set.Contains(link))
                return;

            _queue.Enqueue(link);
            _set.Add(link);

            if (_queue.Count > _max)
            {
                var old = _queue.Dequeue();
                _set.Remove(old);
            }
        }
        public int Count
        {
            get { return _queue.Count; }
        }
    }
}
