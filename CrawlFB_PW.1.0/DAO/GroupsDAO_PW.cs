using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CrawlFB_PW._1._0.DTO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using CrawlFB_PW._1._0.Helper;

namespace CrawlFB_PW._1._0.DAO
{
    /// <summary>
    /// GroupsDAO_PW: chứa logic trích xuất thông tin 1 bài viết từ element handle (Playwright)
    /// - Trả về PostPage (không trả null nếu có thể, gán PostLink="N/A" nếu không tìm được link)
    /// - Parse số comment/share an toàn
    /// - Có log để debug khi bài bị omit
    /// </summary>
    public class GroupsDAO_PW
    {
        private static readonly Lazy<GroupsDAO_PW> _instance =
            new Lazy<GroupsDAO_PW>(() => new GroupsDAO_PW());
        public static GroupsDAO_PW Instance { get { return _instance.Value; } }

        private GroupsDAO_PW() { }
       

        // Parse số từ các chuỗi Facebook như "1,2K", "5 bình luận", "N/A"
        private int ParseFacebookNumber(string text)
      { 
            if (string.IsNullOrEmpty(text)) return 0;
            text = text.ToLower().Trim();
            text = text.Replace("bình luận", "").Replace("comments", "")
                       .Replace("chia sẻ", "").Replace("shares", "")
                       .Replace("lượt", "").Trim();

            try
            {
                if (text.Contains("k"))
                {
                    text = text.Replace("k", "").Replace(",", ".").Trim();
                    double v;
                    double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v);
                    return (int)(v * 1000);
                }
                if (text.Contains("m"))
                {
                    text = text.Replace("m", "").Replace(",", ".").Trim();
                    double v;
                    double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v);
                    return (int)(v * 1000000);
                }

                var digits = new string(text.Where(char.IsDigit).ToArray());
                int r = 0;
                int.TryParse(digits, out r);
                return r;
            }
            catch
            {
                return 0;
            }
        }
        // hàm tổng lấy bài viết 1 groups, đầu vào từng post, lấy feed ở ngoài
        public async Task<PostPage> GetPostGroupsAsync(IPage page, IElementHandle post)
        {
            try
            {
                string PosterName = "N/A", PosterLink = "N/A";
                string OriginalPosterName = "N/A", OriginalPosterLink = "N/A";
                string PostTime = "N/A", OriginalPostTime = "N/A";
                string PostLink = "N/A", OriginalPostLink = "N/A";
                string Content = "N/A", OriginalContent = "N/A";
                string PostStatus = "N/A", Topic = "N/A";
                int CommentCount = 0, ShareCount = 0;
                // 1️⃣ Lấy các node postinfor (giống Selenium)
                var postinfor = await post.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");  
                var (timeList, linkList) = await ExtractTimeAndLinksAsync(postinfor);               
                for (int i = 0; i < Math.Max(timeList.Count, linkList.Count); i++)
                {
                    string time = i < timeList.Count ? timeList[i] : "N/A";
                    string link = i < linkList.Count ? linkList[i] : "N/A";
                }
                if (timeList.Count > 0 || linkList.Count > 0)
                {
                    // ✅ Có dữ liệu time/link → xử lý bình thường
                    (PostTime, OriginalPostTime, PostLink, OriginalPostLink) = await PostTypeDetectorAsync(timeList, linkList);
                }
                else
                {

                    (PostTime, PostLink, OriginalPosterName, OriginalPosterLink, PosterName, PosterLink) = await ExtractFallback(page,post);
                    PostStatus = "Bài Reel";
                }
                try
                {
                    switch (timeList.Count)
                    { case 1: // có 1 link 1 time thì là bài đăng
                            if (postinfor.Count == 3 || postinfor.Count == 2)
                            {
                                var posterContainer = GetSafe(postinfor, 0);
                                (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);

                                if (postinfor.Count == 3)
                                {
                                    var contentContainer = GetSafe(postinfor, 2);
                                    Content = await GetContentTextAsync(page, contentContainer);                                   
                                    // 🔁 Nếu không có nội dung thì fallback sang BackgroundTextAllAsync
                                    if (string.IsNullOrWhiteSpace(Content))
                                    {
                                        Content = await BackgroundTextAllAsync(page, post);
                                        if (!string.IsNullOrWhiteSpace(Content))
                                            PostStatus = "bài đăng kèm ảnh/video";
                                        else
                                            PostStatus = "bài đăng không có nội dung";
                                    }
                                    else
                                    {
                                        PostStatus = "bài đăng có nội dung";
                                    }
                                }//else là 2 nền màu, ảnh
                                else
                                {
                                    // var listText = new List<string>();
                                    Content = await GetBackgroundTextAsync(post);
                                    PostStatus = "bài đăng nền màu/ảnh";                               
                                }

                            }
                       break;
                       case 2:
                            PostStatus = "Bài Share lại";
                            switch (postinfor.Count)
                            {                            
                                case 6:
                                    {
                                        var posterContainer = GetSafe(postinfor, 0);
                                        var contentContainer = GetSafe(postinfor, 2);
                                        Content = await GetContentTextAsync(page, contentContainer);
                                        (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                        var originalPosterContainer = GetSafe(postinfor, 3);
                                        (OriginalPosterName, OriginalPosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                        var originalContentContainer = GetSafe(postinfor, 5);
                                        OriginalContent = await GetContentTextAsync(page, originalPosterContainer);
                                        PostStatus = "Bài share lại đầy đủ";
                                    }                                    
                                    break;
                                case 5:
                                    {
                                        // Người đăng luôn là 0
                                        var posterContainer = GetSafe(postinfor, 0);
                                        (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                        // ✅ Xác định người đăng gốc (vị trí 3 là phổ biến)
                                        int originalPosterIndex = 3;
                                        try
                                        {
                                            var originalPosterContainer = GetSafe(postinfor, originalPosterIndex);
                                            (OriginalPosterName, OriginalPosterLink) = await GetPosterInfoBySelectorsAsync(originalPosterContainer);
                                        }
                                        catch
                                        {
                                            // Fallback nếu không tồn tại 3 thì thử index 2
                                            originalPosterIndex = 2;
                                            var originalPosterContainer = GetSafe(postinfor, originalPosterIndex);
                                            (OriginalPosterName, OriginalPosterLink) = await GetPosterInfoBySelectorsAsync(originalPosterContainer);
                                        }

                                        // ✅ Kiểm tra vị trí 2 (nội dung người chia sẻ)
                                        var maybeContentContainer = GetSafe(postinfor, 2);
                                        string possibleContent = await GetContentTextAsync(page, maybeContentContainer);
                                        bool hasUserContent = !string.IsNullOrWhiteSpace(possibleContent);

                                        if (hasUserContent)
                                        {
                                            // Người chia sẻ có viết => bài gốc thiếu nội dung
                                            Content = possibleContent;
                                            PostStatus = "bài share lại thiếu content bài gốc";
                                        }
                                        else
                                        {
                                            // Người chia sẻ không viết => bài gốc có nội dung ở index 4
                                            var maybeOriginalContentContainer = GetSafe(postinfor, 4);
                                            OriginalContent = await GetContentTextAsync(page, maybeOriginalContentContainer);
                                            PostStatus = "bài share lại thiếu content người chia sẻ";
                                        }
                                        break;
                                    }
                            }
                            break;
                        case 0:
                            {
                                Content = await GetPostTextAsync(post);

                                // Nếu vẫn rỗng, fallback thêm nền màu / alt text
                                if (string.IsNullOrWhiteSpace(Content) || Content == "N/A")
                                {
                                    Content = await BackgroundTextAllAsync(page, post);
                                    if (!string.IsNullOrWhiteSpace(Content))
                                    {
                                        PostStatus = "fallback";
                                    }
                                    else
                                    {
                                        PostStatus = "lỗi";
                                    }                              
                                }
                            }
                            break;
                    }
                }
                catch { }
                // 7️⃣ Đếm bình luận & chia sẻ
                var sharecomment = await post.QuerySelectorAllAsync("span[class*='x1l2wv1i'], span[class*='x6prxxf'], span[class*='x1eso8m3']");
                if (sharecomment.Count >= 2)
                {
                    CommentCount = ParseFacebookNumber(await sharecomment[0].InnerTextAsync());
                    ShareCount = ParseFacebookNumber(await sharecomment[1].InnerTextAsync());
                }

                // 8️⃣ Chuẩn hóa
                if (string.IsNullOrEmpty(PostLink)) PostLink = "N/A";
                if (string.IsNullOrEmpty(PostTime)) PostTime = "N/A";
                if (string.IsNullOrEmpty(PosterName)) PosterName = "(Không đọc được)";

                return new PostPage
                {
                    PosterName = PosterName,
                    PosterLink = PosterLink,
                    PostTime = PostTime,
                    PostLink = PostLink,
                    Content = Content,                   
                    CommentCount = CommentCount,
                    ShareCount = ShareCount,
                    PostType = PostStatus,
                    Topic = Topic
                };
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[GroupsDAO_PW] GetPostGroupsAsync lỗi: " + ex.Message);
                return null;
            }
        }     
        public async Task<(List<string> timeList, List<string> linkList)> ExtractTimeAndLinksAsync(IEnumerable<IElementHandle> postinfor)
        {
            var timeList = new List<string>();
            var linkList = new List<string>();
            var addedLinks = new HashSet<string>();

            int index = 0; // để log từng phần tử postinfor

            foreach (var info in postinfor)
            {
                index++;
                string textContent = (await info.InnerTextAsync())?.Trim() ?? "";

                if (string.IsNullOrEmpty(textContent))
                    continue;

                if (Regex.IsMatch(textContent, @"(\d+\s*(giờ|phút|ngày|hôm qua|tháng))", RegexOptions.IgnoreCase))
                {
                    var anchors = await info.QuerySelectorAllAsync("a[class*='x1i10hfl'], a[href*='posts'], a[href*='videos']");
                    foreach (var a in anchors)
                    {
                        string href = await a.GetAttributeAsync("href");
                        if (!string.IsNullOrEmpty(href) && addedLinks.Add(href))
                        {
                            timeList.Add(textContent);
                            linkList.Add(href);
                        }
                    }
                }
            }
            return (timeList, linkList);
        }
        // hàm dưới lấy thông tin người đăng
        public async Task<(string postTime, string originalPostTime, string postLink, string originalPostLink)>PostTypeDetectorAsync(List<string> timeList, List<string> linkList, IEnumerable<IElementHandle> postinfor = null)
        {
            string postTime = "N/A";
            string originalPostTime = "N/A";
            string postLink = "N/A";
            string originalPostLink = "N/A";

            try
            {
                int timeCount = timeList.Count;
                int linkCount = linkList.Count;

                if (timeCount == 1 && linkCount >= 1)
                {
                    // 🔸 Bài viết tự đăng
                    postTime = TimeHelper.CleanTimeString(timeList[0]);
                    postLink = ProcessingHelper.ShortenFacebookPostLink(linkList[0]);
                }
                else if (timeCount == 2 && linkCount >= 2)
                {
                    // 🔹 Bài viết share
                    postTime = TimeHelper.CleanTimeString(timeList[0]);
                    originalPostTime = TimeHelper.CleanTimeString(timeList[1]);
                    postLink = ProcessingHelper.ShortenFacebookPostLink(linkList[0]);
                    originalPostLink = linkList[1];
                }
                else if (timeCount == 0 && linkCount == 0)
                {
                    // 🔸 Có thể là bài video
                    if (postinfor != null)
                    {
                        var (vtime, vlink) = await HandleVideoPostAsync(postinfor);
                        postTime = vtime;
                        postLink = vlink;
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ [PostTypeDetectorAsync] Lỗi: {ex.Message}");
            }

            return (postTime, originalPostTime, postLink, originalPostLink);
        }

        // hàm lấy link và người đăng video
        public async Task<(string postTime, string postLink)> HandleVideoPostAsync(IEnumerable<IElementHandle> postinfor)
        {
            string postTime = "N/A";
            string postLink = "N/A";

            try
            {
                foreach (var post in postinfor)
                {
                    // Lấy link video
                    var videoAnchors = await post.QuerySelectorAllAsync("a[href*='video']");
                    if (videoAnchors.Count > 0)
                    {
                        string href = await videoAnchors.First().GetAttributeAsync("href");
                        if (!string.IsNullOrEmpty(href))
                            postLink = href;
                    }

                    // Lấy thời gian đăng gần vùng video
                    string txt = (await post.InnerTextAsync())?.Trim() ?? "";
                    if (Regex.IsMatch(txt, @"(\d+\s*(giờ|phút|ngày|hôm qua|tháng))", RegexOptions.IgnoreCase))
                    {
                        postTime = TimeHelper.CleanTimeString(txt);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi trong HandleVideoPostAsync: " + ex.Message);
            }

            return (postTime, postLink);
        }//hàm lấy link và người đăng video
        // hàm chọn postinfor an toàn, tránh lỗi
        public IElementHandle GetSafe(IReadOnlyList<IElementHandle> list, int index)
        {
            if (index < list.Count)
                return list[index];

            throw new ArgumentOutOfRangeException($"postinfor không có phần tử thứ {index}.");
        }
        //--------------hàm lấy thông tin người đăng------------------
        public async Task<(string name, string link)> GetPosterInfoBySelectorsAsync(IElementHandle container)
        {
            try
            {
                // 🧩 Thử selector chính (dạng span > a)
                var el = await container.QuerySelectorAsync("span[class='xjp7ctv'] > a");
                if (el != null)
                {
                    string name = (await el.InnerTextAsync())?.Trim() ?? "N/A";
                    string href = await el.GetAttributeAsync("href") ?? "N/A";
                    return (name, href);
                }

                // 🧩 Thử selector dự phòng (dạng span > span > span > a)
                var el2 = await container.QuerySelectorAsync("span[class='xjp7ctv'] > span > span > a");
                if (el2 != null)
                {
                    string name = (await el2.InnerTextAsync())?.Trim() ?? "N/A";
                    string href = await el2.GetAttributeAsync("href") ?? "N/A";
                    return (name, href);
                }
                

                throw new Exception("Không tìm thấy thẻ chứa thông tin người đăng.");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ GetPosterInfoBySelectorsAsync lỗi: {ex.Message}");
                return ("N/A", "N/A");
            }
        }
        // nếu k lấy được nội dung---thay thế div hàm dưới
        public async Task<string> GetContentTextAsync(IPage page, IElementHandle container)
        {
            // 🔹 Giữ nguyên selector như bản Selenium
            var contentEls = await container.QuerySelectorAllAsync("div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']");
            if (contentEls.Count == 0)
                throw new Exception("Không tìm thấy nội dung bài đăng.");

            // 🔸 Gọi hàm xử lý nội dung đầy đủ (lấy được text r xem có xem thêm)
            return await GetFullContentAsync(page, container);
        }
        // 🧩 Hàm mở “Xem thêm” và lấy nội dung bài đăng chính
        private async Task<string> GetFullContentAsync(IPage page, IElementHandle container)
        {
            if (container == null)
                return "N/A";

            var sb = new StringBuilder();

            try
            {
                // 🔹 Tìm nút "Xem thêm" trong vùng content
                var seeMoreBtn = await container.QuerySelectorAsync(
                    "div[role='button']:has-text('Xem thêm'), div[role='button']:has-text('See more')"
                );

                if (seeMoreBtn != null)
                {
                    // Cuộn vào trung tâm view
                    await page.EvaluateAsync("(el) => el.scrollIntoView({block:'center', behavior:'instant'})", seeMoreBtn);
                    await page.WaitForTimeoutAsync(250);

                    // Click tự nhiên + fallback JS
                    try
                    {
                        await seeMoreBtn.ScrollIntoViewIfNeededAsync();
                        await PageDAO.Instance.RandomDelayAsync(page, 250, 400);
                        await seeMoreBtn.ClickAsync();

                    }
                    catch
                    {
                        await page.EvaluateAsync("(el)=>{try{el.click();}catch(e){}}", seeMoreBtn);
                    }

                    await page.WaitForTimeoutAsync(300);
                }

                // 🔹 Sau khi click, lấy toàn bộ text trong vùng
                var spans = await container.QuerySelectorAllAsync("span[dir='auto']");
                foreach (var span in spans)
                {
                    var text = (await span.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[GroupsDAO_PW] GetFullContentAsync lỗi: " + ex.Message);
            }

            string content = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(content) ? "N/A" : content;
        }
        public async Task<string> GetBackgroundTextAsync(IElementHandle post)
        {
            try
            {
                // 🎯 Tìm vùng có nền màu hoặc ảnh nền
                var bgContainer = await post.QuerySelectorAsync("div[style*='background-color'], div[style*='background-image']");
                if (bgContainer == null)
                    return "N/A";

                var spans = await bgContainer.QuerySelectorAllAsync("span, div[dir='auto']");
                var sb = new StringBuilder();

                foreach (var span in spans)
                {
                    string text = (await span.InnerTextAsync())?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(text))
                        sb.Append(text + " ");
                }

                string result = sb.ToString().Trim();

                // Nếu vẫn trống, thử lấy textContent fallback
                if (string.IsNullOrWhiteSpace(result))
                {
                    var raw = (await bgContainer.GetPropertyAsync("textContent"))?.ToString();
                    result = raw?.Trim('"') ?? "";
                }

                if (string.IsNullOrWhiteSpace(result))
                    return "N/A";

                Libary.Instance.CreateLog($"✅ [GetBackgroundTextAsync] Lấy nội dung background text: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi GetBackgroundTextAsync: {ex.Message}");
                return "N/A";
            }
        }
        public async Task<string> GetReelTextAsync(IElementHandle post)
        {
            try
            {
                // 🎯 Tìm vùng chứa caption của reel/video
                var captionDiv = await post.QuerySelectorAsync("div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']");
                if (captionDiv == null)
                {
                    Libary.Instance.CreateLog("⚠️ Không tìm thấy vùng caption reel.");
                    return "N/A";
                }

                // 🔹 Lấy toàn bộ text (nhiều span nhỏ)
                var spans = await captionDiv.QuerySelectorAllAsync("span[dir='auto'], div[dir='auto']");
                var sb = new StringBuilder();

                foreach (var span in spans)
                {
                    string text = (await span.InnerTextAsync())?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(text))
                        sb.AppendLine(text);
                }

                string content = sb.ToString().Trim();

                // 🔹 Nếu text trống thì lấy luôn textContent toàn bộ div
                if (string.IsNullOrWhiteSpace(content))
                {
                    var raw = (await captionDiv.GetPropertyAsync("textContent"))?.ToString();
                    content = raw?.Trim('"') ?? "";
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    Libary.Instance.CreateLog("⚠️ Không lấy được nội dung caption reel.");
                    return "N/A";
                }

                Libary.Instance.CreateLog($"✅ [GetReelCaptionAsync] Caption reel: {content}");
                return content;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi GetReelCaptionAsync: {ex.Message}");
                return "N/A";
            }
        }

        //-------GetPostTextAsyn: LẤy nhanh Text, fallback
        public async Task<string> GetPostTextAsync(IElementHandle postinfor)
        {
            try
            {
                // 🎯 Thử tìm các div chứa nội dung caption gốc
                var textDivs = await postinfor.QuerySelectorAllAsync(
                    "div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']"
                );

                var sb = new StringBuilder();

                // ✅ Trường hợp tìm được div nội dung
                if (textDivs != null && textDivs.Count > 0)
                {
                    foreach (var div in textDivs)
                    {
                        string inner = (await div.InnerTextAsync())?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(inner))
                        {
                            sb.AppendLine(inner);
                        }
                    }
                }

                string result = sb.ToString().Trim();

                // ⚠️ Nếu không có text hoặc rỗng, thử fallback qua span[dir=auto]
                if (string.IsNullOrWhiteSpace(result))
                {
                    Libary.Instance.CreateLog("⚠️ Không có nội dung từ class xdj266r..., thử fallback span[dir=auto].");

                    var spanElements = await postinfor.QuerySelectorAllAsync("span[dir='auto']");
                    var spanText = new StringBuilder();

                    foreach (var span in spanElements)
                    {
                        string inner = (await span.InnerTextAsync())?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(inner))
                        {
                            // Loại bỏ các ký tự lặp không cần thiết
                            if (!spanText.ToString().Contains(inner))
                                spanText.AppendLine(inner);
                        }
                    }

                    result = spanText.ToString().Trim();
                }

                // ✅ Kiểm tra lần cuối
                if (string.IsNullOrWhiteSpace(result))
                {
                    return "N/A";
                }
                return result;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi GetPostTextAsync: {ex.Message}");
                return "N/A";
            }
        }
        public async Task<string> BackgroundTextAllAsync(IPage page, IElementHandle post)
        {
            var listText = new List<string>();

            // 1️⃣ Click "Xem thêm" (nếu có)
            try
            {
                var seeMoreBtn = (await post.QuerySelectorAllAsync("div[role='button']:has-text('Xem thêm'), div[role='button']:has-text('See more')"
                )).FirstOrDefault();

                if (seeMoreBtn != null)
                {
                    await seeMoreBtn.ScrollIntoViewIfNeededAsync();
                    await PageDAO.Instance.RandomDelayAsync(page,250, 500);

                    try
                    {
                        await seeMoreBtn.ClickAsync();
                    }
                    catch
                    {
                        await page.EvaluateAsync("el => el.click()", seeMoreBtn);
                    }

                    await PageDAO.Instance.RandomDelayAsync(page,600, 900);

                    // chờ phần text sau click xuất hiện
                    try
                    {
                        var expanded = await post.QuerySelectorAllAsync(
                            "div.html-div.xdj266r.x14z9mp.xat24cr.x1lziwak.xexx8yu.xyri2b.x18d9i69.x1c1uobl > div > div > div > span"
                        );
                        foreach (var e in expanded)
                        {
                            string text = (await e.InnerTextAsync())?.Trim();
                            if (!string.IsNullOrEmpty(text))
                            {
                                listText.Add(text);
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("BackgroundTextAllAsync -> lỗi click Xem thêm: " + ex.Message);
            }
            // 2️⃣ Lấy caption (sau khi click mở rộng)
            try
            {
                var captionSpans = await post.QuerySelectorAllAsync("div[class='html-div xdj266r x14z9mp xat24cr x1lziwak x1l90r2v xv54qhq xf7dkkf x1iorvi4']");

                foreach (var e in captionSpans)
                {
                    string txt = (await e.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrEmpty(txt) && !listText.Contains(txt))
                        listText.Add(txt);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("BackgroundTextAllAsync -> lỗi lấy caption: " + ex.Message);
            }
            // 3️⃣ Nền màu kiểu 1
            try
            {
                var bgElements = await post.QuerySelectorAllAsync(
                    "div[class='x1yx25j4 x13crsa5 x1rxj1xn x162tt16 x5zjp28'] > div"
                );
                foreach (var e in bgElements)
                {
                    string text = (await e.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrEmpty(text))
                        listText.Add(text);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("BackgroundTextAllAsync -> lỗi nền màu kiểu 1: " + ex.Message);
            }

            // 4️⃣ Ảnh nền kiểu 2 (alt có text)
            try
            {
                var bgImages = await post.QuerySelectorAllAsync(
                    "img[class='x15mokao x1ga7v0g x16uus16 xbiv7yw x1ey2m1c x5yr21d xtijo5x x1o0tod x10l6tqk x13vifvy xh8yej3 xl1xv1r']"
                );

                foreach (var img in bgImages)
                {
                    var alt = (await img.GetAttributeAsync("alt"))?.Trim();
                    if (!string.IsNullOrEmpty(alt))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(alt, "'([^']+)'");
                        if (match.Success) alt = match.Groups[1].Value.Trim();
                        listText.Add("(" + alt + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("BackgroundTextAllAsync -> lỗi alt ảnh: " + ex.Message);
            }

            // 5️⃣ Lọc trùng & nối text
            var distinctTexts = new List<string>();
            foreach (var t in listText)
            {
                if (!string.IsNullOrWhiteSpace(t) && !distinctTexts.Contains(t.Trim()))
                    distinctTexts.Add(t.Trim());
            }

            string fullcontent = string.Join(Environment.NewLine + Environment.NewLine, distinctTexts);

            // Làm sạch nội dung
            fullcontent = System.Text.RegularExpressions.Regex.Replace(fullcontent, @"\s+", " ").Trim();
            var cleanLines = fullcontent
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            fullcontent = string.Join(Environment.NewLine, cleanLines);
            return fullcontent;
        }
        private async Task<(string postTime, string postLink, string originalTime, string originalLink, string posterName, string posterLink)>ExtractFallback(IPage page, IElementHandle post)
        {
            string postTime = "N/A";
            string postLink = "N/A";
            string originalTime = "N/A";
            string originalLink = "N/A";
            string posterName = "N/A";
            string posterLink = "N/A";

            try
            {
                var linkElems = await post.QuerySelectorAllAsync("a[href]");
                var timeLinks = new List<(string text, string href)>();

                foreach (var a in linkElems)
                {
                    string href = await a.GetAttributeAsync("href") ?? "";
                    string text = (await a.InnerTextAsync() ?? "").Trim();
                    if (string.IsNullOrEmpty(href) || string.IsNullOrEmpty(text))
                        continue;

                    // ✅ Phát hiện link người đăng
                    bool isPosterLink =
                        href.Contains("/user/") ||
                        href.Contains("/people/") ||
                        href.Contains("/profile.php?id=") ||
                        href.Contains("/pages/");

                    if (isPosterLink && posterName == "N/A") // chỉ lấy người đầu tiên
                    {
                        string shortPosterLink = ProcessingHelper.ShortenFacebookPostLink(href);
                        posterName = text;
                        posterLink = shortPosterLink;
                    }

                    // ✅ Link bài đăng hợp lệ
                    bool isPostLink =
                        href.Contains("/posts/") ||
                        href.Contains("/permalink/") ||
                        href.Contains("/photos/") ||
                        href.Contains("/photo/") ||
                        href.Contains("/reel/");

                    if (!isPostLink)
                        continue;

                    // ⚙️ Bỏ qua các link nhiễu (trừ reel)
                    bool isReel = href.Contains("/reel/");
                    bool isVideoLink = href.Contains("video") && !isReel;
                    if (href.Contains("comment_id=") || href.Contains("/story/") ||
                        href.Contains("fbid=") || href.Contains("id=") || isVideoLink)
                        continue;

                    // ✅ Xác định text có phải thời gian
                    bool isTimeText =
                        text.ToLower().Contains("phút") ||
                        text.ToLower().Contains("giờ") ||
                        text.ToLower().Contains("hôm nay") ||
                        text.ToLower().Contains("hôm qua") ||
                        text.ToLower().Contains("ngày") ||
                        Regex.IsMatch(text, @"\d+\s*(phút|giờ|ngày)\s*trước", RegexOptions.IgnoreCase);

                    if (isTimeText)
                    {
                        string cleanText = TimeHelper.CleanTimeString(text);
                        string shortLink = ProcessingHelper.ShortenFacebookPostLink(href);
                        timeLinks.Add((cleanText, shortLink));
                    }
                }

                // 🎯 Phân loại theo số lượng link thời gian
                if (timeLinks.Count >= 1)
                {
                    postTime = timeLinks[0].text;
                    postLink = timeLinks[0].href;
                }

                if (timeLinks.Count >= 2)
                {
                    originalTime = timeLinks[1].text;
                    originalLink = timeLinks[1].href;
                } 
                if(timeLinks.Count == 0)
                {                  
                        (postTime, postLink) = await PageDAO.Instance.ExtractPostLinkByClickAsync(page, post);              
                }    
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ Lỗi ExtractPostTimeAndLinksAsync: " + ex.Message);
            }

            return (postTime, postLink, originalTime, originalLink, posterName, posterLink);
        }

    }
}
