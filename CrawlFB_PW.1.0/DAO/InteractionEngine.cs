using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0;
using Microsoft.Playwright;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
// các hàm mở tab phụ ở đây
public static class InteractionEngine
{
    // ============================================================
    // 1) LẤY CONTEXT HIỆN TẠI (không dùng page feed!)
    // ============================================================
    public static IBrowserContext GetBrowserContext()
    {
        var session = AdsPowerPlaywrightManager.Instance.GetActiveSession();
        return session?.Browser?.Contexts?.FirstOrDefault();
    }

    // ============================================================
    // 2) MỞ TAB PHỤ TỪ LINK (độc lập với feed)
    // ============================================================
    public static async Task<IPage> OpenNewTabAsync(string url)
    {
        var context = GetBrowserContext();
        if (context == null)
        {
            Libary.Instance.CreateLog("[Engine] ❌ Không lấy được BrowserContext.");
            return null;
        }

        Libary.Instance.CreateLog($"[Engine] 🌐 Mở tab phụ: {url}");

        var subPage = await context.NewPageAsync();
        await subPage.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 60000
        });

        await subPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await PageDAO.Instance.RandomDelayAsync(subPage, 500, 900);

        return subPage;
    }

    // ============================================================
    // 3) HÀM TỔNG – MỞ TAB → LẤY TƯƠNG TÁC → ĐÓNG TAB
    // ============================================================
    public static async Task<(int like, int comment, int share, string content)>
        ProcessPostInNewTabAsync(string url)
    {
        int likes = 0, comments = 0, shares = 0;
        string content = "N/A";
        IPage subPage = null;

        try
        {
            subPage = await OpenNewTabAsync(url);
            if (subPage == null)
                return (0, 0, 0, "N/A");

            string realUrl = subPage.Url.ToLower();

            // 📌 1) Reel
            if (realUrl.Contains("/reel/"))
            {
                Libary.Instance.CreateLog("[Engine] 🎬 Đây là REEL");
                (likes, comments, shares) = await ExtractReelInteractionsAsync(subPage);
                content = await ExtractReelContentAsync(subPage);
            }

            // 📌 2) Video Watch
            else if (realUrl.Contains("/watch") || realUrl.Contains("?v="))
            {
                Libary.Instance.CreateLog("[Engine] 🎥 Đây là VIDEO WATCH");
                (likes, comments, shares) = await ExtractWatchVideoInteractionsAsync(subPage);
                content = await ExtractWatchVideoContentAsync(subPage);
            }

            // 📌 3) Post thường
            else
            {
                Libary.Instance.CreateLog("[Engine] 📝 Đây là POST THƯỜNG");

                var postElement = await subPage.QuerySelectorAsync(
                    "div.x1fmog5m.xu25z0z.x140muxe.xo1y3bh.x78zum5.xdt5ytf.x1iyjqo2.x1al4vs7");

                if (postElement == null)
                    postElement = await subPage.QuerySelectorAsync("div[role='article']");

                (likes, comments, shares) = await ExtractPostInteractionsAsync(postElement);
                content = await ExtractPostContentAsync(postElement);
            }

            Libary.Instance.CreateLog($"[Engine] ✅ Done → Like={likes}, Cmt={comments}, Share={shares}");
            return (likes, comments, shares, content);
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog($"❌ [Engine] Lỗi xử lý tab phụ: {ex.Message}");
            return (likes, comments, shares, content);
        }
        finally
        {
            // 🧹 Đóng tab phụ ngay lập tức
            try
            {
                if (subPage != null && !subPage.IsClosed)
                {
                    await subPage.CloseAsync();
                    Libary.Instance.CreateLog("[Engine] 🧹 Đã đóng tab phụ.");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ [Engine] Lỗi đóng tab phụ: {ex.Message}");
            }
        }
    }

    // ============================================================
    // 4) PARSER REEL
    // ============================================================
    public static async Task<(int like, int comment, int share)> ExtractReelInteractionsAsync(IPage page)
    {
        try
        {
            Libary.Instance.CreateLog("[Reel] 🎬 Đọc tương tác Reel");

            var reels = await page.QuerySelectorAllAsync("div.x6s0dn4.x78zum5.x1n2onr6");
            if (reels.Count == 0) return (0, 0, 0);

            var r = reels.First();

            int likes = ParseReelNumber(await TryGetText(r, "div[aria-label='Thích']"));
            int comments = ParseReelNumber(await TryGetText(r, "div[aria-label='Bình luận']"));
            int shares = ParseReelNumber(await TryGetText(r, "div[aria-label='Chia sẻ']"));

            return (likes, comments, shares);
        }
        catch { return (0, 0, 0); }
    }

    public static async Task<string> ExtractReelContentAsync(IPage page)
    {
        var el = await page.QuerySelectorAsync("div[data-ad-preview='message'], span.x1lliihq");
        return await TryGetText(el);
    }

    // ============================================================
    // 5) PARSER WATCH VIDEO
    // ============================================================
    public static async Task<(int like, int comment, int share)> ExtractWatchVideoInteractionsAsync(IPage page)
    {
        try
        {
            Libary.Instance.CreateLog("[Watch] 🎥 Đọc tương tác Video");

            var btns = await page.QuerySelectorAllAsync("div[aria-label]");
            int likes = 0, comments = 0, shares = 0;

            foreach (var b in btns)
            {
                var label = (await b.GetAttributeAsync("aria-label"))?.ToLower() ?? "";
                var txt = (await b.InnerTextAsync())?.Trim() ?? "";

                if (label.Contains("thích"))
                    likes = ParseFacebookNumber(txt);

                if (label.Contains("bình luận"))
                    comments = ParseFacebookNumber(txt);

                if (label.Contains("chia sẻ"))
                    shares = ParseFacebookNumber(txt);
            }

            return (likes, comments, shares);
        }
        catch { return (0, 0, 0); }
    }

    public static async Task<string> ExtractWatchVideoContentAsync(IPage page)
    {
        return await TryGetText(await page.QuerySelectorAsync("div[data-ad-preview='message']"));
    }

    // ============================================================
    // 6) PARSER POST THƯỜNG
    // ============================================================
    public static async Task<(int like, int comment, int share)> ExtractPostInteractionsAsync(IElementHandle post)
    {
        try
        {
            var like = ParseFacebookNumber(await TryGetText(post, "span.x193iq5w"));
            var comment = ParseFacebookNumber(await TryGetText(post, "span[aria-label*='bình luận']"));
            var share = ParseFacebookNumber(await TryGetText(post, "span[aria-label*='chia sẻ']"));

            return (like, comment, share);
        }
        catch { return (0, 0, 0); }
    }

    public static async Task<string> ExtractPostContentAsync(IElementHandle post)
    {
        return await TryGetText(await post.QuerySelectorAsync("div[data-ad-preview='message']"));
    }

    // ============================================================
    // 7) HÀM ĐỌC TEXT AN TOÀN
    // ============================================================
    public static async Task<string> TryGetText(IElementHandle el, string selector = null)
    {
        try
        {
            if (el == null) return "";
            if (!string.IsNullOrEmpty(selector))
                el = await el.QuerySelectorAsync(selector);
            if (el == null) return "";
            return (await el.InnerTextAsync())?.Trim() ?? "";
        }
        catch { return ""; }
    }

    // ============================================================
    // 8) PARSE NUMBER (Reel & Post thường)
    // ============================================================
    public static int ParseReelNumber(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        text = text.ToLower().Trim();

        try
        {
            if (text.Contains("k"))
                return (int)(double.Parse(text.Replace("k", "").Replace(",", "."), CultureInfo.InvariantCulture) * 1000);

            if (text.Contains("m"))
                return (int)(double.Parse(text.Replace("m", "").Replace(",", "."), CultureInfo.InvariantCulture) * 1_000_000);

            return int.Parse(new string(text.Where(char.IsDigit).ToArray()));
        }
        catch { return 0; }
    }

    public static int ParseFacebookNumber(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        text = text.ToLower().Trim()
            .Replace("bình luận", "")
            .Replace("comments", "")
            .Replace("chia sẻ", "")
            .Replace("shares", "")
            .Replace("lượt", "")
            .Trim();

        try
        {
            if (text.Contains("k"))
                return (int)(double.Parse(text.Replace("k", "").Replace(",", "."), CultureInfo.InvariantCulture) * 1000);

            if (text.Contains("m"))
                return (int)(double.Parse(text.Replace("m", "").Replace(",", "."), CultureInfo.InvariantCulture) * 1_000_000);

            return int.Parse(new string(text.Where(char.IsDigit).ToArray()));
        }
        catch { return 0; }
    }
}
