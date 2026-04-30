using CrawlFB_PW._1._0.DAO.Auto;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Service;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using DocumentFormat.OpenXml.Drawing;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
public class AutoService
{
    private readonly List<PageInfo> _pages;
    private readonly List<ProfileDB> _profiles;
    private string _delayRange;
    private TimeSpan _startTime;
    private readonly bool _isAutoSave; // bật tắt chế độ auto save
    private bool _isRunning = false;
    private readonly Dictionary<string, RecentPostCache> _pageCache = new Dictionary<string, RecentPostCache>();

    private readonly SemaphoreSlim _semaphore;
    public enum PageStatus
    {
        Idle,
        Running,
        StopScroll,
        Error,
        Resting
    }
    public event Action<string, PageStatus, int, int> OnProgress;
    public event Action<string, List<PostPage>, List<ShareItem>> OnNewPosts;
    public AutoService(
     List<PageInfo> pages,
     List<ProfileDB> profiles,
     string delayRange,
     bool isAutoSave,
     int maxParallel = 3)
    {
        _pages = pages;
        _profiles = profiles;
        _delayRange = delayRange;
        _isAutoSave = isAutoSave;
        _semaphore = new SemaphoreSlim(maxParallel);
    }
    public void SetConfig(TimeSpan startTime, string delay)
    {
        _startTime = startTime;
        _delayRange = delay;
    }
    public void Stop()
    {
        _isRunning = false;
    }
    private async Task WaitUntilStartTime()
    {
        var now = DateTime.Now.TimeOfDay;

        if (now < _startTime)
        {
            var wait = _startTime - now;

            Libary.Instance.LogForm("AutoService",
                $"⏳ Chờ đến giờ chạy: {_startTime}");

            await Task.Delay(wait);
        }
    }
    public async Task RunAsync()
    {
        _isRunning = true;
        await WaitUntilStartTime();

        Libary.Instance.LogForm("AutoService", $"🚀 START CONTINUOUS MODE");

        var distributor = new PageDistributionService();

        var profileQueues = distributor.Distribute(
            _profiles,
            _pages,
            x => x.PageName,
            "Auto"
        );

        var tasks = new List<Task>();

        foreach (var kv in profileQueues)
        {
            var profile = kv.Key;
            var pages = kv.Value;

            tasks.Add(ProfileWorker(profile, pages));
        }

        await Task.WhenAll(tasks);
    }
    private async Task RunPageLoop(ProfileDB profile, PageInfo pageInfo)
    {
        try
        {
            int noNewPostCount = 0;
            DateTime? lastSeenTime = null;
            DateTime? lastLoopNewestTime = null;

            var page = await AdsPowerPlaywrightManager.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);

            if (page == null) return;

            RecentPostCache cache;

            if (!_pageCache.TryGetValue(pageInfo.PageID, out cache))
            {
                cache = new RecentPostCache();
                _pageCache[pageInfo.PageID] = cache;
            }

            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);
            void PushResult(AutoResult r, int saved)
            {
                if (r == null) return;

                var posts = r.Posts ?? new List<PostPage>();
                var shares = r.Shares ?? new List<ShareItem>();

                OnNewPosts?.Invoke(pageInfo.PageID, posts, shares);
                Libary.Instance.LogForm("AutoService",
        $"📤 PUSH | Page={pageInfo.PageName} | Posts={posts.Count} | Shares={shares.Count} | New={r.NewPosts} | Saved={saved}");
                if (r.NewPosts > 0 || saved > 0)
                {
                    OnProgress?.Invoke(
                        pageInfo.PageID,
                        PageStatus.StopScroll,
                        r.NewPosts,
                        saved
                    );
                }
            }
            // 🔥 START TAB
            OnProgress?.Invoke(pageInfo.PageID, PageStatus.Running, 0, 0);

            // ===== FIRST RUN =====
            var firstResult = await AutoPageDAO.Instance.RunAutoAsync(page, pageInfo, false, cache, lastLoopNewestTime);
            int savedFirst = 0;

            if (_isAutoSave && firstResult?.Posts?.Count > 0)
            {
                savedFirst = SQLDAO.Instance.InsertPostBatchAuto_V3(
                    firstResult.Posts,
                    firstResult.Shares,
                    pageInfo.PageID,
                    pageInfo.PageLink
                );
            }

            PushResult(firstResult, savedFirst); // 🔥 FIX
            if (firstResult?.NewestTime.HasValue == true)
            {
                lastSeenTime = firstResult.NewestTime;
                lastLoopNewestTime = firstResult.NewestTime;
            }

            PushResult(firstResult,0);
            int loopIndex = 1;

            while (_isRunning)
            {
                try
                {
                    var result = await RunPageTask(profile, pageInfo, page, cache, lastLoopNewestTime);// chạy loop nhưng vẫn qua hàm runauto
                    PushResult(result, 0);
                    if (result?.NewestTime.HasValue == true)
                    {
                        lastLoopNewestTime = result.NewestTime;
                    }

                    var newestTime = result?.NewestTime;

                    bool hasNew = newestTime.HasValue &&
                                  (!lastSeenTime.HasValue || newestTime > lastSeenTime);

                    if (hasNew)
                    {
                        noNewPostCount = 0;
                        lastSeenTime = newestTime;
                    }
                    else
                    {
                        noNewPostCount++;
                    }

                    int baseDelay = ParseDelayToMinutes(_delayRange);
                    int delayMinutes = hasNew
                        ? baseDelay
                        : Math.Min(baseDelay + noNewPostCount * 5, 60);

                    var delay = TimeSpan.FromMinutes(delayMinutes);

                    loopIndex++;

                    // 🔥 DELAY CÓ CANCEL
                    int totalMs = (int)delay.TotalMilliseconds;
                    int step = 1000;

                    for (int i = 0; i < totalMs; i += step)
                    {
                        if (!_isRunning)
                            break;

                        await Task.Delay(step);
                    }
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogForm("AutoService",
                        $"❌ LOOP ERROR Page={pageInfo.PageID} | {ex.Message}");

                    await Task.Delay(5000);
                }
            }
        }
        finally
        {
            // 🔥 LUÔN CHẠY (QUAN TRỌNG NHẤT)
            OnProgress?.Invoke(
                pageInfo.PageID,
                PageStatus.Idle,
                0,
                0
            );
        }
    }
    private int ParseDelayToMinutes(string delay)
    {
        if (string.IsNullOrEmpty(delay))
            return 5;

        var parts = delay.Split('-');

        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int min) &&
            int.TryParse(parts[1], out int max))
        {
            var rnd = new Random();
            return rnd.Next(min, max + 1);
        }

        return 5;
    }
    private async Task ProfileWorker(ProfileDB profile, List<PageInfo> pages)
    {
        var managerDao = new ManagerProfileDAO();

        Libary.Instance.LogForm("AutoService",
            $"👤 START PROFILE {profile.ProfileName}");

        // 🔥 mở tab chính
        var mainPage = await AdsPowerPlaywrightManager.Instance .GetPageEnsureSingleTabAsync(profile.IDAdbrowser);

        if (mainPage == null)
        {
            Libary.Instance.LogForm("AutoService", $"❌ Không có tab profile {profile.ProfileName}");
            return;
        }

        var runningTasks = new List<Task>();

        foreach (var pageInfo in pages)
        {
            // 🔥 CHẶN MAX TAB
            while (runningTasks.Count(t => !t.IsCompleted) >= 3)
            {
                Libary.Instance.LogForm("AutoService", $"⛔ Profile={profile.ProfileName} FULL SLOT");

                await Task.Delay(500);
            }

            // 🔥 mapping DB (tuỳ bạn giữ)
            managerDao.InsertMapping(new ManagerProfileDTO
            {
                IDProfile = profile.ID,
                PageIDCrawl = pageInfo.PageID,
                LinkFBCrawl = pageInfo.PageLink
            });

            var task = RunPageLoop(profile, pageInfo);

            runningTasks.Add(task);

            await Task.Delay(200);
        }

        // 🔥 KHÔNG cần WhenAll nếu muốn chạy liên tục
        await Task.WhenAll(runningTasks);
    }

    private async Task<AutoResult> RunPageTask(
     ProfileDB profile,
     PageInfo pageInfo,
     IPage page,
     RecentPostCache cache,
     DateTime? lastLoopNewestTime)
    {
        await _semaphore.WaitAsync();

        var profileDao = new ProfileInfoDAO();    
        // 🔥 TĂNG TAB
        profileDao.ChangeRuntimeUseTab(profile.ID, +1);

        try
        {
            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);

            Libary.Instance.LogForm("AutoService",$"▶ START Page={pageInfo.PageID}");

            Libary.Instance.LogForm("AutoService",$"📦 CACHE LOOP trước khi runauto | Page={pageInfo.PageName} | Count={cache.Count}");
            var result = await AutoPageDAO.Instance.RunAutoAsync(
                page,
                pageInfo,
                true,
                cache,
                lastLoopNewestTime
            );
            Libary.Instance.LogForm("AutoService", $"📦 CACHE LOOP sau runauto | Page={pageInfo.PageName} | Count={cache.Count}");
            Libary.Instance.LogForm("AutoService", $"📥 DONE Page={pageInfo.PageID} | New={result?.NewPosts}");

            // =========================
            // 🔥 SAVE / UI
            // =========================
            int saved = 0;

            if (_isAutoSave)
            {
                if (result?.Posts?.Count > 0)
                {
                    saved = SQLDAO.Instance.InsertPostBatchAuto_V3(result.Posts,result.Shares, pageInfo.PageID, pageInfo.PageLink);

                }
            }
            else
            {
                if (result?.Posts?.Count > 0)
                    OnNewPosts?.Invoke(pageInfo.PageID, result.Posts, result.Shares);
            }

            // =========================
            // 🔥 STATUS
            // =========================
            var hasData = (result?.NewPosts ?? 0) > 0 || saved > 0;

            OnProgress?.Invoke(
                pageInfo.PageID,
                hasData ? PageStatus.StopScroll : PageStatus.Resting,
                hasData ? result.NewPosts : 0,
                hasData ? saved : 0
            );

            return result;
        }
        catch (Exception ex)
        {
            Libary.Instance.LogForm("AutoService", $"❌ ERROR Page={pageInfo.PageID} | {ex.Message}");

            return new AutoResult();
        }
        finally
        {
            // 🔥 GIẢM TAB (CỰC QUAN TRỌNG)
            profileDao.ChangeRuntimeUseTab(profile.ID, -1);

            _semaphore.Release();
            Libary.Instance.ClearProfileContext();
        }
    }
    // hàm dung multi tab
    
}