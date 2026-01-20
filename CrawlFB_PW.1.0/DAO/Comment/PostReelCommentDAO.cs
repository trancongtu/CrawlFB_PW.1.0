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
    internal sealed class PostReelCommentDAO : BasePostCommentDAO
    {
        private static readonly Lazy<PostReelCommentDAO> _instance =
    new Lazy<PostReelCommentDAO>(() => new PostReelCommentDAO());

        public static PostReelCommentDAO Instance => _instance.Value;
        private PostReelCommentDAO() { }
        public async Task<bool> LoadReelPageAsync(IPage page, string reelUrl)
        {
            if (page == null || page.IsClosed)
                return false;

            try
            {

                Libary.Instance.LogDebug("1-[REEL] 🌐 Goto reel: " + reelUrl);

                await page.GotoAsync(reelUrl, new PageGotoOptions
                {
                    Timeout = AppConfig.DEFAULT_TIMEOUT,
                    WaitUntil = WaitUntilState.DOMContentLoaded
                });

                // ⏳ Chờ UI Reel ổn định nhẹ
                await page.WaitForTimeoutAsync(500);

                // 🔎 Check nhanh: có nút Bình luận hoặc video reel
                var commentBtn = await page.QuerySelectorAsync("div[role='button'][aria-label='Bình luận'], div[role='button'][aria-label='Comments']");

                if (commentBtn == null)
                {
                    Libary.Instance.LogDebug("[REEL] ⚠️ Load xong nhưng chưa thấy nút Bình luận");
                    // vẫn return true vì có thể layout load chậm
                }
                else
                {
                    Libary.Instance.LogDebug("[REEL] ✅ Reel layout sẵn sàng");
                    Libary.Instance.LogTech($"{Libary.IconInfo} Load DOM thành công");
                }

                return true;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[REEL] ❌ LoadReelPageAsync lỗi: " + ex.Message);
                return false;
            }
        }
        // Click vô ô binfhh luận
        public async Task<bool> ClickReelCommentOnceAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return false;

            try
            {
                // 1️⃣ Chờ nút Bình luận xuất hiện & visible
                var btn = await page.WaitForSelectorAsync("div[role='button'][aria-label='Bình luận'], div[role='button'][aria-label='Comments']",
                    new PageWaitForSelectorOptions
                    {
                        Timeout = 10000,
                        State = WaitForSelectorState.Visible
                    }
                );

                // 2️⃣ Scroll nhẹ để tránh click hụt
                await btn.ScrollIntoViewIfNeededAsync();
                await page.WaitForTimeoutAsync(150);

                // 3️⃣ CLICK 1 LẦN DUY NHẤT
                await btn.ClickAsync();

                // 4️⃣ CHỜ xem bình luận có hiện không (KHÔNG click lại)
                var opened = await page.WaitForSelectorAsync("div[aria-label='Viết bình luận'], textarea, div[contenteditable='true']",
                    new PageWaitForSelectorOptions
                    {
                        Timeout = 3000
                    }
                );

                if (opened != null)
                {
                    Libary.Instance.LogTech("[REEL] 💬 Bình luận đã hiện sau click");
                    return true;
                }

                Libary.Instance.LogTech("[REEL] ⚠️ Click xong nhưng không thấy bình luận (giữ nguyên trạng thái)");
                return false;
            }
            catch (TimeoutException)
            {
                Libary.Instance.LogTech("[REEL] ⚠️ Click xong nhưng bình luận KHÔNG hiện trong thời gian chờ");
                return false;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech("[REEL] ❌ ClickReelCommentOnceAsync lỗi: " + ex.Message);
                return false;
            }
        }
        public async Task<List<IElementHandle>> WaitForReelCommentNodes_DirectAsync(IPage page, int timeoutMs = 8000)
        {
            try
            {
                // ⏳ Chờ node comment block xuất hiện trực tiếp ở page
                await page.WaitForSelectorAsync("div.x18xomjl.xbcz3fp", new PageWaitForSelectorOptions { Timeout = timeoutMs });
                var blocks = await page.QuerySelectorAllAsync("div.x18xomjl.xbcz3fp");
                Libary.Instance.LogDebug($"[COMMENT][DIRECT] ✅ Block x18xomjl = {blocks.Count}");

                // Từ block → lấy comment thật (role=article)
                var commentNodes = new List<IElementHandle>();

                foreach (var block in blocks)
                {
                    var articles = await block.QuerySelectorAllAsync("div[role='article']");

                    if (articles.Count > 0)
                        commentNodes.AddRange(articles);
                }

                Libary.Instance.LogTech($"[COMMENT][DIRECT] 🎯 Thấy có Comment: = {commentNodes.Count}");

                return commentNodes;
            }
            catch (TimeoutException)
            {
                Libary.Instance.LogTech("[COMMENT][DIRECT] ⚠️ Timeout khi chờ x18xomjl");
                return new List<IElementHandle>();
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[COMMENT][DIRECT] ❌ Lỗi: " + ex.Message);
                return new List<IElementHandle>();
            }
        }
        // hảm tổng đơn giản
        public async Task<List<CommentItem>> ScanReelCommentsAsync(IPage reelPage, string reelUrl)
        {
            var result = new List<CommentItem>();

            if (reelPage == null || reelPage.IsClosed)
                return result;

            try
            {
                // ===============================
                // 1️⃣ LOAD REEL + MỞ BÌNH LUẬN
                // ===============================
                bool loaded = await LoadReelPageAsync(reelPage, reelUrl);
                if (!loaded)
                    return result;

                await ClickReelCommentOnceAsync(reelPage);
                await reelPage.WaitForTimeoutAsync(600);

                // ===============================
                // 2️⃣ CHUẨN BỊ BIẾN ĐIỀU KHIỂN
                // ===============================
                var collectedIds = new HashSet<string>();
                int noNewRound = 0;
                int maxNoNewRound = 3;

                // ===============================
                // 3️⃣ LOOP: CLICK → LẤY → SCROLL
                // ===============================
                while (noNewRound < maxNoNewRound)
                {
                    // 🔽 1️⃣ Click "Xem thêm bình luận" nếu có
                    bool clickedMore =
                        await ClickLoadMoreCommentsIfExistsAsync(reelPage);

                    if (clickedMore)
                        await reelPage.WaitForTimeoutAsync(600);

                    // 🔽 Click mở phản hồi trong viewport
                    await ClickViewRepliesFromPageAsync(reelPage);
                    await reelPage.WaitForTimeoutAsync(500);

                    // 🔎 Lấy comment hiện có
                    var nodes =
                        await WaitForReelCommentNodes_DirectAsync(reelPage);

                    int addedThisRound = 0;

                    foreach (var node in nodes)
                    {
                        // ===============================
                        // POSTER
                        // ===============================
                        var (posterName, rawPosterLink) =
                            await ExtractCommentPosterAsync(node);

                        if (string.IsNullOrWhiteSpace(posterName))
                            posterName = "N/A";

                        if (string.IsNullOrWhiteSpace(rawPosterLink))
                            rawPosterLink = "N/A";


                        string commentId = "N/A";
                        string posterLink = "N/A";

                        if (rawPosterLink != "N/A")
                        {
                            commentId =
                                ExtractCommentIdFromLink(rawPosterLink) ?? "N/A";

                            posterLink =
                                ShortenPosterLinkFromComment(rawPosterLink);
                        }

                        // ❌ Không có ID → bỏ
                        if (commentId == "N/A")
                            continue;

                        // ❌ Trùng → bỏ
                        if (!collectedIds.Add(commentId))
                            continue;

                        addedThisRound++;

                        // ===============================
                        // CONTENT
                        // ===============================
                        string content =
                            await ExtractCommentContentAsync(node);

                        if (string.IsNullOrWhiteSpace(content))
                            content = "N/A";

                        // ===============================
                        // TIME
                        // ===============================
                        string timeRaw =
                            await ExtractCommentTimeAsync(node);

                        if (string.IsNullOrWhiteSpace(timeRaw))
                            timeRaw = "N/A";

                        DateTime? realTime = null;

                        if (timeRaw != "N/A")
                        {
                            var parsed = TimeHelper.ParseFacebookTime(timeRaw);

                            if (parsed != DateTime.MinValue)
                                realTime = parsed;
                        }

                        // ===============================
                        // MAP CHA / CON
                        // ===============================
                        result.Add(new CommentItem
                        {
                            CommentId = commentId,
                            PosterName = posterName,
                            PosterLink = posterLink,
                            Content = content,
                            TimeRaw = timeRaw,
                            RealCommentTime = realTime,
                            Status = "Bình luận tại post"
                        });
                    }

                    // ===============================
                    // 4️⃣ KIỂM TRA CÒN COMMENT MỚI?
                    // ===============================
                    if (addedThisRound == 0)
                        noNewRound++;
                    else
                        noNewRound = 0;

                    // ===============================
                    // 5️⃣ SCROLL XUỐNG → LOAD DOM MỚI
                    // ===============================
                    await ProcessingDAO.Instance.HumanScrollAsync(reelPage);
                    await reelPage.WaitForTimeoutAsync(400);
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
        // hàm tổng full
        // hàm click tất cả bình luận
        public async Task<bool> ClickAllCommentsMenuAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return false;

            try
            {
                // ===============================
                // 1️⃣ TÌM BUTTON MENU (aria-haspopup="menu")
                // ===============================
                var buttons = await page.QuerySelectorAllAsync("div[role='button'][aria-haspopup='menu']");

                if (buttons == null || buttons.Count == 0)
                {
                    Libary.Instance.LogDebug("[COMMENT][MENU] ⚠️ Không tìm thấy button menu");
                    return false;
                }

                IElementHandle menuButton = buttons.First();

                await menuButton.ScrollIntoViewIfNeededAsync();
                await Task.Delay(150);
                await menuButton.ClickAsync();

                Libary.Instance.LogDebug("[COMMENT][MENU] 🔽 Đã click mở menu lọc bình luận");

                await Task.Delay(400);

                // ===============================
                // 2️⃣ LẤY DANH SÁCH MENU ITEM
                // ===============================
                var menuItems = await page.QuerySelectorAllAsync("div[role='menuitem']");

                if (menuItems == null || menuItems.Count == 0)
                {
                    Libary.Instance.LogDebug("[COMMENT][MENU] ⚠️ Không có menuitem sau khi mở menu");
                    return false;
                }

                // ===============================
                // 3️⃣ TÌM & CLICK "TẤT CẢ BÌNH LUẬN"
                // ===============================
                foreach (var item in menuItems)
                {
                    var span = await item.QuerySelectorAsync("span");
                    if (span == null) continue;

                    string text = (await span.InnerTextAsync())?.Trim();
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    Libary.Instance.LogDebug($"[COMMENT][MENU] 🔍 Menu item: {text}");

                    if (text.IndexOf("tất cả bình luận", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        await item.ScrollIntoViewIfNeededAsync();
                        await Task.Delay(100);
                        await item.ClickAsync();

                        Libary.Instance.LogDebug("[COMMENT][MENU] ✅ Đã chọn 'Tất cả bình luận'");
                        Libary.Instance.LogTech($"{Libary.IconOK} ✅ Click 'Tất cả bình luận' OK ");
                        // ⏳ Chờ Facebook load lại comment
                        await page.WaitForTimeoutAsync(800);
                        return true;
                    }
                }

                Libary.Instance.LogDebug("[COMMENT][MENU] ⚠️ Không tìm thấy 'Tất cả bình luận' trong menu"
                );

                return false;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"{Libary.IconFail} ❌ Click Tất cả bình luận Lỗi: " + ex.Message
                );
                return false;
            }
        }
        public async Task<List<CommentItem>> ScanPostRellFulllAsync(IPage reelPage, string reelUrl, Func<bool> shouldStop)
        {
            var result = new List<CommentItem>();
            if (reelPage == null || reelPage.IsClosed)
                return result;
            try
            {
                bool loaded = await LoadReelPageAsync(reelPage, reelUrl);
                if (!loaded) return result;

                await ClickReelCommentOnceAsync(reelPage);
                await reelPage.WaitForTimeoutAsync(600);
                await SwitchToAllCommentsAsync(reelPage);
                //await ClickAllCommentsMenuAsync(reelPage); hãm cũ của reel
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
                    bool clickedMore = await ClickLoadMoreCommentsIfExistsAsync(reelPage);

                    if (clickedMore) await reelPage.WaitForTimeoutAsync(600);
                    if (shouldStop()) break;
                    await ClickViewRepliesFromPageAsync(reelPage);
                    await reelPage.WaitForTimeoutAsync(400);

                    var nodes = await WaitForReelCommentNodes_DirectAsync(reelPage);

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
                    await ProcessingDAO.Instance.HumanScrollAsync(reelPage);
                    await reelPage.WaitForTimeoutAsync(400);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    "[REEL][DAO] ❌ ScanPostRellFulllAsync lỗi: " + ex.Message);
            }
            return result;
        }
    }
}
