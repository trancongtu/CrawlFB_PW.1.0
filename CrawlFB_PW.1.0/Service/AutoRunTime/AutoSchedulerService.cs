using CrawlFB_PW._1._0.DAO.Auto;
using CrawlFB_PW._1._0.DTO;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Service.AutoRunTime
{
    public class AutoSchedulerService
    {
        private readonly Dictionary<string, List<PageRuntime>> _pagesByProfile
            = new Dictionary<string, List<PageRuntime>>();

        private CancellationTokenSource _cts;

        // 🔥 EVENTS (giữ nguyên hệ cũ)
        public event Action<string, AutoService.PageStatus, int, int> OnProgress;
        public event Action<string, List<PostPage>, List<ShareItem>> OnNewPosts;

        // ============================
        // ADD PAGE
        // ============================
        public void AddPage(PageRuntime runtime)
        {
            if (!_pagesByProfile.ContainsKey(runtime.ProfileId))
                _pagesByProfile[runtime.ProfileId] = new List<PageRuntime>();

            _pagesByProfile[runtime.ProfileId].Add(runtime);
        }

        // ============================
        // START
        // ============================
        public void Start()
        {
            _cts = new CancellationTokenSource();

            foreach (var kv in _pagesByProfile)
            {
                var profileId = kv.Key;
                var pages = kv.Value;

                // 🔥 mỗi profile = 1 thread riêng
                Task.Run(() => RunProfileLoop(profileId, pages, _cts.Token));
            }
        }

        // ============================
        // STOP
        // ============================
        public void Stop()
        {
            _cts?.Cancel();
        }

        // ============================
        // LOOP THEO PROFILE
        // ============================
        private async Task RunProfileLoop(string profileId, List<PageRuntime> pages, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var p in pages)
                {
                    if (token.IsCancellationRequested)
                        break;

                    // 🔥 check delay
                    if (DateTime.Now < p.NextRunTime)
                        continue;

                    await RunSession(p);
                }

                await Task.Delay(1000, token);
            }
        }

        // ============================
        // CHẠY 1 PAGE
        // ============================
        private async Task RunSession(PageRuntime p)
        {
            if (p.IsRunning) return;

            p.IsRunning = true;

            // 🔥 SET PROFILE CONTEXT (QUAN TRỌNG)
            Libary.Instance.SetProfileContext(p.ProfileId, p.ProfileName);

            OnProgress?.Invoke(p.PageId, AutoService.PageStatus.Running, 0, 0);

            try
            {
                var result = await AutoPageDAO.Instance.RunAutoAsync(
                    p.Page,
                    p.PageInfo,
                    p.IsSecondRun,
                    p.Cache,
                    p.LastLoopNewestTime
                );

                int newPosts = result.NewPosts;
                int saved = result.Posts?.Count ?? 0;

                OnProgress?.Invoke(
                    p.PageId,
                    AutoService.PageStatus.StopScroll,
                    newPosts,
                    saved
                );

                if (result.Posts != null && result.Posts.Count > 0)
                {
                    OnNewPosts?.Invoke(
                        p.PageId,
                        result.Posts,
                        result.Shares ?? new List<ShareItem>()
                    );
                }

                Libary.Instance.LogService(
                    $"📥 [{p.ProfileName}] {p.PageName}: +{newPosts} post"
                );

                // 🔥 update state
                p.IsSecondRun = true;

                if (result.NewestTime.HasValue)
                    p.LastLoopNewestTime = result.NewestTime;

                if (newPosts == 0)
                    p.NoPostCount++;
                else
                    p.NoPostCount = 0;

                // 🔥 delay (có thể tinh chỉnh)
                int delay = p.NoPostCount >= 3 ? 60 : 30;
                p.NextRunTime = DateTime.Now.AddSeconds(delay);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService(
                    $"❌ [{p.ProfileName}] {p.PageName}: {ex.Message}"
                );

                p.NextRunTime = DateTime.Now.AddSeconds(60);
            }
            finally
            {
                p.IsRunning = false;

                OnProgress?.Invoke(
                    p.PageId,
                    AutoService.PageStatus.Resting,
                    0,
                    0
                );
            }
        }
        public List<PageRuntime> GetAllRuntimes()
        {
            return _pagesByProfile.Values
                .SelectMany(x => x)
                .ToList();
        }
    }
}