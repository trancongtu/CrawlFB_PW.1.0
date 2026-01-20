
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.ViewModels;
using System.Linq;
using CrawlFB_PW._1._0.DAO.Page;
using CrawlFB_PW._1._0.Enums;
namespace CrawlFB_PW._1._0.DAO
{
    public class UpdatePostPageDAO
    {
        private static UpdatePostPageDAO _instance;
        public static UpdatePostPageDAO Instance
        {
            get
            {
                if (_instance == null) _instance = new UpdatePostPageDAO();
                return _instance;
            }
        }

        private UpdatePostPageDAO() { }
        
        //===========HÀM CHẠY CHÍNH================
        public async Task<PostResult> UpdatePostPageAsync( IPage page,string url, string pageId, DateTime? lastPostTime, int maxPosts = 200)
        {
            var result = new PostResult();
            string urlgoc = url;
            try
            {
                if (url.IndexOf("sorting_setting=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    if (url.Contains("?"))
                        url += "&sorting_setting=CHRONOLOGICAL";
                    else
                        url += "?sorting_setting=CHRONOLOGICAL";
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
                    Libary.Instance.CreateLog("UpdatePostPage", "❌ Không tìm thấy feed");
                    return result;
                }

                int processedIndex = 0;
                int scrollRound = 0;
                const int maxScrollRounds = 50;

                int duplicateCount = 0;
                const int maxDuplicate = 3;

                int oldCount = 0;
                const int maxOld = 3;

                bool stop = false;

                while (!stop &&
                       scrollRound < maxScrollRounds &&
                       duplicateCount < maxDuplicate &&
                       result.Posts.Count < maxPosts)
                {
                    var nodes = await feed.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");

                    for (int i = processedIndex; i < nodes.Count; i++)
                    {
                        var node = nodes[i];

                        bool isGroup = PageDAO.Instance.IsFacebookGroup(urlgoc);
                        string pageName = await PageDAO.Instance.GetPageNameAsync(page);

                        PostResult pr = await CrawlPageDAO.Instance.CrawlPagePostAsync(
                              page,
                              node,
                              pageName,
                              urlgoc,
                              isGroup ? CrawlContext.Group : CrawlContext.Fanpage
                          );


                        if (pr == null)
                        {
                            processedIndex = i + 1;
                            continue;
                        }

                        // ============================
                        // 🔄 GOM SHARE (KHÔNG PHỤ THUỘC POST)
                        // ============================
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
                            {
                                stop = true;
                                break;
                            }

                            DateTime? postTime = TimeHelper.ParseFacebookTime(post.PostTime);

                            // ============================
                            // ⏳ DỪNG THEO LAST POST TIME
                            // ============================
                            if (lastPostTime.HasValue &&
                                postTime.HasValue &&
                                postTime.Value <= lastPostTime.Value)
                            {
                                oldCount++;
                                Libary.Instance.LogTech(                              
                                    $"⏳ Bài cũ {postTime:dd/MM HH:mm} ≤ {lastPostTime:dd/MM HH:mm} ({oldCount}/3)"
                                );

                                if (oldCount >= maxOld)
                                {
                                    Libary.Instance.CreateLog(
                                        "UpdatePostPage",
                                        "⛔ Gặp 3 bài cũ liên tiếp → DỪNG UPDATE"
                                    );
                                    stop = true;
                                    break;
                                }

                                continue;
                            }
                            else
                            {
                                oldCount = 0;
                            }

                            // ============================
                            // 🔁 DỪNG KHI TRÙNG POST
                            // ============================
                            if (!string.IsNullOrEmpty(post.PostLink) &&
                                SQLDAO.Instance.ExistPostByLink(post.PostLink))
                            {
                                duplicateCount++;
                                Libary.Instance.CreateLog(
                                    "UpdatePostPage",
                                    $"⚠ Trùng #{duplicateCount} → {post.PostLink}"
                                );

                                if (duplicateCount >= maxDuplicate)
                                {
                                    Libary.Instance.CreateLog(
                                        "UpdatePostPage",
                                        "⛔ Gặp 3 bài trùng → DỪNG UPDATE"
                                    );
                                    stop = true;
                                    break;
                                }

                                continue;
                            }
                            else
                            {
                                duplicateCount = 0;
                            }

                            // ============================
                            // ✅ POST MỚI → ADD
                            // ============================
                            result.Posts.Add(post);
                        }

                        processedIndex = i + 1;
                        if (stop) break;
                    }

                    if (stop) break;

                    await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 1);
                    await page.WaitForTimeoutAsync(700);
                    scrollRound++;
                }

                Libary.Instance.CreateLog(
                    $"[UpdatePostPage] DONE: new={result.Posts.Count}, share={result.Shares.Count}, stop={stop}"
                );
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[UpdatePostPageAsync] ❌ ERROR: " + ex.Message);
            }

            return result;
        }


        // Lấy danh sách page để load vào grid

    }
}
