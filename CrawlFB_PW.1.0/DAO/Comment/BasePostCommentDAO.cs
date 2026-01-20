using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DAO.Post
{
    public abstract class BasePostCommentDAO
    {
        protected BasePostCommentDAO() { }
        #region ===== MENU / FILTER =====

        /// <summary>
        /// Chuyển sang chế độ "Tất cả bình luận" (Reel / Watch / PostNormal)
        /// </summary>
        protected async Task<bool> SwitchToAllCommentsAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return false;

            try
            {
                // WATCH / POST NORMAL – filter "Phù hợp nhất"
                var watchFilter = await page.QuerySelectorAsync(
                    "div.x9f619.x1n2onr6.x1ja2u2z.xt0psk2.xuxw1ft"
                );

                if (watchFilter != null)
                {
                    await watchFilter.ScrollIntoViewIfNeededAsync();
                    await Task.Delay(100);

                    await watchFilter.ClickAsync(new ElementHandleClickOptions
                    {
                        Force = true,
                        Timeout = 2000
                    });

                    await page.WaitForTimeoutAsync(300);
                }
                else
                {
                    // FALLBACK – Reel menu
                    var buttons = await page.QuerySelectorAllAsync(
                        "div[role='button'][aria-haspopup='menu']"
                    );

                    foreach (var btn in buttons)
                    {
                        var text = (await btn.InnerTextAsync())?.Trim();
                        if (string.IsNullOrWhiteSpace(text)) continue;

                        if (ProcessingHelper.ContainsIgnoreCase(text, "Tất cả bình luận") || ProcessingHelper.ContainsIgnoreCase(text, "All comments"))

                        {
                            await btn.ClickAsync(new ElementHandleClickOptions
                            {
                                Force = true,
                                Timeout = 2000
                            });
                            await page.WaitForTimeoutAsync(300);
                            break;
                        }
                    }
                }

                // ===== CHỌN "TẤT CẢ BÌNH LUẬN" =====
                var menuItems = await page.QuerySelectorAllAsync("div[role='menuitem']");

                foreach (var item in menuItems)
                {
                    var text = (await item.InnerTextAsync())?.Trim();
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    if (ProcessingHelper.ContainsIgnoreCase(text, "Tất cả bình luận") || ProcessingHelper.ContainsIgnoreCase(text, "All comments"))
                    {
                        await item.ScrollIntoViewIfNeededAsync();
                        await Task.Delay(80);

                        await item.ClickAsync(new ElementHandleClickOptions
                        {
                            Force = true,
                            Timeout = 2000
                        });

                        await page.WaitForTimeoutAsync(800);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // lấy comment lớp 2
        protected async Task<bool> ClickViewRepliesFromPageAsync(IPage page)
        {
            if (page == null)
                return false;

            try
            {
                // bắt tất cả button đúng class + role
                var buttons = await page.QuerySelectorAllAsync(
                    "div[role='button']." +
                    "x1i10hfl.xjbqb8w.xjqpnuy.xc5r6h4.xqeqjp1.x1phubyo.x13fuv20.x18b5jzi" +
                    ".x1q0q8m5.x1t7ytsu.x972fbf.x10w94by.x1qhh985.x14e42zd.x9f619.x1ypdohk" +
                    ".xdl72j9.x3ct3a4.xdj266r.x14z9mp.xat24cr.x2lwn1j.xeuugli.xexx8yu" +
                    ".x18d9i69.x1c1uobl.x1n2onr6.x16tdsg8.x1hl2dhg.xggy1nq.x1ja2u2z" +
                    ".x1t137rt.x1fmog5m.xu25z0z.x140muxe.xo1y3bh.x3nfvp2.x87ps6o" +
                    ".x1lku1pv.x1a2a7pz.x6s0dn4.xi81zsa.x1q0g3np.x1iyjqo2.xs83m0k" +
                    ".x1icxu4v.xdzw4kq"
                );

                Libary.Instance.LogDebug($"[REPLY] 🔎 Found reply-button candidates = {buttons.Count}");

                foreach (var btn in buttons)
                {
                    string text = (await btn.InnerTextAsync())?.Trim().ToLower() ?? "";

                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    if (text.Contains("phản hồi")
                        || text.Contains("trả lời")
                        || text.Contains("reply")
                        || text.Contains("replies"))
                    {
                        await btn.ScrollIntoViewIfNeededAsync();
                        await Task.Delay(120);

                        await btn.ClickAsync();

                        Libary.Instance.LogDebug($"[REPLY] 🔽 Click phản hồi (page): {text}");
                        Libary.Instance.LogTech($"{Libary.IconOK} Click comment phản hồi OK");
                        return true;
                    }
                }

                Libary.Instance.LogDebug("[REPLY] ℹ️ Không tìm thấy nút phản hồi trên page");
                return false;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech("[REPLY] ❌ ClickViewRepliesFromPageAsync lỗi: " + ex.Message);
                return false;
            }
        }

        // LẤY THÔNG TIN NGƯỜI COMMENT
        protected async Task<(string PosterName, string RawPosterLink)> ExtractCommentPosterAsync(IElementHandle commentNode)
        {
            string name = "N/A";
            string rawLink = "N/A";

            try
            {
                var aPosters = await commentNode.QuerySelectorAllAsync("a[role='link']");

                Libary.Instance.LogDebug($"[COMMENT][POSTER] 🔎 Found a[role=link] = {aPosters.Count}");

                if (aPosters.Count >= 2)
                {
                    // 👉 COMMENT CON: lấy thằng thứ 2
                    var a = aPosters[1];

                    rawLink = await a.GetAttributeAsync("href") ?? "N/A";

                    // tên thường nằm trong span
                    var span = await a.QuerySelectorAsync("span");
                    if (span != null)
                        name = (await span.InnerTextAsync())?.Trim() ?? "N/A";
                    else
                        name = (await a.InnerTextAsync())?.Trim() ?? "N/A";

                    Libary.Instance.LogDebug($"[COMMENT][POSTER] ↳ REPLY | 👤 '{name}' | 🔗 {rawLink}");
                }
                else if (aPosters.Count == 1)
                {
                    // 👉 COMMENT CHA
                    var a = aPosters[0];

                    rawLink = await a.GetAttributeAsync("href") ?? "N/A";
                    name = (await a.InnerTextAsync())?.Trim() ?? "N/A";

                    Libary.Instance.LogDebug($"[COMMENT][POSTER] 🧱 PARENT | 👤 '{name}' | 🔗 {rawLink}");
                    Libary.Instance.LogTech($"{Libary.IconOK} Lấy THÔNG TIN NGƯỜI bình luận OK");
                }
                else
                {
                    Libary.Instance.LogDebug("[COMMENT][POSTER] ⚠️ Không tìm thấy thẻ a[role=link]");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[COMMENT][POSTER] ❌ Lỗi ExtractCommentPosterAsync: " + ex.Message);
                Libary.Instance.LogTech($"{Libary.IconFail} ❌ Lỗi ExtractCommentPosterAsync: " + ex.Message);
            }

            return (name, rawLink);
        }
        //LẤY NỘI DUNG
        protected async Task<string> ExtractCommentContentAsync(IElementHandle commentNode)
        {
            if (commentNode == null)
                return "N/A";

            try
            {
                // 1️⃣ Click "Xem thêm" nếu có
                await ClickSeeMoreIfExistsAsync(commentNode);

                // 2️⃣ Lấy nội dung
                var contentDiv = await commentNode.QuerySelectorAsync("div.xdj266r.x14z9mp.xat24cr.x1lziwak.x1vvkbs");

                if (contentDiv == null)
                    return "N/A";

                string content = (await contentDiv.InnerTextAsync())?.Trim() ?? "N/A";

                if (!string.IsNullOrWhiteSpace(content))
                {
                    Libary.Instance.LogDebug($"[COMMENT] 📝 Content ({content.Length} chars)");
                    Libary.Instance.LogTech($"{Libary.IconOK} LẤY NỘI DUNG COMMENT OK: {content.Length} KÝ TỰ");
                    return content;
                }

                return "N/A";
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[COMMENT] ❌ ExtractCommentContentAsync lỗi: " + ex.Message);
                Libary.Instance.LogTech($"{Libary.IconFail} kHÔNG LẤY ĐƯỢC NỘI DUNG BL");
                return "N/A";
            }
        }

        // click xem thêm 
        protected async Task ClickSeeMoreIfExistsAsync(IElementHandle container)
        {
            if (container == null)
                return;

            try
            {
                var seeMoreBtn = await container.QuerySelectorAsync(
                    "div[role='button']:has-text('Xem thêm'), div[role='button']:has-text('See more')"
                );

                if (seeMoreBtn == null)
                    return;

                await seeMoreBtn.ScrollIntoViewIfNeededAsync();
                await Task.Delay(120);

                await seeMoreBtn.ClickAsync();

                Libary.Instance.LogDebug("[COMMENT] 🔽 Click 'Xem thêm'");
                await Task.Delay(150);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[COMMENT] ⚠️ ClickSeeMoreIfExistsAsync lỗi: " + ex.Message);
            }
        }
        protected async Task<bool> ClickLoadMoreCommentsIfExistsAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return false;

            try
            {
                // selector đúng cái anh mô tả: role=button + text
                var buttons = await page.QuerySelectorAllAsync("div[role='button']");

                foreach (var btn in buttons)
                {
                    string text = (await btn.InnerTextAsync())?.Trim().ToLower() ?? "";

                    if (string.IsNullOrWhiteSpace(text))
                        continue;
                    // 🎯 BẮT ĐÚNG: Xem thêm bình luận
                    if (text.Contains("xem thêm bình luận") || text.Contains("more comments"))
                    {
                        await btn.ScrollIntoViewIfNeededAsync();
                        await Task.Delay(150);

                        await btn.ClickAsync();

                        Libary.Instance.LogDebug($"{Libary.IconOK}[COMMENT] 🔽 Click 'Xem thêm bình luận'");
                        Libary.Instance.LogDebug($"{Libary.IconOK} Click 'Xem thêm bình luận' OK ");
                        await Task.Delay(400); // chờ FB load DOM mới
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"{Libary.IconFail}❌ Click Xem thêm lỗi: " + ex.Message);
                return false;
            }
        }
        // Lấy ID
        protected string ExtractCommentIdFromLink(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return null;

            try
            {
                var uri = new Uri(rawUrl);

                // uri.Query dạng: ?comment_id=XXX&__tn__=R
                var query = uri.Query.TrimStart('?').Split('&');

                foreach (var part in query)
                {
                    if (part.StartsWith("comment_id=", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = part.Substring("comment_id=".Length);
                        return Uri.UnescapeDataString(value);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        // lấy time
        protected async Task<string> ExtractCommentTimeAsync(IElementHandle commentNode)
        {
            if (commentNode == null)
                return "N/A";

            try
            {
                var timeAnchor = await commentNode.QuerySelectorAsync(
                    "a.x1i10hfl.xjbqb8w.x1ejq31n.x18oe1m7.x1sy0etr.xstzfhl.x972fbf.x10w94by" +
                    ".x1qhh985.x14e42zd.x9f619.x1ypdohk.xt0psk2.x3ct3a4.xdj266r.x14z9mp" +
                    ".xat24cr.x1lziwak.xexx8yu.xyri2b.x18d9i69.x1c1uobl.x16tdsg8.x1hl2dhg" +
                    ".xggy1nq.x1a2a7pz.xkrqix3.x1sur9pj.xi81zsa.x1s688f"
                );

                if (timeAnchor == null)
                    return "N/A";

                string timeText = (await timeAnchor.InnerTextAsync())?.Trim();

                if (string.IsNullOrWhiteSpace(timeText))
                    return "N/A";

                Libary.Instance.LogDebug($"[COMMENT] ⏰ TimeRaw = {timeText}");
                timeText = TimeHelper.CleanTimeString(timeText);
                Libary.Instance.LogDebug($"[COMMENT] ⏰ TimeRaw sau Clean = {timeText}");
                DateTime? timepar = TimeHelper.ParseFacebookTime(timeText);
                Libary.Instance.LogTech($"{Libary.IconOK} lẤY THỜI GIAN OK TIME PAR: {timepar}");
                string timedep = TimeHelper.NormalizeTime(timepar);
                Libary.Instance.LogTech($"{Libary.IconOK} lẤY THỜI GIAN OK TIME ĐẸP: {timedep}");
                return timeText;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[COMMENT] ❌ ExtractCommentTimeAsync lỗi: " + ex.Message);
                Libary.Instance.LogDebug($"{Libary.IconFail} LỖI LẤY THỜI GIAN ");
                return "N/A";
            }
        }
        #endregion
        protected string ShortenPosterLinkFromComment(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return "";

            // tìm &comment_id hoặc ?comment_id (lấy cái xuất hiện đầu tiên)
            int idx1 = rawUrl.IndexOf("&comment_id", StringComparison.OrdinalIgnoreCase);
            int idx2 = rawUrl.IndexOf("?comment_id", StringComparison.OrdinalIgnoreCase);

            int cutIndex = -1;

            if (idx1 >= 0 && idx2 >= 0)
                cutIndex = Math.Min(idx1, idx2);
            else if (idx1 >= 0)
                cutIndex = idx1;
            else if (idx2 >= 0)
                cutIndex = idx2;

            if (cutIndex > 0)
                return rawUrl.Substring(0, cutIndex);

            return rawUrl;
        }
        protected class ParsedAriaComment
        {
            public bool IsReply { get; set; }
            public string PosterName { get; set; }
            public string ParentPosterName { get; set; }
            public string TimeRaw { get; set; }
        }
        protected ParsedAriaComment ParseAriaLabel(string aria)
        {
            if (string.IsNullOrWhiteSpace(aria))
            {
                Libary.Instance.LogDebug("[ARIA] ⚠️ aria-label rỗng");
                return null;
            }

            aria = aria.Trim();

            Libary.Instance.LogDebug($"[ARIA] 🔎 Raw: {aria}");

            // ===============================
            // 🔹 PHẢN HỒI
            // ===============================
            var reply = Regex.Match(
                aria,
                @"^Phản hồi bình luận của (.+?) dưới tên (.+?) vào (.+)$",
                RegexOptions.IgnoreCase);

            if (reply.Success)
            {
                var meta = new ParsedAriaComment
                {
                    IsReply = true,
                    ParentPosterName = reply.Groups[1].Value.Trim(),
                    PosterName = reply.Groups[2].Value.Trim(),
                    TimeRaw = reply.Groups[3].Value.Trim()
                };

                Libary.Instance.LogDebug("[ARIA] ↳ REPLY\n" +
                    $"   👤 Người phản hồi : {meta.PosterName}\n" +
                    $"   🧱 Phản hồi của  : {meta.ParentPosterName}\n" +
                    $"   ⏰ Thời gian     : {meta.TimeRaw}"
                );
                DateTime? timepars = TimeHelper.ParseFacebookTime(meta.TimeRaw);
                Libary.Instance.LogTech($"{Libary.IconOK} lẤY TIME OK Timepars {timepars.ToString()}");
                string timedep = TimeHelper.NormalizeTime(timepars);
                Libary.Instance.LogTech($"{Libary.IconOK} lẤY TIME OK Timeđep {timedep}");
                return meta;
            }

            // ===============================
            // 🔹 BÌNH LUẬN GỐC
            // ===============================
            var parent = Regex.Match(
                aria,
                @"^Bình luận dưới tên (.+?) vào (.+)$",
                RegexOptions.IgnoreCase);

            if (parent.Success)
            {
                var meta = new ParsedAriaComment
                {
                    IsReply = false,
                    PosterName = parent.Groups[1].Value.Trim(),
                    ParentPosterName = null,
                    TimeRaw = parent.Groups[2].Value.Trim()
                };

                Libary.Instance.LogDebug(
                    "[ARIA] 🧱 PARENT\n" +
                    $"   👤 Người bình luận : {meta.PosterName}\n" +
                    $"   ⏰ Thời gian       : {meta.TimeRaw}"
                );

                return meta;
            }
            // ===============================
            // ❌ KHÔNG MATCH
            // ===============================
            Libary.Instance.LogDebug(
                "[ARIA] ❌ Không match pattern\n" +
                $"   Raw: {aria}"
            );

            return null;
        }
        public async Task HumanScrollAsync(IPage page, IElementHandle container)
        {
            await container.EvaluateAsync(@"el => {
        el.scrollTop = el.scrollTop + el.clientHeight * 0.8;
    }");
        }
    }
}
