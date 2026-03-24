using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DAO.Post
{
    internal sealed class PostNormalCommentDAO : BasePostCommentDAO
    {
        private static readonly Lazy<PostNormalCommentDAO> _instance =
            new Lazy<PostNormalCommentDAO>(() => new PostNormalCommentDAO());

        public static PostNormalCommentDAO Instance => _instance.Value;

        private PostNormalCommentDAO() { }
        /// <summary>
        /// Lấy feed comment của Post thường
        /// </summary>
        /// 
        // Node tổng
        private async Task<IElementHandle> GetFeedCommentAsync(IPage page)
        {
            try
            {
                Libary.Instance.LogDebug("[POST-NORMAL] 🔍 Tìm feed comment");

                var feedNode = await page.QuerySelectorAsync(
                    "div.x1qjc9v5.x78zum5.xdt5ytf.x1n2onr6.x1al4vs7"
                );

                if (feedNode == null)
                {
                    Libary.Instance.LogTech(
                        $"{Libary.IconFail}❌ [POST-NORMAL] Không tìm thấy feed comment"
                    );
                    return null;
                }

                Libary.Instance.LogDebug(
                    $"{Libary.IconOK} [POST-NORMAL] Feed comment OK"
                );

                return feedNode;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    $"❌ [POST-NORMAL] Lỗi GetFeedCommentAsync: {ex.Message}"
                );
                return null;
            }
        }
        // node comment tổng

        private async Task<List<IElementHandle>> GetCommentNodesAsync(IElementHandle feedNode)
        {
            var nodes = new List<IElementHandle>();

            try
            {
                await feedNode.WaitForSelectorAsync(
                    "div.x18xomjl",
                    new ElementHandleWaitForSelectorOptions
                    {
                        Timeout = 5000
                    });

                var blocks = await feedNode.QuerySelectorAllAsync("div.x18xomjl");

                foreach (var block in blocks)
                {
                    var articles = await block.QuerySelectorAllAsync(
                        "div[role='article']"
                    );

                    nodes.AddRange(articles);
                }

                Libary.Instance.LogDebug(
                    $"[POST-NORMAL] 🧩 Found comment nodes = {nodes.Count}"
                );
            }
            catch
            {
                Libary.Instance.LogDebug(
                    "[POST-NORMAL] ⚠ Timeout khi chờ comment nodes"
                );
            }

            return nodes;
        }
        // node comment con
        private async Task<List<IElementHandle>> WaitForPostNormalCommentNodesAsync(IElementHandle feedNode)
        {
            var nodes = new List<IElementHandle>();

            try
            {
                await feedNode.WaitForSelectorAsync(
                    "div.x18xomjl",
                    new ElementHandleWaitForSelectorOptions
                    {
                        Timeout = 5000
                    });

                var blocks = await feedNode.QuerySelectorAllAsync("div.x18xomjl");

                foreach (var block in blocks)
                {
                    var articles = await block.QuerySelectorAllAsync("div[role='article']");
                    nodes.AddRange(articles);
                }
            }
            catch
            {
                // timeout là bình thường
            }

            return nodes;
        }

        // hàm tổng
        public async Task<List<CommentItem>> ScanPostNormalFullAsync( IPage page,string postUrl,Func<bool> shouldStop)
        {
            var result = new List<CommentItem>();
            if (page == null || page.IsClosed)
                return result;

            try
            {
                // ===============================
                // 1️⃣ LOAD POST THƯỜNG
                // ===============================
                Libary.Instance.LogTech($"[POST][NORMAL] 🌐 Goto: {postUrl}");
                bool opened = await AdsPowerHelper.OpenFacebookLinkSafeAsync(page, postUrl);

                if (!opened)
                {
                    Libary.Instance.LogTech("❌ Không mở được link post (safe-open)");
                    return result; // hoặc continue / break tuỳ flow
                }

                await page.WaitForTimeoutAsync(800);


                // ===============================
                // 2️⃣ LẤY FEED COMMENT (CHUẨN THEO ẢNH)
                // ===============================
                var feedNode = await page.QuerySelectorAsync(
                    "div.x1qjc9v5.x78zum5.xdt5ytf.x1n2onr6.x1al4vs7"
                );

                if (feedNode == null)
                {
                    Libary.Instance.LogTech(
                        $"{Libary.IconFail}❌ [POST][NORMAL] Không tìm thấy feed comment"
                    );
                    return result;
                }

                Libary.Instance.LogTech(
                    $"{Libary.IconOK} [POST][NORMAL] Feed comment OK"
                );

                // ===============================
                // 3️⃣ CHUYỂN SANG TẤT CẢ BÌNH LUẬN
                // ===============================
                await SwitchToAllCommentsAsync(page);
                await page.WaitForTimeoutAsync(600);

                var collectedIds = new HashSet<string>();
                var parentNameToId = new Dictionary<string, string>();

                int noNewRound = 0;
                int maxNoNewRound = 3;

                // ===============================
                // 4️⃣ LOOP SCAN (Y HỆT REEL)
                // ===============================
                while (noNewRound < maxNoNewRound)
                {
                    if (shouldStop())
                    {
                        Libary.Instance.LogTech("[POST][NORMAL][STOP] ⛔ Stop requested");
                        break;
                    }

                    bool clickedMore = await ClickLoadMoreCommentsIfExistsAsync(page);
                    if (clickedMore) await page.WaitForTimeoutAsync(600);

                    await ClickViewRepliesFromPageAsync(page);
                    await page.WaitForTimeoutAsync(400);

                    // ⚠️ KHÁC REEL: WAIT + QUERY TRONG FEED
                    var nodes = await WaitForPostNormalCommentNodesAsync(feedNode);

                    int addedThisRound = 0;

                    foreach (var node in nodes)
                    {
                        if (shouldStop()) break;

                        string aria = await node.GetAttributeAsync("aria-label");
                        var meta = ParseAriaLabel(aria);
                        if (meta == null)
                            continue;

                        var (posterName, rawPosterLink) =
                            await ExtractCommentPosterAsync(node);

                        string commentId = ExtractCommentIdFromLink(rawPosterLink);

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

                    // ⚠️ KHÁC REEL: SCROLL TRONG FEED
                    await feedNode.EvaluateAsync(@"el => {
                el.scrollTop = el.scrollTop + el.clientHeight * 0.8;
            }");

                    await page.WaitForTimeoutAsync(400);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    "[POST][NORMAL][DAO] ❌ ScanPostNormalFullAsync lỗi: " + ex.Message
                );
            }

            return result;
        }

    }
}


 