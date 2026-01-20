using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;

namespace CrawlFB_PW._1._0.DAO.Post
{
    internal sealed class PostWatchCommentDAO : BasePostCommentDAO
    {
        private static readonly Lazy<PostWatchCommentDAO> _instance =
    new Lazy<PostWatchCommentDAO>(() => new PostWatchCommentDAO());

        public static PostWatchCommentDAO Instance => _instance.Value;
        private PostWatchCommentDAO() { }
        public async Task<IElementHandle> LoadWatchPageAsync(IPage page, string url)
        {
            if (page == null || page.IsClosed)
                return null;

            try
            {
                Libary.Instance.LogDebug("1-[WATCH] 🌐 Goto Watch: " + url);

                await page.GotoAsync(url, new PageGotoOptions
                {
                    Timeout = AppConfig.DEFAULT_TIMEOUT,
                    WaitUntil = WaitUntilState.DOMContentLoaded
                });

                // ⏳ Chờ UI ổn định nhẹ
                await page.WaitForTimeoutAsync(500);

                // 🔎 Check WATCH feed container
                // Đây là container chính của Watch/Reel feed
                var feed = await page.QuerySelectorAsync(
                    "div.x78zum5.xdt5ytf.x1huibft.x1n6yrxt"
                );

                if (feed == null)
                {
                    Libary.Instance.LogDebug(
                        "[WATCH] ⚠️ Chưa tìm thấy Watch feed container"
                    );
                    return null;
                }

                Libary.Instance.LogDebug("[WATCH] ✅ Watch feed sẵn sàng");
                Libary.Instance.LogTech($"{Libary.IconInfo} Load Watch DOM thành công");

                return feed;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[WATCH] ❌ LoadWatchPageAsync lỗi: " + ex.Message);
                return null;
            }
        }
        public async Task<List<IElementHandle>> WaitForWatchCommentNodesAsync(IElementHandle feedNode, int timeoutMs = 8000)
        {
            try
            {
                await feedNode.WaitForSelectorAsync(
                    "div.x18xomjl.xbcz3fp",
                    new ElementHandleWaitForSelectorOptions { Timeout = timeoutMs }
                );

                var blocks = await feedNode.QuerySelectorAllAsync(
                    "div.x18xomjl.xbcz3fp"
                );

                Libary.Instance.LogDebug($"[WATCH][DIRECT] Block x18xomjl = {blocks.Count}");

                var commentNodes = new List<IElementHandle>();

                foreach (var block in blocks)
                {
                    var articles = await block.QuerySelectorAllAsync("div[role='article']");
                    if (articles.Count > 0)
                        commentNodes.AddRange(articles);
                }

                Libary.Instance.LogTech($"[WATCH][DIRECT] 🎯 Comment nodes = {commentNodes.Count}");
                return commentNodes;
            }
            catch (TimeoutException)
            {
                Libary.Instance.LogTech("[WATCH][DIRECT] ⚠️ Timeout khi chờ comment trong feed");
                return new List<IElementHandle>();
            }
        }
       
        //=========================
        public async Task<List<CommentItem>> ScanWatchsCommentsAsync(IPage page, string Url, Func<bool> shouldStop)
        {
            var result = new List<CommentItem>();

            if (page == null || page.IsClosed)
                return result;

            try
            {
                var feedNode = await LoadWatchPageAsync(page, Url);
                if (feedNode == null)
                {
                    Libary.Instance.LogTech($"{Libary.IconFail}❌ Không load được Watch feed");
                    return result;
                }
                //await page.WaitForTimeoutAsync(100);
                await SwitchToAllCommentsAsync(page);
                await page.WaitForTimeoutAsync(400);
                var collectedIds = new HashSet<string>();
                var parentNameToId = new Dictionary<string, string>();
                int noNewRound = 0;
                int maxNoNewRound = 3;
                while (noNewRound < maxNoNewRound)
                {
                    if (shouldStop())
                    {
                        Libary.Instance.LogTech("[REEL][STOP] ⛔ Stop requested – kết thúc scan");
                        break;
                    }
                    if (shouldStop()) break;
                    bool clickedMore = await ClickLoadMoreCommentsIfExistsAsync(page);

                    if (clickedMore) await page.WaitForTimeoutAsync(600);
                    if (shouldStop()) break;
                    await ClickViewRepliesFromPageAsync(page);
                    await page.WaitForTimeoutAsync(400);
                    var nodes = await WaitForWatchCommentNodesAsync(feedNode);
                    int addedThisRound = 0;

                    foreach (var node in nodes)
                    {
                        if (shouldStop())
                        {
                            Libary.Instance.LogTech("[REEL][STOP] ⛔ Stop trong khi parse comment");
                            break;
                        }
                        string aria = await node.GetAttributeAsync("aria-label");
                        var meta = ParseAriaLabel(aria);
                        if (meta == null)
                            continue;

                        var (posterName, rawPosterLink) =
                            await ExtractCommentPosterAsync(node);

                        string commentId =
                            ExtractCommentIdFromLink(rawPosterLink);

                        if (string.IsNullOrWhiteSpace(commentId))
                            continue;

                        if (!collectedIds.Add(commentId))
                            continue;

                        addedThisRound++;

                        string content = await ExtractCommentContentAsync(node);

                        DateTime? realTime = null;
                        if (!string.IsNullOrWhiteSpace(meta.TimeRaw))
                            realTime = TimeHelper.ParseFacebookTime(meta.TimeRaw);

                        string parentId = null;

                        if (!meta.IsReply)
                        {
                            parentNameToId[meta.PosterName] = commentId;
                        }
                        else
                        {
                            parentNameToId.TryGetValue(meta.ParentPosterName, out parentId);
                        }

                        result.Add(new CommentItem
                        {
                            CommentId = commentId,
                            ParentCommentId = parentId,
                            PosterName = meta.PosterName,
                            PosterLink = ShortenPosterLinkFromComment(rawPosterLink),
                            Content = content,
                            TimeRaw = meta.TimeRaw,
                            RealCommentTime = realTime,
                            Status = meta.IsReply
                                ? "Bình luận phản hồi"
                                : "Bình luận gốc"
                        });
                    }

                    if (addedThisRound == 0 && !clickedMore)
                        noNewRound++;
                    else
                        noNewRound = 0;
                    if (shouldStop()) break;
                    await HumanScrollAsync(page, feedNode);
                    await page.WaitForTimeoutAsync(400);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    "[REEL][DAO] ❌ ScanReelCommentsAsync lỗi: " + ex.Message
                );
            }

            return result;
        }
    }
}
