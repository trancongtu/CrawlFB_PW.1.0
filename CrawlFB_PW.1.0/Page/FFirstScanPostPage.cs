using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Profile;
using Microsoft.Playwright;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using DevExpress.XtraGrid.Columns;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using System.Data.SQLite;
using DevExpress.XtraGrid.Views.Grid;
using CrawlFB_PW._1._0.Helpers;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using System.Security.Cryptography;
using DevExpress.XtraGrid.Views.Base;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Service;
using CrawlFB_PW._1._0.Helper.UI;

using DevExpress.Utils;
using CrawlFB_PW._1._0.Helper.Mapper;
namespace CrawlFB_PW._1._0.Page
{   
    public partial class FFirstScanPostPage : Form
    {
        const string module = nameof(FFirstScanPostPage);
        private List<PageInfo> _selectedPages = new List<PageInfo>();
        private BindingList<PageInfoViewModel> _pageList = new BindingList<PageInfoViewModel>();
        private BindingList<PostInfoViewModel> _postList = new BindingList<PostInfoViewModel>();

        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>(); //lưu danh sách profile

        private List<string> _listUrls = new List<string>();// lưu danh sách page
            
       // private int _maxPost = 50;  // mặc định
        private Dictionary<string, List<ShareItem>> _shareMap = new Dictionary<string, List<ShareItem>>();// lưa để thêm bảng share
        // Lock để tránh race condition khi chạy đa profile
        private readonly object _shareMapLock = new object();
        public FFirstScanPostPage()
        {
            InitializeComponent();
            this.Load += FFirstScanPostPage_Load;                         
            UICommercialHelper.StyleGrid(gridViewPost);
            UIStyleHelper.StyleBarManager(barManager1);
            UIStyleHelper.StyleBarManager(barManager2);
        }
        private void InitPageGrid()
        {
            var gv = gridViewPage;
            gv.PopulateColumns();
            // ===== STT =====
            UIGridHelper.EnableRowIndicatorSTT(gv);
            // ===== Select / Status =====
            UIGridHelper.ApplySelect(gv, gridControlPage);
            UIGridHelper.LockAllColumnsExceptSelect(gv);
            UIGridHelper.EnableStatusDisplay(gv);
            UIGridHelper.ApplyRowColorByStatus(gv, "Status");
            // ===== Caption tiếng Việt =====
            UIGridHelper.ApplyVietnameseCaption(gv);
            // ⭐ ẨN HẾT – CHỈ MỞ NHỮNG CỘT CẦN
            UIGridHelper.ShowOnlyColumns(
                gv,
                "Select",
                "PageName",
                "Status"
            );           
            gv.BestFitColumns();
        }
        private void InitPagePost()
        {
            var gv = gridViewPost;

            if (gv.Tag as string == "INIT_DONE")
                return;

            gv.BeginUpdate();
            try
            {
                gv.OptionsBehavior.Editable = true;
                UIGridHelper.EnableRowIndicatorSTT(gv);
                UIGridHelper.ApplyVietnameseCaption(gv);

                UIGridHelper.ShowOnlyColumns(
                    gv,
                    "PostLink",
                    "TimeView",
                    "Content",
                    "AttachmentView",
                    "Like",
                    "Share",
                    "Comment",
                    "PostType",
                    "PosterName",
                    "PosterLink",
                    "PosterNote",
                    "PageName",
                    "PageLink"
                );

                UIGridHelper.ApplyHyperlinkColumn(gridViewPost, gridControlPost, "PostLink", "🔗 Mở Bài");
                UIGridHelper.ApplyHyperlinkColumn(gridViewPost, gridControlPost, "PageLink", "📄 Mở Page");
                UIGridHelper.ApplyHyperlinkColumn(gridViewPost, gridControlPost, "PosterLink", "👤 Mở Người đăng");

                UIGridHelper.ApplyAttachmentLink(gridViewPost, gridControlPost, "AttachmentView");

                UIGridHelper.ApplyLinkTooltip(gridViewPost, gridControlPost);
                 UIGridHelper.LockAllColumnsExceptLinks(gv);
                gv.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
                gv.OptionsSelection.EnableAppearanceFocusedCell = false;
                gv.FocusRectStyle = DrawFocusRectStyle.RowFocus;

                gv.Tag = "INIT_DONE";
            }
            finally
            {
                gv.EndUpdate();
            }
        }
        private void FFirstScanPostPage_Load(object sender, EventArgs e)
        {
            EditMaxDay.EditValue = "10";
            EditMaxPost.EditValue = "50";
            gridViewPage.OptionsBehavior.AutoPopulateColumns = false;
            gridViewPost.OptionsBehavior.AutoPopulateColumns = false;
            LoadPageList();
            gridViewPage.FocusedRowChanged -= gridViewPage_FocusedRowChanged;
            gridViewPage.FocusedRowChanged += gridViewPage_FocusedRowChanged;
            gridControlPost.DataSourceChanged += GridControlPost_DataSourceChanged;
        }
        private void GridControlPost_DataSourceChanged(object sender, EventArgs e)
        {
            var gv = gridViewPost;

            gv.BeginUpdate();
            try
            {
                // 🔥 BẮT BUỘC: tạo cột từ ViewModel
                gv.PopulateColumns();                           
                // 4️⃣ Thứ tự cột (STT là RowIndicator)
                gv.Columns[nameof(PostInfoViewModel.TimeView)].VisibleIndex = 1;
                
                // ép render
                gridControlPost.ForceInitialize();
                gv.RefreshData();

                // reset init flag
                gv.Tag = null;

                // init UI (show/hide, hyperlink, caption…)
                InitPagePost();
            }
            finally
            {
                gv.EndUpdate();
            }
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
        /*
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
                        Libary.Instance.LogForm(module, $"[FirstScan] ❌ Không mở được tab mới cho profile {profile.ProfileName}");
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
                            Libary.Instance.LogForm(module,$"⚠ posts null/empty tại index {i}");
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
                            Libary.Instance.LogForm(module,$"⏱ ParsedTime: {postDate}");
                            if (days == 0)
                            {
                                oldCount = 0;      // reset tránh sai logic
                            }
                            else
                            {
                                // ⭐ Kiểm tra bài quá hạn
                                if (postDate is DateTime dt && (DateTime.Now - dt).TotalDays > days)
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
                            if (string.IsNullOrEmpty(key))key = $"nolink_{scrollRound}_{i}";
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
        */
        private int GetMaxDays(int defaultValue = 10)
        {
            try
            {
                var val = EditMaxDay.EditValue?.ToString()?.Trim();

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
        private int GetMaxPosts(int defaultValue = 50)
        {
            try
            {
                var val = EditMaxPost.EditValue?.ToString()?.Trim();

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
        private void RefreshSlotManagerUI()
        {
            // chạy trên UI thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshSlotManagerUI));
                return;
            }

            // tìm FSlotManagerProfile trong các form MDI con
            foreach (Form child in Application.OpenForms)
            {
                if (child is FSlotManagerProfile slotForm)
                {
                    slotForm.ForceReload();   // 🔥 gọi hàm reload của SlotManager
                    break;
                }
            }
        }
        private void panelControlResult_Paint(object sender, PaintEventArgs e)
        {

        }
        private void LoadPostsForPage(PageInfoViewModel page)
        {
            if (page == null)
            {
                _postList.Clear();
                return;
            }

            _postList = new BindingList<PostInfoViewModel>(page.Posts);
            gridControlPost.DataSource = _postList;

            // 🔥 ÉP GRID RENDER
            gridControlPost.ForceInitialize();
            gridViewPost.RefreshData();

            // reset init flag
            gridViewPost.Tag = null;

            InitPagePost();
        }
        // chuyển post theo page (KIẾN TRÚC MỚI)
        private void gridViewPage_FocusedRowChanged(object sender,FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
                return;

            var page = gridViewPage.GetRow(e.FocusedRowHandle)
                as PageInfoViewModel;

            if (page == null)
                return;

            // bind post theo page đang chọn
            _postList = new BindingList<PostInfoViewModel>(page.Posts);
            gridControlPost.DataSource = _postList;
        }
        private void LoadPageList()
        {
            _pageList.Clear();

            if (_selectedPages == null || _selectedPages.Count == 0)
            {
                gridControlPage.DataSource = _pageList;
                return;
            }

            foreach (var p in _selectedPages)
            {
                var vm = new PageInfoViewModel
                {
                    PageID = p.PageID,
                    PageName = p.PageName,
                    PageLink = p.PageLink,
                    IDFBPage = p.IDFBPage,
                    Status = UIStatus.Pending,   // trạng thái ban đầu
                    Select = false
                };

                _pageList.Add(vm);
            }

            gridControlPage.DataSource = _pageList;

            InitPageGrid(); // ⭐ cấu hình grid
        }
        private void Btn_LoadPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var f = new FSelectPageScanNew())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    _selectedPages = f.SelectedPages;

                    LoadPageList(); // ⭐ load lại grid

                    MessageBox.Show($"✔ Đã chọn {_selectedPages.Count} page!");
                }
            }
        }               
        private void btn_DeletePage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var gv = gridViewPage;
            int rowHandle = gv.FocusedRowHandle;
            if (rowHandle < 0)
                return;

            var page = gv.GetRow(rowHandle) as PageInfoViewModel;
            if (page == null)
                return;

            // confirm
            if (MessageBox.Show(
                $"❓ Xóa page '{page.PageName}' khỏi danh sách quét?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // 1️⃣ Xóa page khỏi danh sách Page (ViewModel)
            _pageList.Remove(page);

            // 2️⃣ Nếu page đang được focus → clear grid Post
            var focusedPage = gridViewPage.GetFocusedRow() as PageInfoViewModel;
            if (focusedPage == null || focusedPage == page)
            {
                _postList.Clear();
                gridControlPost.DataSource = _postList;
            }

            // 3️⃣ Refresh grid Page (thường không cần nhưng cho chắc)
            gridViewPage.RefreshData();
        }
        private List<string> GetSelectedPageIds()
        {
            var result = new List<string>();

            var gv = gridViewPage;
            var selectedRows = gv.GetSelectedRows();

            foreach (var rowHandle in selectedRows)
            {
                if (rowHandle < 0) continue;

                string pageId = gv.GetRowCellValue(rowHandle, "PageID")?.ToString();
                if (!string.IsNullOrWhiteSpace(pageId))
                    result.Add(pageId);
            }

            return result;
        }      
        private void btn_SaveDB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 1️⃣ Lấy page được chọn theo ViewModel
            var selectedPages = _pageList
                .Where(x => x.Select)
                .ToList();

            if (selectedPages.Count == 0)
            {
                MessageBox.Show("⚠ Vui lòng chọn Page cần lưu!");
                return;
            }

            // 2️⃣ Confirm
            if (MessageBox.Show(
                $"Lưu kết quả quét cho {selectedPages.Count} page?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int totalPages = 0;
            int totalPosts = 0;
            int totalShares = 0;
            foreach (var page in selectedPages)
            {
                bool hasSavedData = false;
                totalPages++;

                // 1️⃣ LƯU POST
                foreach (var vm in page.Posts)
                {
                    var dto = vm.ToPostPage();
                    SQLDAO.Instance.InsertOrIgnorePost(dto);

                    totalPosts++;
                    hasSavedData = true;
                }
                // 2️⃣ LƯU SHARE
                if (_shareMap.TryGetValue(page.PageLink, out var shares))
                {
                    foreach (var share in shares)
                    {
                        if (string.IsNullOrEmpty(share.PostLinkB))
                            continue;

                        string postId = SQLDAO.Instance.GetPostIdByPostLink(share.PostLinkB);
                        if (string.IsNullOrEmpty(postId))
                            continue;

                        SQLDAO.Instance.InsertTablePostShare(
                            postId,
                            page.PageID,
                            null,
                            TimeHelper.NormalizeTime(share.ShareTimeReal),
                            share.ShareTimeReal
                        );

                        totalShares++;
                        hasSavedData = true;
                    }
                }

                // 3️⃣ CẬP NHẬT IsScanned = 1 nếu page có dữ liệu được lưu
                if (hasSavedData)
                {
                    SQLDAO.Instance.UpdatePageIsScanned(page.PageID, 1);
                }
            }

            MessageBox.Show(
                $"Lưu database thành công!\n\n" +
                $"Page  : {totalPages}\n" +
                $"Post  : {totalPosts}\n" +
                $"Share : {totalShares}",
                "Save DB",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

        }
        private void btn_resetTable_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show(
           "❓ Reset toàn bộ kết quả quét?",
           "Xác nhận",
           MessageBoxButtons.YesNo,
           MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            // ===========================================
            // 1️⃣ RESET PAGE + POST (VIEWMODEL)
            // ===========================================
            foreach (var page in _pageList)
            {
                page.Posts.Clear();               // xóa post của page
                page.Status = UIStatus.Pending;   // reset trạng thái page
                page.Select = false;              // bỏ chọn (tuỳ bạn)
            }

            // ===========================================
            // 2️⃣ CLEAR GRID POST
            // ===========================================
            _postList.Clear();
            gridControlPost.DataSource = _postList;

            // ===========================================
            // 3️⃣ REFRESH GRID PAGE
            // ===========================================
            gridViewPage.RefreshData();

            MessageBox.Show("✔ Đã reset dữ liệu quét!");
        }
        private void btn_AddPageMonitor_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var gv = gridViewPage;
                int row = gv.FocusedRowHandle;

                if (row < 0)
                {
                    MessageBox.Show("⚠ Vui lòng chọn Page trên danh sách!");
                    return;
                }

                // 1️⃣ Lấy PageID từ grid
                string pageId = gv.GetRowCellValue(row, "PageID")?.ToString();
                if (string.IsNullOrWhiteSpace(pageId))
                {
                    MessageBox.Show("❌ Không lấy được PageID!");
                    return;
                }

                // 2️⃣ Kiểm tra Page có tồn tại trong DB không (an toàn)
                var page = SQLDAO.Instance.GetPageByID(pageId);
                if (page == null)
                {
                    MessageBox.Show("❌ Page không tồn tại trong DB!");
                    return;
                }

                // 3️⃣ Insert / Update PageMonitor
                SQLDAO.Instance.InsertPageMonitor(pageId);

                // 4️⃣ Log + thông báo
                Libary.Instance.LogForm(
                    module,
                    $"➕ Add PageMonitor: {page.PageName} ({page.PageLink})"
                );

                MessageBox.Show("✔ Đã thêm Page vào danh sách Monitor!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi Add Page Monitor: " + ex.Message);
                Libary.Instance.LogForm(module, "[AddPageMonitor] ❌ " + ex.Message);
            }
        }
        private void btn_AddAllPageMonitor_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var gv = gridViewPage;
                if (gv.RowCount == 0)
                {
                    MessageBox.Show("⚠ Không có page nào trong danh sách!");
                    return;
                }
                int addedCount = 0;

                for (int i = 0; i < gv.RowCount; i++)
                {
                    string pageId = gv.GetRowCellValue(i, "PageID")?.ToString();
                    if (string.IsNullOrWhiteSpace(pageId))
                        continue;

                    // kiểm tra page có tồn tại DB không (an toàn)
                    var page = SQLDAO.Instance.GetPageByID(pageId);
                    if (page == null)
                        continue;

                    // insert / update PageMonitor
                    SQLDAO.Instance.InsertPageMonitor(pageId);
                    addedCount++;

                    // log từng page (có thể bỏ nếu sợ log nhiều)
                    Libary.Instance.LogForm(
                        module,
                        $"➕ Add PageMonitor: {page.PageName} ({pageId})"
                    );
                }

                // log tổng kết
                Libary.Instance.LogForm(
                    module,
                    $"✅ Add ALL PageMonitor: {addedCount} page"
                );

                MessageBox.Show(
                    $"✔ Đã thêm {addedCount} page vào Monitor!",
                    "Hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi Add All Page Monitor: " + ex.Message);
                Libary.Instance.LogForm(module, "[AddAllPageMonitor] ❌ " + ex.Message);
            }
        }
        // log form
        private void LogPostResult(string module, PostPage post)
        {
            var sb = new StringBuilder();

            sb.AppendLine("📝 POST RESULT");
            sb.AppendLine($"Page Chứa: {post.PageName}");
            sb.AppendLine($"PageLink: {post.PageLink ?? "N/A"}");
            sb.AppendLine($"PageID: {post.PageID ?? "N/A"}");
            sb.AppendLine($"ContainerIdFB: {post.ContainerIdFB ?? "N/A"}");

            sb.AppendLine($"Poster: {post.PosterName}");
            sb.AppendLine($"PosterLink: {post.PosterLink ?? "N/A"}");
            sb.AppendLine($"PosterIdFB: {post.PosterIdFB ?? "N/A"}");
            sb.AppendLine($"PosterNote: {post.PosterNote}");

            sb.AppendLine($"Link: {post.PostLink}");
            sb.AppendLine($"Time: {post.PostTime}");
            sb.AppendLine($"RealTime: {post.RealPostTime}");
            sb.AppendLine($"PostType: {post.PostType}");

            sb.AppendLine($"Like: {post.LikeCount ?? 0}");
            sb.AppendLine($"Comment: {post.CommentCount ?? 0}");
            sb.AppendLine($"Share: {post.ShareCount ?? 0}");

            sb.AppendLine($"ContentLen: {post.Content?.Length ?? 0}");
            sb.AppendLine($"Attachment: {post.Attachment ?? "N/A"}");

            Libary.Instance.LogForm(module, sb.ToString());
        }
        private async void btnFirstRun_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!SelectProfilesForScan(out _selectedProfiles))
                {
                    MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                    return;
                }
                // ===========================================
                // 2️⃣ CẤU HÌNH SCAN
                int days = GetMaxDays();     // default = 10
                int maxPosts = GetMaxPosts(); // default = 50

                Libary.Instance.LogForm(module, $"[FirstScan] days={days}, maxPosts={maxPosts}");
                // ===========================================
                // 3️⃣ CHIA PAGE THEO PROFILE
                // ===========================================
                // ===========================================
                // 3️⃣ LẤY PAGE ĐƯỢC CHỌN TỪ GRID (VIEWMODEL)
                // ===========================================
                var runPages = _pageList
                    .Where(x => x.Select)
                    .ToList();
                if (runPages.Count == 0)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn Page nào để quét!");
                    return;
                }
                // ===========================================
                // 4️⃣ CHIA PAGE THEO PROFILE (SERVICE)
                // ===========================================
                var service = new PageDistributionService();

                var jobMap = service.Distribute(
                    _selectedProfiles,
                    runPages,                // ⚠️ CHỈ PAGE ĐƯỢC SELECT
                    x => x.PageName,          // key để log / debug
                    "FirstScan"
                );

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
                        foreach (var page in pages)
                        {
                            string pageId = page.PageID;
                            Libary.Instance.LogForm(module, "chạy PageLink: " + page.PageLink);
                            // update UI trước khi quét
                            this.Invoke(new Action(() =>
                            {
                                page.Status = UIStatus.Running;
                            }));

                            var mainPage = await Ads.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
                            if (mainPage == null)
                                continue;
                            var crawlPage = await Ads.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);
                           
                            var result = await FirstScanPostPageDAO.Instance.FirstScanAsync(crawlPage, page.PageLink, page.PageID, page.IDFBPage, maxPosts);
                            var posts = result.Posts;
                            
                            foreach (var post in posts)
                            {
                                LogPostResult(module, post);
                            }    
                      
                            var shares = result.Shares;
                            var postVMs = posts.Select(p =>
                             {
                                 var vm = p.ToViewModel(); // 🔥 dùng mapper

                                 vm.Status = UIStatus.Done;
                                 vm.ParentPage = page;

                                 return vm;
                             }).ToList();

                            Libary.Instance.LogForm(module, "Chạy " + page.PageLink + " được tổng: " + posts.Count() + " bài viết");
                            this.Invoke(new Action(() =>
                            {
                                // gán post cho page
                                page.Posts.Clear();
                                foreach (var vm in postVMs)
                                    page.Posts.Add(vm);
                                // update trạng thái page
                                page.Status = UIStatus.Done;
                                // nếu page đang focus thì load post grid
                                var focusedPage = gridViewPage.GetFocusedRow() as PageInfoViewModel;
                                if (focusedPage == page)
                                {
                                    _postList = new BindingList<PostInfoViewModel>(page.Posts);
                                    gridControlPost.DataSource = _postList;
                                }
                            }));

                            await Ads.Instance.ClosePageAsync(crawlPage);
                        }
                    });

                    allTasks.Add(t);
                }
                // ⭐ CHỜ TẤT CẢ PROFILE CHẠY XONG SONG SONG
                await Task.WhenAll(allTasks);

                MessageBox.Show("🎉 Hoàn tất quét đồng thời!");

            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi: " + ex.Message);
                Libary.Instance.LogForm(module, "[FirstScan] ❌ " + ex.Message);
            }

        }
    }
}
