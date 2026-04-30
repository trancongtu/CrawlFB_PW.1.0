using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;
namespace CrawlFB_PW._1._0.Helper
{
    public class ProcessingHelper
    {
       public static bool ContainsIgnoreCase(string source, string keyword)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(keyword))
                return false;

            return source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }
       
        // hàm lấy ID Post từ Link
        public static string ExtractPostIdFromLink(string link) 
        {
            if (string.IsNullOrWhiteSpace(link))
                return "N/A";

            try
            {
                // Dạng 1: .../posts/{id}
                var match = Regex.Match(link, @"/posts/(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value;

                // Dạng 2: .../videos/{id}
                match = Regex.Match(link, @"/videos/(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value;

                // Dạng 3: permalink.php?story_fbid={id}
                match = Regex.Match(link, @"story_fbid=(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value;

                // Dạng 4: watch/?v={id}
                match = Regex.Match(link, @"[?&]v=(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value;

                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }
       public static string ExtractPostIdFromUrl(string postUrl) 
       {
            if (string.IsNullOrWhiteSpace(postUrl))
                return null;

            try
            {
                postUrl = postUrl.Trim();

                // permalink.php?story_fbid=xxx&id=yyy
                var m1 = Regex.Match(postUrl, @"story_fbid=(\d+)", RegexOptions.IgnoreCase);
                if (m1.Success)
                    return m1.Groups[1].Value;

                // fbid=xxx (photo.php, video.php)
                var m2 = Regex.Match(postUrl, @"fbid=(\d+)", RegexOptions.IgnoreCase);
                if (m2.Success)
                    return m2.Groups[1].Value;

                // /posts/xxx
                var m3 = Regex.Match(postUrl, @"/posts/(\d+)", RegexOptions.IgnoreCase);
                if (m3.Success)
                    return m3.Groups[1].Value;

                // /videos/xxx
                var m4 = Regex.Match(postUrl, @"/videos/(\d+)", RegexOptions.IgnoreCase);
                if (m4.Success)
                    return m4.Groups[1].Value;

                // /reel/xxx
                var m5 = Regex.Match(postUrl, @"/reel/(\d+)", RegexOptions.IgnoreCase);
                if (m5.Success)
                    return m5.Groups[1].Value;

                // /groups/{gid}/posts/{pid}
                var m6 = Regex.Match(postUrl, @"/groups/\d+/posts/(\d+)", RegexOptions.IgnoreCase);
                if (m6.Success)
                    return m6.Groups[1].Value;
            }
            catch
            {
                // ignore
            }

            // ❗ pfbid → KHÔNG xử lý ở đây
            return null;
        }
       public static string ExtractPostIdFromHtml(string html)
       {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            string[] patterns =
            {
        "\"post_id\"\\s*:\\s*\"(\\d+)\"",
        "\"story_fbid\"\\s*:\\s*\"(\\d+)\"",
        "\"feedback\"\\s*:\\s*\\{[^}]*?\"id\"\\s*:\\s*\"(\\d+)\""
    };

            foreach (string p in patterns)
            {
                var m = Regex.Match(html, p, RegexOptions.IgnoreCase);
                if (m.Success)
                    return m.Groups[1].Value;
            }

            return null;
        }
       // Lấy ID từ Link
       public static string ExtractFacebookId(string url) 
       {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                // profile.php?id=123
                var m = System.Text.RegularExpressions.Regex.Match(
                    url, @"[?&]id=(\d+)"
                );
                if (m.Success)
                    return m.Groups[1].Value;

                // /groups/123456789/
                m = System.Text.RegularExpressions.Regex.Match(
                    url, @"/groups/(\d+)"
                );
                if (m.Success)
                    return m.Groups[1].Value;

                return null;
            }
            catch
            {
                return null;
            }
       }
       public static string ExtractIdFromUrl(string url)
       {
            // /profile.php?id=1000...
            var m1 = Regex.Match(url, "profile.php\\?id=(\\d+)");
            if (m1.Success) return m1.Groups[1].Value;

            // /groups/123456789
            var m2 = Regex.Match(url, "/groups/(\\d+)");
            if (m2.Success) return m2.Groups[1].Value;

            // /pages/123456789/abc
            var m3 = Regex.Match(url, "/pages/(\\d+)");
            if (m3.Success) return m3.Groups[1].Value;

            // /people/name/123456789
            var m4 = Regex.Match(url, "/people/.+?/(\\d+)");
            if (m4.Success) return m4.Groups[1].Value;

            return null;
        }
        // LẤy IDFB và Link FB người share từ Link Share
       public static (string idfb, string linkfb) ExtractFbInfoFromHrefShare(string link)
       {
            string idfb = "";
            string linkfb = "";

            int i = link.IndexOf("&id=");
            int j = link.IndexOf("&__");
            int k = link.IndexOf("?__");
            int t = link.IndexOf("/posts/");

            // Trường hợp có định dạng &id=...&__
            if ((i != -1) && (j != -1))
            {
                idfb = link.Substring(i + 4, j - i - 4);
                linkfb = "https://facebook.com/" + idfb;
            }

            // Trường hợp link có định dạng bài viết /posts/... kèm ?__
            if ((k != -1) && (t != -1))
            {
                linkfb = link.Substring(0, t);
            }

            return (idfb, linkfb);
        }
       public static string ExtractFbShortLink(string url)
       {
            // Kiểm tra nếu có ID
            var matchId = Regex.Match(url, @"id=(\d+)");
            if (matchId.Success)
            {
                return $"https://facebook.com/{matchId.Groups[1].Value}";
            }

            // Nếu không có ID, lấy tên rút gọn
            var matchShortName = Regex.Match(url, @"facebook\.com/([^/?]+)");
            if (matchShortName.Success)
            {
                return $"https://facebook.com/{matchShortName.Groups[1].Value}";
            }

            return "Invalid URL";
        }
        // Từ Link Share ra ID FB người share
        public static string HrefShareGroupsToIdFb(string link) 
       {

            string idfb = "";

            int i = link.IndexOf("/groups/");
            int k = link.IndexOf("?__");
            int t = link.IndexOf("/user/");
            if ((i != -1) && (k != -1) && (t != -1))
            {
                idfb = link.Substring(t + 6, k - t - 7);
            }
            return idfb;
       }
        //xử lý ảnh Video kèm theo
        // Lấy ID từ DOM Full hơn
        public static string ExtractFacebookObjectIdFromHtml(string html,string rawUrl)
        {
            try
            {
                // ===============================
                // 0️⃣ ƯU TIÊN: ID CÓ SẴN TRONG URL
                // ===============================
                if (!string.IsNullOrWhiteSpace(rawUrl))
                {
                    var m = Regex.Match(rawUrl, @"/groups/(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) return m.Groups[1].Value;

                    m = Regex.Match(rawUrl, @"[?&]id=(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) return m.Groups[1].Value;

                    m = Regex.Match(rawUrl, @"/user/(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) return m.Groups[1].Value;

                    m = Regex.Match(rawUrl, @"/people/.+?/(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) return m.Groups[1].Value;
                }

                if (string.IsNullOrWhiteSpace(html))
                    return "";

                // ===============================
                // 1️⃣ GROUP → groupID
                // ===============================
                var match = Regex.Match(
                    html,
                    "\"groupID\"\\s*:\\s*\"?(\\d+)\"?",
                    RegexOptions.IgnoreCase);

                if (match.Success)
                    return match.Groups[1].Value;

                // ===============================
                // 2️⃣ PAGE / PERSON → selectedID
                // ===============================
                match = Regex.Match(
                    html,
                    "\"selectedID\"\\s*:\\s*\"(\\d+)\"",
                    RegexOptions.IgnoreCase);

                if (match.Success)
                    return match.Groups[1].Value;

                // ===============================
                // 3️⃣ PAGE / PERSON → userID
                // ===============================
                match = Regex.Match(
                    html,
                    "\"userID\"\\s*:\\s*\"?(\\d+)\"?",
                    RegexOptions.IgnoreCase);

                if (match.Success)
                    return match.Groups[1].Value;
            }
            catch
            {
                // helper THUẦN → không log ở đây
            }

            return "";
        }

        public static string DetectFileType(string url) 
       {
            if (string.IsNullOrWhiteSpace(url))
                return "unknown";

            url = url.ToLower();
            if (url.Contains(".jpg") || url.Contains(".jpeg") || url.Contains(".png") || url.Contains(".gif"))
                return "image";
            if (url.Contains(".mp4") || url.Contains(".mov") || url.Contains(".avi"))
                return "video";
            if (url.StartsWith("http"))
                return "link";
            return "unknown";
        }
        // chuẩn hóa url
        public static string NormalizeFacebookUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "";

            url = url.Trim().ToLower();

            // Bỏ http, https, www
            url = Regex.Replace(url, @"^https?://", "", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"^www\.", "", RegexOptions.IgnoreCase);

            // 🔥 CHUẨN DOMAIN VỀ facebook.com
            url = Regex.Replace(url, @"^fb\.com", "facebook.com", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"^m\.facebook\.com", "facebook.com", RegexOptions.IgnoreCase);
            url = Regex.Replace(url, @"^web\.facebook\.com", "facebook.com", RegexOptions.IgnoreCase);

            // Tách query
            string path = url;
            string query = "";

            int qIndex = url.IndexOf("?");
            if (qIndex >= 0)
            {
                path = url.Substring(0, qIndex);
                query = url.Substring(qIndex);
            }

            // giữ query cho profile.php
            if (path.EndsWith("profile.php"))
            {
                return "https://" + path + query;
            }

            // bỏ hash
            int h = path.IndexOf("#");
            if (h >= 0)
                path = path.Substring(0, h);

            path = path.TrimEnd('/');

            return "https://" + path;
        }
        // sử dụng trong scancheckpageDAO
        public  static string ExtractPageInfoFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return null;

            // Regex tìm div.xieb3on
            var match = Regex.Match(html,
                "<div[^>]*class=\"[^\"]*xieb3on[^\"]*\"[^>]*>(.*?)</div>",
                RegexOptions.Singleline);

            if (!match.Success)
                return null;

            string raw = match.Groups[1].Value;

            // Loại bỏ HTML tags bên trong
            string text = Regex.Replace(raw, "<.*?>", "").Trim();

            return text;
        }

        ////================= các hàm shorten
        public static string NormalizeInputUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "";

            url = url.Trim();

            // 1️⃣ Nếu chỉ nhập số → profile/page ID
            if (Regex.IsMatch(url, @"^\d+$"))
                return $"https://facebook.com/{url}";

            // 2️⃣ Thêm scheme nếu thiếu
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = "https://" + url;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return "";

            // 3️⃣ Chuẩn hoá domain
            string host = uri.Host
                .Replace("www.", "")
                .Replace("m.facebook.com", "facebook.com")
                .Replace("web.facebook.com", "facebook.com");


            // 4️⃣ GIỮ QUERY cho profile.php
            if (uri.AbsolutePath.Equals("/profile.php", StringComparison.OrdinalIgnoreCase))
            {
                return $"https://{host}{uri.AbsolutePath}{uri.Query}";
            }

            // 5️⃣ Các link còn lại → bỏ query + hash
            string path = uri.AbsolutePath.TrimEnd('/');

            return $"https://{host}{path}";
        }
        public static string ShortLinkPage(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return "";

            rawUrl = rawUrl.Trim();

            // =========================
            // FIX 1️⃣: LINK RELATIVE
            // =========================
            if (rawUrl.StartsWith("/"))
            {
                rawUrl = "https://facebook.com" + rawUrl;
            }
            else if (rawUrl.StartsWith("groups/", StringComparison.OrdinalIgnoreCase))
            {
                rawUrl = "https://facebook.com/" + rawUrl;
            }

            try
            {
                var uri = new Uri(rawUrl);

                // Normalize host
                string host = uri.Host
                    .Replace("web.facebook.com", "facebook.com")
                    .Replace("m.facebook.com", "facebook.com")
                    .Replace("www.facebook.com", "facebook.com");

                string path = uri.AbsolutePath.TrimEnd('/');
                string query = uri.Query;

                // =========================
                // CASE 0: PROFILE ID
                // =========================
                if (path.Equals("/profile.php", StringComparison.OrdinalIgnoreCase))
                {
                    var m = Regex.Match(query, @"[?&]id=(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        string id = m.Groups[1].Value;

                        string finalHost = host;
                        if (string.IsNullOrWhiteSpace(finalHost))
                            finalHost = "facebook.com";

                        finalHost = finalHost
                            .Replace("www.facebook.com", "facebook.com")
                            .Replace("m.facebook.com", "facebook.com")
                            .Replace("web.facebook.com", "facebook.com");

                        return $"https://{finalHost}/profile.php?id={id}";
                    }
                }

                // =========================
                // CASE 1: GROUP
                // =========================
                if (path.StartsWith("/groups/", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = path.Split('/');
                    if (parts.Length >= 3)
                    {
                        string groupId = parts[2];
                        return $"https://{host}/groups/{groupId}";
                    }
                }

                // =========================
                // CASE 2: FANPAGE / PROFILE
                // =========================
                var segments = path.Split(
                    new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length >= 1)
                {
                    string user = segments[0].ToLower();

                    string[] invalid =
                    {
                "posts", "watch", "events", "stories",
                "photo", "videos", "reel", "marketplace"
            };

                    if (!invalid.Contains(user))
                    {
                        return $"https://facebook.com/{segments[0]}";
                    }
                }

                // fallback
                return $"https://{host}{path}";
            }
            catch
            {
                return rawUrl;
            }
        }
        public static string ShortLinkPageOriginal(string href)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(href) ||
                    href == "N/A" ||
                    href == "#" ||
                    href.StartsWith("javascript", StringComparison.OrdinalIgnoreCase))
                    return "N/A";

                href = href.Trim();

                // relative -> absolute
                if (href.StartsWith("/"))
                    href = "https://facebook.com" + href;

                href = href.Replace("www.facebook.com", "facebook.com")
                           .Replace("m.facebook.com", "facebook.com")
                           .Replace("web.facebook.com", "facebook.com");

                if (!Uri.TryCreate(href, UriKind.Absolute, out var uri))
                    return href;

                string host = "facebook.com";
                string path = uri.AbsolutePath;
                string query = uri.Query ?? "";

                // profile.php?id=...
                if (path.Equals("/profile.php", StringComparison.OrdinalIgnoreCase))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(
                        query,
                        @"[?&]id=(\d+)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (m.Success)
                        return $"https://{host}/profile.php?id={m.Groups[1].Value}";
                }

                // groups
                var segs = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (segs.Length >= 2 && segs[0].Equals("groups", StringComparison.OrdinalIgnoreCase))
                    return $"https://{host}/groups/{segs[1]}";

                // username/page slug
                if (segs.Length >= 1)
                    return $"https://{host}/{segs[0]}";

                return href;
            }
            catch
            {
                return href;
            }
        }
        public static string ShortenFacebookPostLink(string originalLink)
        {
            if (string.IsNullOrWhiteSpace(originalLink))
                return "N/A";

            try
            {
                originalLink = originalLink.Trim();          
                // CASE: permalink.php => giữ story_fbid + id, cắt bỏ toàn bộ rác sau &__cft__
                if (originalLink.Contains("permalink.php") && originalLink.Contains("story_fbid"))
                {
                    int idx = originalLink.IndexOf("&__cft__");
                    if (idx != -1)
                    {
                        originalLink = originalLink.Substring(0, idx); // cắt toàn bộ rác
                    }

                    return originalLink.TrimEnd('&');   // tránh dư ký tự &
                }

                // CASE NORMAL POST => cắt query bình thường
                int qIndex = originalLink.IndexOf("?");
                if (qIndex != -1)
                    originalLink = originalLink.Substring(0, qIndex);

                return originalLink;
            }
            catch
            {
                return originalLink;
            }
        }        
        public static string ShortenPosterLink(string href)
        {
            try
            {
                // 1️⃣ Chặn toàn bộ invalid
                if (string.IsNullOrWhiteSpace(href) ||
                    href == "N/A" ||
                    href == "#" ||
                    href.StartsWith("javascript"))
                {
                    return "N/A";
                }

                href = href.Trim();

                // 2️⃣ Chỉ cho phép duy nhất dạng /user/xxxx/
                if (href.Contains("/user/"))
                {
                    int start = href.IndexOf("/user/") + 6;
                    int end = href.IndexOfAny(new[] { '/', '?', '&' }, start);
                    if (end == -1) end = href.Length;

                    string id = href.Substring(start, end - start);

                    // validate id
                    if (!string.IsNullOrWhiteSpace(id))
                        return $"https://facebook.com/{id}";
                }

                // 3️⃣ Các loại link khác → trả N/A hết
                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }
        public static string ShortenPosterLinkReel(string href)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(href) ||
                    href == "N/A" ||
                    href == "#" ||
                    href.StartsWith("javascript"))
                {
                    return "N/A";
                }

                href = href.Trim();

                // Tìm /user/
                int userIndex = href.IndexOf("/user/");
                if (userIndex == -1)
                    return "N/A";

                // Vị trí bắt đầu ID
                int idStart = userIndex + 6;

                // Duyệt thủ công từ idStart đến hết chuỗi để lấy toàn bộ các ký tự dạng số
                var sb = new StringBuilder();
                while (idStart < href.Length && char.IsDigit(href[idStart]))
                {
                    sb.Append(href[idStart]);
                    idStart++;
                }
                string id = sb.ToString();

                if (string.IsNullOrWhiteSpace(id))
                    return "N/A";

                return $"https://facebook.com/{id}";
            }
            catch
            {
                return "N/A";
            }
        }
        public static string NormalizePersonProfileLink(string href)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(href))
                    return null;

                href = href.Trim();

                if (href == "#" || href.StartsWith("javascript", StringComparison.OrdinalIgnoreCase))
                    return null;

                // Chuẩn domain
                href = href
                    .Replace("m.facebook.com", "www.facebook.com")
                    .Replace("web.facebook.com", "www.facebook.com");

                // Nếu là profile.php?id=xxxx → chuyển sang dạng /xxxx
                var m = Regex.Match(href, @"profile\.php\?id=(\d+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    return $"https://www.facebook.com/{m.Groups[1].Value}";
                }

                // Parse URI
                if (!Uri.TryCreate(href, UriKind.Absolute, out var uri))
                    return href;

                var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);


                if (segments.Length == 0)
                    return href;

                string first = segments[0];

                // ❌ loại các path không phải profile person
                string[] invalid =
                {
            "groups", "pages", "watch", "reel", "videos",
            "photo", "photos", "posts", "events", "stories"
        };

                if (invalid.Contains(first, StringComparer.OrdinalIgnoreCase))
                {
                    // Không phải profile person → trả link gốc
                    return href;
                }

                // ✅ profile person (username hoặc username.id)
                return $"https://Facebook.com/{first}";
            }
            catch
            {
                // Fallback tuyệt đối an toàn
                return href;
            }
        }
        // các hàm string
        // hàm in 100 ký tự thôi
        public static string PreviewText(string text, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "(Không có nội dung)";

            text = text.Trim();

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }
        // Parse số từ các chuỗi Facebook như "1,2K", "5 bình luận", "N/A"
        public static int ParseFacebookNumber(string text)
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
                    double.TryParse(text, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double v);
                    return (int)(v * 1000);
                }
                if (text.Contains("m"))
                {
                    text = text.Replace("m", "").Replace(",", ".").Trim();
                    double.TryParse(text, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double v);
                    return (int)(v * 1000000);
                }

                var digits = new string(text.Where(char.IsDigit).ToArray());
                int.TryParse(digits, out int r);
                return r;
            }
            catch
            {
                return 0;
            }
        }
        // chuẩn hóa nhận link đầu vào
        public static bool IsValidPostPath(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return false;

            href = href.ToLowerInvariant();

            // ❌ loại mấy link linh tinh
            if (href.StartsWith("#") ||
                href.Contains("comment_id") ||
                href.Contains("reply_comment_id"))
                return false;

            // ✅ CHỈ NHẬN LINK CÓ PATH BÀI VIẾT
            return
                href.Contains("/posts/") ||
                href.Contains("/reel/") ||
                href.Contains("/videos/") ||
                href.Contains("/watch/") ||
                href.Contains("/photo/") ||
                href.Contains("permalink.php") ||
                href.Contains("/story.php");
        }
        // chuyển đối string -> enum
        public static PostType MapPostType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return PostType.Page_Unknow;

            raw = raw.Trim();

            // 1️⃣ Parse thẳng nếu DB lưu đúng tên enum
            if (Enum.TryParse<PostType>(raw, true, out var exact))
                return exact;

            // 2️⃣ Fallback theo nội dung (phòng DB bẩn)
            string s = raw.ToLower();

            if (s.Contains("share"))
                return s.Contains("content")
                    ? PostType.Share_WithContent
                    : PostType.Share_NoContent;

            if (s.Contains("reel"))
                return s.Contains("cap")
                    ? PostType.page_Real_Cap
                    : PostType.Page_Reel_NoCap;

            if (s.Contains("video"))
                return s.Contains("cap")
                    ? PostType.Page_Video_Cap
                    : PostType.Page_Video_Nocap;

            if (s.Contains("photo") || s.Contains("pho"))
                return s.Contains("cap")
                    ? PostType.Page_Photo_Cap
                    : PostType.Page_Photo_NoCap ;

            if (s.Contains("background"))
                return PostType.Page_BackGround;

            if (s.Contains("link"))
                return PostType.Page_LinkWeb;

            if (s.Contains("nocontent"))
                return PostType.Page_NoConent;

            return PostType.Page_Normal;
        }
        public static string GetPostTypeView(PostType type)
        {
            switch (type)
            {
                case PostType.Page_Normal: return "Bài thường";
                case PostType.Page_Photo_Cap: return "Ảnh có cap";
                case PostType.Page_Photo_NoCap: return "Ảnh không cap";
                case PostType.Page_NoConent: return "Không nội dung";
                case PostType.Share_WithContent: return "Share có nội dung";
                case PostType.Share_NoContent: return "Share không nội dung";
                case PostType.Page_Video_Cap: return "Video có cap";
                case PostType.Page_Video_Nocap: return "Video không cap";
                case PostType.page_Real_Cap: return "Reel có cap";
                case PostType.Page_Reel_NoCap: return "Reel không cap";
                case PostType.Page_BackGround: return "Background";
                case PostType.Page_LinkWeb: return "Link web";
                default: return "Không rõ";
            }
        }
        //REEL
        public static string NormalizeReelLink(string reelLink)
        {
            if (string.IsNullOrWhiteSpace(reelLink))
                return "";

            reelLink = reelLink.Trim().ToLower();

            // đã full link → normalize domain luôn
            if (reelLink.StartsWith("http://") || reelLink.StartsWith("https://"))
                return NormalizeFacebookUrl(reelLink);

            // dạng /reel/xxx
            if (reelLink.StartsWith("/reel/"))
                return "https://facebook.com" + reelLink;

            // dạng reel/xxx
            if (reelLink.StartsWith("reel/"))
                return "https://facebook.com/" + reelLink;

            // fallback
            return NormalizeFacebookUrl(reelLink);
        }
        public static string NormalizeReelPosterProfile(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return "N/A";

            href = href.Trim();

            if (href.StartsWith("/"))
                href = "https://facebook.com" + href;

            // bỏ &sk=...
            int sk = href.IndexOf("&sk=", StringComparison.OrdinalIgnoreCase);
            if (sk > 0)
                href = href.Substring(0, sk);

            return NormalizePersonProfileLink(href);
        }

        // kiểm tra dữ liệu rỗng hay N.a k
        public static bool IsValidContent(string content)
        {
            return !string.IsNullOrWhiteSpace(content) && content != "N/A";
        }
        // MAPTYPE
        public static FBType MapPageTypeToFBType(string pageType)
        {
            if (string.IsNullOrWhiteSpace(pageType))
                return FBType.Unknown;

            pageType = pageType.Trim().ToLowerInvariant();

            // GROUP
            if (pageType.Contains("group"))
            {
                if (pageType.Contains("off"))
                    return FBType.GroupOff;

                return FBType.GroupOn;
            }

            // FANPAGE / PAGE
            if (pageType.Contains("page") ||
                pageType.Contains("fan"))
            {
                return FBType.Fanpage;
            }

            return FBType.Unknown;
        }
        public static FBType MapPersonNoteToFBType(string personNote)
        {
            if (string.IsNullOrWhiteSpace(personNote))
                return FBType.Unknown;

            personNote = personNote.Trim().ToLowerInvariant();

            // KOL / CREATOR
            if (personNote.Contains("kol") ||
                personNote.Contains("creator"))
            {
                return FBType.PersonKOL;
            }

            // PERSON
            if (personNote.Contains("person"))
            {
                return FBType.Person;
            }

            return FBType.Unknown;
        }
        // hàn cho commetn
        public static string NormalizeCommentActorProfileLink(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return "";

            string url = rawUrl.Trim();

            // =========================
            // 1️⃣ Prefix domain nếu là relative
            // =========================
            if (url.StartsWith("/"))
                url = "https://www.facebook.com" + url;

            // =========================
            // 2️⃣ Bỏ query rác
            // =========================
            int q = url.IndexOf("?", StringComparison.Ordinal);
            if (q > 0)
                url = url.Substring(0, q);

            // =========================
            // 3️⃣ CASE: /groups/{gid}/user/{uid}
            // =========================
            int idxUser = url.IndexOf("/user/", StringComparison.OrdinalIgnoreCase);
            if (idxUser >= 0)
            {
                string uid = url.Substring(idxUser + "/user/".Length);

                int slash = uid.IndexOf("/", StringComparison.Ordinal);
                if (slash > 0)
                    uid = uid.Substring(0, slash);

                if (!string.IsNullOrWhiteSpace(uid))
                    return "https://www.facebook.com/profile.php?id=" + uid;
            }

            // =========================
            // 4️⃣ CASE: profile.php?id=...
            // =========================
            if (url.IndexOf("profile.php", StringComparison.OrdinalIgnoreCase) >= 0)
                return url;

            // =========================
            // 5️⃣ CASE: username (/abc.xyz)
            // loại trừ group / page
            // =========================
            bool isGroup =
                url.IndexOf("/groups/", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isPage =
                url.IndexOf("/pages/", StringComparison.OrdinalIgnoreCase) >= 0
                || url.IndexOf("/community/", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isGroup && !isPage)
                return url;

            // =========================
            // 6️⃣ FALLBACK
            // =========================
            return url;
        }

    }
}
