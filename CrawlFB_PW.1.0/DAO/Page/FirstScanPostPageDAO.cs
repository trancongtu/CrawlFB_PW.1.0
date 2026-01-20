using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DAO.Page;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.DAO
{
    public class FirstScanPostPageDAO
    {
        private static FirstScanPostPageDAO _instance;
        public static FirstScanPostPageDAO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FirstScanPostPageDAO();
                return _instance;
            }
        }

        private FirstScanPostPageDAO() { }

        /// <summary>
        /// FIRST SCAN – crawl toàn bộ post page/group
        /// ⚠️ Stop theo nghiệp vụ do FORM quyết định
        /// DAO chỉ crawl + chống trùng kỹ thuật
        /// </summary>
        public async Task<PostResult> FirstScanAsync(
            IPage page,
            string url,
            string pageId,
            int maxPosts = 500
        )
        {
            var result = new PostResult();
            string urlgoc = url;

            try
            {
                // =========================
                // 1️⃣ CHUẨN HÓA SORT
                // =========================
                if (url.IndexOf("sorting_setting=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    url += url.Contains("?")
                        ? "&sorting_setting=CHRONOLOGICAL"
                        : "?sorting_setting=CHRONOLOGICAL";
                }

                await page.GotoAsync(url, new PageGotoOptions
                {
                    Timeout = AppConfig.DEFAULT_TIMEOUT,
                    WaitUntil = WaitUntilState.DOMContentLoaded
                });

                await page.WaitForTimeoutAsync(1200);
                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 2);

                var feed = await PageDAO.Instance.GetFeedContainerAsync(page);
                if (feed == null)
                {
                    Libary.Instance.CreateLog("FirstScan", "❌ Không tìm thấy feed");
                    return result;
                }

                bool isGroup = PageDAO.Instance.IsFacebookGroup(urlgoc);
                string pageName = await PageDAO.Instance.GetPageNameAsync(page);

                var crawlContext = isGroup
                    ? CrawlContext.Group
                    : CrawlContext.Fanpage;

                Libary.Instance.LogForm(
                    "FirstScan",
                    $"Bắt đầu FIRST SCAN {(isGroup ? "GROUP" : "FANPAGE")} | {urlgoc}"
                );

                int processedIndex = 0;
                int scrollRound = 0;
                const int maxScrollRounds = 80;

                int duplicateCount = 0;
                const int maxDuplicate = 3;

                // =========================
                // 2️⃣ LOOP CRAWL FEED
                // =========================
                while (scrollRound < maxScrollRounds &&
                       result.Posts.Count < maxPosts)
                {
                    var nodes = await feed.QuerySelectorAllAsync(
                        "div[class='x1n2onr6 x1ja2u2z']"
                    );

                    for (int i = processedIndex; i < nodes.Count; i++)
                    {
                        var node = nodes[i];

                        PostResult pr = await CrawlPageDAO.Instance.CrawlPagePostAsync(
                            page,
                            node,
                            pageName,
                            urlgoc,
                            crawlContext
                        );

                        if (pr == null)
                        {
                            processedIndex = i + 1;
                            continue;
                        }

                        // =========================
                        // 3️⃣ GOM SHARE (ĐỘC LẬP POST)
                        // =========================
                        if (pr.Shares != null && pr.Shares.Count > 0)
                        {
                            result.Shares.AddRange(pr.Shares);
                        }

                        if (pr.Posts == null || pr.Posts.Count == 0)
                        {
                            processedIndex = i + 1;
                            continue;
                        }

                        foreach (var post in pr.Posts)
                        {
                            if (result.Posts.Count >= maxPosts)
                                break;

                            // =========================
                            // 🔁 CHỐNG TRÙNG KỸ THUẬT
                            // =========================
                            if (!string.IsNullOrEmpty(post.PostLink) &&
                                SQLDAO.Instance.ExistPostByLink(post.PostLink))
                            {
                                duplicateCount++;
                                if (duplicateCount >= maxDuplicate)
                                {
                                    Libary.Instance.CreateLog(
                                        "FirstScan",
                                        "⛔ Feed bắt đầu lặp → dừng crawl"
                                    );
                                    return result;
                                }
                                continue;
                            }
                            else
                            {
                                duplicateCount = 0;
                            }

                            result.Posts.Add(post);
                        }

                        processedIndex = i + 1;
                    }

                    await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 1);
                    await page.WaitForTimeoutAsync(700);
                    scrollRound++;
                }

                Libary.Instance.CreateLog(
                    "FirstScan",
                    $"DONE | post={result.Posts.Count}, share={result.Shares.Count}"
                );
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[FirstScanAsync] ❌ ERROR: " + ex.Message);
            }

            return result;
        }
    }
}
