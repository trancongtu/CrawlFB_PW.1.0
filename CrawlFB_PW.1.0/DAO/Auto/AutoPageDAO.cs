using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.DAO.Page;
using CrawlFB_PW._1._0.Enums;
namespace CrawlFB_PW._1._0.DAO.Auto
{
    public class AutoPageDAO
    {
        public static AutoPageDAO Instance = new AutoPageDAO();

        public async Task<AutoResult> RunAutoAsync(
     IPage page,
     PageInfo pageInfo,
     bool isSecondRun,
     RecentPostCache cache,
     DateTime? lastLoopNewestTime)
        {
            var result = new AutoResult();
            int newPostCount = 0;
            try
            {
                string baseUrl = pageInfo.PageLink;

                // =========================
                // 🔥 LOAD PAGE
                // =========================
                if (!isSecondRun)
                {
                    await EnsureChronologicalAsync(page, baseUrl, true);
                    Libary.Instance.LogTech("---chạy lần 1: " );
                }
                else
                {
                    // 🔥 THAY SCROLL = RELOAD
                    Libary.Instance.LogTech("🔄 RELOAD PAGE");

                    await page.ReloadAsync();
                    await page.WaitForTimeoutAsync(3000);

                    // 🔥 set lại chronological (rất quan trọng)
                    await EnsureChronologicalAsync(page, baseUrl, false);

                    Libary.Instance.LogTech("---chạy lần tiếp theo (reload): ");
                }

                var feed = await PageDAO.Instance.GetFeedContainerAsync(page);
                if (feed == null) return result;

                DateTime? newestCrawledTime = null;
                DateTime? dbLastTime = SQLDAO.Instance.GetNewestPostTime(pageInfo.PageID);
                if (!dbLastTime.HasValue)
                {
                    dbLastTime = DateTime.Now.AddHours(-12); //lùi 12h
                }
                int processedIndex = 0;
                int scrollRound = 0;
                const int maxScroll = 30;

                bool stop = false;
                bool isGroup = PageDAO.Instance.IsFacebookGroup(baseUrl);

                // =========================
                // 🔄 MAIN LOOP
                // =========================
                while (!stop && scrollRound < maxScroll)
                {
                    var nodes = await feed.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");
                    if (nodes == null || nodes.Count == 0)
                    {
                        Libary.Instance.LogTech("⚠️ NO NODES");
                        continue;
                    }
                    for (int i = processedIndex; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (node == null)
                        {
                            Libary.Instance.LogTech("⚠️ NODE NULL");
                            continue;
                        }
                        var pr = await CrawlPageDAO.Instance.CrawlPagePostAsync(
                            page,
                            node,
                            pageInfo.PageName,
                            baseUrl,
                            isGroup ? CrawlContext.Group : CrawlContext.Fanpage,
                            pageInfo.PageID,
                            pageInfo.IDFBPage
                        );

                        if (pr == null || pr.Posts == null || pr.Posts.Count == 0)
                        {
                            processedIndex = i + 1;
                            continue;
                        }
                      
                        foreach (var post in pr.Posts)
                        {
                            if (string.IsNullOrEmpty(post.PostLink))
                                continue;
                            post.PostLink = UrlHelper.NormalizeFacebookUrl(post.PostLink);
                            post.PageLink = UrlHelper.NormalizeFacebookUrl(post.PageLink);
                            bool isMainPagePost = post.PageLink == pageInfo.PageLink;
                            Libary.Instance.LogTech($"📌 PostType={(isMainPagePost ? "MAIN" : "SHARE")} | {post.PostLink}");
                            result.TotalRead++;

                            // =========================
                            // 🔥 TIME (tách rõ)
                            // =========================
                            DateTime? realTime = post.RealPostTime;
                            DateTime? parsedTime = TimeHelper.ParseFacebookTime(post.PostTime);
                            DateTime? compareTime = realTime ?? parsedTime;
                           Libary.Instance.LogTech(
                            $"⏰ TIME DEBUG | " +
                            $"Post={post.PostLink} | " +
                            $"Real={(realTime.HasValue ? realTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null")} | " +
                            $"Parsed={(parsedTime.HasValue ? parsedTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null")} | " +
                            $"Compare={(compareTime.HasValue ? compareTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null")} | " +
                            $"Source={(realTime.HasValue ? "REAL" : (parsedTime.HasValue ? "PARSED" : "NONE"))}");
                            if (isMainPagePost)
                            {
                                // CACHE STOP
                                if (isSecondRun && cache.Contains(post.PostLink))
                                {
                                    Libary.Instance.LogTech("⛔ STOP CACHE HIT: " + post.PostLink);
                                    result.NewPosts = newPostCount;
                                    result.StopReason = "CACHE_STOP";
                                    stop = true;
                                    break;
                                }

                                // DB STOP
                                if (!isSecondRun && dbLastTime.HasValue && compareTime.HasValue)
                                {
                                    if (newestCrawledTime.HasValue && compareTime <= dbLastTime)
                                    {
                                        Libary.Instance.LogTech(
                                            $"⛔ STOP DB | " +
                                            $"C={(compareTime?.ToString("HH:mm:ss") ?? "null")} <= " +
                                            $"DB={(dbLastTime?.ToString("HH:mm:ss") ?? "null")}"
                                        );
                                        result.NewPosts = newPostCount;
                                        result.StopReason = "OLD_DB";
                                        stop = true;
                                        break;
                                    }
                                }

                                // LOOP STOP
                                if (isSecondRun && lastLoopNewestTime.HasValue && compareTime.HasValue)
                                {
                                    if (compareTime <= lastLoopNewestTime)
                                    {
                                 Libary.Instance.LogTech(
                                    $"🔍 CHECK LOOP | " +
                                    $"C={(compareTime?.ToString("HH:mm:ss") ?? "null")} | " +
                                    $"L={(lastLoopNewestTime?.ToString("HH:mm:ss") ?? "null")}"
                                );
                                        result.NewPosts = newPostCount;
                                        result.StopReason = "OLD_LOOP";
                                        stop = true;
                                        break;
                                    }
                                }
                            }
                            // =========================
                            // ✅ ADD POST
                            // =========================
                            result.Posts.Add(post);
                            newPostCount++;
                            if (isMainPagePost)
                            {
                                cache.Add(post.PostLink);                               
                            }

                            // =========================
                            // 🔥 UPDATE NEWEST
                            // =========================
                            if (isMainPagePost && compareTime.HasValue)
                            {
                                if (!newestCrawledTime.HasValue || compareTime > newestCrawledTime)
                                {
                                    newestCrawledTime = compareTime;
                                }
                            }
                        }
                        // shares
                        if (pr.Shares != null && pr.Shares.Count > 0)
                        {
                            result.Shares.AddRange(pr.Shares);
                        }
                        processedIndex = i + 1;

                        if (stop) break;
                    }

                    if (stop) break;

                    // =========================
                    // 🔽 SCROLL
                    // =========================
                    await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 1);
                    await page.WaitForTimeoutAsync(800);

                    scrollRound++;
                }
                result.NewestTime = newestCrawledTime;
                DatabaseDAO.Instance.UpdatePageLastScan(pageInfo.PageID);
            }
            catch (Exception ex)
            {
                result.StopReason = "ERROR";
                Libary.Instance.CreateLog("[AUTO ERROR] " + ex.Message);
            }
            result.NewPosts = newPostCount;
            return result;
        }
        // reload lại url
        private async Task EnsureChronologicalAsync(IPage page, string url, bool isFirstRun)
        {
            if (isFirstRun)
            {
                if (!url.Contains("sorting_setting="))
                {
                    url += url.Contains("?")
                        ? "&sorting_setting=CHRONOLOGICAL"
                        : "?sorting_setting=CHRONOLOGICAL";
                }

                await page.GotoAsync(url);
                return;
            }

            // reload
            await page.ReloadAsync();
            await page.WaitForTimeoutAsync(1200);

            // check lại
            if (!page.Url.Contains("sorting_setting=CHRONOLOGICAL"))
            {
                await page.GotoAsync(page.Url.Contains("?")
                    ? page.Url + "&sorting_setting=CHRONOLOGICAL"
                    : page.Url + "?sorting_setting=CHRONOLOGICAL");
            }
        }
        public static bool IsOldPostForCrawl(
    DateTime? crawlTime,
    DateTime? dbTime,
    string postLink,
    Func<string, bool> isLinkExistInDb)
        {
            // Không có DB → luôn cho qua
            if (!dbTime.HasValue)
                return false;

            // Không có time → không chặn (tránh miss bài)
            if (!crawlTime.HasValue)
                return false;

            DateTime current = crawlTime.Value;
            DateTime newest = dbTime.Value;

            bool currentHasTime = current.TimeOfDay != TimeSpan.Zero;
            bool newestHasTime = newest.TimeOfDay != TimeSpan.Zero;

            // =========================
            // 🥇 CẢ 2 CÓ GIỜ → so full
            // =========================
            if (currentHasTime && newestHasTime)
            {
                return current <= newest;
            }

            // =========================
            // 🥈 KHÁC NGÀY → so ngày
            // =========================
            if (current.Date < newest.Date)
                return true;

            if (current.Date > newest.Date)
                return false;

            // =========================
            // 🥉 CÙNG NGÀY nhưng thiếu giờ
            // → fallback check link DB
            // =========================
            if (!currentHasTime || !newestHasTime)
            {
                if (isLinkExistInDb != null && isLinkExistInDb(postLink))
                {
                    return true; // đã có → coi là cũ → stop
                }

                return false; // chưa có → vẫn lấy
            }

            return false;
        }
    }
}
