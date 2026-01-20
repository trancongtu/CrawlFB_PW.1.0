using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;

namespace CrawlFB_PW._1._0.DAO
{
    public class AdsPowerHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Lấy WebSocket endpoint của profile AdsPower
        /// </summary>
        /// 
        public static async Task<string> GetWsEndpointAsync(string profileId, bool headless = false)
        {
            try
            {
                string mode = headless ? "&headless=1" : "";
                string url = $"http://127.0.0.1:50325/api/v1/browser/start?user_id={profileId}{mode}";
                string response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                return json["data"]?["ws"]?["puppeteer"]?.ToString();
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService("❌ AdsPower start profile FAIL (" + profileId + "): " + ex.Message);

                return null;
            }
        }
        /// <summary>
        /// Mở profile AdsPower và chỉ giữ đúng 1 tab Facebook thật (đã login)
        /// </summary>
        public static async Task<IPage> OpenAdsPowerProfileAsync(string profileId)
        {
            string ws = await GetWsEndpointAsync(profileId);
            if (string.IsNullOrEmpty(ws))
                throw new Exception($"Không lấy được wsEndpoint cho profile {profileId}");

            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.ConnectOverCDPAsync(ws);

            // Đợi profile load ổn định
            await Task.Delay(2000);

            // Lấy toàn bộ tab đang mở
            var allPages = browser.Contexts.SelectMany(c => c.Pages).ToList();
            Console.WriteLine($"📑 Profile {profileId} có {allPages.Count} tab.");

            // 🔹 Xóa tab AdsPower mặc định (start.adspower.net)
            foreach (var p in allPages.Where(p => p.Url != null && p.Url.Contains("start.adspower.net")))
            {
                try { await p.CloseAsync(); } catch { }
            }

            // Cập nhật lại danh sách tab sau khi xóa
            allPages = browser.Contexts.SelectMany(c => c.Pages).ToList();

            // 🔹 Tìm tab Facebook (ưu tiên tab đang login)
            var fbPage = allPages.FirstOrDefault(p =>
                p.Url != null && p.Url.ToLower().Contains("facebook.com"));

            // Nếu có nhiều tab facebook → giữ 1, đóng các tab còn lại
            var fbTabs = allPages.Where(p => p.Url != null && p.Url.ToLower().Contains("facebook.com")).ToList();
            if (fbTabs.Count > 1)
            {
                fbPage = fbTabs.First();
                foreach (var extra in fbTabs.Skip(1))
                {
                    try { await extra.CloseAsync(); } catch { }
                }
            }

            // Nếu chưa có tab FB nào → mở mới
            if (fbPage == null)
            {
                var context = browser.Contexts.FirstOrDefault() ?? await browser.NewContextAsync();
                fbPage = await context.NewPageAsync();
                await fbPage.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                {
                    Timeout = 20000,
                    WaitUntil = WaitUntilState.NetworkIdle
                });
            }

            Console.WriteLine($"✅ Đã gắn tab Facebook: {fbPage.Url}");
            await fbPage.BringToFrontAsync();
            return fbPage;
        }
        // hàm sau xóa hết tab cũ
        public static async Task<IPage> ResetProfileToFacebookAsync(string profileId)
        {
            try
            {
                string ws = await GetWsEndpointAsync(profileId);
                if (string.IsNullOrEmpty(ws))
                    throw new Exception($"Không lấy được wsEndpoint cho profile {profileId}");

                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.ConnectOverCDPAsync(ws);

                // Đợi profile load đầy đủ (đảm bảo context đã có)
                await Task.Delay(2000);

                var allPages = browser.Contexts.SelectMany(c => c.Pages).ToList();
                if (allPages.Count == 0)
                {
                    // Nếu chưa có tab nào -> tạo mới
                    var ctx = browser.Contexts.FirstOrDefault() ?? await browser.NewContextAsync();
                    var newPage = await ctx.NewPageAsync();
                    await newPage.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                    {
                        Timeout = 20000,
                        WaitUntil = WaitUntilState.NetworkIdle
                    });
                    return newPage;
                }

                // 🔹 Giữ lại tab đầu tiên (thường là start.adspower.net)
                var keepPage = allPages.First();

                // 🔹 Đóng tất cả tab khác
                foreach (var p in allPages.Skip(1))
                {
                    try { await p.CloseAsync(); } catch { }
                }

                // 🔹 Chuyển tab giữ lại về facebook.com
                try
                {
                    await keepPage.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                    {
                        Timeout = 20000,
                        WaitUntil = WaitUntilState.NetworkIdle
                    });
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogService($"[adsPowerHelper] - ⚠️ Lỗi khi load facebook.com: {ex.Message}");
                }

                await keepPage.WaitForTimeoutAsync(1500);
                Libary.Instance.LogService($"[adsPowerHelper]-✅ Reset profile {profileId} -> Chuyển tab chính về https://facebook.com/");
                return keepPage;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService($"[adsPowerHelper] ❌ [ResetProfileToFacebookAsync] lỗi: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Kiểm tra đăng nhập Facebook (ổn định cho layout 2025)
        /// </summary>
        public static async Task<bool> CheckFacebookLoginAsync(IPage page)
        {
            try
            {
                if (string.IsNullOrEmpty(page.Url) || !page.Url.Contains("facebook.com"))
                {
                    await page.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                    {
                        Timeout = 20000,
                        WaitUntil = WaitUntilState.NetworkIdle
                    });
                }

                await page.WaitForTimeoutAsync(1500);

                // Redirect về login
                if (page.Url.Contains("login") || page.Url.Contains("checkpoint"))
                    return false;

                // Layout tiếng Việt / tiếng Anh / mới
                string[] selectors =
                {
                    "div[role='feed']",
                    "a[aria-label*='Trang chủ']",
                    "a[aria-label*='Home']",
                    "div[aria-label*='Tạo bài viết']",
                    "div[aria-label*='Create a post']"
                };

                foreach (var sel in selectors)
                {
                    var el = await page.QuerySelectorAsync(sel);
                    if (el != null)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService("[adshelper]⚠️ Lỗi check login: " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Lấy tên Facebook thật (ổn định cho FB 2024–2025)
        /// </summary>
        public static async Task<string> GetFacebookNameAsync(IPage page)
        {
            try
            {
                // Nếu chưa vào Facebook thì vào trước
                if (string.IsNullOrEmpty(page.Url) || !page.Url.Contains("facebook.com"))
                {
                    await page.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                    {
                        Timeout = 45000,
                        WaitUntil = WaitUntilState.DOMContentLoaded
                    });
                }

                await page.WaitForTimeoutAsync(1500);

                // --- Nếu đã ở trang profile thì chỉ đọc h1 thôi, không goto nữa
                if (page.Url.Contains("profile.php?id=") || page.Url.Contains("/people/"))
                {
                    var nameNode = await page.QuerySelectorAsync("div[class='x1e56ztr x1xmf6yo'] > span > h1, div.x1e56ztr.x1xmf6yo span h1");
                    if (nameNode != null)
                    {
                        string name = await nameNode.InnerTextAsync();
                        if (!string.IsNullOrWhiteSpace(name))
                            return name.Trim();
                    }
                    return "(Không đọc được tên)";
                }

                // --- Tìm link profile cá nhân
                var linkNode = await page.QuerySelectorAsync("div[class='x1cy8zhl x78zum5 x1iyjqo2 xs83m0k xh8yej3'] > a");
                if (linkNode == null)
                    linkNode = await page.QuerySelectorAsync("div.x1cy8zhl.x78zum5.x1iyjqo2.xs83m0k.xh8yej3 a");

                if (linkNode == null)
                    return "(Không tìm thấy link profile)";

                string href = await linkNode.GetAttributeAsync("href");
                if (string.IsNullOrEmpty(href))
                    return "(Không có href)";

                // Nếu link rút gọn, thêm prefix
                if (!href.StartsWith("https://"))
                    href = "https://www.facebook.com" + href;

                // --- Nếu đang ở đúng link này rồi, khỏi goto lại
                if (page.Url.TrimEnd('/') == href.TrimEnd('/'))
                {
                    var h1Node = await page.QuerySelectorAsync("div[class='x1e56ztr x1xmf6yo'] > span > h1, div.x1e56ztr.x1xmf6yo span h1");
                    if (h1Node != null)
                    {
                        string name = await h1Node.InnerTextAsync();
                        if (!string.IsNullOrWhiteSpace(name))
                            return name.Trim();
                    }
                    return "(Không đọc được tên)";
                }

                // --- Dùng Evaluate để đổi URL nhanh hơn Goto (tránh timeout)
                await page.EvaluateAsync($"window.location.href = '{href}'");
                await page.WaitForURLAsync("**/profile.php?id=*", new PageWaitForURLOptions { Timeout = 40000 });

                // --- Chờ DOM load h1
                await page.WaitForSelectorAsync("div[class='x1e56ztr x1xmf6yo'] > span > h1", new PageWaitForSelectorOptions { Timeout = 15000 });

                var nameNode2 = await page.QuerySelectorAsync("div[class='x1e56ztr x1xmf6yo'] > span > h1, div.x1e56ztr.x1xmf6yo span h1");
                if (nameNode2 != null)
                {
                    string name = await nameNode2.InnerTextAsync();
                    if (!string.IsNullOrWhiteSpace(name))
                        return name.Trim();
                }

                return "(Không đọc được tên)";
            }
            catch (Exception ex)
            {
                return "(Lỗi lấy tên FB: " + ex.Message + ")";
            }
        }
        public static async Task<bool> OpenFacebookLinkSafeAsync(IPage page,string url, int retry = 1)
        {
            if (page == null || page.IsClosed || string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                // ===============================
                // 1️⃣ VÀO FEED TẠO CONTEXT
                // ===============================
                Libary.Instance.LogDebug($"[SAFE-OPEN] 🌐 Goto feed");

                await page.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = AppConfig.DEFAULT_TIMEOUT
                });

                await page.WaitForTimeoutAsync(800);

                // scroll nhẹ như người dùng
                await ProcessingDAO.Instance.HumanScrollAsync(page);
                await page.WaitForTimeoutAsync(500);

                // ===============================
                // 2️⃣ MỞ LINK BẰNG window.location
                // ===============================
                Libary.Instance.LogDebug($"[SAFE-OPEN] 🔗 Open link: {url}");

                await page.EvaluateAsync(
                    @"(u) => { window.location.href = u; }",
                    url
                );

                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await page.WaitForTimeoutAsync(800);

                // ===============================
                // 3️⃣ CHECK TRANG LỖI FACEBOOK
                // ===============================
                bool isBroken = await IsFacebookBrokenPageAsync(page);

                if (!isBroken)
                {
                    Libary.Instance.LogDebug("[SAFE-OPEN] ✅ Page opened OK");
                    return true;
                }

                Libary.Instance.LogTech(
                    "⚠ [SAFE-OPEN] Facebook trả về trang lỗi"
                );

                // ===============================
                // 4️⃣ RETRY (NẾU CÒN)
                // ===============================
                if (retry > 0)
                {
                    Libary.Instance.LogTech(
                        $"🔁 [SAFE-OPEN] Retry mở link ({retry})"
                    );

                    await page.WaitForTimeoutAsync(1000);
                    return await OpenFacebookLinkSafeAsync(page, url, retry - 1);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    $"❌ [SAFE-OPEN] Lỗi mở link: {ex.Message}"
                );
            }

            return false;
        }

        // ===============================
        // 🔧 CHECK FACEBOOK BROKEN PAGE
        // ===============================
        private static async Task<bool> IsFacebookBrokenPageAsync(IPage page)
        {
            try
            {
                return await page.Locator("text=Trang này hiện không hiển thị").CountAsync() > 0
                    || await page.Locator("text=This page isn't available").CountAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
