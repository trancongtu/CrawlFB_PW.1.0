using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.ViewModels;
using Microsoft.Playwright;

namespace CrawlFB_PW._1._0.DAO.Post
{
    public class SharePostDAO : BasePostCommentDAO
    {
        // ===============================
        // SINGLETON
        // ===============================
        private static SharePostDAO _instance;
        public static SharePostDAO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SharePostDAO();
                return _instance;
            }
        }

        private SharePostDAO() { }

        // ===============================
        // CONSTANT SELECTOR
        // ===============================
        private const string SHARE_POPUP_DIV =
            "div[aria-label='Những người đã chia sẻ điều này']";

        private const string SHARE_SPAN_FALLBACK =
            "span:has-text('Những người đã chia sẻ điều này')";
        public class ShareInfo
        {
            // Người chia sẻ
            public string SharerName { get; set; } = "N/A";
            public string SharerLink { get; set; } = "N/A";

            // Nơi chia sẻ
            public string TargetName { get; set; } = "N/A";
            public string TargetLink { get; set; } = "N/A";

            // Cờ nghiệp vụ
            public bool IsSelfShare { get; set; } = false;
        }

        // =================================================
        // CHECK SHARE ENABLE
        // =================================================
        /// <summary>
        /// Kiểm tra bài viết có bật chia sẻ hay chưa
        /// Ưu tiên popup div, fallback span text
        /// </summary>
        public async Task<bool> IsShareEnabledAsync(IPage page)
        {
            // 1️⃣ Popup đã mở
            var popup = await page.QuerySelectorAsync(SHARE_POPUP_DIV);
            if (popup != null)
                return true;

            // 2️⃣ Fallback span
            var span = await page.QuerySelectorAsync(SHARE_SPAN_FALLBACK);
            return span != null;
        }

        // =================================================
        // PREPARE SHARE SCAN
        // =================================================
        /// <summary>
        /// Chuẩn bị quét share:
        /// - false: chưa bật chia sẻ
        /// - true : đã mở popup sẵn sàng quét
        /// </summary>
        public async Task<bool> PrepareShareScanAsync(IPage page)
        {
            if (!await IsShareEnabledAsync(page))
                return false;

            // Popup chưa mở → click span để mở
            var popup = await page.QuerySelectorAsync(SHARE_POPUP_DIV);
            if (popup == null)
            {
                await page.ClickAsync(SHARE_SPAN_FALLBACK);

                await page.WaitForSelectorAsync(
                    SHARE_POPUP_DIV,
                    new PageWaitForSelectorOptions
                    {
                        Timeout = 5000
                    });
            }

            return true;
        }

        public async Task<IElementHandle> GetShareFeedAsync(IPage page,int timeoutMs = 15000,int pollMs = 500)
        {
            int waited = 0;

            while (waited < timeoutMs)
            {
                var feed = await page.QuerySelectorAsync(
                    "div[aria-label='Những người đã chia sẻ điều này']"
                );

                if (feed != null)
                {
                    Libary.Instance.LogTech("[ShareFeed] ✅ Feed detected");
                    return feed;
                }

                await Task.Delay(pollMs);
                waited += pollMs;
            }

            Libary.Instance.LogTech("[ShareFeed] ❌ Timeout waiting feed");
            return null; // ✅ C# 7.3 cho phép return null
        }

        //==== LẤY NGƯỜI SHARE, NƠI SHARE
        private async Task<IElementHandle> GetProfileNameBlockAsync(IElementHandle node)
        {
            if (node == null) return null;

            return await node.QuerySelectorAsync(
                "div[data-ad-rendering-role='profile_name']"
            );
        }
        //lấy time và link share
        public async Task<(string postShareTime, string postShareLink)>ExtractShareTimeAndLinkAsync(IElementHandle shareNode)
        {
            string postShareTime = "N/A";
            string postShareLink = "N/A";

            if (shareNode == null)
                return (postShareTime, postShareLink);

            try
            {
                // 1️⃣ Lấy các div time/link
                var timeBlocks = await shareNode.QuerySelectorAllAsync(
                    "div.xu06os2.x1ok221b"
                );

                foreach (var block in timeBlocks)
                {
                    string text = (await block.InnerTextAsync())?.Trim();
                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    // 2️⃣ Kiểm tra text có phải thời gian không
                    if (!ProcessingDAO.Instance.IsTime(text))
                        continue;

                    postShareTime = TimeHelper.CleanTimeString(text);

                    // 3️⃣ Lấy link trong cùng block
                    var links = await block.QuerySelectorAllAsync("a[href]");

                    foreach (var a in links)
                    {
                        string href = await a.GetAttributeAsync("href");
                        if (ProcessingHelper.IsValidPostPath(href))
                        {
                            postShareLink =
                                ProcessingHelper.ShortenFacebookPostLink(href);
                            break;
                        }
                    }

                    // 👉 Lấy được 1 cặp là dừng
                    break;
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"❌ [ExtractShareTimeAndLinkAsync] {ex.Message}");
            }

            return (postShareTime, postShareLink);
        }
        //lấy thông tin ng share, nơi share
        private async Task<List<(string Text, string Href)>>ExtractLinksFromProfileNameAsync(IElementHandle profileBlock)
        {
            var result = new List<(string, string)>();

            if (profileBlock == null) return result;

            var links = await profileBlock.QuerySelectorAllAsync("a[href]");

            foreach (var a in links)
            {
                string href = await a.GetAttributeAsync("href");
                string text = (await a.InnerTextAsync())?.Trim();

                if (string.IsNullOrWhiteSpace(href)) continue;

                result.Add((text ?? "N/A", href));
            }

            return result;
        }
        private async Task<ShareInfo> ParseShareInfoAsync(IElementHandle node)
        {
            var info = new ShareInfo();

            try
            {
                var profileBlock = await GetProfileNameBlockAsync(node);
                if (profileBlock == null)
                    return info;

                var links = await ExtractLinksFromProfileNameAsync(profileBlock);

                // =========================
                // CASE 1: KHÔNG LINK
                // =========================
                if (links.Count == 0)
                    return info;
                // =========================
                // CASE 2: 1 LINK → TỰ SHARE
                // =========================
                if (links.Count == 1)
                {
                    info.SharerName = links[0].Text;

                    // link KHÔNG có /user/ thì vẫn coi là self
                    info.SharerLink = links[0].Href.Contains("/user/")
                        ? ProcessingHelper.NormalizePersonProfileLink(links[0].Href)
                        : ProcessingHelper.ShortLinkPage(links[0].Href);

                    info.TargetName = info.SharerName;
                    info.TargetLink = info.SharerLink;

                    info.IsSelfShare = true;
                    return info;
                }
                //case 3 2 link
                var userLink = links.FirstOrDefault(l =>!string.IsNullOrWhiteSpace(l.Href) && l.Href.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) >= 0);

                var otherLink = links.FirstOrDefault(l => !l.Equals(userLink));

                // fallback nếu không có /user/
                if (string.IsNullOrWhiteSpace(userLink.Href))
                {
                    userLink = links[0];
                    otherLink = links.Count > 1 ? links[1] : links[0];
                }

                // Sharer
                info.SharerName = userLink.Text;
                info.SharerLink =
                    userLink.Href.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) >= 0
                        ? ProcessingHelper.NormalizePersonProfileLink(userLink.Href)
                        : ProcessingHelper.ShortLinkPage(userLink.Href);

                // Target
                info.TargetName = otherLink.Text;
                info.TargetLink =
                    otherLink.Href.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) >= 0
                        ? ProcessingHelper.NormalizePersonProfileLink(otherLink.Href)
                        : ProcessingHelper.ShortLinkPage(otherLink.Href);

                info.IsSelfShare = false;


            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"[ParseShareInfo] ❌ {ex.Message}");
            }

            return info;
        }
        public static bool IsUserLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            url = url.ToLowerInvariant();

            // 👤 profile chuẩn
            if (url.Contains("/profile.php"))
                return true;

            // 👤 user trong group: /groups/{gid}/user/{uid}
            if (url.Contains("/user/"))
                return true;

            // 👤 username (KHÔNG phải group / page)
            if (!url.Contains("/groups/")
                && !url.Contains("/pages/")
                && !url.Contains("/community/"))
                return true;

            return false;
        }

        //=====LẤY COMMENT
        public class CommentInfo
        {
            public string CommenterName { get; set; } = "N/A";
            public string CommenterLink { get; set; } = "N/A";
            public string Content { get; set; } = "N/A";
        }
        // lấy kiểu k áp dụng DAO, chỉ lấy được ng bình luận, content, k có cấp độ, k có thời gian
        private async Task<IReadOnlyList<IElementHandle>> GetCommentNodesAsync(IElementHandle feedOrPost)
        {
            if (feedOrPost == null)
                return new List<IElementHandle>();

            return await feedOrPost.QuerySelectorAllAsync(
                "div[class='xwib8y2 xpdmqnj x1g0dm76 x1y1aw1k']"
            );
        }
        private async Task<CommentInfo> ParseCommentNodeAsync(IElementHandle node)
        {
            var info = new CommentInfo();

            if (node == null)
                return info;

            try
            {
                // =========================
                // 1️⃣ TÊN + LINK NGƯỜI BL
                // =========================
                var nameAnchor = await node.QuerySelectorAsync( "span.xt0psk2 a[href]" );

                if (nameAnchor != null)
                {
                    info.CommenterLink =
                        await nameAnchor.GetAttributeAsync("href") ?? "N/A";

                    info.CommenterName =
                        (await nameAnchor.InnerTextAsync())?.Trim() ?? "N/A";
                }

                // =========================
                // 2️⃣ NỘI DUNG BÌNH LUẬN
                // =========================
                var contentDivs = await node.QuerySelectorAllAsync(
                    "div.x1lliihq.xjkvuk6.x1iorvi4"
                );

                if (contentDivs.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var div in contentDivs)
                    {
                        var text = (await div.InnerTextAsync())?.Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            if (sb.Length > 0) sb.AppendLine();
                            sb.Append(text);
                        }
                    }

                    info.Content = sb.Length > 0 ? sb.ToString() : "N/A";
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"[ParseCommentNode] ❌ {ex.Message}");
            }

            return info;
        }
     
        // =================================================
        //scroll SHARE CONTAINER
        // =================================================
        private async Task<IElementHandle> FindRealScrollBoxAsync(IElementHandle feed)
        {
            if (feed == null) return null;

            try
            {
                var jsHandle = await feed.EvaluateHandleAsync(
                    @"root => {
                const all = root.querySelectorAll('div');
                for (const el of all) {
                    const style = window.getComputedStyle(el);
                    if (
                        (style.overflowY === 'auto' || style.overflowY === 'scroll') &&
                        el.scrollHeight > el.clientHeight
                    ) {
                        return el;
                    }
                }
                return null;
            }"
                );

                return jsHandle?.AsElement();
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"[FindScrollBox] ❌ {ex.Message}");
                return null;
            }
        }

        private async Task<bool> ScrollShareContainerAsync(IElementHandle scrollBox)
        {
            if (scrollBox == null)
                return false;

            try
            {
                return await scrollBox.EvaluateAsync<bool>(
                    @"el => {
                const before = el.scrollTop;
                el.scrollTop = el.scrollTop + el.clientHeight * 0.8;
                return el.scrollTop > before;
            }"
                );
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"[ScrollShareContainer] ❌ {ex.Message}");
                return false;
            }
        }
        // hàm tổng

        public async Task<List<SharePostVM>> CrawlShareAsync(IPage page,IElementHandle feed)
        {
            var result = new List<SharePostVM>();

            int lastCount = 0;
            int noNewRound = 0;
            int maxNoNewRound = 3;

            var scrollBox = await FindRealScrollBoxAsync(feed);
            if (scrollBox == null)
            {
                Libary.Instance.LogTech("[ShareCrawl] ❌ Không tìm được scroll box");
                return result;
            }

            while (true)
            {
                var nodes = await feed.QuerySelectorAllAsync("div[class='x1yztbdb']");
                int currentCount = nodes.Count;
                Libary.Instance.LogTech( $"[ShareCrawl] Nodes={currentCount}, Last={lastCount}");

                if (currentCount > lastCount)
                {
                    for (int i = lastCount; i < currentCount; i++)
                    {
                        var node = nodes[i];
                        Libary.Instance.LogTech($"====BÀI SHARE THỨ {i}");
                        // =========================
                        // SHARE INFO
                        // =========================
                        var shareInfo = await ParseShareInfoAsync(node);
                        if(shareInfo != null) Libary.Instance.LogTech($"Lấy Share info thành công");
                        // 🔧 FINAL NORMALIZE (CHỐT)
                        if (!string.IsNullOrWhiteSpace(shareInfo.SharerLink))
                        {
                            if (shareInfo.SharerLink.IndexOf("/user/", StringComparison.OrdinalIgnoreCase) >= 0)
                                shareInfo.SharerLink =
                                    ProcessingHelper.NormalizeCommentActorProfileLink(shareInfo.SharerLink);
                            else
                                shareInfo.SharerLink =
                                    ProcessingHelper.ShortLinkPage(shareInfo.SharerLink);
                        }

                        if (!string.IsNullOrWhiteSpace(shareInfo.TargetLink))
                        {
                            shareInfo.TargetLink =
                                ProcessingHelper.ShortLinkPage(shareInfo.TargetLink);
                        }
                        var (shareTime, shareLink) = await ExtractShareTimeAndLinkAsync(node);

                        DateTime? realShareTime = null;
                        if (!string.IsNullOrWhiteSpace(shareTime))
                            realShareTime = TimeHelper.ParseFacebookTime(shareTime);
                        Libary.Instance.LogTech(
                        "[SHARE-RAW]\n" +
                        $"  👤 SharerName     : {shareInfo?.SharerName}\n" +
                        $"  🔗 SharerLink     : {shareInfo?.SharerLink}\n" +
                        $"  📍 TargetName     : {shareInfo?.TargetName}\n" +
                        $"  🔗 TargetLink     : {shareInfo?.TargetLink}\n" +
                        $"  ⏰ TimeShare(raw) : {shareTime}\n" +
                        $"  🕒 TimeShare(real): {(realShareTime.HasValue ? realShareTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "NULL")}\n" +
                        $"  🧾 PostLinkShare  : {shareLink}"
                    );

                        var vm = new SharePostVM
                        {
                            Select = false,

                            SharerName = shareInfo.SharerName,
                            SharerLink = shareInfo.SharerLink,

                            TargetName = shareInfo.TargetName,
                            TargetLink = shareInfo.TargetLink,

                            TimeShare = shareTime,
                            RealShareTime = realShareTime,
                            PostLinkShare = shareLink
                        };

                        // =========================
                        // COMMENTS
                        // =========================
                        var commentNodes = await node.QuerySelectorAllAsync("div[role='article']");

                        foreach (var cNode in commentNodes)
                        {
                            string aria = await cNode.GetAttributeAsync("aria-label");
                            var meta = ParseAriaLabel(aria);
                            if (meta == null)
                                continue;

                            var (posterName, rawPosterLink) =  await ExtractCommentPosterAsync(cNode);
                            Libary.Instance.LogDebug($"rawPosterLink Comment: "+rawPosterLink);
                            string posterLink = ProcessingHelper.NormalizeCommentActorProfileLink(rawPosterLink);
                            Libary.Instance.LogDebug($"posterlink Comment sau xử lý: " + posterLink);
                            string content =
                                await ExtractCommentContentAsync(cNode);

                            DateTime? realTime = null;
                            if (!string.IsNullOrWhiteSpace(meta.TimeRaw))
                                realTime = TimeHelper.ParseFacebookTime(meta.TimeRaw);

                            vm.Comments.Add(new CommentGridRow
                            {
                                Select = false,
                                ActorName = meta.PosterName,
                                PosterFBType = FBType.Person,
                                Time = meta.TimeRaw,
                                RealPostTime = realTime,
                                Link = posterLink,
                                LinkView = posterLink,
                                Content = content,
                                Level = meta.IsReply ? 1 : 0
                            });
                        }

                        result.Add(vm);

                        Libary.Instance.LogTech($"[ShareVM] {vm.SharerName} → {vm.TargetName} | Comments={vm.TotalComment}");
                    }

                    lastCount = currentCount;
                    noNewRound = 0;
                }
                else
                {
                    noNewRound++;
                }

                if (noNewRound >= maxNoNewRound)
                {
                    Libary.Instance.LogTech("[ShareCrawl] ⏹️ Đã tới đáy popup");
                    break;
                }

                bool scrolled = await ScrollShareContainerAsync(scrollBox);
                await Task.Delay(1200);

                if (!scrolled)
                    noNewRound++;
            }

            return result;
        }


    }
}
