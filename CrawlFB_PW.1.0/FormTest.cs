using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Playwright;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Security.Policy;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using DocumentFormat.OpenXml.Drawing;
using DevExpress.Export.Xl;
using DevExpress.XtraPrinting.Native;
using DocumentFormat.OpenXml.Drawing.Charts;
using FBType = CrawlFB_PW._1._0.Enums.FBType;
using CrawlFB_PW._1._0.Helper;
using DevExpress.DocumentView;
using DevExpress.XtraPrinting;
using CrawlFB_PW._1._0.DAO.Page;
using IPage = Microsoft.Playwright.IPage;
using PageInfoDTO = CrawlFB_PW._1._0.DTO.PageInfo;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using System.Security.Cryptography;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using static CrawlFB_PW._1._0.DAO.Page.CrawlPageDAO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.DAO.Post;
using CrawlFB_PW._1._0.ViewModels;
using DocumentFormat.OpenXml.Office2019.Drawing.Animation.Model3D;
using DocumentFormat.OpenXml.Wordprocessing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
namespace CrawlFB_PW._1._0
{
    public partial class FormTest : Form
    {
        string currentUrl = null;
        public FormTest()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.FormTest_Load);
        }
        private void FormTest_Load(object sender, EventArgs e)
        {
            Libary.Instance.ClearAllLogs();
            Libary.Instance.CreateLog("[App] 🚀 Ứng dụng khởi động - bắt đầu phiên mới");
        }
        private void txbUrl_TextChanged(object sender, EventArgs e)
        {
            currentUrl = txbUrl.Text.Trim();  
        }
        public class GroupAboutInfo
        {
            public string CreatedDateExact { get; set; } = "N/A";
            public string MemberShort { get; set; } = "N/A";
            public string MemberTotal { get; set; } = "N/A";
            public string PageName { get; set; } = "N/A";
        }
        public class FanpageAboutInfo
        {
            public string CreatedDateExact { get; set; } = "N/A";  
            public string MemberTotal { get; set; } = "N/A";
            public string PageName { get; set; } = "N/A";
            public string PageInfo { get; set; } = "N/A";
        }
        private void AppendLog(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendLog(text)));
                return;
            }
            rtbLog.AppendText($"{DateTime.Now:HH:mm:ss} - {text}\n");
            rtbLog.ScrollToCaret();
        }
        public class PageScanResult
        {
            public string Name { get; set; } = "N/A";
            public string Type { get; set; } = "Unknown";
            public string MemberText { get; set; } = "N/A";
            public string CreatedDate { get; set; } = "N/A";
            public string LatestPostTime { get; set; } = "N/A";
        }
        public async Task DebugGroupAboutBlocksAsync(IPage page, string groupUrl)
        {
            try
            {
                string aboutUrl = groupUrl.TrimEnd('/') + "/about/";
                Libary.Instance.CreateLog($"🔎 Đang mở ABOUT: {aboutUrl}");

                await page.GotoAsync(aboutUrl);
                await page.WaitForTimeoutAsync(2000);

                var blocks = await page.QuerySelectorAllAsync("div.xu06os2.x1ok221b");

                if (blocks == null || blocks.Count == 0)
                {
                    Libary.Instance.CreateLog("❌ Không tìm thấy div.xu06os2.x1ok221b");
                    return;
                }

                Libary.Instance.CreateLog($"📌 Tìm thấy {blocks.Count} block:");

                int index = 1;
                foreach (var block in blocks)
                {
                    string txt = (await block.InnerTextAsync() ?? "").Trim();

                    Libary.Instance.CreateLog("------------------------------------------------");
                    Libary.Instance.CreateLog($"🔷 BLOCK {index}:");
                    Libary.Instance.CreateLog(txt);
                    Libary.Instance.CreateLog("------------------------------------------------");

                    index++;
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ DebugGroupAboutBlocksAsync ERROR: " + ex.Message);
            }
        }
        //===============================================
        public async Task<PageInfoDTO> ScanPageInfoAsync(IPage page, string pageUrl)
        {
            var info = new PageInfoDTO();
            string TimeLastPost = "";
            info.PageLink = pageUrl;
            await page.GotoAsync(pageUrl, new PageGotoOptions
            {
                Timeout = AppConfig.DEFAULT_TIMEOUT,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            await page.WaitForTimeoutAsync(2000);
            string time = await GetPostTimeAsync(page);
            // 2️⃣ Check Type
            var type = await PageDAO.Instance.CheckFBTypeAsync(page);
            info.PageType = type;

            AppendLog($"Thời gian bài viết: {time}");
            info.PageInfoText = time.ToString();
            
            AppendLog($"📌 Latest Post = {TimeLastPost}");
            // 3️⃣ Nếu là Group → đọc ABOUT
            if (type == FBType.GroupOn || type == FBType.GroupOff)
            {
                var about = await GetGroupAboutAsync(page, pageUrl);

                info.PageMembers = about.MemberShort != "N/A"
                                   ? about.MemberShort
                                   : about.MemberTotal;

                info.PageInfoText += "/Ngày tạo: " + about.CreatedDateExact;
                info.PageName = about.PageName;
            }

            // 4️⃣ Nếu là Fanpage → lấy followers
            else if (type == FBType.Fanpage)
            {
                var about = await GetFanpageAboutAsync(page, pageUrl);
                info.PageMembers = about.MemberTotal;
                info.PageInfoText += about.PageInfo;
                info.PageName = about.PageName;
            }

            // 5️⃣ Nếu là Person → lấy followers + friends
            else if (type == FBType.Person || type == FBType.PersonKOL)
            {
                //info.PageMembers = await GetPersonFollowersAsync(page);
                //info.PageInteraction = await GetPersonFriendsAsync(page);
            }
            return info;
        }
        public async Task<GroupAboutInfo> GetGroupAboutAsync(IPage page, string groupUrl)
        {
            GroupAboutInfo info = new GroupAboutInfo();

            string aboutUrl = groupUrl.TrimEnd('/') + "/about/";
            await page.GotoAsync(aboutUrl);
            await page.WaitForTimeoutAsync(1500);
            var anchors = await page.QuerySelectorAllAsync("a[href]");
            if (anchors == null || anchors.Count == 0)
            {
                Libary.Instance.CreateLog("❌ Không tìm thấy thẻ <a> nào.");
            }
            else
            {
                foreach (var a in anchors)
                {
                    string href = (await a.GetAttributeAsync("href") ?? "").Trim();
                    string text = (await a.InnerTextAsync() ?? "").Trim();

                    if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(text))
                        continue;

                    string hrefLower = href.ToLower();
                    string cleanGroupUrl = groupUrl.TrimEnd('/').ToLower();

                    // --- MATCH CẢ 3 DẠNG ---
                    bool match =
                        hrefLower.Contains(cleanGroupUrl) ||                                    // full URL
                        (hrefLower.Contains("/groups/") && hrefLower.EndsWith("/")) &&         // relative /groups/xxx/
                        !hrefLower.Contains("/members") && !hrefLower.Contains("/admin") &&
                        !hrefLower.Contains("/rules") && !hrefLower.Contains("/files");
                    if (match)
                    {
                        info.PageName = text;
                    }
                    else
                    {
                        var ElName = await page.QuerySelectorAllAsync("div [class = 'x1e56ztr x1xmf6yo']");
                        if (anchors == null || anchors.Count == 0)
                        {
                            Libary.Instance.CreateLog("❌ Không lấy được Name");
                        }
                        else
                        {
                            try
                            {
                                // phần tử đầu tiên
                                var first = ElName[0];

                                // lấy text
                                string name = (await first.InnerTextAsync() ?? "").Trim();

                                Libary.Instance.CreateLog($"📌 NAME FOUND: {name}");

                                info.PageName = name;   // gán vào DTO nếu cần
                            }
                            catch (Exception ex)
                            {
                                Libary.Instance.CreateLog("❌ Lỗi lấy Name: " + ex.Message);
                            }
                        }
                    }
                    // 👉 Gán vào info.PageName
                   

                    // Thoát ngay vì đã lấy được tên group đúng
                    break;
                }
            }


            var blocks = await page.QuerySelectorAllAsync("div.xu06os2.x1ok221b");
            foreach (var block in blocks)
            {
                string txt = (await block.InnerTextAsync() ?? "").Trim();
                // 1️⃣ Lấy ngày tạo (mọi dạng)
                if (txt.Contains("tạo"))   // thay vì StartsWith
                {
                    string date = ExtractCreatedDate(txt);
                    if (date != "N/A")
                    {
                        info.CreatedDateExact = date;
                        Libary.Instance.CreateLog($"📅 CREATED DATE = {date}");
                    }
                }
                // Block 10 → Thành viên dạng ngắn
                if (txt.Contains("Thành viên") && txt.Contains("·"))
                    info.MemberShort = ExtractShortMembers(txt);

                // Block 15 → Tổng thành viên dạng dài
                if (txt.Contains("Tổng cộng"))
                    info.MemberTotal = ExtractTotalMembers(txt);
            }

            return info;
        }
        public async Task<FanpageAboutInfo> GetFanpageAboutAsync(IPage page, string groupUrl)
        {
            FanpageAboutInfo info = new FanpageAboutInfo();

            string aboutUrl = groupUrl.TrimEnd('/') + "/about/";
            await page.GotoAsync(aboutUrl);
            await page.WaitForTimeoutAsync(1500);
            try
            {
                var eleFollowers = await page.QuerySelectorAsync("a[href*='followers']");
                if (eleFollowers != null)
                {
                    info.MemberTotal = (await eleFollowers.InnerTextAsync() ?? "").Trim();
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"[GetFanpageAbout: Lỗi lấy theo dõi]{ex.Message}");
            }
            try
            {
                var eleName = await page.QuerySelectorAsync("div.x1e56ztr.x1xmf6yo");
                info.PageName = eleName != null
                    ? (await eleName.InnerTextAsync()).Trim()
                    : "N/A";
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"[GetFanpageAbout: Lỗi lấy Tên Page]{ex.Message}");
            }

            string fullText = "";

            try
            {
                // Lấy node đầu tiên
                var ele = await page.QuerySelectorAsync("div.xieb3on");

                if (ele != null)
                {
                    // Lấy toàn bộ text (gồm span, xuống dòng)
                    fullText = (await ele.InnerTextAsync() ?? "").Trim();
                }
                else
                {
                    fullText = "N/A";
                }
                info.PageInfo = fullText;
            }
            catch (Exception ex)
            {
                info.PageInfo = fullText;
                Console.WriteLine("Lỗi lấy nội dung xieb3on: " + ex.Message);
            }
        
            return info;
        }
     
        public string ExtractRelativeCreated(string text)
        {
            var lines = text.Split('\n');
            if (lines.Length >= 2)
                return lines[1].Trim();

            return "N/A";
        }
        public string ExtractShortMembers(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"\d+[.,]?\d*[KkMm]?",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return match.Success ? match.Value : "N/A";
        }
        public string ExtractTotalMembers(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"\d{1,3}(?:[.,]\d{3})*",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return match.Success ? match.Value : "N/A";
        }
        public string ExtractCreatedDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "N/A";

            // REGEX BẮT MỌI DẠNG NGÀY THÁNG FACEBOOK
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"(\d{1,2}\s+tháng\s+\d{1,2}(?:,|\s+năm)?\s+\d{4})",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return "N/A";
        }
        private bool IsFacebookTimeText(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) return false;

            t = t.ToLower().Trim();

            // 1) Dạng tương đối KHÔNG có "trước"
            // "1 giờ", "2 phút", "3 ngày", "4 tuần", "5 tháng", "1 năm"
            if (Regex.IsMatch(t, @"^\d+\s+(giờ|phút|ngày|tuần|tháng|năm)$"))
                return true;

            // 2) Dạng tương đối CÓ "trước"
            if (Regex.IsMatch(t, @"\d+\s+(giờ|phút|ngày|tuần|tháng|năm)\s+trước"))
                return true;

            // 3) Dạng "hôm qua" hoặc "hôm nay"
            if (t.Contains("hôm qua") || t.Contains("hôm nay"))
                return true;

            // 4) Dạng tuyệt đối: 21 tháng 7, 2024  |  21 tháng 7 năm 2024
            if (Regex.IsMatch(t, @"\d{1,2}\s+tháng\s+\d{1,2}(?:,|\s+năm)?\s+\d{4}"))
                return true;

            // 5) Dạng có chữ "lúc"
            // ví dụ: "Hôm nay lúc 10:30", "Hôm qua lúc 19:20", "21 tháng 7 lúc 08:10"
            if (t.Contains("lúc"))
                return true;

            return false;
        }
        public async Task<string> GetPostTimeAsync(IPage page)
        {
            List<string> timeList = new List<string>();
            await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, AppConfig.scrollCount);
            await page.WaitForTimeoutAsync(2000);
            var nodes = await page.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");
            if (nodes == null || nodes.Count == 0)
            {
                Libary.Instance.CreateLog("❌ Không tìm thấy node feed time.");
                return "N/A";
            }
            foreach (var node in nodes)
            {
                var postinfor = await node.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");
                foreach (var info in postinfor)
                {
                    var links = await info.QuerySelectorAllAsync("a[href]");
                    if (links != null && links.Count > 0)
                    {
                        foreach (var link in links)
                        {
                            string t = (await link.InnerTextAsync() ?? "").Trim();

                            if (string.IsNullOrWhiteSpace(t))
                                continue;                       
                            if (IsFacebookTimeText(t))
                            {                             
                                timeList.Add(TimeHelper.ParseFacebookTime(t).ToString());
                            }
                        }
                    }
                }
            }
            if (timeList.Count == 0)
            {
                Libary.Instance.CreateLog("❌ timeList rỗng");
                return "N/A";
            }
            DateTime maxTime = DateTime.MinValue;

            foreach (var t in timeList)
            {             
                DateTime dt;

                if (DateTime.TryParse(t, out dt))
                {
                    if (dt > maxTime)
                        maxTime = dt;
                }
            }

            if (maxTime == DateTime.MinValue)
                return "N/A";
            // Trả về thời gian sớm nhất dạng đẹp
            return maxTime.ToString("dd-MM-yyyy HH:mm:ss");
        }
        //------------========LẤY ID
        public async Task<string> GetFacebookIdSmartAsync(IPage page, string url)
        {
            
            await page.WaitForTimeoutAsync(500);

            string html = await page.ContentAsync();

            // =====================
            // 1️⃣ FANPAGE
            // =====================
            if (url.Contains("/pages/") || html.Contains("\"pageID\""))
            {
                // pageID CHUẨN NHẤT
                var pageId = Regex.Match(html, "\"pageID\":\"(\\d+)\"");
                if (pageId.Success)
                    return pageId.Groups[1].Value;

                // fallback: entity_id
                var entity = Regex.Match(html, "\"entity_id\":\"(\\d+)\"");
                if (entity.Success)
                    return entity.Groups[1].Value;
            }

            // =====================
            // 2️⃣ GROUP
            // =====================
            if (url.Contains("/groups/") || html.Contains("\"groupID\""))
            {
                var g = Regex.Match(html, "\"groupID\":\"(\\d+)\"");
                if (g.Success)
                    return g.Groups[1].Value;

                // fallback mbasic
                string mbasic = url.Replace("www.facebook.com", "mbasic.facebook.com");
                await page.GotoAsync(mbasic);
                string mbasicHtml = await page.ContentAsync();

                var g2 = Regex.Match(mbasicHtml, "/groups/(\\d+)");
                if (g2.Success)
                    return g2.Groups[1].Value;
            }

            // =====================
            // 3️⃣ PROFILE CÁ NHÂN
            // =====================
            {
                // CHỈ LẤY 1 THẰNG NÀY
                var entity = Regex.Match(html, "\"entity_id\":\"(\\d+)\"");
                if (entity.Success)
                    return entity.Groups[1].Value;

                // fallback mbasic
                string mbasic = url.Replace("www.facebook.com", "mbasic.facebook.com");
                await page.GotoAsync(mbasic);
                string mbasicHtml = await page.ContentAsync();

                var p = Regex.Match(mbasicHtml, "/profile.php\\?id=(\\d+)");
                if (p.Success)
                    return p.Groups[1].Value;
            }

            return null;
        }

        public async Task<List<string>> DumpPageIdOnlyAsync(IPage page, string url)
        {
            // luôn chuyển sang trang /about
            string aboutUrl = url.TrimEnd('/') + "/about";

            await page.GotoAsync(aboutUrl, new PageGotoOptions
            {
                Timeout = 30000,
                WaitUntil = WaitUntilState.NetworkIdle
            });

            await page.WaitForTimeoutAsync(600);

            string html = await page.ContentAsync();

            // pageID chính
            var matches1 = Regex.Matches(html, "\"pageID\":\"(\\d+)\"");

            // fallback: page_id
            var matches2 = Regex.Matches(html, "\"page_id\":\"(\\d+)\"");

            var list = new List<string>();

            foreach (Match m in matches1)
                if (m.Success) list.Add(m.Groups[1].Value);

            foreach (Match m in matches2)
                if (m.Success) list.Add(m.Groups[1].Value);

            return list.Distinct().ToList();
        }
        public async Task<string> ExtractProfileIdFromPhotoHrefAsync(IPage page)

        {
            // Lấy toàn bộ href từ trang
            var hrefs = await page.EvaluateAsync<string[]>(@"Array.from(document.querySelectorAll('a[href]')).map(a => a.getAttribute('href'))");

            Dictionary<string, int> countMap = new Dictionary<string, int>();

            // Regex lấy UID sau pb.
            Regex pbRegex = new Regex(@"set=pb\.(\d+)\.", RegexOptions.Compiled);

            foreach (var href in hrefs)
            {
                if (string.IsNullOrEmpty(href)) continue;

                // Chỉ lấy link chứa facebook.com/photo/…set=pb.
                if (!href.Contains("/photo/") || !href.Contains("set=pb."))
                    continue;

                var m = pbRegex.Match(href);
                if (m.Success)
                {
                    string uid = m.Groups[1].Value;

                    if (!countMap.ContainsKey(uid))
                        countMap[uid] = 0;

                    countMap[uid]++;
                }
            }

            if (countMap.Count == 0)
                return null;

            // UID xuất hiện nhiều nhất = chính xác UID Profile
            string bestUid = countMap.OrderByDescending(x => x.Value).First().Key;
            return bestUid;
        }
        public async Task<string> ExtractProfileIdFromPhotoHref_DebugAsync(IPage page)
        {
            Console.WriteLine("===== BẮT ĐẦU DEBUG UID PERSON =====");

            // Lấy toàn bộ href
            var hrefs = await page.EvaluateAsync<string[]>(@"
        Array.from(document.querySelectorAll('a[href]'))
             .map(a => a.getAttribute('href'))
    ");

            Console.WriteLine($"Tổng số href thu được: {hrefs.Length}");

            Dictionary<string, int> countMap = new Dictionary<string, int>();
            Regex regex = new Regex(@"set=pb\.(\d+)\.", RegexOptions.Compiled);

            foreach (var href in hrefs)
            {
                if (string.IsNullOrEmpty(href)) continue;

                // In thử URL nếu là link photo
                if (href.Contains("/photo/"))
                {
                    Console.WriteLine($"[PHOTO LINK] {href}");
                }

                // Chỉ lấy link photo có set=pb.
                if (!href.Contains("/photo/") || !href.Contains("set=pb."))
                    continue;

                Console.WriteLine($"[MATCH PHOTO] {href}");

                // Bắt UID sau pb.
                var m = regex.Match(href);
                if (m.Success)
                {
                    string uid = m.Groups[1].Value;

                    Console.WriteLine($" → UID bắt được: {uid}");

                    if (!countMap.ContainsKey(uid))
                        countMap[uid] = 0;

                    countMap[uid]++;
                }
                else
                {
                    Console.WriteLine(" → KHÔNG match regex UID");
                }
            }

            Console.WriteLine("===== KẾT QUẢ UID =====");

            if (countMap.Count == 0)
            {
                Console.WriteLine("⚠ Không thu được UID nào từ href photo.");
                return null;
            }

            foreach (var kv in countMap)
            {
                Console.WriteLine($"UID: {kv.Key} — Xuất hiện: {kv.Value} lần");
            }

            // UID xuất hiện nhiều nhất = UID chính xác
            string bestUid = countMap.OrderByDescending(x => x.Value).First().Key;

            Console.WriteLine($"🎯 UID CHÍNH XÁC = {bestUid}");

            return bestUid;
        }
        public async Task<string> GetFacebookIdSmartAsync(IPage page, string url, string fbType)
        {
            // 0️⃣ LẤY ID TRỰC TIẾP TỪ URL (ưu tiên cao nhất)
            string directId = ExtractIdFromUrl(url);
            if (!string.IsNullOrEmpty(directId))
                return directId;

            // Load HTML lần đầu
            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = 30000,
                WaitUntil = WaitUntilState.NetworkIdle
            });

            await page.WaitForTimeoutAsync(500);
            string html = await page.ContentAsync();

            // =====================
            // 1️⃣ FANPAGE
            // =====================
            if (fbType == "Fanpage")
            {
                // Sử dụng hàm mới DumpPageIdOnlyAsync để lấy đúng pageID
                var ids = await DumpPageIdOnlyAsync(page, url);
                if (ids != null && ids.Count > 0)
                    return ids.First(); // CHỈ NHẬN 1 ID chuẩn duy nhất

                return null;
            }

            // =====================
            // 2️⃣ GROUP (giữ code cũ)
            // =====================
            if (fbType == "Groups")
            {
                var g = Regex.Match(html, "\"groupID\":\"(\\d+)\"");
                if (g.Success)
                    return g.Groups[1].Value;

                // fallback mbasic
                string mbasic = url.Replace("www.facebook.com", "mbasic.facebook.com");
                await page.GotoAsync(mbasic);
                string mbasicHtml = await page.ContentAsync();

                var g2 = Regex.Match(mbasicHtml, "/groups/(\\d+)");
                if (g2.Success)
                    return g2.Groups[1].Value;

                return null;
            }

            // =====================
            // 3️⃣ PROFILE (giữ code cũ)
            // =====================
            if (fbType == "Person")
            {

                // 1) ƯU TIÊN NHẤT: lấy UID từ href photo
                // string uid = await ExtractProfileIdFromPhotoHrefAsync(page);
                //if (!string.IsNullOrEmpty(uid))
                //return uid;
                // 2) JSON Comet → fallback xịn (CHUẨN hơn entity_id)
                
            }

            return null;
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
        public string Debug_ExtractEntityId(string html)
        {
            var m = Regex.Match(html, "\"entity_id\":\"(\\d+)\"");
            if (m.Success)
                return m.Groups[1].Value;

            return null;
        }
        public async Task<string> ExtractProfileIdFromJsonAsync(IPage page)
        {
            // Lấy toàn bộ script content
            var scripts = await page.EvaluateAsync<string[]>(@"
        Array.from(document.scripts).map(s => s.innerText)
    ");

            List<string> ids = new List<string>();

            Regex regex = new Regex("\"profileID\"\\s*:\\s*\"(\\d+)\"", RegexOptions.Compiled);

            foreach (var js in scripts)
            {
                if (string.IsNullOrEmpty(js))
                    continue;

                if (!js.Contains("profileID"))
                    continue;

                foreach (Match m in regex.Matches(js))
                {
                    ids.Add(m.Groups[1].Value);
                }
            }

            if (ids.Count == 0)
                return null;

            // UID xuất hiện nhiều nhất
            return ids
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        /// <summary>
        /// Trích xuất thời gian bài viết FB 2025 – xử lý DOM bị xé chữ.
        /// </summary>
        public async Task<List<string>> GetAllTimesAsync(IPage page)
        {
            string[] selectors = {
        "a[aria-label] time",
        "a[role='link'] time",
        "time",
        "abbr",
        "a[aria-label]",
        "span"
    };

            var results = new List<string>();

            foreach (var sel in selectors)
            {
                var nodes = await page.QuerySelectorAllAsync(sel);

                foreach (var n in nodes)
                {
                    string t = (await n.InnerTextAsync() ?? "").Trim();

                    // Lọc time hợp lệ
                    if (string.IsNullOrWhiteSpace(t)) continue;

                    if (ProcessingDAO.Instance.IsTime(t))
                    {
                        results.Add(t);
                    }
                }

                if (results.Count > 0)
                    break; // chọn selector nào có time đầu tiên
            }

            return results;
        }
        public async Task<string> ExtractPostContentAsync(IElementHandle post)
        {
            try
            {
                // Lấy text gọn
                string text = await post.InnerTextAsync();
                if (string.IsNullOrWhiteSpace(text))
                    return "N/A";

                // rút gọn
                text = text.Trim();
                if (text.Length > 120)
                    text = text.Substring(0, 120) + "…";

                return text;
            }
            catch
            {
                return "N/A";
            }
        }

        private string CleanTimeString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "N/A";

            // Xóa ký tự ẩn cực kỳ quan trọng với DOM FB 2024–2025
            s = s.Replace("\u200B", "")   // zero-width space
                 .Replace("\u200E", "")   // LTR mark
                 .Replace("\u200F", "")   // RTL mark
                 .Replace("\u2060", "")   // word-joiner
                 .Replace("\u00A0", " "); // &nbsp;

            // Ký tự rác hay gặp
            s = s.Replace("·", " ")
                 .Replace("•", " ")
                 .Replace("|", " ")
                 .Replace("  ", " ");

            return s.Trim();
        }
        public async Task HumanScrollAsync(IPage page, int rounds = 8)
        {
            Random rd = new Random();

            for (int i = 0; i < rounds; i++)
            {
                int delta = rd.Next(250, 600);   // mức scroll tự nhiên
                int wait = rd.Next(300, 900);    // delay ngẫu nhiên

                await page.Mouse.WheelAsync(0, delta);

                Libary.Instance.CreateLog($"🖱️ HumanScroll: {delta}px, wait={wait}ms");
                await page.WaitForTimeoutAsync(wait);
            }
        }

        private async void btnTest_Click_1(object sender, EventArgs e)
        {
            string profileId = txbProfileId.Text.Trim();
            string url = txbUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Không có URL");
                return;
            }
           string urlgoc = url;
          /*  if (url.IndexOf("sorting_setting=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                if (url.Contains("?"))
                    url += "&sorting_setting=CHRONOLOGICAL";
                else
                    url += "?sorting_setting=CHRONOLOGICAL";
            }
          */
            var page = await Ads.Instance.OpenNewTabAsync(profileId);
            Libary.Instance.SetProfileContext(profileId, "profile.ProfileName");
            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = AppConfig.DEFAULT_TIMEOUT,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            // scroll nhẹ 1–2 lần
            await page.EvaluateAsync(@"() => {
    window.scrollBy(0, document.body.scrollHeight);
}");
            await page.WaitForTimeoutAsync(1200);

            await page.EvaluateAsync(@"() => {
    window.scrollBy(0, document.body.scrollHeight);
}");
            await page.WaitForTimeoutAsync(1200);

            AppendLog("");
            AppendLog("══════════════════════════════════════════════════════");
            AppendLog("🧪 TEST POPUP FULL");
            AppendLog("══════════════════════════════════════════════════════");
            var postDiv = await CrawlBaseDAO.Instance.GetFeedPostOriginalNormalAsync(page);
            if (postDiv != null)
            {
                AppendLog("tìm thấy PostDiv");

            }
            else
            {
                AppendLog("tìm thấy PostDiv");
            }
            var postinfor = await postDiv.QuerySelectorAllAsync("div.xu06os2.x1ok221b");
            foreach (var el in postinfor)
            {
                var txt = (await el.InnerTextAsync())?.Trim().ToLower();
                if (ProcessingDAO.Instance.IsTime(txt))
                {
                    txt = TimeHelper.CleanTimeString(txt);
                    //info.PostTime = txt;
                    //info.RealPostTime = TimeHelper.ParseFacebookTime(txt);
                    //Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] thời gian lấy trong hàm ok {txt}");
                    AppendLog($"{Libary.IconOK} [ORIGINAL POST] thời gian lấy trong hàm ok {txt}");
                }
            }
            var (LikeCount, CommentCount, ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(postDiv);
            AppendLog($"Lấy tương tác: Like {LikeCount}, Comment {CommentCount}, Share {ShareCount}");
            var (pageName, pageLink) = await CrawlBaseDAO.Instance.GetPageContainerFromFeedAsync(postDiv);
            AppendLog($"Lấy thông tin page: name {pageName}, link {pageLink}");
            var (posterName, posterLink) = await PopupDAO.Instance.GetPosterGroupsPopupPost(postDiv);
            AppendLog($"Lấy thông tin người đăng: name {posterName}, link {posterLink}");
            var content = await PopupDAO.Instance.GetContentPopup(postDiv);
            AppendLog($"Lấy nội dung: {content}");
            /*
                    var dialog = await PopupDAO.Instance.GetFeedPopupAsync(page);

                    if (dialog == null)
                    {
                        AppendLog("❌ Không tìm thấy popup dialog");
                        return;
                    }

                    AppendLog("✅ Found popup dialog");

                    // ===============================
                    // 👤 POSTER
                    // ===============================
                    var (name, link) = await PopupDAO.Instance.GetPosterPopup(dialog);


                    //======
                    bool isGroup = !string.IsNullOrWhiteSpace(url) &&
                       url.Contains("/groups/");

                    if (isGroup)
                    {
                        AppendLog($"👤 Groups NAME   : {name}");
                        AppendLog($"🔗 Groups LINK   : {link}");
                        // 🔥 dùng dialog thay vì postDiv
                        var (posterName, posterLink) = await PopupDAO.Instance.GetPosterGroupsPopupPost(dialog);

                        if (!string.IsNullOrWhiteSpace(posterLink) && posterLink != "N/A")
                        {

                            AppendLog($"👤 Poster Groups NAME   : {posterName}");
                            AppendLog($"🔗 Poster Groups LINK   : {posterLink}");
                        }

                    }
                    else
                    {

                        AppendLog($"👤 Fanpage/Person NAME   : {name}");
                        AppendLog($"🔗 Fanpage/Person LINK   : {link}");

                    }
                    // ===============================
                    // ⏰ TIME
                    // ===============================
                    var (time, realTime) = await PopupDAO.Instance.GetTimePopup(dialog);

                    AppendLog($"⏰ TIME   : {time}");
                    AppendLog($"📅 REAL   : {realTime}");

                    // ===============================
                    // 📝 CONTENT (CALL DAO)
                    // ===============================
                    var content = await PopupDAO.Instance.GetContentPopup(dialog);

                    AppendLog($"📝 CONTENT LEN : {content.Length}");

                    if (!string.IsNullOrEmpty(content))
                    {
                        AppendLog("📄 CONTENT:");
                        AppendLog(content);
                    }
                    else
                    {
                        AppendLog("❌ CONTENT EMPTY");
                    }
                    // ===============================
                    // 📊 INTERACTION
                    // ===============================
                    var (like, comment, share) = await PopupDAO.Instance.GetInteractionPopup(dialog);

                    AppendLog($"👍 LIKE   : {like}");
                    AppendLog($"💬 COMMENT: {comment}");
                    AppendLog($"🔁 SHARE  : {share}");

                    // ===============================
                    // 🔍 DEBUG THÊM (QUAN TRỌNG)
                    // ===============================
                    var feedBlocks = await dialog.QuerySelectorAllAsync("div.x1n2onr6");
                    AppendLog($"📦 FeedBlocks: {feedBlocks.Count}");

                    var spans = await dialog.QuerySelectorAllAsync("span");
                    AppendLog($"📊 TotalSpan: {spans.Count}");

                    // ===============================
                    AppendLog("══════════════════════════════════════════════════════");
                    AppendLog("✅ TEST DONE");
                    AppendLog("══════════════════════════════════════════════════════");
            */
        }
        private PostKind DetectPostKind(RawPostInfo raw)
        {
            // ========================
            // 1️⃣ REEL GỐC (không có time)
            // ========================
            if (raw.HasReel && raw.TimeCount == 0)
            {
                Libary.Instance.LogTech("[DetectPostKind] 👉 Result = Reel (Reel + TimeCount = 0)");
                return PostKind.Reel;
            }
            // ========================
            // 2️⃣ CÓ REEL
            // ========================
            if (raw.HasReel)
            {
                // GROUP + 1 TIME → Reel (chưa rõ gốc/share)
                if (raw.Context == CrawlContext.Group && raw.TimeCount == 1)
                {
                    Libary.Instance.LogTech("[DetectPostKind] 👉 Result = ReelUnknow (Group + 1 time + Reel)");
                    return PostKind.ReelUnknow;
                }

                // FANPAGE + 1 TIME
                if (raw.Context == CrawlContext.Fanpage && raw.TimeCount == 1)
                {
                    if (raw.PostLink == raw.ReelLink)
                    {
                        Libary.Instance.LogTech("[DetectPostKind] 👉 Result = Reel (Fanpage + 1 time + Reel)");
                        return PostKind.ReelHasTime;
                    }

                    Libary.Instance.LogTech( "[DetectPostKind] 👉 Result = ShareReel (Fanpage + 1 time + Reel)");
                    return PostKind.ShareReel;
                }

                // GROUP + >=2 TIME → SHARE REEL
                if (raw.Context == CrawlContext.Group && raw.TimeCount >= 2)
                {
                    Libary.Instance.LogTech("[DetectPostKind] 👉 Result = ShareReel (Group + >=2 time + Reel)");
                    return PostKind.ShareReel;
                }
            }
            // ========================
            // 3️⃣ KHÔNG REEL + >=2 TIME
            // ========================
            if (!raw.HasReel && raw.TimeCount >= 2)
            {
                Libary.Instance.LogTech("[DetectPostKind] 👉 Result = ShareNormal (>=2 time, no reel)");
                return PostKind.ShareNormal;
            }

            // ========================
            // 4️⃣ CÒN LẠI
            // ========================
            Libary.Instance.LogTech("[DetectPostKind] 👉 Result = Normal");
            return PostKind.Normal;
        }
        private void AppendTestCrawlResult( int index, RawPostInfo raw, PostKind kind, PostResult result)
        {
            AppendLog("");
            AppendLog("══════════════════════════════════════════════════════");
            AppendLog($"🧪 TEST POST #{index}");
            AppendLog("══════════════════════════════════════════════════════");

            // ==================================================
            // A️⃣ RAW INFO
            // ==================================================
            AppendLog("🧱 [RAW INFO]");
            AppendLog($"Context        : {raw.Context}");
            AppendLog($"PageName       : {raw.PageName}");
            AppendLog($"PageLink       : {raw.PageLink}");
            AppendLog($"TimeCount      : {raw.TimeCount}");
            AppendLog($"LinkCount      : {raw.LinkCount}");
            AppendLog($"HasReel        : {raw.HasReel}");
            AppendLog($"ReelLink       : {raw.ReelLink}");
            AppendLog($"PostTime       : {raw.PostTime}");
            AppendLog($"PostLink       : {raw.PostLink}");
            AppendLog($"OriPostTime    : {raw.OriginalPostTime}");
            AppendLog($"OriPostLink    : {raw.OriginalPostLink}");

            // ==================================================
            // B️⃣ CHỐT LOẠI
            // ==================================================
            AppendLog("----------------------------------------------");
            AppendLog("🏷 [DETECT RESULT]");
            AppendLog($"PostKind       : {kind}");

            // ==================================================
            // C️⃣ RESULT SAU PARSE
            // ==================================================
            AppendLog("----------------------------------------------");
            AppendLog("📦 [PARSE RESULT]");

            if (result == null)
            {
                AppendLog("❌ Result = NULL");
                return;
            }

            AppendLog($"Posts.Count    : {result.Posts?.Count ?? 0}");
            AppendLog($"Shares.Count   : {result.Shares?.Count ?? 0}");

            if (result.Posts != null)
            {
                int i = 1;
                foreach (var p in result.Posts)
                {
                    AppendLog($"  ▶ Post[{i}]");
                    AppendLog($"     PAGELink      : {p.PageLink}");
                    AppendLog($"     PAGEnAME      : {p.PageName}");
                    AppendLog($"     pOSTLink      : {p.PostLink}");
                    AppendLog($"     Time      : {p.PostTime}");
                    AppendLog($"     Content      : {ProcessingHelper.PreviewText(p.Content)}");
                    AppendLog($"     TimeReal     : {p.RealPostTime.ToString()}");
                    AppendLog($"     Poster    : {p.PosterName} ({p.PosterNote})");
                    AppendLog($"     PosterLink      : {p.PosterLink}");
                    AppendLog($"     Type      : {p.PostType}");
                    AppendLog($"     👍 {p.LikeCount} | 💬 {p.CommentCount} | 🔁 {p.ShareCount}");
                    i++;
                }
            }

            AppendLog("══════════════════════════════════════════════════════");
        }
        private async Task<PostResult> ParseByKindAsync(RawPostInfo raw, PostKind kind)
        {
            switch (kind)
            {
                case PostKind.Normal:
                    return await ParseNormalPostAsync(raw);

                case PostKind.ShareNormal:
                    return await ParseSharePostAsync(raw, ShareMode.Normal);

                case PostKind.ShareReel:
                    return await ParseSharePostAsync(raw, ShareMode.Reel);

                case PostKind.Reel:
                    return await ParseReelAsync(raw);
                case PostKind.ReelUnknow:
                    return await ParseReelUnknowAsync(raw);
                case PostKind.ReelHasTime:
                    return await ParseReelHasTimeAsync(raw);
                default:
                    return new PostResult();
            }
        }
        // =====================================================
        // PARSE — NORMAL POST
        // =====================================================
        private async Task<PostResult> ParseNormalPostAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };
            Libary.Instance.LogTech($"{Libary.IconInfo}▶ Start | Context={raw.Context} | Page={raw.PageName}");
            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;
            // ========================
            // INIT RAW INFO
            // ========================
            var info = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink,
                PostType = PostType.Page_Normal
            };
            Libary.Instance.LogTech($"[ParseNormalPost] PostLink={info.PostLink} | PostTime={info.PostTime}");

            // ========================
            // 1️⃣ POSTER
            // ========================
            if (raw.Context == CrawlContext.Fanpage)
            {
                info.PosterName = raw.PageName;
                info.PosterLink = raw.PageLink;
                info.PosterNote = FBType.Fanpage;
                Libary.Instance.LogTech("[ParseNormalPost] Poster = Fanpage (gán cứng)");
            }
            else
            {
                await FillPosterInfoAsync(info, page, post, postinfor);
            }
            // ========================
            // 2️⃣ CONTENT
            // ========================
            await FillFullContentPostNormalAsync(info, page, post, postinfor);
            // ========================
            // 3️⃣ INTERACTION
            // ========================
            (info.LikeCount,info.CommentCount,info.ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(post);
            Libary.Instance.LogTech($"[ParseNormalPost] 👍{info.LikeCount} 💬{info.CommentCount} 🔁{info.ShareCount}");
            // ========================
            // 4️⃣ BUILD POST
            // ========================
            var postPage = BuildPostPage(info);
            result.Posts.Add(postPage);

            Libary.Instance.LogTech(
                $"[ParseNormalPost] ✅ ADD POST | Link={postPage.PostLink} | Type={postPage.PostType}");

            Libary.Instance.LogTech("[ParseNormalPost] ◀ End | Normal post OK");

            return result;       
        }

        //===================
        // PARSE - REEL VẪN LẤY KIỂU THƯỜNG
        //===============
        private async Task<PostResult> ParseReelHasTimeAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };

            Libary.Instance.LogTech($"{Libary.IconInfo}▶ Start | Context={raw.Context} | Page={raw.PageName}");
            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;
            // =================================================
            // INIT INFO
            // =================================================
            var info = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink              
            };

            // =================================================
            // 1️⃣ POSTER
            // =================================================
            if (raw.Context == CrawlContext.Fanpage)
            {
                info.PosterName = raw.PageName;
                info.PosterLink = raw.PageLink;
                info.PosterNote = FBType.Fanpage;
            }
            else
            {
                await FillPosterInfoAsync(info, page, post, postinfor);
            }

            // =================================================
            // 2️⃣ CONTENT + INTERACTION (FEED)
            // =================================================
            int c = postinfor?.Count ?? 0;
            if (c >= 3)
            {
                info.Content = await CrawlBaseDAO.Instance.GetContentTextAsync(page, postinfor[2]);
            }
            info.PostType = ProcessingHelper.IsValidContent(info.Content)? PostType.page_Real_Cap: PostType.Page_Reel_NoCap;   // hoặc PostType.Page_Unknow

            (info.LikeCount, info.CommentCount, info.ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(post);
            // =================================================
            // 3️⃣ BỔ SUNG REEL DETAIL NẾU THIẾU
            // =================================================
            if (CrawlBaseDAO.Instance.NeedFetchReelDetail(info))
            {
                Libary.Instance.LogTech(
                    "[ParseReelHasTime] 🔎 Thiếu dữ liệu → mở Reel để lấy tiếp");

                var reel = await CrawlPostReelDAO.Instance
                    .ExtractPostReelAll(page, post);

                if (reel != null && reel.PostLink != "N/A")
                {
                    CrawlBaseDAO.Instance.MergeReelInfoIfEmpty(info, reel);
                }
                else
                {
                    Libary.Instance.LogTech(
                        "[ParseReelHasTime] ⚠️ Không lấy được Reel detail");
                }
            }

            // =================================================
            // 4️⃣ BUILD POST
            // =================================================
            var postPage = BuildPostPage(info);
            result.Posts.Add(postPage);

            Libary.Instance.LogTech( $"[ParseReelHasTime] ✅ ADD POST | Link={postPage.PostLink} | Type={postPage.PostType}");

            Libary.Instance.LogTech(
                "[ParseReelHasTime] ◀ End | ReelHasTime OK");

            return result;
        }
        // =====================================================
        // PARSE — SHARE POST (NORMAL + REEL)
        // =====================================================
        private async Task<PostResult> ParseSharePostAsync(RawPostInfo raw, ShareMode mode)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };
            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;

            Libary.Instance.LogTech($"[ParseSharePost] ▶ Start | Mode={mode} | PostLink={raw.PostLink}");

            // =====================================================
            // A️⃣ POST SHARE (A)
            // =====================================================
            var infoA = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink
            };

            // ---------- POSTER ----------
            if (raw.Context == CrawlContext.Fanpage)
            {
                infoA.PosterName = raw.PageName;
                infoA.PosterLink = raw.PageLink;
                infoA.PosterNote = FBType.Fanpage;
            }
            else
            {
                await FillPosterInfoAsync(infoA, page, post, postinfor);
            }
            // ---------- CONTENT SHARE ----------
            // ⭐ tách từ case 2 cũ
            var (contentShare, contentOriginal, postTypeShare, originalPostType) = await ParseShareContentAsync(page, post, postinfor, mode == ShareMode.Reel);
            infoA.Content = contentShare;
            infoA.PostType = postTypeShare;
            Libary.Instance.LogTech($"[Share-A] ContentLen={(infoA.Content?.Length ?? 0)} | Type={infoA.PostType}");
            // ---------- INTERACTION ----------
            (infoA.LikeCount, infoA.CommentCount, infoA.ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(post);
            Libary.Instance.LogTech($"[Share-A] 👍{infoA.LikeCount} 💬{infoA.CommentCount} 🔁{infoA.ShareCount}");
            // ---------- ADD POST A ----------
            var postA = BuildPostPage(infoA);
            result.Posts.Add(postA);
            // =====================================================
            // B️⃣ POST GỐC (B)
            // =====================================================
            PostPage postB = null;
            var infoB = new PostInfoRawVM
            {
                PostLink = raw.OriginalPostLink,
                PostTime = raw.OriginalPostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.OriginalPostTime),

            };
            if (mode == ShareMode.Reel)
            {
                Libary.Instance.LogTech("[Share-B] Open REEL original");
                if (raw.Context == CrawlContext.Group)
                {
                    Libary.Instance.LogTech("[Share-B] Context=Group → lấy Page / Poster từ container Group Reel");

                    // 1️⃣ PAGE GỐC (Group / Page chứa reel)
                    var (oriPageName, oriPageLink) = await CrawlPostReelDAO.Instance.ExtractPageGroupsReel(page, post);
                    if (oriPageLink != "N/A")
                    {
                        infoB.PageName = oriPageName;
                        infoB.PageLink = oriPageLink;
                        Libary.Instance.LogTech($"[Share-B] GroupPage='{oriPageName}' | Link={oriPageLink}");
                    }
                    var (posterName, posterLink) = await CrawlPostReelDAO.Instance.ExtractPosterGroupsReel(post);

                    if (ProcessingHelper.IsValidContent(posterLink))
                    {
                        infoB.PosterName = posterName;
                        infoB.PosterLink = posterLink;
                        Libary.Instance.LogTech($"[Share-B] GroupPoster='{posterName}' | Link={posterLink}");
                        Libary.Instance.LogDebug($"{Libary.IconInfo} dùng CheckType lấy type người đăng Reel");
                        (infoB.PosterNote, infoB.PosterIdFB) = await CrawlBaseDAO.Instance.CheckTypeCachedAsync(page, infoB.PosterLink);
                        var (content, likes, comments, shares) = await CrawlPostReelDAO.Instance.OpenReelTabAndExtractAsync(page, infoB.PostLink);
                        infoB.Content = content; infoB.LikeCount = likes; infoB.CommentCount = comments; infoB.ShareCount = shares;
                    }
                    postB = BuildPostPage(infoB);
                }
                else if(raw.Context == CrawlContext.Fanpage)
                {
                    var (oriPageName, oriPageLink) = await CrawlPostReelDAO.Instance.ExtractPageGroupsReel(page, post);
                    if (oriPageLink != "N/A")
                    {
                        infoB.PageName = oriPageName;
                        infoB.PageLink = oriPageLink;
                        Libary.Instance.LogTech($"[Share-B] GroupPage='{oriPageName}' | Link={oriPageLink}");
                    }
                    infoB.PosterName = infoB.PageName;
                    infoB.PosterLink = infoB.PageLink;
                    (infoB.PosterNote, infoB.PosterIdFB) = await CrawlBaseDAO.Instance.CheckTypeCachedAsync(page, infoB.PosterLink);
                    var (content, likes, comments, shares) = await CrawlPostReelDAO.Instance.OpenReelTabAndExtractAsync(page, infoB.PostLink);
                    infoB.Content = content; infoB.LikeCount = likes; infoB.CommentCount = comments; infoB.ShareCount = shares;
                }
                postB = BuildPostPage(infoB);
            }
            else
            {
                Libary.Instance.LogTech($"[Share-B] Open ORIGINAL post | Link={raw.OriginalPostLink}");

                
            }
        
            if (postB != null && postB.PostLink != "N/A")
            {
                result.Posts.Add(postB);

                // =================================================
                // C️⃣ SHARE MAP
                // =================================================
                result.Shares.Add(new ShareItem
                {
                    PageLinkA = raw.PageLink,
                    PostLinkB = postB.PostLink,
                    ShareTimeRaw = infoA.PostTime,
                    ShareTimeReal = infoA.RealPostTime ?? DateTime.MinValue
                });

                Libary.Instance.LogTech($"[Share-MAP] A={raw.PageLink} → B={postB.PostLink}");
            }

            Libary.Instance.LogTech($"[ParseSharePost] ◀ End | Posts={result.Posts.Count}, Shares={result.Shares.Count}");

            return result;
        }

        private async Task<(string contentShare, string contentOriginal, PostType postTypeShare, PostType originalPostType)> ParseShareContentAsync(
         IPage page,
         IElementHandle post,
         IReadOnlyList<IElementHandle> postinfor,
         bool isReel)
        {
            string noidung = "N/A";
            string noidunggoc = "N/A";

            PostType postType = PostType.Share_NoContent;
            PostType originalPostType = PostType.Page_Unknow;

            int c = postinfor?.Count ?? 0;

            Libary.Instance.LogDebug(
                $"[ParseShareContent] Start | isReel={isReel} | postinfor.Count={c}");

            try
            {
                // =================================================
                // 🟦 SHARE REEL (ĐẶC BIỆT)
                // postinfor.Count == 3
                // content share nằm ở index 2
                // =================================================
                if (isReel && c == 3)
                {
                    noidung = await CrawlBaseDAO.Instance
                        .GetContentTextAsync(page, postinfor[2]);

                    if (!string.IsNullOrWhiteSpace(noidung) && noidung != "N/A")
                    {
                        postType = PostType.Share_WithContent;
                        originalPostType = PostType.Page_Unknow;

                        Libary.Instance.LogDebug(
                            "[ParseShareContent] SHARE REEL: Có content share (index 2)");
                    }
                    else
                    {
                        postType = PostType.Share_NoContent;

                        Libary.Instance.LogDebug(
                            "[ParseShareContent] SHARE REEL: Không có content share");
                    }

                    return (noidung, noidunggoc, postType, originalPostType);
                }

                // =================================================
                // 🟩 SHARE THƯỜNG (LOGIC CŨ)
                // =================================================
                if (c >= 5)
                {
                    var el2 = postinfor[2];
                    var el4 = postinfor[4];

                    string content2 = await CrawlBaseDAO.Instance.GetContentTextAsync(page, el2);
                    string content4 = await CrawlBaseDAO.Instance.GetContentTextAsync(page, el4);

                    bool hasContent2 = !string.IsNullOrWhiteSpace(content2) && content2 != "N/A";
                    bool hasContent4 = !string.IsNullOrWhiteSpace(content4) && content4 != "N/A";

                    Libary.Instance.LogDebug(
                        $"[ParseShareContent] content2.len={(content2?.Length ?? 0)}, content4.len={(content4?.Length ?? 0)}");

                    // =========================
                    // CASE 6
                    // =========================
                    if (c == 6)
                    {
                        var el5 = postinfor[5];
                        string content5 = await CrawlBaseDAO.Instance.GetContentTextAsync(page, el5);
                        bool hasContent5 = !string.IsNullOrWhiteSpace(content5) && content5 != "N/A";

                        Libary.Instance.LogDebug(
                            $"[ParseShareContent] content5.len={(content5?.Length ?? 0)}");

                        if (hasContent2 && hasContent5)
                        {
                            noidung = content2;
                            noidunggoc = content5;

                            postType = PostType.Share_WithContent;
                            originalPostType = PostType.Page_Normal;

                            Libary.Instance.LogDebug(
                                "[ParseShareContent] CASE 6: Share + Original đều có content");
                        }
                        else if (!hasContent2 && !hasContent5 && hasContent4)
                        {
                            noidunggoc = content4;

                            postType = PostType.Share_NoContent;
                            originalPostType = PostType.Page_Normal;

                            Libary.Instance.LogDebug(
                                "[ParseShareContent] CASE 6: Chỉ có content gốc (container 4)");
                        }
                        else if (hasContent2)
                        {
                            noidung = content2;

                            postType = PostType.Share_WithContent;
                            originalPostType = PostType.Page_Unknow;

                            Libary.Instance.LogDebug(
                                "[ParseShareContent] CASE 6: Chỉ có content share");
                        }
                        else
                        {
                            Libary.Instance.LogDebug(
                                "[ParseShareContent] CASE 6: Không lấy được content");
                        }
                    }
                    // =========================
                    // CASE 5
                    // =========================
                    else
                    {
                        if (hasContent2)
                        {
                            noidung = content2;
                            postType = PostType.Share_WithContent;

                            Libary.Instance.LogDebug(
                                "[ParseShareContent] CASE 5: Share có content");
                        }
                        else if (hasContent4)
                        {
                            noidunggoc = content4;
                            postType = PostType.Share_NoContent;
                            originalPostType = PostType.Page_Normal;

                            Libary.Instance.LogDebug(
                                "[ParseShareContent] CASE 5: Chỉ có content gốc");
                        }
                        else
                        {
                            Libary.Instance.LogDebug(
                                "[ParseShareContent] CASE 5: Không lấy được content");
                        }
                    }
                }
                // =========================
                // CASE 4
                // =========================
                else if (c == 4)
                {
                    noidung = await CrawlBaseDAO.Instance.BackgroundTextAllAsync(page, post);

                    if (!string.IsNullOrWhiteSpace(noidung) && noidung != "N/A")
                    {
                        postType = PostType.Share_NoContent;

                        Libary.Instance.LogDebug(
                            "[ParseShareContent] CASE 4: Background share");
                    }
                    else
                    {
                        Libary.Instance.LogDebug(
                            "[ParseShareContent] CASE 4: Không có content");
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"[ParseShareContent] ❌ Exception: {ex.Message}");
            }

            Libary.Instance.LogDebug(
                $"[ParseShareContent] End | ShareType={postType}, ShareLen={(noidung?.Length ?? 0)}, OriginalLen={(noidunggoc?.Length ?? 0)}");

            return (noidung, noidunggoc, postType, originalPostType);
        }

        // =====================================================
        // PARSE — REEL ORIGINAL
        // =====================================================
        private async Task<PostResult> ParseReelAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };

            Libary.Instance.LogTech($"[ParseReelOriginal] ▶ Start | Page={raw.PageName}");
            var reelPost = new PostPage();
            try
            {

                if (raw.Context == CrawlContext.Fanpage)
                {
                    Libary.Instance.LogTech("----FanPage Reel----");
                    // ========================
                    // 1️⃣ OPEN & EXTRACT REEL
                    // ========================
                    reelPost = await CrawlPostReelDAO.Instance.ExtractPostReelAll(raw.Page, raw.PostNode);
                    reelPost.PageName = raw.PageName;
                    reelPost.PageLink = raw.PageLink;
                }
                else if(raw.Context == CrawlContext.Group)
                {
                    reelPost = await CrawlPostReelDAO.Instance.ExtractPostReelAll(raw.Page, raw.PostNode);
                }    
                if (reelPost == null || reelPost.PostLink == "N/A")
                    {
                        Libary.Instance.LogTech("[Reel] ❌ ExtractPostReelAll trả về NULL / PostLink=N/A");
                        return result;
                    }
                    // ========================
                    // 2️⃣ GÁN CONTEXT PAGE
                    // ========================
                    Libary.Instance.LogTech(
                        $"[Reel] ✅ Lấy Reel thành công\n" +
                        $"   🔗 Link      : {reelPost.PostLink}\n" +
                        $"   👤 Người đăng: {reelPost.PosterName}\n" +
                        $"   ⏰ Thời gian Raw : {reelPost.PostTime}\n" +
                         $"   ⏰ Thời gian Raw : {TimeHelper.NormalizeTime(reelPost.RealPostTime)}\n" +
                         $"   👍 PosterNote      : {reelPost.PosterNote}\n" +
                        $"   👍 Like      : {reelPost.LikeCount}\n" +
                        $"   💬 Comment   : {reelPost.CommentCount}\n" +
                        $"   🔁 Share     : {reelPost.ShareCount}"
                    );

                    if (!string.IsNullOrWhiteSpace(reelPost.Content) && reelPost.Content != "N/A")
                    {
                        Libary.Instance.LogTech(
                            $"   📝 Caption   : {ProcessingHelper.PreviewText(reelPost.Content)}");
                    }
                    else
                    {
                        Libary.Instance.LogTech(
                            "   📝 Caption   : (không có)");
                    }
                    // ========================
                    // 3️⃣ ADD RESULT
                    // ========================
                    result.Posts.Add(reelPost);

                    Libary.Instance.LogTech( "[ParseReelOriginal] ◀ End | Reel OK");
                    
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    $"[ParseReelOriginal] ❌ Exception: {ex.Message}");
            }

            return result;
        }
        private async Task<PostResult> ParseReelUnknowAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };

            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;

            Libary.Instance.LogTech($"[ParseReelUnknow] ▶ Start | Group={raw.PageName}");

            // =================================================
            // A️⃣ POST TRONG GROUP (LUÔN CÓ – LUÔN FILL)
            // =================================================
            var infoA = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink
            };

            // 1️⃣ Poster
            await FillPosterInfoAsync(infoA, page, post, postinfor);

            // 2️⃣ Content + Interaction (LUÔN LẤY)
            await FillContentAndInteractionNormalAsync(infoA, page, post, postinfor);

            Libary.Instance.LogTech( $"[Reel-A] Poster={infoA.PosterName} | ContentLen={(infoA.Content?.Length ?? 0)}");

            try
            {
                // =================================================
                // B️⃣ OPEN REEL GỐC
                // =================================================
                Libary.Instance.LogTech("[Reel] 🔗 Open reel to detect REAL vs SHARE");

                var reelPost = await CrawlPostReelDAO.Instance.ExtractPostReelAll(page, post);

                if (reelPost == null || reelPost.PostLink == "N/A")
                {
                    // fallback an toàn
                    Libary.Instance.LogTech( "[Reel] ❌ Không mở được reel → dùng post A");

                    result.Posts.Add(BuildPostPage(infoA));
                    return result;
                }

                Libary.Instance.LogTech($"[Reel-B] Poster={reelPost.PosterName} | Page={reelPost.PageName} | ContentLen={(reelPost.Content?.Length ?? 0)}");
                LogReelDebug(reelPost);
                // =================================================
                // C️⃣ SO SÁNH NGHIỆP VỤ (KHÔNG SO LINK)
                // =================================================
                bool samePoster = infoA.PosterName == reelPost.PosterName && infoA.PosterLink == reelPost.PosterLink;

                bool samePage = true;
                    //infoA.PageLink == reelPost.PageLink;

                double similarity = TextSimilarity.Similarity(
                    infoA.Content,
                    reelPost.Content
                );

                Libary.Instance.LogTech( $"[Reel-Compare] SamePoster={samePoster} | SamePage={samePage} | Similarity={similarity:0.00}");

                // =================================================
                // D️⃣ KẾT LUẬN
                // =================================================
                if (samePoster && samePage && similarity >= 0.7)
                {
                    // ✅ REEL ĐĂNG TRỰC TIẾP TRONG GROUP (1 BÀI)
                    reelPost.PostType = PostType.page_Real_Cap;

                    Libary.Instance.LogTech("[Reel] ✅ GROUP REEL (1 post, NOT share)");

                    result.Posts.Add(reelPost);
                    return result;
                }
                // =================================================
                // E️⃣ SHARE REEL THỰC SỰ
                // =================================================
                Libary.Instance.LogTech("[Reel] 🔁 SHARE REEL (Group)");

                var postA = BuildPostPage(infoA);

                result.Posts.Add(postA);
                result.Posts.Add(reelPost);

                result.Shares.Add(new ShareItem
                {
                    PageLinkA = raw.PageLink,
                    PostLinkB = reelPost.PostLink,
                    ShareTimeRaw = infoA.PostTime,
                    ShareTimeReal = infoA.RealPostTime ?? DateTime.MinValue
                });

                Libary.Instance.LogTech(
                    $"[Share-MAP] A={raw.PageLink} → B={reelPost.PostLink}");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    $"[ParseReelUnknow] ❌ Exception: {ex.Message}");
            }
            return result;
        }
        private async Task FillPosterInfoAsync( PostInfoRawVM info,IPage page,IElementHandle post,IReadOnlyList<IElementHandle> postinfor)
        {
            var posterContainer = postinfor != null && postinfor.Count > 0
                ? postinfor[0]
                : null;

            try
            {
                if (posterContainer != null)
                    (info.PosterName, info.PosterLink) =
                        await CrawlBaseDAO.Instance
                            .GetPosterInfoBySelectorsAsync(posterContainer);
            }
            catch
            {
                (info.PosterName, info.PosterLink) =
                    await CrawlBaseDAO.Instance
                        .GetPosterFromProfileNameAsync(post);
            }

            if (!string.IsNullOrWhiteSpace(info.PosterLink) &&
                info.PosterLink != "N/A")
            {
                (info.PosterNote, info.PosterIdFB) =await CrawlBaseDAO.Instance.CheckTypeCachedAsync(page, info.PosterLink);
            }
        }
        private async Task FillContentAndInteractionNormalAsync(
        PostInfoRawVM info,
        IPage page,
        IElementHandle post,
        IReadOnlyList<IElementHandle> postinfor)
        {
            int c = postinfor?.Count ?? 0;

            if (c >= 3)
            {
                info.Content = await CrawlBaseDAO.Instance.GetContentTextAsync(page, postinfor[2]);

                info.PostType =
                    !string.IsNullOrWhiteSpace(info.Content) &&
                    info.Content != "N/A"
                        ? PostType.Share_WithContent
                        : PostType.Share_NoContent;
            }

            (info.LikeCount,info.CommentCount,info.ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(post);
        }
        private async Task FillFullContentPostNormalAsync(
    PostInfoRawVM info,
    IPage page,
    IElementHandle post,
    IReadOnlyList<IElementHandle> postinfor)
        {
            int c = postinfor?.Count ?? 0;
            Libary.Instance.LogTech($"[FillFullContentPostNormal] postinfor.Count={c}");

            string content = "N/A";
            PostType postType = PostType.Page_Unknow;

            // ========================
            // CASE c == 3
            // ========================
            if (c == 3)
            {
                content = await CrawlBaseDAO.Instance
                    .GetContentTextAsync(page, postinfor[2]);

                if (!ProcessingHelper.IsValidPostPath(content))
                {
                    content = await CrawlBaseDAO.Instance
                        .BackgroundTextAllAsync(page, post);

                    if (ProcessingHelper.IsValidPostPath(content))
                        postType = PostType.Page_BackGround;
                }
                else
                {
                    postType = PostType.Page_Normal;
                }
            }
            // ========================
            // CASE c == 2
            // ========================
            else if (c == 2)
            {
                content = await CrawlBaseDAO.Instance
                    .BackgroundTextAllAsync(page, post);

                postType = ProcessingHelper.IsValidPostPath(content)
                    ? PostType.Page_BackGround
                    : PostType.Page_NoConent;
            }
            // ========================
            // CASE c == 4
            // ========================
            else if (c == 4)
            {
                content = await CrawlBaseDAO.Instance
                    .GetContentTextAsync(page, postinfor[2]);

                if (!ProcessingHelper.IsValidPostPath(content))
                {
                    content = await CrawlBaseDAO.Instance
                        .BackgroundTextAllAsync(page, post);

                    postType = ProcessingHelper.IsValidPostPath(content)
                        ? PostType.Page_BackGround
                        : PostType.Page_BackGround_Nocap;
                }
                else
                {
                    postType = PostType.Page_LinkWeb;
                }
            }
            // ========================
            // CASE OTHER
            // ========================
            else
            {
                content = await CrawlBaseDAO.Instance
                    .BackgroundTextAllAsync(page, post);

                if (ProcessingHelper.IsValidPostPath(content))
                    postType = PostType.Page_BackGround;
            }

            info.Content = content;
            info.PostType = postType;

            Libary.Instance.LogTech(
                $"[FillFullContentPostNormal] ContentLen={(content?.Length ?? 0)} | PostType={postType}");
        }

        private void LogReelDebug(PostPage reel)
        {
            Libary.Instance.LogTech( $"[Reel-B] Link={reel.PostLink} | Poster={reel.PosterName} | Page={reel.PageName}");
        }
        private PostPage BuildPostPage(PostInfoRawVM info)
        {
            return new PostPage
            {
                PostLink = info.PostLink,
                PostTime = info.PostTime,
                RealPostTime = info.RealPostTime,

                PosterName = info.PosterName,
                PosterLink = info.PosterLink,
                PosterNote = info.PosterNote,

                PageName = info.PageName,
                PageLink = info.PageLink,

                Content = info.Content,

                LikeCount = info.LikeCount,
                CommentCount = info.CommentCount,
                ShareCount = info.ShareCount,

                PostType = info.PostType
            };
        }

        private async void btn_TestPopup_Click(object sender, EventArgs e)
        {
            string profileId = txbProfileId.Text.Trim();
            string url = txbUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Không có URL");
                return;
            }
            string urlgoc = url;
            var page = await Ads.Instance.OpenNewTabAsync(profileId);
            Libary.Instance.SetProfileContext(profileId, "profile.ProfileName");
            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = AppConfig.DEFAULT_TIMEOUT,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            // scroll nhẹ 1–2 lần
            await page.EvaluateAsync(@"() => {window.scrollBy(0, document.body.scrollHeight);}");
            await page.WaitForTimeoutAsync(1200);

            await page.EvaluateAsync(@"() => {window.scrollBy(0, document.body.scrollHeight);}");
            await page.WaitForTimeoutAsync(1200);
            AppendLog("");
            AppendLog("══════════════════════════════════════════════════════");
            AppendLog("🧪 TEST POPUP FULL");
            AppendLog("══════════════════════════════════════════════════════");
            var postDiv = await CrawlBaseDAO.Instance.GetFeedPostOriginalNormalAsync(page);
            if (postDiv != null)
            {
                AppendLog("tìm thấy PostDiv");

            }
            else
            {
                AppendLog("tìm thấy PostDiv");
            }
            var postinfor = await postDiv.QuerySelectorAllAsync("div.xu06os2.x1ok221b");

            string timeText = null;

            foreach (var el in postinfor)
            {
                var txt = (await el.InnerTextAsync())?.Trim().ToLower();

                if (string.IsNullOrEmpty(txt))
                    continue;

                if (ProcessingDAO.Instance.IsTime(txt))
                {
                    timeText = TimeHelper.CleanTimeString(txt);

                    AppendLog($"{Libary.IconOK} [ORIGINAL POST] lấy time đầu tiên: {timeText}");

                    break; // 🔥 QUAN TRỌNG: dừng ngay
                }
            }
            var (LikeCount, CommentCount, ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(postDiv);
            AppendLog($"Lấy tương tác: Like {LikeCount}, Comment {CommentCount}, Share {ShareCount}");
            var content = await PopupDAO.Instance.GetContentPopup(postDiv);
            AppendLog($"Lấy nội dung: {content}");
            List<(string Src, string Alt)> photos = await CrawlBaseDAO.Instance.DetectPhotosFromPostAsync(postDiv);
            foreach (var p in photos)
            {
                AppendLog($"Ảnh: {p.Src} | ALT: {p.Alt}");
                if (string.IsNullOrWhiteSpace(content))
                {
                    content = ""; // 🔥 đảm bảo không null                                 
                        if (!string.IsNullOrWhiteSpace(p.Alt))
                        {
                            content += (content.Length == 0 ? "" : "\n") + p.Alt;
                        }                   
                }
            }              
            // ===============================
            // 👤 POSTER
            // ===============================
            string PageName = "", PageLink = "", PageID = "", ContainerIdFB= "";
            string PosterName, PosterLink, PosterIdFB, PosterNote = "";
            //lấy kết quả từ crawl
            var (nametemp, linktemp) = await PopupDAO.Instance.GetPosterPopup(postDiv);
            // check type nó
            var (containerType, idfbContainer) = await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(page, linktemp);
            AppendLog($"ContainerType: {containerType}");
            // Gán IDFB container
            // xem có phải Page trong DB k
            if (!string.IsNullOrWhiteSpace(idfbContainer) && idfbContainer != "N/A") 
            {
                var dbPage = SQLDAO.Instance.GetPageByIDFB(idfbContainer);
                if (dbPage != null)
                {
                    PageID = dbPage.PageID;
                    PageName = dbPage.PageName;
                    PageLink = dbPage.PageLink;
                    ContainerIdFB = dbPage.IDFBPage;

                    AppendLog($"Page đã có trong DB {PageName}");
                }
            }
            if (containerType == FBType.Fanpage)
            {
                PosterName = nametemp;
                PosterLink = linktemp;
                PosterIdFB = ContainerIdFB;
                PosterNote = FBType.Fanpage.ToString();
                PageName = nametemp;
                PageLink = linktemp;
                AppendLog($"Page là Fanpage container là poster luôn {PageName} // PageLink {PageLink}");
            }
            else if (containerType == FBType.GroupOn)
            {
                (PosterName, PosterLink) = await PopupDAO.Instance.GetPosterGroupsPopupPost(postDiv);

                var (posterType, idfbPoster) = await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(page, PosterLink);

                if (posterType != FBType.Unknown) AppendLog($"PosterNote: không xác định");

                if (!string.IsNullOrWhiteSpace(idfbPoster) && idfbPoster != "N/A") PosterIdFB = idfbPoster;
                AppendLog($"Page là Group container: {PageName} // PageLink {PageLink}");
                AppendLog($"Poster người đăng là: {PosterName} // Link {PosterLink} // idfb {idfbPoster}");
            }

            // 🔥 PERSON (THIẾU – BỔ SUNG)
            // ========================
            else if (containerType == FBType.Person ||
                 containerType == FBType.PersonKOL)
            {
                // ❗ container không tồn tại
              PageName = "N/A";
              PageLink = "N/A";
              ContainerIdFB = null;
              PosterName = nametemp.ToString();
               PosterLink = linktemp;
                // poster chính là user
                if (ProcessingHelper.IsValidContent(linktemp))
                {
                    var (posterType, idfbPoster) =
                        await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(page, linktemp);

                    if (posterType != FBType.Unknown)
                        PosterNote = posterType.ToString();

                    if (ProcessingHelper.IsValidContent(idfbPoster))
                        PosterIdFB = idfbPoster;
                }

                AppendLog($"không phải PAge mà person {PosterName} // Link {PosterLink}");
            }
           
        }
    }
}
