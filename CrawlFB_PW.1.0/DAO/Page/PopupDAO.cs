using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.ViewModels;
using Microsoft.Playwright;

namespace CrawlFB_PW._1._0.DAO.Page
{
    public class PopupDAO
    {
        private static PopupDAO _instance;

        public static PopupDAO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PopupDAO();

                return _instance;
            }
        }

        private PopupDAO() { }

        // ===============================
        // 🔥 GET FEED POPUP (DIALOG)
        // ===============================
        public async Task<IElementHandle> GetFeedPopupAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return null;

            try
            {
                // ===============================
                // 1️⃣ WAIT POPUP
                // ===============================
                try
                {
                    await page.WaitForSelectorAsync(
                        "div[role='dialog']",
                        new PageWaitForSelectorOptions
                        {
                            Timeout = 5000
                        }
                    );
                }
                catch
                {
                    Libary.Instance.LogDebug("⚠️ Popup dialog không xuất hiện");
                }

                // ===============================
                // 2️⃣ ƯU TIÊN selector cụ thể
                // ===============================
                var dialog = await page.QuerySelectorAsync(
                    "div[role='dialog'][aria-labelledby]"
                );

                if (dialog != null)
                {
                    Libary.Instance.LogDebug("✅ [PopupDAO] Found dialog (aria-labelledby)");
                    return dialog;
                }

                // ===============================
                // 3️⃣ FALLBACK: dialog chung
                // ===============================
                dialog = await page.QuerySelectorAsync("div[role='dialog']");

                if (dialog != null)
                {
                    Libary.Instance.LogDebug("⚠️ [PopupDAO] Found dialog fallback");
                    return dialog;
                }

                Libary.Instance.LogDebug("❌ [PopupDAO] Không tìm thấy popup dialog");
            }
            catch (System.Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [PopupDAO] Exception: {ex.Message}");
            }

            return null;
        }
        ///===Person
        public async Task<(string name, string link)> GetPosterPopup(IElementHandle dialog)
        {
            string name = "N/A";
            string link = "N/A";

            try
            {
                if (dialog == null)
                    return (name, link);

                // ===============================
                // 🔥 1️⃣ ƯU TIÊN: profile_name
                // ===============================
                var profileDiv = await dialog.QuerySelectorAsync("div[data-ad-rendering-role='profile_name']");

                if (profileDiv != null)
                {
                    Libary.Instance.LogDebug("✅ [PosterPopup] Found profile_name");

                    var a = await profileDiv.QuerySelectorAsync("a[href]");

                    if (a != null)
                    {
                        name = (await a.InnerTextAsync())?.Trim() ?? "N/A";
                        link = await a.GetAttributeAsync("href") ?? "N/A";

                        Libary.Instance.LogDebug($"👤 [PosterPopup] NAME = {name}");
                        return (name, link);
                    }
                }
                else
                {
                    Libary.Instance.LogDebug("⚠️ [PosterPopup] profile_name NOT FOUND");
                }

                // ===============================
                // 🔥 2️⃣ FALLBACK: a[role='link']
                // ===============================
                var nameNodes = await dialog.QuerySelectorAllAsync("a[role='link']");

                Libary.Instance.LogDebug($"🔎 [PosterPopup] fallback nodes = {nameNodes.Count}");

                foreach (var a in nameNodes)
                {
                    var txt = (await a.InnerTextAsync())?.Trim();

                    if (string.IsNullOrWhiteSpace(txt))
                        continue;

                    // ❗ lọc rác
                    if (txt.Length > 80) continue;
                    if (txt.Contains("bình luận")) continue;
                    if (txt.Contains("chia sẻ")) continue;
                    if (txt.Contains("Thích")) continue;

                    name = txt;
                    link = await a.GetAttributeAsync("href") ?? "N/A";

                    Libary.Instance.LogDebug($"👤 [PosterPopup][Fallback] NAME = {name}");
                    break;
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [PosterPopup] Exception: {ex.Message}");
            }

            return (name, link);
        }
        public async Task<(string time, DateTime? realTime)> GetTimePopup(IElementHandle dialog)
        {
            string time = "N/A";
            DateTime? realTime = null;

            try
            {
                if (dialog == null)
                    return (time, realTime);

                var timeNodes = await dialog.QuerySelectorAllAsync("div.xu06os2.x1ok221b");

                Libary.Instance.LogDebug($"⏰ [TimePopup] nodes = {timeNodes.Count}");

                foreach (var el in timeNodes)
                {
                    var txt = (await el.InnerTextAsync())?.Trim().ToLower();

                    if (string.IsNullOrEmpty(txt))
                        continue;

                    // 🔥 lọc đúng time (tránh dính tên như log trước)
                    if (!ProcessingDAO.Instance.IsTime(txt))
                        continue;

                    txt = TimeHelper.CleanTimeString(txt);

                    time = txt;
                    realTime = TimeHelper.ParseFacebookTime(txt);

                    Libary.Instance.LogDebug($"✅ [TimePopup] TIME = {time} | {realTime}");
                    break;
                }

                if (time == "N/A")
                {
                    Libary.Instance.LogDebug("❌ [TimePopup] Không tìm được time hợp lệ");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [TimePopup] Exception: {ex.Message}");
            }

            return (time, realTime);
        }
        public async Task<string> GetContentPopup(IElementHandle dialog)
        {
            string content = "";

            try
            {
                if (dialog == null)
                    return content;

                // ===============================
                // 🔥 1️⃣ ƯU TIÊN story_message (CHUẨN NHẤT)
                // ===============================
                var story = await dialog.QuerySelectorAsync("div[data-ad-rendering-role='story_message']");

                if (story != null)
                {
                    Libary.Instance.LogDebug("✅ [ContentPopup] Found story_message");

                    // lấy text bên trong
                    var nodes = await story.QuerySelectorAllAsync("div.xdj266r");

                    var sb = new StringBuilder();

                    foreach (var n in nodes)
                    {
                        var txt = (await n.InnerTextAsync())?.Trim();

                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            sb.AppendLine(txt);
                        }
                    }

                    content = sb.ToString().Trim();

                    if (!string.IsNullOrEmpty(content))
                    {
                        Libary.Instance.LogDebug($"📝 [ContentPopup] LEN = {content.Length}");
                        return content;
                    }
                }
                else
                {
                    Libary.Instance.LogDebug("⚠️ [ContentPopup] story_message NOT FOUND");
                }

                // ===============================
                // 🔥 2️⃣ FALLBACK (background kiểu cũ)
                // ===============================
                var nodesFallback = await dialog.QuerySelectorAllAsync("div.xdj266r");

                var sbFallback = new StringBuilder();

                foreach (var n in nodesFallback)
                {
                    var txt = (await n.InnerTextAsync())?.Trim();

                    if (string.IsNullOrWhiteSpace(txt))
                        continue;

                    // ❗ lọc bỏ comment / UI text
                    if (txt.Contains("bình luận")) continue;
                    if (txt.Contains("chia sẻ")) continue;
                    if (txt.Contains("Thích")) continue;
                    if (txt.Length < 20) continue;

                    sbFallback.AppendLine(txt);
                }

                content = sbFallback.ToString().Trim();

                if (!string.IsNullOrEmpty(content))
                {
                    Libary.Instance.LogDebug($"📝 [ContentPopup][Fallback] LEN = {content.Length}");
                }
                else
                {
                    Libary.Instance.LogDebug("❌ [ContentPopup] EMPTY");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [ContentPopup] Exception: {ex.Message}");
            }

            return content;
        }
        public async Task<(int like, int comment, int share)> GetInteractionPopup(IElementHandle dialog)
        {
            int like = 0, comment = 0, share = 0;

            bool found = false; // 🔥 chỉ cần thấy element là true

            try
            {
                if (dialog == null)
                    return (like, comment, share);

                // =====================================================
                // 🔥 1️⃣ FEED (Group / Page / Share)
                // =====================================================
                var feeds = await dialog.QuerySelectorAllAsync("div.x1n2onr6");

                Libary.Instance.LogDebug($"📦 [AutoInteract] feed blocks = {feeds.Count}");

                foreach (var feed in feeds)
                {
                    var container = await feed.QuerySelectorAsync(
                        "div.x6s0dn4.xi81zsa.x78zum5.x6prxxf.x13a6bvl.xvq8zen");

                    if (container == null)
                        continue;

                    Libary.Instance.LogDebug("✅ [AutoInteract] FEED container found");

                    found = true;

                    // 👍 LIKE
                    var likeBtn = await container.QuerySelectorAsync("span[aria-label*='cảm xúc']");

                    if (likeBtn != null)
                    {
                        var likeSpan = await likeBtn.QuerySelectorAsync("span");

                        if (likeSpan != null)
                        {
                            var txt = (await likeSpan.InnerTextAsync())?.Trim();
                            like = ProcessingHelper.ParseFacebookNumber(txt);
                        }
                    }

                    // 💬 COMMENT + 🔁 SHARE
                    var spans = await container.QuerySelectorAllAsync("span");

                    foreach (var sp in spans)
                    {
                        var txt = (await sp.InnerTextAsync())?.Trim().ToLower();

                        if (string.IsNullOrEmpty(txt)) continue;

                        if (txt.Contains("bình luận") || txt.Contains("comment"))
                            comment = ProcessingHelper.ParseFacebookNumber(txt);

                        else if (txt.Contains("chia sẻ") || txt.Contains("share"))
                            share = ProcessingHelper.ParseFacebookNumber(txt);
                    }

                    break; // 🔥 đã đúng container → dừng luôn
                }

                // =====================================================
                // 🔥 2️⃣ PERSON PROFILE (nếu FEED không thấy)
                // =====================================================
                if (!found)
                {
                    Libary.Instance.LogDebug("⚠️ [AutoInteract] fallback → PERSON");

                    var blocks = await dialog.QuerySelectorAllAsync("div.xn3w4p2.x1gslohp");

                    foreach (var block in blocks)
                    {
                        // 👍 LIKE
                        var likeNode = await block.QuerySelectorAsync("div[aria-label='Thích']");

                        if (likeNode != null)
                        {
                            found = true;

                            var txt = (await likeNode.InnerTextAsync())?.Trim();
                            like = ProcessingHelper.ParseFacebookNumber(txt);
                        }

                        // 💬 COMMENT
                        var commentNode = await block.QuerySelectorAsync("div[aria-label='Viết bình luận']");

                        if (commentNode != null)
                        {
                            found = true;

                            var txt = (await commentNode.InnerTextAsync())?.Trim();
                            comment = ProcessingHelper.ParseFacebookNumber(txt);
                        }

                        // 🔁 SHARE
                        var shareNode = await block.QuerySelectorAsync(
                            "div[aria-label*='Gửi nội dung này cho bạn bè']");

                        if (shareNode != null)
                        {
                            found = true;

                            var txt = (await shareNode.InnerTextAsync())?.Trim();
                            share = ProcessingHelper.ParseFacebookNumber(txt);
                        }

                        if (found)
                            break;
                    }
                }

                // =====================================================
                // 🔥 3️⃣ FALLBACK (CHỈ KHI KHÔNG THẤY ELEMENT)
                // =====================================================
                if (!found)
                {
                    Libary.Instance.LogDebug("⚠️ [AutoInteract] fallback → GLOBAL");

                    var spans = await dialog.QuerySelectorAllAsync("span");

                    foreach (var sp in spans)
                    {
                        var txt = (await sp.InnerTextAsync())?.Trim().ToLower();

                        if (string.IsNullOrEmpty(txt)) continue;

                        if (txt.Contains("bình luận"))
                            comment = ProcessingHelper.ParseFacebookNumber(txt);

                        else if (txt.Contains("chia sẻ"))
                            share = ProcessingHelper.ParseFacebookNumber(txt);

                        else if (txt.Contains("cảm xúc") || txt.Contains("like"))
                            like = ProcessingHelper.ParseFacebookNumber(txt);
                    }
                }

                Libary.Instance.LogDebug($"🔥 FINAL AUTO: 👍{like} 💬{comment} 🔁{share}");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [AutoInteract] Exception: {ex.Message}");
            }

            return (like, comment, share);
        }
        public async Task<(string name, string link)> GetPosterGroupsPopupPost(IElementHandle dialog)
        {
            string name = "N/A";
            string link = "N/A";

            try
            {
                if (dialog == null)
                    return (name, link);

                // ===============================
                // 🔥 1️⃣ LẤY BLOCK CHỨA LINK USER
                // ===============================
                var blocks = await dialog.QuerySelectorAllAsync("div.xu06os2.x1ok221b");

                Libary.Instance.LogDebug($"👥 [GroupPosterPopup] blocks = {blocks.Count}");

                foreach (var block in blocks)
                {
                    var links = await block.QuerySelectorAllAsync("a[href]");

                    foreach (var a in links)
                    {
                        var href = await a.GetAttributeAsync("href");

                        if (string.IsNullOrWhiteSpace(href))
                            continue;

                        // 🔥 chỉ lấy link user
                        if (!href.Contains("/user/"))
                            continue;

                        var txt = (await a.InnerTextAsync())?.Trim();

                        if (string.IsNullOrWhiteSpace(txt))
                            continue;

                        var cleanLink = ProcessingHelper.ShortenPosterLink(href);

                        name = txt;
                        link = cleanLink;

                        Libary.Instance.LogDebug($"👤 [GroupPosterPopup] NAME = {name}");
                        Libary.Instance.LogDebug($"🔗 [GroupPosterPopup] LINK = {link}");

                        return (name, link);
                    }
                }

                Libary.Instance.LogDebug("❌ [GroupPosterPopup] Không tìm thấy user link");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [GroupPosterPopup] Exception: {ex.Message}");
            }

            return (name, link);
        }
        ///
        // hàm tổng
        public async Task<PostInfoRawVM> GetPostOriginalInfoByPopupDAO(IPage page, string url)
        {
            var info = new PostInfoRawVM
            {
                PostLink = url
            };

            try
            {
                Libary.Instance.LogTech("🚀 ===== [ORIGINAL POPUP DAO] START =====");

                if (page == null || page.IsClosed)
                {
                    Libary.Instance.LogTech("❌ Page null/closed");
                    return info;
                }

                // ===============================
                // 🔥 1️⃣ ROOT = FEED POPUP (QUAN TRỌNG NHẤT)
                // ===============================
                var dialog = await PopupDAO.Instance.GetFeedPopupAsync(page);

                if (dialog == null)
                {
                    Libary.Instance.LogTech("❌ Không tìm thấy popup dialog");
                    return info;
                }

                Libary.Instance.LogTech("✅ Found popup dialog");

                // ===============================
                // ⏰ TIME
                // ===============================
                var (time, realTime) = await PopupDAO.Instance.GetTimePopup(dialog);

                info.PostTime = time;
                info.RealPostTime = realTime;

                Libary.Instance.LogTech($"⏰ TIME: {time} | {realTime}");

                // ===============================
                // 📝 CONTENT
                // ===============================
                var content = await PopupDAO.Instance.GetContentPopup(dialog);

                info.Content = content;

                if (!string.IsNullOrWhiteSpace(content))
                {
                    Libary.Instance.LogTech($"📝 CONTENT LEN = {content.Length}");
                }
                else
                {
                    Libary.Instance.LogTech("❌ CONTENT EMPTY");
                }

                // ===============================
                // 📊 INTERACTION
                // ===============================
                var (like, comment, share) = await PopupDAO.Instance.GetInteractionPopup(dialog);

                info.LikeCount = like;
                info.CommentCount = comment;
                info.ShareCount = share;

                Libary.Instance.LogTech($"🔥 RESULT: 👍{like} 💬{comment} 🔁{share}");


                // ===============================
                // 👤 POSTER
                // ===============================
                var (nametemp, linktemp) = await PopupDAO.Instance.GetPosterPopup(dialog);
                bool isGroup = !string.IsNullOrWhiteSpace(url) && url.Contains("/groups/");
                if (isGroup)
                {
                    info.PageName = nametemp;
                    info.PageLink = linktemp;
                    var (posterName, posterLink) = await PopupDAO.Instance.GetPosterGroupsPopupPost(dialog);
                    info.PosterName = posterName;
                    info.PosterLink = posterLink;
                    Libary.Instance.LogTech("Groups ");
                }
                else
                {
                    Libary.Instance.LogTech("FanPage hoặc Person");
                }


                // ===============================
                // 🧠 POST TYPE (basic)
                // ===============================
                bool hasContent = ProcessingHelper.IsValidContent(info.Content);

                info.PostType = hasContent
                    ? PostType.Page_Normal
                    : PostType.Page_NoConent;

                Libary.Instance.LogTech($"📌 TYPE = {info.PostType}");

                Libary.Instance.LogTech("🚀 ===== [ORIGINAL POPUP DAO] END =====");

                return info;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"❌ [ORIGINAL POPUP DAO] {ex.Message}");
                return info;
            }
        }
    }
}
