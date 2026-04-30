using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Playwright;
namespace CrawlFB_PW._1._0.DAO
{
    public class ProcessingDAO
    {
        public static ProcessingDAO instance;
        public static ProcessingDAO Instance
        {
            get { if (instance == null) instance = new ProcessingDAO(); return ProcessingDAO.instance; }
            private set { ProcessingDAO.instance = value; }
        }
        // hàm lấy ID Post từ Link
        public string ExtractPostIdFromLink(string link)
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
        // lấy id từ link có id
        public string ExtractFacebookId(string url)
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

        public string ExtractFbShortLink(string url)
        {
            // Kiểm tra nếu có ID
            var matchId = Regex.Match(url, @"id=(\d+)");
            if (matchId.Success)
            {
                return $"https://fb.com/{matchId.Groups[1].Value}";
            }

            // Nếu không có ID, lấy tên rút gọn
            var matchShortName = Regex.Match(url, @"facebook\.com/([^/?]+)");
            if (matchShortName.Success)
            {
                return $"https://fb.com/{matchShortName.Groups[1].Value}";
            }

            return "Invalid URL";
        }
        public string ShortLinkPage(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return "";

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

                // ⭐ CASE 0: PROFILE ID (profile.php?id=...)
                if (path.Equals("/profile.php", StringComparison.OrdinalIgnoreCase)
                    && query.Contains("id="))
                {
                    return $"https://{host}/profile.php{query}";
                }

                // ⭐ CASE 1: GROUP
                if (path.StartsWith("/groups/", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = path.Split('/');
                    if (parts.Length >= 3)
                    {
                        string groupId = parts[2];
                        return $"https://facebook.com/groups/{groupId}";
                    }
                }

                // ⭐ CASE 2: FANPAGE / PROFILE USERNAME
                var segments = path.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length >= 1)
                {
                    string user = segments[0].ToLower();

                    string[] invalid =
                    {
                "posts", "watch", "events", "stories", "photo",
                "videos", "reel", "marketplace"
            };

                    if (!invalid.Contains(user))
                    {
                        return $"https://fb.com/{segments[0]}";
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
      
        // 2 hàm dưới dùng thống kê share
        public string HrefShareGroupsToIdFb(string link)
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
        public (string idfb, string linkfb) ExtractFbInfoFromHrefShare(string link)
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
                linkfb = "https://Fb.com/" + idfb;
            }

            // Trường hợp link có định dạng bài viết /posts/... kèm ?__
            if ((k != -1) && (t != -1))
            {
                linkfb = link.Substring(0, t);
            }

            return (idfb, linkfb);
        }
        public void ShowError(string message, string title = "Lỗi")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public bool IsPostInTimeRange(string postTime, int soNgay)
        {
            if (string.IsNullOrWhiteSpace(postTime))
                return false;

            string time = postTime.ToLower().Trim();

            // 1️⃣ Trong ngày (phút, giờ, hôm nay)
            if (time.Contains("phút") || time.Contains("giờ") || time.Contains("hôm nay"))
                return true;

            // 2️⃣ Dạng "x ngày"
            var match = System.Text.RegularExpressions.Regex.Match(time, @"(\d+)\s*ngày");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int dayNum))
            {
                int maxDay = soNgay == 7 ? 6 : soNgay;
                return dayNum <= maxDay;
            }

            // 3️⃣ Dạng "26 tháng 10" hoặc "26/10"
            if (time.Contains("tháng") || time.Contains("/"))
            {
                try
                {
                    var numbers = System.Text.RegularExpressions.Regex.Matches(time, @"\d+")
                        .Cast<System.Text.RegularExpressions.Match>()
                        .Select(m => int.Parse(m.Value))
                        .ToList();

                    if (numbers.Count >= 2)
                    {
                        int dayPart = numbers[0];
                        int monthPart = numbers[1];
                        int year = DateTime.Now.Year;

                        if (monthPart < 1 || monthPart > 12) return false;

                        DateTime date;
                        try { date = new DateTime(year, monthPart, dayPart); }
                        catch { return false; }

                        if (date > DateTime.Now)
                            date = date.AddYears(-1);

                        double diff = (DateTime.Now - date).TotalDays;
                        return diff <= soNgay;
                    }
                }
                catch { return false; }
            }

            return false;
        }
        //các hàm xử lý thời gian
       
        /*public DateTime ParseFacebookTime(string timeText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(timeText))
                    return DateTime.MinValue;

                string lower = timeText.ToLower().Trim();

                if (lower.Contains("phút"))
                    return DateTime.Now.AddMinutes(-GetNumber(lower));
                if (lower.Contains("giờ"))
                    return DateTime.Now.AddHours(-GetNumber(lower));
                if (lower.Contains("hôm nay"))
                    return DateTime.Now;
                if (lower.Contains("ngày"))
                    return DateTime.Now.AddDays(-GetNumber(lower));

                var match = Regex.Match(lower, @"(\d{1,2})\s*tháng\s*(\d{1,2})(?:\s*lúc\s*(\d{1,2})[:h](\d{0,2}))?");
                if (match.Success)
                {
                    int day = int.Parse(match.Groups[1].Value);
                    int month = int.Parse(match.Groups[2].Value);
                    int hour = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
                    int minute = match.Groups[4].Success && match.Groups[4].Value != "" ? int.Parse(match.Groups[4].Value) : 0;

                    int year = DateTime.Now.Year;
                    if (month > DateTime.Now.Month)
                        year--; // nếu tháng lớn hơn hiện tại => bài cũ năm trước

                    return new DateTime(year, month, day, hour, minute, 0);
                }

                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }*/
        public int ConvertToDays(string sel)
        {
            if (string.IsNullOrEmpty(sel)) return 7;
            sel = sel.ToLower();
            if (sel.Contains("hôm nay") || sel.Contains("1 ngày")) return 1;
            if (sel.Contains("1 tuần") || sel.Contains("7 ngày")) return 7;
            if (sel.Contains("1 tháng") || sel.Contains("30 ngày")) return 30;
            int v;
            if (int.TryParse(new string(sel.Where(char.IsDigit).ToArray()), out v)) return v;
            return 7;
        }     
       
        public bool HasTime(string parsedTime) // kiểm tra xem có dấu : k 
        {
            if (string.IsNullOrWhiteSpace(parsedTime))
                return false;

            return parsedTime.Contains(":");   // có giờ nếu có dấu :
        }
        public bool IsOldPost(DateTime? crawlTime, DateTime? lastPostTime)
        {
            // Không có mốc so sánh → coi là mới
            if (!lastPostTime.HasValue)
                return false;

            // Không parse được thời gian crawl → coi là mới
            if (!crawlTime.HasValue)
                return false;

            DateTime newest = lastPostTime.Value;
            DateTime current = crawlTime.Value;

            // xác định "không có giờ" = 00:00:00
            bool crawlHasTime = current.TimeOfDay != TimeSpan.Zero;
            bool newestHasTime = newest.TimeOfDay != TimeSpan.Zero;

            // 1️⃣ Nếu 1 trong 2 KHÔNG có giờ → so sánh NGÀY
            if (!crawlHasTime || !newestHasTime)
            {
                return current.Date < newest.Date;
            }

            // 2️⃣ Nếu cả 2 CÓ giờ → so sánh full DateTime
            return current < newest;
        }


        public bool IsTime(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
                return false;

            string lower = txt.ToLower().Trim();

            string[] keywords =
            {
        "giờ", "phút", "ngày", "tháng", "năm",
        "lúc", "hôm nay", "hôm qua",
        "today", "yesterday"
    };

            foreach (var k in keywords)
            {
                if (lower.Contains(k))
                    return true;
            }

            // Regex: dạng "2 giờ", "15 phút", "1 ngày", "3 tháng", "5 năm"
            if (Regex.IsMatch(lower, @"\d+\s*(phút|giờ|ngày|tháng|năm)", RegexOptions.IgnoreCase))
                return true;

            // Regex: dạng "lúc 3:45" , "3:50 PM"
            if (Regex.IsMatch(lower, @"(lúc\s*\d+)|(\d{1,2}:\d{2})", RegexOptions.IgnoreCase))
                return true;

            return false;
        }
        //------------------//
        //Lấy ID sau /User/
        public string ShortenPosterLink(string href)
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
        public string ShortenPosterLinkReel(string href)
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
        public string NormalizePersonProfileLink(string href)
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

                var segments = uri.AbsolutePath .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);


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
                return $"https://Fb.com/{first}";
            }
            catch
            {
                // Fallback tuyệt đối an toàn
                return href;
            }
        }
        public string ExtractPostIdFromHtml(string html)
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
        public string ExtractPostIdFromUrl(string postUrl)
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
        public async Task ScrollToLoadPostsAsync(IPage page, int maxRounds)
        {
            int lastCount = 0;
            int noNewCount = 0;

            for (int i = 0; i < maxRounds; i++)
            {
                var posts = await page.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");
                int current = posts.Count;

                // 🧭 Kéo xuống với tốc độ trong AppConfig
                await page.Mouse.WheelAsync(0, AppConfig.ScrollSpeedMs);
                int wait1 = new Random().Next(AppConfig.ScrollWaitMinMs, AppConfig.ScrollWaitMaxMs);
                await Task.Delay(wait1);

                if (current > lastCount)
                {
                    lastCount = current;
                    noNewCount = 0;

                    // Có bài mới → nghỉ ngắn để Facebook load ổn định
                    int wait2 = new Random().Next(AppConfig.ScrollWaitMinMs, AppConfig.ScrollWaitMaxMs);
                    Libary.Instance.CreateLog($"🧩 Scroll round {i + 1}: phát hiện thêm bài mới ({current}), chờ {wait2}ms...");
                    await Task.Delay(wait2);
                }
                else
                {
                    noNewCount++;
                    Libary.Instance.CreateLog($"⚠️ Scroll round {i + 1}: không thấy bài mới ({noNewCount}/{AppConfig.ScrollMaxNoNewRounds})...");

                    // Nếu quá số vòng không có bài mới → dừng
                    if (noNewCount >= AppConfig.ScrollMaxNoNewRounds)
                    {
                        Libary.Instance.CreateLog("⏹️ Dừng scroll vì không có bài mới.");
                        break;
                    }
                }
            }
        }
        //giả lập người dùng click
        public async Task HumanScrollAndClickAsync(IPage page, IElementHandle element, string desc = "")
        {
            try
            {
                await page.EvaluateAsync("(el) => el.scrollIntoView({block:'center', behavior:'instant'})", element);

                int offset = new Random().Next(30, 60);
                bool down = new Random().Next(0, 2) == 0;

                await page.Mouse.WheelAsync(0, down ? offset : -offset);
                await page.WaitForTimeoutAsync(150);
                await page.Mouse.WheelAsync(0, down ? -offset / 2 : offset / 2);

                int delay = new Random().Next(200, 400);
                await page.WaitForTimeoutAsync(delay);

                await element.ClickAsync();
                Libary.Instance.CreateLog($"🖱️ Click {desc} (cuộn và delay {delay}ms)");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ HumanScrollAndClick lỗi {desc}: {ex.Message}");
            }
        }
        //giả lập người dùng k click
        public async Task HumanScrollAsync(IPage page)
        {
            try
            {
                var rnd = new Random();

                // scroll nhẹ xuống
                int offset1 = rnd.Next(200, 400);
                await page.Mouse.WheelAsync(0, offset1);
                await page.WaitForTimeoutAsync(rnd.Next(200, 350));

                // scroll xuống nữa
                int offset2 = rnd.Next(300, 600);
                await page.Mouse.WheelAsync(0, offset2);
                await page.WaitForTimeoutAsync(rnd.Next(250, 400));

                // scroll lên lại một đoạn
                int offset3 = rnd.Next(150, 350);
                await page.Mouse.WheelAsync(0, -offset3);
                await page.WaitForTimeoutAsync(rnd.Next(200, 350));

                Libary.Instance.CreateLog($"🖱️ HumanScroll simulated: {offset1}, {offset2}, -{offset3}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ HumanScroll Error: {ex.Message}");
            }
        }

        // hàm xử lý thời gian post để so sánh  
        private int GetNumber(string input)
        {
            var digits = new string(input.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int val) ? val : 1;
        }
        //----------------xử lý ảnh video kèm theo
        public string DetectFileType(string url)
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
        // hàm in 100 ký tự thôi
        public string PreviewText(string text, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "(Không có nội dung)";

            text = text.Trim();

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }
        // Lấy ID từ DOM
        public async Task<string> ExtractProfileIdFromSelectedIdAsync(IPage page)
        {
            try
            {
                // Lấy toàn bộ HTML sau khi DOM load
                string html = await page.ContentAsync();
                if (string.IsNullOrEmpty(html))
                    return "";

                // Regex tìm "selectedID":"123456"
                var match = Regex.Match(
                    html,
                    "\"selectedID\"\\s*:\\s*\"(\\d+)\"",
                    RegexOptions.IgnoreCase
                );

                if (match.Success)
                {
                    Libary.Instance.LogDebug($"{Libary.IconOK} OK ID FB {match.Groups[1].Value}");
                    return match.Groups[1].Value;
                    
                }
                return "";
            }
            catch
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} Không lấy được ID FB");
                return "";
               
            }
        }
     
        public string ExtractIdFromUrl(string url)
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
        // xử lý content
        // human trong auto
        public async Task HumanScrollRefreshAsync(IPage page, bool goTop = true)
        {
            var rnd = new Random();

            if (goTop && rnd.NextDouble() < 0.6)
            {
                // 🔼 về đầu trang (giống user check bài mới)
                await page.Keyboard.PressAsync("Home");
                await page.WaitForTimeoutAsync(rnd.Next(300, 800));
            }

            int steps = rnd.Next(3, 6);

            for (int i = 0; i < steps; i++)
            {
                await page.Mouse.WheelAsync(0, rnd.Next(250, 600));
                await page.WaitForTimeoutAsync(rnd.Next(200, 600));

                if (rnd.NextDouble() < 0.3)
                {
                    await page.Mouse.WheelAsync(0, -rnd.Next(100, 250));
                    await page.WaitForTimeoutAsync(rnd.Next(200, 500));
                }
            }

            await page.WaitForTimeoutAsync(rnd.Next(800, 1500));
        }
    }
}
