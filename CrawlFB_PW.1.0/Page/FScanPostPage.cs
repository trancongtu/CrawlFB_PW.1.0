using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Profile;
using DevExpress.DocumentView;
using Microsoft.Playwright;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using IPage = Microsoft.Playwright.IPage;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.Page
{
    public partial class FScanPostPage : Form
    {
        public FScanPostPage()
        {
            InitializeComponent();
            this.Load += FScanPostPage_Load;
            this.FormClosing += FScanPostPage_FormClosing;
        }
        const string module = nameof(FScanPostPage);
        private int _maxPosts = AppConfig.MAX_POSTS_DEFAULT;
        private List<PageInfo> _selectedPages = new List<PageInfo>();
        //private List<PostPage> _listPostFull = new List<PostPage>();
        private Dictionary<string, List<PostPage>> _resultDict = new Dictionary<string, List<PostPage>>();
        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>(); //lưu danh sách profile
        private Dictionary<string, List<ShareItem>> _shareMap = new Dictionary<string, List<ShareItem>>();// lưa để thêm bảng share
        // Lock để tránh race condition khi chạy đa profile
        private readonly object _shareMapLock = new object();
        // chia url cho Profile
        private Dictionary<ProfileDB, List<PageInfo>> DistributePages(List<ProfileDB> profiles, List<PageInfo> pages)
        {
            var result = new Dictionary<ProfileDB, List<PageInfo>>();

            // 1️⃣ Khởi tạo
            foreach (var p in profiles)
                result[p] = new List<PageInfo>();
            int index = 0;
            // 2️⃣ Chia page + log chi tiết
            foreach (var page in pages)
            {
                var profile = profiles[index];
                result[profile].Add(page);

                // 📝 LOG: profile phụ trách page nào
                Libary.Instance.LogForm(
                    module,
                    $"👤 Profile '{profile.ProfileName}' ({profile.IDAdbrowser}) → Page: {page.PageLink}"
                );

                index = (index + 1) % profiles.Count;
            }
            // 3️⃣ LOG TỔNG KẾT
            foreach (var kv in result)
            {
                var p = kv.Key;
                int count = kv.Value.Count;

                Libary.Instance.LogForm(
                    module,
                    $"📦 Profile '{p.ProfileName}' ({p.IDAdbrowser}) phụ trách {count} page"
                );
            }
            return result;
        }
        private bool SelectProfilesForScan(out List<ProfileDB> profiles)
        {
            profiles = null;

            try
            {
                using (var frm = new SelectProfileDB())
                {
                    if (frm.ShowDialog() != DialogResult.OK)
                    {
                        Libary.Instance.LogForm(module, "Người dùng hủy chọn profile");
                        return false;
                    }

                    var selected = frm.Tag as List<ProfileDB>;
                    if (selected == null || selected.Count == 0)
                    {
                        MessageBox.Show("❌ Chưa chọn profile nào!");
                        Libary.Instance.LogForm(module, "Không có profile nào được chọn");
                        return false;
                    }

                    // 🚦 Check UseTab
                    var invalid = selected.Where(p => p.UseTab >= 3).ToList();
                    if (invalid.Count > 0)
                    {
                        string msg = string.Join("\n",
                            invalid.Select(p =>
                                $"{p.ProfileName} (Đang chạy {p.UseTab}/3 tab)"));

                        MessageBox.Show(
                            $"❌ Các profile sau đã đạt giới hạn 3 tab:\n\n{msg}",
                            "Không thể chọn profile",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        Libary.Instance.LogForm(
                            module,
                            $"Profile vượt quá tab: {string.Join(", ", invalid.Select(x => x.ProfileName))}"
                        );

                        return false;
                    }

                    profiles = selected;

                    Libary.Instance.LogForm(
                        module,
                        $"Đã chọn {profiles.Count} profile để chạy FirstScan"
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi chọn profile: " + ex.Message);
                Libary.Instance.LogForm(module, "Exception chọn profile: " + ex.Message);
                return false;
            }
        }
        // hàm chạy 1 page
        private async Task<List<PostPage>> SuperviseOnePageAsync(string url, int days, int maxPosts, ProfileDB profile, IPage page = null)
        {
            var listPosts = new List<PostPage>();
            var uniqueLinks = new HashSet<string>();
            string PageName = "";
            string urlgoc = url;
            try
            {
                if (page == null)
                {
                    // mở TAB MỚI, KHÔNG dùng tab gốc
                    page = await AdsPowerPlaywrightManager.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                    if (page == null)
                    {
                        Libary.Instance.LogForm(module, $"❌ Không mở được tab mới cho profile {profile.ProfileName}");
                        return listPosts;
                    }
                }
                Libary.Instance.LogForm(module, $"[SuperviseOnePageAsync] 🚀 Bắt đầu giám sát với profile {profile.IDAdbrowser}");
                // 🧩 Ép lấy bài viết mới nhất (Newest)
                if (url.IndexOf("sorting_setting=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    if (url.Contains("?"))
                        url += "&sorting_setting=CHRONOLOGICAL";
                    else
                        url += "?sorting_setting=CHRONOLOGICAL";
                }
                // 2️⃣ Truy cập trang cần giám sát
                await page.GotoAsync(url, new PageGotoOptions
                {
                    Timeout = AppConfig.DEFAULT_TIMEOUT,
                    WaitUntil = WaitUntilState.DOMContentLoaded
                });

                await page.WaitForTimeoutAsync(2000);
                if (string.IsNullOrEmpty(PageName) || PageName == "N/A")
                {
                    await page.WaitForTimeoutAsync(2000);  // đợi render
                    PageName = await PageDAO.Instance.GetPageNameAsync(page);
                }
                // 3️⃣ Cuộn để load thêm bài
                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, AppConfig.scrollCount);
                int count = 0;
                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 1);
                var feed = await PageDAO.Instance.GetFeedContainerAsync(page);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int processedIndex = 0;
                int scrollRound = 0;
                int maxScrollRounds = 50;
                int waitAfterScrollMs = 800;
                bool stopBecauseOld = false;
                int oldCount = 0;      // đếm số bài quá hạn liên tiếp
                int maxOld = 3;
                while (count < maxPosts && scrollRound < maxScrollRounds)
                {
                    if (feed == null)
                    {
                        Libary.Instance.LogTech("❌ Không tìm thấy feed");
                        break;
                    }

                    var nodes = await feed.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");
                    Libary.Instance.LogTech($"🔍 Round {scrollRound}: {nodes.Count} node, processedIndex={processedIndex}");
                    bool addedInThisBatch = false;
                    for (int i = processedIndex; i < nodes.Count; i++)
                    {
                        if (count >= maxPosts) break;
                        var node = nodes[i];
                        // 🔥 V3 trả LIST<PostPage>
                        PostResult result;
                        bool IsGroups = PageDAO.Instance.IsFacebookGroup(urlgoc);
                        if (IsGroups)
                        {
                            Libary.Instance.LogForm(module, "Chạy Groups: " + urlgoc);
                            result = await PageDAO.Instance.GetPostAutoGroupsAsync(page, node, PageName, urlgoc);
                        }
                        else
                        {
                            result = await PageDAO.Instance.GetPostAutoFanpageAsync(page, node, PageName, urlgoc);
                            Libary.Instance.LogForm(module, "Chạy fanpage: " + urlgoc);
                        }
                        var posts = result.Posts;
                        var shares = result.Shares;
                        if (shares != null && shares.Count > 0)
                        {
                            lock (_shareMapLock)
                            {
                                if (!_shareMap.ContainsKey(urlgoc))
                                    _shareMap[urlgoc] = new List<ShareItem>();
                                _shareMap[urlgoc].AddRange(shares);
                            }
                        }
                        if (posts == null || posts.Count == 0)
                        {
                            Libary.Instance.LogForm(module, $"⚠ posts null/empty tại index {i}");
                            processedIndex = i + 1;
                            continue;
                        }
                        foreach (var post in posts)
                        {
                            // ⚠ Nếu vượt max thì dừng
                            if (count >= maxPosts) break;
                            // ✅ Log kết quả post (bài nào log bài đó)
                            LogPostResult(module, post);
                            var postDate = TimeHelper.ParseFacebookTime(post.PostTime);
                            Libary.Instance.LogForm(module, $"⏱ ParsedTime: {postDate}");
                            if (days == 0)
                            {
                                oldCount = 0;      // reset tránh sai logic
                            }
                            else
                            {
                                // ⭐ Kiểm tra bài quá hạn
                                if (postDate.HasValue &&(DateTime.Now - postDate.Value).TotalDays > days)
                                {
                                    oldCount++;
                                    if (oldCount >= maxOld)
                                    {
                                        Libary.Instance.LogForm(module, "⛔ Gặp 3 bài QUÁ NGÀY liên tiếp → Dừng crawl");
                                        stopBecauseOld = true;
                                        break;
                                    }
                                    continue;   // skip bài này nhưng KHÔNG dừng
                                }
                                else
                                {
                                    oldCount = 0;  // reset vì bài mới hợp lệ
                                }
                            }
                            // tạo key chống trùng
                            string key = post.PostLink;
                            if (string.IsNullOrEmpty(key)) key = $"nolink_{scrollRound}_{i}";
                            if (uniqueLinks.Contains(key))
                            {
                                Libary.Instance.LogForm(module, $"⛔ Skip DUPLICATE: {key}");
                                continue;
                            }
                            uniqueLinks.Add(key);
                            listPosts.Add(post);
                            count++;
                            addedInThisBatch = true;
                        }
                        processedIndex = i + 1;
                        if (stopBecauseOld) break;
                    }
                    // nếu quá ngày → out while
                    if (stopBecauseOld) break;
                    // nếu batch này có thêm bài → không scroll sớm
                    if (addedInThisBatch)
                    {
                        Libary.Instance.LogForm(module, "✅ Có bài mới trong batch → tiếp tục xử lý batch hiện tại.");
                        continue;
                    }
                    // nếu đã xử lý hết → scroll load thêm
                    if (processedIndex >= nodes.Count)
                    {
                        if (count >= maxPosts) break;
                        Libary.Instance.LogForm(module, "➡ Hết batch hiện tại → scroll load thêm...");
                        await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 1);
                        await page.WaitForTimeoutAsync(waitAfterScrollMs);
                        scrollRound++;
                        continue;
                    }
                    // fallback hiếm gặp
                    processedIndex = Math.Min(processedIndex + 1, nodes.Count);
                }
                // tránh loop vô hạn
                if (scrollRound >= maxScrollRounds)
                    Libary.Instance.LogForm(module, "⚠ Đạt maxScrollRounds, dừng lại.");
                watch.Stop();
                Libary.Instance.LogForm(module, $"[SuperviseOnePageAsync] ✅ Đã lấy {listPosts.Count}/{maxPosts} bài trong {watch.Elapsed.TotalSeconds:0.0}s.");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogForm(module, "[SuperviseOnePageAsync] ❌ Exception: " + ex.Message);
            }
            return listPosts;
        }       
        private void FScanPostPage_Load(object sender, EventArgs e)
        {
            UICommercialHelper.StyleGrid(gridView1);
        }
        private int GetMaxDays(int defaultValue = 7)
        {
            try
            {
                var val = txb_Maxday.EditValue?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(val))
                    return defaultValue;

                // Cho phép nhập 0 = toàn thời gian
                if (int.TryParse(val, out int days))
                {
                    return days < 0 ? defaultValue : days;
                }
            }
            catch { }

            return defaultValue;
        }
        private int GetMaxPosts(int defaultValue = 15)
        {
            try
            {
                var val = txb_SetupMaxPost.EditValue?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(val))
                    return defaultValue;

                if (int.TryParse(val, out int maxPosts))
                {
                    return maxPosts <= 0 ? defaultValue : maxPosts;
                }
            }
            catch { }

            return defaultValue;
        }
        private void FScanPostPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // cleanup sessions (fire and forget)
            AdsPowerPlaywrightManager.Instance.CleanupAsync().GetAwaiter().GetResult();
        }    
        private void GridView1_CustomDrawRowIndicator(object sender,DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle >= 0)
                e.Info.DisplayText = (e.RowHandle + 1).ToString();
        }
        private DataTable BuildPostTableForCrawl(List<PostPage> posts)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Địa chỉ", typeof(string));
            dt.Columns.Add("LinkThật", typeof(string));
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Nội dung", typeof(string));

            // ===== CỘT NGHIỆP VỤ =====
            dt.Columns.Add("Người/Page đăng", typeof(string));      // PosterName
            dt.Columns.Add("Nguồn tạo bài", typeof(string));        // PosterNote (Page / Person)
            dt.Columns.Add("Nơi đăng (Page/Group)", typeof(string)); // PageName

            dt.Columns.Add("Like", typeof(int));
            dt.Columns.Add("Share", typeof(int));
            dt.Columns.Add("Comment", typeof(int));

            int i = 1;
            foreach (var p in posts)
            {
                dt.Rows.Add(
                    i++,
                    "Xem link",
                    p.PostLink,
                    p.PostTime,
                    p.Content,

                    // ===== map nghiệp vụ =====
                    p.PosterName,     // Người hoặc Page đăng
                    p.PosterNote,     // Create post: Page / Person / Admin
                    p.PageName,       // Page / Group chứa bài

                    p.LikeCount ?? 0,
                    p.ShareCount ?? 0,
                    p.CommentCount ?? 0
                );
            }

            return dt;
        }
        private void ShowCrawlPostsToGrid()
        {
            var allPosts = _resultDict
                .SelectMany(x => x.Value)
                .ToList();

            DataTable dt = BuildPostTableForCrawl(allPosts);

            gridControl1.DataSource = dt;
            gridView1.PopulateColumns();

            UICommercialHelper.StyleGrid(gridView1);
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;

            UIPostGridHelper.ApplyFakeLink(gridView1, "Địa chỉ", "LinkThật");

            if (gridView1.Columns["LinkThật"] != null)
                gridView1.Columns["LinkThật"].Visible = false;

            gridView1.BestFitColumns();
        }
        private void btn_LoadFile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var f = new FSelectPageScanNew())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    _selectedPages = f.SelectedPages;
                    MessageBox.Show($"✔ Đã chọn {_selectedPages.Count} page!");
                }
            }
        }
        private void LoadPageFromText()
        {
            string rawUrl = txb_UrlPage.EditValue?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(rawUrl))
                return;

            // Chuẩn hóa chữ thường để check
            string url = rawUrl.ToLowerInvariant();

            bool isValidFacebookUrl =
                url.StartsWith("https://www.facebook.com") ||
                url.StartsWith("https://fb.com") ||
                url.StartsWith("https://www.fb.com");

            if (!isValidFacebookUrl)
            {
                MessageBox.Show(
                    "❌ URL không hợp lệ!\n\nChỉ chấp nhận:\n" +
                    "• https://www.facebook.com/...\n" +
                    "• https://fb.com/...\n" +
                    "• https://www.fb.com/...",
                    "URL không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                Libary.Instance.LogForm(
                    module,
                    $"❌ Reject URL không hợp lệ: {rawUrl}"
                );
                return;
            }

            // Gán vào _selectedPages
            _selectedPages = new List<PageInfo>
    {
        new PageInfo
        {
            PageID = Guid.NewGuid().ToString(),
            PageLink = rawUrl, // giữ nguyên chữ hoa chữ thường
            PageName = "Manual URL"
        }
    };

            Libary.Instance.LogForm(
                module,
                $"📌 Nhận URL từ textbox: {rawUrl}"
            );
        }           
        private async void btn_StartScan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!SelectProfilesForScan(out _selectedProfiles))
                {
                    MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(txb_UrlPage.EditValue?.ToString()))
                {
                    LoadPageFromText();
                }

                // Nếu vẫn chưa có page
                if (_selectedPages == null || _selectedPages.Count == 0)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn Page hoặc nhập URL hợp lệ!");
                    return;
                }
               
                int days = GetMaxDays();     // default = 10
                int maxPosts = GetMaxPosts(); // default = 50

                Libary.Instance.LogForm(module, $"[FirstScan] days={days}, maxPosts={maxPosts}");
                // ===========================================
                // 3️⃣ CHIA PAGE THEO PROFILE
                // ===========================================
                var jobMap = DistributePages(_selectedProfiles, _selectedPages);
                // ===========================================
                // 4️⃣ CHẠY TỪNG PROFILE – PAGE
                // ===========================================
                List<Task> allTasks = new List<Task>();

                foreach (var kv in jobMap)
                {
                    var profile = kv.Key;
                    var pages = kv.Value;

                    // Mỗi profile chạy 1 task song song
                    var t = Task.Run(async () =>
                    {
                        foreach (var pageInfo in pages)
                        {
                            string pageId = pageInfo.PageID;
                             // tạo page còn 1 tab
                            var mainPage = await Ads.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
                            if (mainPage == null)
                                return;
                            var crawlPage = await Ads.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);
                            var posts = await SuperviseOnePageAsync(pageInfo.PageLink, days, maxPosts, profile, crawlPage);
                            Libary.Instance.LogForm(module, "Chạy " + pageInfo.PageLink + " được tổng: " + posts.Count() + " bài viết");                          
                            lock (_resultDict)
                            {
                                if (!_resultDict.ContainsKey(pageId))
                                    _resultDict[pageId] = new List<PostPage>();

                                _resultDict[pageId].AddRange(posts);
                            }
                           
                            await Ads.Instance.ClosePageAsync(crawlPage);
                        }
                    });

                    allTasks.Add(t);
                }
                // ⭐ CHỜ TẤT CẢ PROFILE CHẠY XONG SONG SONG
                await Task.WhenAll(allTasks);
               
                // ⭐ GOM POST & HIỂN THỊ GRID (UI THREAD)
                this.Invoke(new Action(() =>
                {
                    ShowCrawlPostsToGrid();
                }));
                MessageBox.Show("🎉 Hoàn tất quét");

            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi: " + ex.Message);
                Libary.Instance.LogForm(module, "[FirstScan] ❌ " + ex.Message);
            }

        }
        //log
        private void LogPostResult(string module, PostPage post)
        {
            var sb = new StringBuilder();

            sb.AppendLine("📝 POST RESULT");
            sb.AppendLine($"{Libary.IconOK} Poster: {post.PosterName}");
            sb.AppendLine($"🔗 Link: {post.PostLink}");
            sb.AppendLine($"⏱ Time: {post.PostTime}");
            sb.AppendLine( $"🧾 Content(100): {(post.Content == null ? "" : post.Content.Length <= 100 ? post.Content : post.Content.Substring(0, 100) + "...")}");
            sb.AppendLine($"PageName: {post.PageName}");
            bool hasInteract =
                (post.LikeCount + post.CommentCount + post.ShareCount) > 0;

            sb.AppendLine(
                $"{Libary.BoolIcon(hasInteract)} Interact: " +
                $"👍={post.LikeCount} 💬={post.CommentCount} 🔁={post.ShareCount}"
            );

            sb.AppendLine($"📌 Status: {post.PostType.ToString()}");

            Libary.Instance.LogForm(module, sb.ToString());
        }
    }
}
