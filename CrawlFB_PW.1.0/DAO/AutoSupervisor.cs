using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;

namespace CrawlFB_PW._1._0.DAO
{
    public class AutoSupervisor
    {
        public event Action<string> OnStatusChanged;
        public event Action<int> OnTotalPostUpdated;
        public event Action<int> OnNewPostFetched;
        public event Action<int> OnCountdownUpdated;
        public event Action<int> OnPagesCountUpdated;

        private readonly List<string> _pageUrls;
        private readonly ProfileInfo _profile;
        private readonly int _days;
        private readonly int _maxPosts;
        private bool _isRunning = false;
        private int _totalPosts = 0;

        public AutoSupervisor(List<string> pageUrls, ProfileInfo profile, int days = 3, int maxPosts = 20)
        {
            _pageUrls = pageUrls;
            _profile = profile;
            _days = days;
            _maxPosts = maxPosts;
        }

        public async Task StartAsync(int intervalMinutes, int randomExtraMinutesMin = 0, int randomExtraMinutesMax = 0)
        {
            _isRunning = true;
            OnPagesCountUpdated?.Invoke(_pageUrls.Count);

            while (_isRunning)
            {
                // Reserve slot
                bool reserved = ProfileSlotManager.Instance.TryReserveSlot(_profile.ProfileId);
                if (!reserved)
                {
                    OnStatusChanged?.Invoke("❌ Không còn slot trống cho profile.");
                    await Task.Delay(5000);
                    continue;
                }

                int totalNewThisRound = 0;
                OnStatusChanged?.Invoke("🔍 Đang quét các page...");

                foreach (var url in _pageUrls)
                {
                    if (!_isRunning) break;
                    OnStatusChanged?.Invoke($"Đang quét: {url}");
                    var progress = new Progress<string>(s => OnStatusChanged?.Invoke(s));

                    try
                    {
                        var posts = await SupervisorHelper.SuperviseOnePageAsync(url, _days, _maxPosts, _profile, progress);
                        int newCount = posts?.Count ?? 0;
                        totalNewThisRound += newCount;
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.CreateLog($"[AutoSupervisor] Lỗi quét {url}: {ex.Message}");
                    }
                }

                // Release slot
                ProfileSlotManager.Instance.ReleaseSlot(_profile.ProfileId);

                _totalPosts += totalNewThisRound;
                OnNewPostFetched?.Invoke(totalNewThisRound);
                OnTotalPostUpdated?.Invoke(_totalPosts);

                OnStatusChanged?.Invoke($"🟢 Hoàn tất lượt quét ({totalNewThisRound} bài mới). Chuẩn bị nghỉ...");

                // random extra minutes (so sánh yêu cầu random 120-150 etc)
                int extra = 0;
                if (randomExtraMinutesMax > 0 && randomExtraMinutesMax >= randomExtraMinutesMin)
                {
                    var rnd = new Random();
                    extra = rnd.Next(randomExtraMinutesMin, randomExtraMinutesMax + 1);
                }
                int totalInterval = intervalMinutes + extra;
                int countdown = totalInterval * 60;
                while (countdown > 0 && _isRunning)
                {
                    OnCountdownUpdated?.Invoke(countdown);
                    await Task.Delay(1000);
                    countdown--;
                }
            } // while

            OnStatusChanged?.Invoke("🛑 Đã dừng auto.");
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;
    }
}
