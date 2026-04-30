using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;

namespace CrawlFB_PW._1._0.DAO
{
    /// <summary>
    /// Singleton manager:
    /// - Quản lý 1 session Playwright (browser/context/page) per AdsPower profileId
    /// - Nếu session tồn tại và page chưa đóng => tái sử dụng
    /// - Nếu không => gọi API AdsPower để start profile, kết nối CDP, trả IPage
    /// NOTE: compatible .NET Framework 4.8 (C#7.3) -> dùng Dispose() không DisposeAsync()
    /// </summary>
    public class AdsPowerPlaywrightManager
    {
        private static readonly Lazy<AdsPowerPlaywrightManager> _instance =
            new Lazy<AdsPowerPlaywrightManager>(() => new AdsPowerPlaywrightManager());
        public static AdsPowerPlaywrightManager Instance { get { return _instance.Value; } }

        private static readonly HttpClient _http = new HttpClient();

        public class BrowserSession
        {
            public IPlaywright Playwright;
            public IBrowser Browser;
            public IBrowserContext Context;
            public IPage Page;
            public string ProfileId;
            public string WsUrl;
            public DateTime LastUsed;
        }

        private readonly Dictionary<string, BrowserSession> _sessions = new Dictionary<string, BrowserSession>();

        private AdsPowerPlaywrightManager() { }

        /// <summary>
        /// Lấy IPage cho profileId. Nếu session tồn tại thì reuse, nếu không tạo mới.
        /// Gọi nội bộ API AdsPower start profile để lấy WS (nếu cần).
        /// </summary>
        /*  public async Task<IPage> GetPageAsync(string profileId)
          {
              try
              {
                  lock (_sessions)
                  {
                      if (_sessions.ContainsKey(profileId))
                      {
                          var s = _sessions[profileId];
                          if (s.Page != null && !s.Page.IsClosed)
                          {
                              s.LastUsed = DateTime.Now;
                              Libary.Instance.CreateLog($"[Manager] 🔁 Reuse page for {profileId}");
                              return s.Page;
                          }
                      }
                  }

                  // Nếu chưa có session hoặc đã đóng -> gọi API lấy WS rồi tạo mới
                  string ws = await GetWsEndpointAsync(profileId);
                  if (string.IsNullOrEmpty(ws))
                  {
                      Libary.Instance.CreateLog("[Manager] ❌ GetPageAsync: WS endpoint null");
                      return null;
                  }

                  var playwright = await Playwright.CreateAsync();
                  var browser = await playwright.Chromium.ConnectOverCDPAsync(ws);
                  var context = browser.Contexts.FirstOrDefault() ?? await browser.NewContextAsync();
                  var page = await context.NewPageAsync();
                  // ⚙️ Ép viewport nhỏ để Facebook hiển thị text thay vì icon
                  await page.SetViewportSizeAsync(980, 900);
                  var session = new BrowserSession
                  {
                      Playwright = playwright,
                      Browser = browser,
                      Context = context,
                      Page = page,
                      ProfileId = profileId,
                      WsUrl = ws,
                      LastUsed = DateTime.Now
                  };

                  lock (_sessions)
                  {
                      _sessions[profileId] = session;
                  }

                  Libary.Instance.CreateLog($"[Manager] ✅ Created session for {profileId}");
                  return page;
              }
              catch (Exception ex)
              {
                  Libary.Instance.CreateLog($"[Manager] Exception GetPageAsync: {ex.Message}");
                  return null;
              }
          }
        */
        public async Task<IPage> GetPageAsync(string profileId)
        {
            try
            {
                // 🔹 1️⃣ Reuse session nếu có
                lock (_sessions)
                {
                    if (_sessions.ContainsKey(profileId))
                    {
                        var s = _sessions[profileId];
                        if (s.Page != null && !s.Page.IsClosed)
                        {
                            s.LastUsed = DateTime.Now;
                            Libary.Instance.LogService($"[Manager] 🔁 Reuse page for {profileId}");
                            return s.Page;
                        }
                    }
                }

                // 🛡 0️⃣ Check AdsPower sống hay chết
                if (!await CheckAdsPowerAliveAsync())
                {
                    Libary.Instance.LogService("⛔ AdsPower chưa sẵn sàng → bỏ qua profile " + profileId);
                    return null;
                }

                // 🔹 1️⃣ Lấy WS
                string ws = await GetWsEndpointAsync(profileId);
                if (string.IsNullOrEmpty(ws))
                {
                    Libary.Instance.LogService("❌ Không lấy được WS cho profile " + profileId);
                    return null;
                }
                // 🔹 3️⃣ Kết nối Playwright qua CDP (không tạo context mới)
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.ConnectOverCDPAsync(ws);

                // Lấy context có sẵn — KHÔNG tạo NewContextAsync (tránh spawn tab thừa)
                var context = browser.Contexts.FirstOrDefault();
                if (context == null)
                {
                    Libary.Instance.LogService($"[Manager] ⚠️ Không tìm thấy context sẵn, tạo context mới cho {profileId}");
                    context = await browser.NewContextAsync();
                }

                // 🔹 4️⃣ Lấy tab gốc (about:blank) của AdsPower — KHÔNG tạo tab mới
                var page = context.Pages.FirstOrDefault();
                if (page == null)
                {
                    Libary.Instance.LogService($"[Manager] ⚠️ Context trống, tạo page mới cho {profileId}");
                    page = await context.NewPageAsync();
                }

                // 🔹 5️⃣ Nếu tab gốc đang ở about:blank → điều hướng về Facebook
                try
                {
                    if (string.IsNullOrEmpty(page.Url) || page.Url == "about:blank")
                    {
                        await page.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                        {
                            Timeout = 20000,
                            WaitUntil = WaitUntilState.NetworkIdle
                        });
                        Libary.Instance.LogService($"[Manager] 🌐 Tab gốc chuyển về Facebook cho {profileId}");
                    }
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogService($"[Manager] ⚠️ Lỗi chuyển tab gốc về Facebook ({profileId}): {ex.Message}");
                }

                // 🔹 6️⃣ Cấu hình viewport
                try { await page.SetViewportSizeAsync(980, 900); } catch { }

                // 🔹 7️⃣ Lưu session lại
                var session = new BrowserSession
                {
                    Playwright = playwright,
                    Browser = browser,
                    Context = context,
                    Page = page,
                    ProfileId = profileId,
                    WsUrl = ws,
                    LastUsed = DateTime.Now
                };

                lock (_sessions)
                {
                    _sessions[profileId] = session;
                }

                Libary.Instance.LogService($"[Manager] ✅ Created session for {profileId} (Reuse tab gốc, không spawn tab mới)");
                return page;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService($"[Manager] ❌ Exception GetPageAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gọi API AdsPower start profile & trả puppeteer ws endpoint.
        /// NOTE: nếu AdsPower server của bạn ở cổng khác, chỉnh URL tại đây.
        /// </summary>
        private async Task<string> GetWsEndpointAsync(string profileId)
        {
            try
            {
                string mode = AppConfig.HEADLESS_MODE ? "&headless=1" : "";
                string url = $"http://127.0.0.1:50325/api/v1/browser/start?user_id={profileId}{mode}";

                var res = await _http.GetStringAsync(url);
                var jobj = JObject.Parse(res);

                if (jobj["code"]?.ToString() == "0")
                {
                    var ws = jobj["data"]?["ws"]?["puppeteer"]?.ToString();
                    if (!string.IsNullOrEmpty(ws))
                        return ws;
                }

                Libary.Instance.LogService("[Manager] API error: " + res);
                return null;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService("[Manager] GetWsEndpointAsync exception: " + ex.Message);
                return null;
            }
        }
        // kiểm tra ADS
        private async Task<bool> CheckAdsPowerAliveAsync()
        {
            try
            {
                var res = await _http.GetStringAsync("http://127.0.0.1:50325/status");
                return res.Contains("success");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService("❌ AdsPower API không phản hồi: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Đóng session 1 profile (gọi khi user stop hoặc cleanup)
        /// </summary>
        public async Task CloseProfileAsync(string profileId)
        {
            try
            {
                if (!_sessions.ContainsKey(profileId)) return;
                var s = _sessions[profileId];

                try { if (s.Page != null) await s.Page.CloseAsync(); } catch { }
                try { if (s.Context != null) await s.Context.CloseAsync(); } catch { }
                try { if (s.Browser != null) await s.Browser.CloseAsync(); } catch { }
                try { if (s.Playwright != null) s.Playwright.Dispose(); } catch { }

                lock (_sessions)
                {
                    _sessions.Remove(profileId);
                }

                Libary.Instance.CreateLog($"[Manager] Closed session {profileId}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"[Manager] CloseProfileAsync exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Dọn session lâu không dùng (mặc định >10 phút)
        /// </summary>
        public async Task CleanupAsync()
        {
            List<string> removeKeys = new List<string>();
            lock (_sessions)
            {
                foreach (var kv in _sessions)
                {
                    if ((DateTime.Now - kv.Value.LastUsed).TotalMinutes > 10)
                        removeKeys.Add(kv.Key);
                }
            }

            foreach (var k in removeKeys)
                await CloseProfileAsync(k);
        }
        /// <summary>
        /// Kiểm tra đã đăng nhập Facebook chưa (ổn định cho layout 2025)
        /// </summary>
        public async Task<bool> CheckFacebookLoginAsync(string profileId)
        {
            var page = await GetPageAsync(profileId);
            if (page == null) return false;

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

                // Nếu đang bị redirect về login / checkpoint
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
                Libary.Instance.CreateLog($"[Manager] ⚠️ CheckFacebookLoginAsync lỗi: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> CheckFacebookLoginAsync(IPage page)
        {
            if (page == null) return false;

            try
            {
                await page.WaitForTimeoutAsync(300); // Cho DOM ổn định nhẹ

                string url = page.Url?.ToLower() ?? "";

                // 1️⃣ Nếu đang ở login / checkpoint → Chắc chắn chưa login
                if (url.Contains("login") || url.Contains("checkpoint") || url.Contains("recover"))
                    return false;

                // 2️⃣ Nếu xuất hiện form đăng nhập
                var loginForm = await page.QuerySelectorAsync("input#email, input[name='email'], button[name='login']");
                if (loginForm != null)
                    return false;

                // 3️⃣ Nếu có nút Tạo bài viết → chắc chắn login
                var createPost = await page.QuerySelectorAsync(
                    "div[aria-label='Tạo bài viết'], div[aria-label='Create a post']"
                );
                if (createPost != null)
                    return true;

                // 4️⃣ Có feed → login
                var feed = await page.QuerySelectorAsync("div[role='feed']");
                if (feed != null)
                    return true;

                // 5️⃣ Navigation bar của người dùng đã login
                var home = await page.QuerySelectorAsync(
                    "a[aria-label='Trang chủ'], a[aria-label='Home'], [aria-label='Messenger'], [aria-label='Thông báo']"
                );
                if (home != null)
                    return true;

                // 6️⃣ Nếu URL là facebook.com mà không phải login → gần như chắc chắn login
                if (url.Contains("facebook.com") && !url.Contains("login"))
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy tên Facebook thật (ổn định cho FB 2024–2025)
        /// </summary>
        public async Task<string> GetFacebookNameAsyncold(string profileId)
        {
            var page = await GetPageAsync(profileId);
            if (page == null) return "(Không đọc được tên)";

            try
            {
                if (string.IsNullOrEmpty(page.Url) || !page.Url.Contains("facebook.com"))
                {
                    await page.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                    {
                        Timeout = 45000,
                        WaitUntil = WaitUntilState.DOMContentLoaded
                    });
                }

                await page.WaitForTimeoutAsync(1500);

                // --- Nếu đã ở trang profile thì chỉ đọc h1
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

                if (!href.StartsWith("https://"))
                    href = "https://www.facebook.com" + href;

                // --- Nếu đang ở đúng link rồi thì đọc h1 luôn
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

                // --- Dùng Evaluate để điều hướng nhanh
                await page.EvaluateAsync($"window.location.href = '{href}'");
                await page.WaitForURLAsync("**/profile.php?id=*", new PageWaitForURLOptions { Timeout = 40000 });
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
        public async Task<string> GetFacebookNameAsync(IPage page)
        {
            if (page == null)
                return "N/A";

            try
            {
                // QuerySelectorAsync phải await
                var elename = await page.QuerySelectorAsync("div.x1e56ztr.x1xmf6yo > span");

                if (elename == null)
                    return "N/A";

                string name = (await elename.InnerTextAsync() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(name))
                    return "N/A";

                return name;
            }
            catch (Exception ex)
            {
                return "(Lỗi lấy tên FB: " + ex.Message + ")";
            }
        }
        public async Task<string> GetFacebookLinkAsync(IPage page)
        {
            try
            {
                // 1) Tìm đúng DIV nút tạo bài viết
                var node = await page.QuerySelectorAsync("div[aria-label='Tạo bài viết']");
                if (node == null)
                    return "(Không tìm thấy nút Tạo bài viết)";

                // 2) Từ node này, tìm thẻ <a> bên trong nó
                var linkNode = await node.QuerySelectorAsync("a[href]");
                if (linkNode != null)
                {
                    string href = await linkNode.GetAttributeAsync("href");
                    if (!string.IsNullOrWhiteSpace(href))
                        return ProcessingDAO.Instance.ExtractFbShortLink(href);
                }

                return "(Không tìm thấy thẻ <a> bên trong Tạo bài viết)";
            }
            catch (Exception ex)
            {
                return "(Lỗi lấy link: " + ex.Message + ")";
            }
        }       
        public async Task CloseAllTabsExceptOneAsync(string profileId)
        {
            if (!_sessions.ContainsKey(profileId)) return;
            var s = _sessions[profileId];
            var browser = s.Browser;
            var allPages = browser.Contexts.SelectMany(c => c.Pages).ToList();

            foreach (var p in allPages.Skip(1))
            {
                try { await p.CloseAsync(); } catch { }
            }

            var firstPage = allPages.FirstOrDefault();
            if (firstPage != null)
            {
                await firstPage.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                {
                    Timeout = 20000,
                    WaitUntil = WaitUntilState.NetworkIdle
                });
            }

            Libary.Instance.LogService($"[Manager] 🔄 Reset {profileId} còn 1 tab Facebook");
        }
        //hàm hỗ trợ bật tab mới lấy link
      
        //---xóa hết tab giữ 1
        public async Task<bool> CloseExtraTabsAsync(string profileId, bool forceKill = false)
        {
            try
            {
                BrowserSession session = null;

                // 0️⃣ Lấy session ra khỏi lock (không await trong lock)
                lock (_sessions)
                {
                    if (_sessions.ContainsKey(profileId))
                        session = _sessions[profileId];
                }

                // --- 1️⃣ Nếu có session Playwright -> đóng trực tiếp
                if (session != null)
                {
                    try
                    {
                        var browser = session.Browser;
                        if (browser != null)
                        {
                            var allPages = browser.Contexts.SelectMany(c => c.Pages).ToList();
                            if (allPages.Count > 1)
                            {
                                // đóng tất cả trừ tab đầu tiên
                                for (int i = 1; i < allPages.Count; i++)
                                {
                                    try { await allPages[i].CloseAsync(); }
                                    catch (Exception ex)
                                    {
                                        Libary.Instance.LogService($"[Manager] ⚠️ Lỗi đóng tab phụ trong session {profileId}: {ex.Message}");
                                    }
                                    await Task.Delay(120);
                                }
                            }

                            // reload tab đầu tiên
                            var first = allPages.FirstOrDefault();
                            if (first != null)
                            {
                                try
                                {
                                    await first.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                                    {
                                        Timeout = 20000,
                                        WaitUntil = WaitUntilState.NetworkIdle
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Libary.Instance.LogService($"[Manager] ⚠️ Lỗi reload tab gốc {profileId}: {ex.Message}");
                                }
                            }

                            Libary.Instance.LogService($"[Manager] ✅ (Playwright) Đã đóng tab phụ cho session {profileId}, giữ lại tab gốc.");
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogService($"[Manager] ⚠️ Lỗi xử lý session {profileId}: {ex.Message}");
                        // tiếp tục fallback sang AdsPower API
                    }
                }

                // --- 2️⃣ Fallback: gọi AdsPower API
                using (var client = new HttpClient())
                {
                    var resp = await client.GetAsync($"http://local.adspower.net:50325/api/v1/browser/tabs?profile_id={profileId}");
                    string json = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode || string.IsNullOrWhiteSpace(json))
                    {
                        Libary.Instance.LogService($"[Manager] ⚠️ API tabs lỗi hoặc rỗng ({profileId})");
                        return false;
                    }

                    JToken root;
                    try { root = JToken.Parse(json); }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogService($"[Manager] ⚠️ JSON parse lỗi ({profileId}): {ex.Message} | raw: {json}");
                        return false;
                    }

                    if (root["code"]?.Value<int>() != 0)
                    {
                        string msg = root["msg"]?.ToString() ?? "";
                        if (msg.Contains("Profile does not exist"))
                        {
                            Libary.Instance.LogService($"[Manager] ℹ️ Profile {profileId} không tồn tại → coi như sạch tab.");
                            return true;
                        }
                        Libary.Instance.LogService($"[Manager] ⚠️ API tabs code != 0 ({profileId}): {json}");
                        return false;
                    }

                    var tabs = root["data"] as JArray;
                    if (tabs == null || tabs.Count == 0)
                    {
                        Libary.Instance.CreateLog($"[Manager] 🟢 Không có tab nào mở ({profileId})");
                        return true;
                    }

                    int closed = 0;
                    foreach (var tab in tabs)
                    {
                        bool isDefault = tab["is_default"]?.Value<bool>() == true;
                        string tabId = tab["tab_id"]?.ToString() ?? "";

                        if (!isDefault && !string.IsNullOrEmpty(tabId))
                        {
                            try
                            {
                                await client.GetAsync($"http://local.adspower.net:50325/api/v1/browser/close_tab?profile_id={profileId}&tab_id={tabId}");
                                closed++;
                                await Task.Delay(150);
                            }
                            catch (Exception ex)
                            {
                                Libary.Instance.LogService($"[Manager] ⚠️ Lỗi đóng tab API ({profileId}/{tabId}): {ex.Message}");
                            }
                        }
                    }

                    Libary.Instance.LogService($"[Manager] ✅ Đóng {closed}/{tabs.Count} tab, giữ lại tab gốc ({profileId})");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService($"[Manager] ❌ CloseExtraTabsAsync exception ({profileId}): {ex.Message}");
                return false;
            }
        }
        //----hàm đếm tab
        public async Task<int> CountTabsAsync(string profileId)
        {
            try
            {
                // 1️⃣ Nếu đang có session Playwright => đếm trực tiếp
                BrowserSession session = null;
                lock (_sessions)
                {
                    if (_sessions.ContainsKey(profileId))
                        session = _sessions[profileId];
                }

                if (session != null && session.Browser != null)
                {
                    try
                    {
                        var count = session.Browser.Contexts.SelectMany(c => c.Pages).Count();
                        Libary.Instance.LogService($"[Manager] 🔍 CountTabsAsync({profileId}) = {count} (qua Playwright)");
                        return count;
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogService($"[Manager] ⚠️ Lỗi đếm tab Playwright {profileId}: {ex.Message}");
                    }
                }

                // 2️⃣ Fallback: gọi API AdsPower nếu không có session
                using (var client = new HttpClient())
                {
                    var resp = await client.GetAsync($"http://local.adspower.net:50325/api/v1/browser/tabs?profile_id={profileId}");
                    string json = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        Libary.Instance.LogService($"[Manager] ⚠️ API tabs lỗi ({resp.StatusCode}) cho {profileId}");
                        return 0;
                    }

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Libary.Instance.LogService($"[Manager] ⚠️ API tabs trả rỗng ({profileId})");
                        return 0;
                    }

                    JToken root;
                    try { root = JToken.Parse(json); }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogService($"[Manager] ⚠️ Parse JSON lỗi ({profileId}): {ex.Message}");
                        return 0;
                    }

                    if (root["code"]?.Value<int>() != 0)
                    {
                        string msg = root["msg"]?.ToString() ?? "";
                        if (msg.Contains("Profile does not exist"))
                        {
                            Libary.Instance.LogService($"[Manager] ℹ️ Profile {profileId} không tồn tại → coi như 0 tab.");
                            return 0;
                        }
                        Libary.Instance.LogService($"[Manager] ⚠️ API tabs code != 0 ({profileId}): {json}");
                        return 0;
                    }

                    var tabs = root["data"] as JArray;
                    int countTabs = tabs?.Count ?? 0;

                    Libary.Instance.LogService($"[Manager] 📊 CountTabsAsync({profileId}) = {countTabs}");
                    return countTabs;
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService($"[Manager] ❌ CountTabsAsync exception ({profileId}): {ex.Message}");
                return 0;
            }
        }
        public async Task<IPage> GetPageEnsureSingleTabAsync(string profileId)
        {
            // 1️⃣ Lấy page như bình thường (reuse hoặc tạo mới)
            var page = await GetPageAsync(profileId);
            if (page == null)
                return null;

            try
            {
                // 2️⃣ Đếm số tab hiện tại
                int tabCount = await CountTabsAsync(profileId);

                Libary.Instance.LogService(
                    $"[Manager] 🔍 Profile {profileId} hiện có {tabCount} tab");

                // 3️⃣ Nếu > 1 tab → reset về 1 tab
                if (tabCount > 1)
                {
                    Libary.Instance.LogService(
                        $"[Manager] 🔄 Profile {profileId} có nhiều tab → reset về 1 tab");

                    await CloseExtraTabsAsync(profileId);

                    // 👉 lấy lại page gốc sau khi reset
                    page = await GetPageAsync(profileId);
                }

                return page;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService(
                    $"[Manager] ❌ EnsureSingleTabBeforeWorkAsync lỗi ({profileId}): {ex.Message}");
                return page; // vẫn trả page nếu có
            }
        }

        // Trả về session đang chạy của profile hiện tại
        // mở tab phụ
        public BrowserSession GetActiveSession()
        {
            try
            {
                // thường bạn sẽ có dictionary lưu session theo profile_id
                // ví dụ _sessions["k122im4k"]
                lock (_sessions)
                {
                    foreach (var kv in _sessions)
                    {
                        var s = kv.Value;
                        if (s != null && s.Page != null && !s.Page.IsClosed)
                            return s; // session đang hoạt động
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService($"[Manager] ❌ GetActiveSession lỗi: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// MỞ tab mới (không reuse tab gốc). 
        /// Phù hợp khi bạn muốn chạy nhiều tab song song trong cùng 1 profile.
        /// </summary>
        public async Task<IPage> OpenNewTabAsync(string profileId, bool applyViewport = true)
        {
            try
            {
                // ensure session exists (reuse or create session)
                BrowserSession session = null;
                lock (_sessions)
                {
                    if (_sessions.ContainsKey(profileId))
                        session = _sessions[profileId];
                }

                if (session == null)
                {
                    // gọi GetPageAsync để tạo session ban đầu (nó tạo session và page gốc)
                    var p = await GetPageAsync(profileId);
                    // GetPageAsync đã lưu session vào _sessions, lấy lại
                    lock (_sessions)
                    {
                        if (_sessions.ContainsKey(profileId))
                            session = _sessions[profileId];
                    }
                    if (session == null)
                    {
                        Libary.Instance.LogService($"[Manager] OpenNewTabAsync: không tạo được session cho {profileId}");
                        return null;
                    }
                }

                // Lấy context từ session — tạo context nếu null (hiếm)
                var context = session.Context ?? await session.Browser.NewContextAsync();

                // MỞ TAB MỚI THẬT SỰ
                var newPage = await context.NewPageAsync();
                try
                {
                    // optional: set viewport giống main page
                    // 👉 CHỈ ép viewport khi được phép
                    if (applyViewport)
                    {
                        try { await newPage.SetViewportSizeAsync(980, 900); } catch { }
                    }

                    try { await newPage.BringToFrontAsync(); } catch { }


                    // bring to front cho dễ debug
                    try { await newPage.BringToFrontAsync(); } catch { }

                    Libary.Instance.CreateLog($"[Manager] ✅ OpenNewTabAsync: Mở tab mới cho {profileId}. Url={newPage.Url}");
                }
                catch (Exception ex)
                {
                    Libary.Instance.CreateLog($"[Manager] ⚠️ OpenNewTabAsync: lỗi thao tác với tab mới: {ex.Message}");
                }

                // Không thay đổi session.Page (vẫn giữ tab gốc), nhưng session.Context.Pages đã có tab mới
                session.LastUsed = DateTime.Now;
                return newPage;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"[Manager] ❌ OpenNewTabAsync exception ({profileId}): {ex.Message}");
                return null;
            }
        }
        public async Task ClosePageAsync(IPage page)
        {
            if (page == null) return;
            try
            {
                if (!page.IsClosed)
                {
                    await page.CloseAsync();
                    Libary.Instance.LogService("[Manager] Đóng tab phụ thành công");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogService($"[Manager] ⚠️ ClosePageAsync lỗi: {ex.Message}");
            }
        }
        // mở link an toàn chống check
        public static async Task<bool> OpenFacebookLinkSafeAsync(IPage page,string url)
        {
            try
            {

                if (page == null || page.IsClosed)
                {
                 
                    return false;
                }
                // ===============================
                // 2️⃣ VÀO FEED TẠO CONTEXT
                // ===============================
                Libary.Instance.LogDebug("[SAFE-OPEN] 🌐 Goto Facebook feed");

                await page.GotoAsync("https://www.facebook.com/", new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = AppConfig.DEFAULT_TIMEOUT
                });

                await page.WaitForTimeoutAsync(300);
                await ProcessingDAO.Instance.HumanScrollAsync(page);
                await page.WaitForTimeoutAsync(400);
                // ===============================
                // 3️⃣ MỞ LINK BẰNG window.location
                // ===============================
                Libary.Instance.LogDebug($"[SAFE-OPEN] 🔗 Open link: {url}");

                await page.EvaluateAsync( @"(u) => { window.location.href = u; }",
                    url
                );

                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await page.WaitForTimeoutAsync(300);

                // ===============================
                // 4️⃣ CHECK TRANG LỖI FACEBOOK
                // ===============================
                bool isBroken = await IsFacebookBrokenPageAsync(page);

                if (!isBroken)
                {
                    Libary.Instance.LogDebug("[SAFE-OPEN] ✅ Page opened OK");
                    return true;
                }

                Libary.Instance.LogTech( "⚠ [SAFE-OPEN] Facebook trả về trang lỗi");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech( $"❌ [SAFE-OPEN] Lỗi mở link: {ex.Message}"
                );
            }

            return false;
        }
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
        public string NormalizePostLinkToOpen(string rawLink, string pageOrUserId = null)
        {
            if (string.IsNullOrWhiteSpace(rawLink))
                return rawLink;

            string link = rawLink.Trim();

            // 🔥 BẮT BUỘC: ép domain CHUẨN
            link = link
                .Replace("https://fb.com/", "https://www.facebook.com/")
                .Replace("http://fb.com/", "https://www.facebook.com/")
                .Replace("https://facebook.com/", "https://www.facebook.com/")
                .Replace("http://facebook.com/", "https://www.facebook.com/")
                .Replace("https://m.facebook.com/", "https://www.facebook.com/")
                .Replace("http://m.facebook.com/", "https://www.facebook.com/");

            // 1️⃣ permalink đã chuẩn
            if (link.Contains("permalink.php"))
                return link;

            // 2️⃣ /posts/pfbid...
            var m = Regex.Match(link, @"/posts/(pfbid[0-9a-zA-Z]+)");
            if (m.Success)
            {
                string pfbid = m.Groups[1].Value;

                if (!string.IsNullOrWhiteSpace(pageOrUserId))
                {
                    return $"https://www.facebook.com/permalink.php?story_fbid={pfbid}&id={pageOrUserId}";
                }

                // fallback chưa có ID
                return $"https://www.facebook.com{m.Value}?__tn__=R";
            }

            // 3️⃣ reel / watch / post khác → ép full page
            if (!link.Contains("?"))
                link += "?";

            return link + "&__tn__=R";
        }
        // mở tab mới đơn giản
        public async Task<IPage> OpenNewTabSimpleAsync(IPage mainPage, string url,int timeoutMs = 6000)
        {
            if (mainPage == null || mainPage.IsClosed)
                return null;

            try
            {
                var popupTask = mainPage.Context.WaitForPageAsync();

                await mainPage.EvaluateAsync(
                    "(u) => window.open(u, '_blank')",
                    url
                );

                var finished = await Task.WhenAny(
                    popupTask,
                    Task.Delay(timeoutMs)
                );

                if (finished != popupTask)
                {
                    Libary.Instance.LogDebug( "❌ [OpenNewTabSimple] Timeout mở tab");
                    return null;
                }

                var newPage = await popupTask;
                await newPage.GotoAsync(url);
                await newPage.WaitForLoadStateAsync(
                    LoadState.DOMContentLoaded);

                await newPage.WaitForTimeoutAsync(400);

                return newPage;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug( $"❌ [OpenNewTabSimple] Exception: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> OpenFacebookLinkHumanAsync(IPage page,string url,int maxRetry = 2)
        {
            if (page == null || page.IsClosed || string.IsNullOrWhiteSpace(url))
                return false;

            url = NormalizePostLinkToOpen(url, null);

            for (int attempt = 1; attempt <= maxRetry; attempt++)
            {
                try
                {
                    Libary.Instance.LogDebug(
                        $"[OPEN-HUMAN] 🔗 Try {attempt}/{maxRetry} | {url}");

                    // =========================
                    // 1️⃣ TRY GOTO
                    // =========================
                    try
                    {
                        await page.GotoAsync(url, new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded,
                            Timeout = 12000
                        });

                        if (!await IsFacebookBrokenPageAsync(page))
                            return true;
                    }
                    catch { }

                    // =========================
                    // 2️⃣ TRY window.location
                    // =========================
                    try
                    {
                        await page.EvaluateAsync(
                            "(u) => window.location.href = u", url);

                        await page.WaitForLoadStateAsync(
                            LoadState.DOMContentLoaded);

                        if (!await IsFacebookBrokenPageAsync(page))
                            return true;
                    }
                    catch { }

                    // =========================
                    // 3️⃣ FINAL: ADDRESS BAR
                    // =========================
                    Libary.Instance.LogDebug(
                        "[OPEN-HUMAN] ⌨️ Fallback AddressBar");

                    await OpenByAddressBarAsync(page, url);

                    if (!await IsFacebookBrokenPageAsync(page))
                        return true;

                    // =========================
                    // 4️⃣ RESET CONTEXT TRƯỚC RETRY
                    // =========================
                    await page.GotoAsync("https://www.facebook.com/",
                        new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded
                        });

                    await page.WaitForTimeoutAsync(1200);
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogTech(
                        $"[OPEN-HUMAN] ❌ Attempt {attempt} error: {ex.Message}");
                }
            }

            Libary.Instance.LogTech(
                $"[OPEN-HUMAN] ⛔ FAIL ALL | {url}");

            return false;
        }
        // mở tab mới popup
        public async Task<IPage> OpenTabAndPrepareDomAsync(IPage mainPage,string url,int timeoutMs = 15000)
        {
            if (mainPage == null || mainPage.IsClosed || string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                // ===============================
                // 1️⃣ MỞ TAB MỚI (GIỮ CONTEXT)
                // ===============================
                var popupTask = mainPage.Context.WaitForPageAsync();

                await mainPage.EvaluateAsync(
                    "(u) => window.open(u, '_blank')",
                    url
                );

                var finished = await Task.WhenAny(
                    popupTask,
                    Task.Delay(timeoutMs)
                );

                if (finished != popupTask)
                {
                    Libary.Instance.LogDebug("❌ [OpenTab] Timeout mở tab");
                    return null;
                }

                var newPage = await popupTask;

                // ===============================
                // 2️⃣ ÉP LOAD LẠI URL (QUAN TRỌNG)
                // ===============================
                await newPage.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = timeoutMs
                });

                // ===============================
                // 3️⃣ ĐỢI LOAD NHẸ
                // ===============================
                await newPage.WaitForTimeoutAsync(500);

                // ===============================
                // 4️⃣ SCROLL GIẢ LẬP USER
                // ===============================
                for (int i = 0; i < 2; i++)
                {
                    await newPage.EvaluateAsync("() => window.scrollBy(0, document.body.scrollHeight)");
                    await newPage.WaitForTimeoutAsync(1000);
                }

                // scroll ngược lên (rất quan trọng cho FB)
                await newPage.EvaluateAsync("() => window.scrollTo(0, 0)");
                await newPage.WaitForTimeoutAsync(500);

                // ===============================
                // 5️⃣ WAIT DOM CHÍNH (POST)
                // ===============================
                try
                {
                    await newPage.WaitForSelectorAsync(
                        "div[data-ad-rendering-role='story_message']",
                        new PageWaitForSelectorOptions { Timeout = 4000 });
                }
                catch
                {
                    // fallback nhẹ thôi, không fail
                }

                // ===============================
                // 6️⃣ DEBUG URL (optional)
                // ===============================
                Libary.Instance.LogDebug($"[OpenTab] Final URL: {newPage.Url}");

                return newPage;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [OpenTab] Exception: {ex.Message}");
                return null;
            }
        }
        public static async Task OpenByAddressBarAsync(IPage page, string url)
        {
            // Đảm bảo đang có page mở
            await page.BringToFrontAsync();
            await page.WaitForTimeoutAsync(500);

            // Focus thanh địa chỉ
            await page.Keyboard.PressAsync("Control+L");
            await page.WaitForTimeoutAsync(200);

            // Dán URL
            await page.Keyboard.TypeAsync(url, new KeyboardTypeOptions
            {
                Delay = 20 // giống người dùng
            });

            // Enter
            await page.Keyboard.PressAsync("Enter");

            // Đợi load
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.WaitForTimeoutAsync(2000);
        }
        // hàm kiểm tra profile đang chạy thật
        public bool IsProfileActive(string profileId)
        {
            try
            {
                lock (_sessions)
                {
                    if (_sessions.ContainsKey(profileId))
                    {
                        var s = _sessions[profileId];

                        if (s != null &&
                            s.Page != null &&
                            !s.Page.IsClosed &&
                            s.Context != null)
                        {
                            return true; // 🔥 đang chạy thật
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
