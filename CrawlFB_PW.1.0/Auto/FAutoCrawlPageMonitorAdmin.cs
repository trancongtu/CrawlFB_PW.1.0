using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Page;
using CrawlFB_PW._1._0.Profile;
using CrawlFB_PW._1._0.ViewModels;
using DevExpress.XtraBars;
using CrawlFB_PW._1._0.DAO;
using PageInfoDTO = CrawlFB_PW._1._0.DTO.PageInfo;
using System.Data.SqlClient;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.DAO.Auto;
using System.Threading;
using CrawlFB_PW._1._0.Service;
using System.Security.Cryptography;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using DocumentFormat.OpenXml.Wordprocessing;
using CrawlFB_PW._1._0.Helper.UI;
using CrawlFB_PW._1._0.Runtime;
using DevExpress.XtraGrid;
using CrawlFB_PW._1._0.Helper.Mapper;
using CrawlFB_PW._1._0.Service.AutoRunTime;
namespace CrawlFB_PW._1._0.Auto
{
    public partial class FAutoCrawlPageMonitorAdmin : Form
    {
        const string module = nameof(FAutoCrawlPageMonitor);
        const int MaxTabsPerProfile = 3;      
        private static readonly Random _rnd = new Random();
        private BindingList<PageMonitorViewModel> _monitorList;
        Dictionary<string, PageMonitorViewModel> _monitorDict;
 
        Dictionary<string, PageRuntimeContext> _pageContexts;
        private bool _isAutoSave = false;
        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>(); //lưu danh sách profile
        private System.Windows.Forms.Timer _countdownTimer;
        private bool _isInit = true; // chặn lỗi null
        //đếm tab
        private HashSet<string> _runningPages = new HashSet<string>();
        private readonly object _runningLock = new object();
        //new multi
        private AutoSchedulerService _scheduler;
        public FAutoCrawlPageMonitorAdmin()
        {
            InitializeComponent();
            this.Load += FAutoCrawlPageMonitor_Load;
            btn_AutoSave.EditValueChanged -= btn_AutoSave_EditValueChanged;
            btn_AutoSave.EditValueChanged += btn_AutoSave_EditValueChanged;

            // 🔥 set trạng thái ban đầu
            btn_AutoSave.EditValue = _isAutoSave;
            _isInit = false; // 🔥 sau cùng
        }
        private void FAutoCrawlPageMonitor_Load(object sender, EventArgs e)
        {
            // ===== TIME START =====
            repositoryItemTimeEdit1.Mask.EditMask = "HH:mm";
            repositoryItemTimeEdit1.Mask.UseMaskAsDisplayFormat = true;

            // mặc định: hiện tại + 5 phút
            Edit_TimeStart.EditValue = DateTime.Now.AddMinutes(5).TimeOfDay;

            // ===== DELAY COMBO =====
            repositoryItemComboBox1.Items.Clear();

            repositoryItemComboBox1.Items.AddRange(new string[]
            {
                "5-10 phút",
                "10-20 phút",
                "30-60 phút",
                "1-2 giờ"
            });

            // default
            CbEdit_TimeDelay.EditValue = "5-10 phút";
            LoadMonitor();
            _countdownTimer = new System.Windows.Forms.Timer();
            _countdownTimer.Interval = 1000;
            _countdownTimer.Tick += (s, ev) =>
            {
                foreach (var vm in _monitorList)
                {
                    if (vm.Countdown > 0)
                        vm.Countdown--;
                }

                gridView1.RefreshData();
            };
            _countdownTimer.Start();
        }

        // =============================
        // LẤY CONFIG AUTO
        // =============================
        private (TimeSpan startTime, string delay) GetAutoConfig()
        {
            TimeSpan startTime = TimeSpan.Zero;
            string delay = "";

            try
            {
                if (Edit_TimeStart.EditValue != null)
                    startTime = (TimeSpan)Edit_TimeStart.EditValue;

                delay = CbEdit_TimeDelay.EditValue?.ToString();
            }
            catch { }

            return (startTime, delay);
        }

        // =============================
        // RANDOM DELAY
        // =============================

        private TimeSpan RandomDelay(string range)
        {
            if (string.IsNullOrEmpty(range))
                return TimeSpan.FromMinutes(10);

            if (range.Contains("5-10"))
                return TimeSpan.FromMinutes(_rnd.Next(5, 11));

            if (range.Contains("10-20"))
                return TimeSpan.FromMinutes(_rnd.Next(10, 21));

            if (range.Contains("30-60"))
                return TimeSpan.FromMinutes(_rnd.Next(30, 61));

            if (range.Contains("1-2"))
                return TimeSpan.FromMinutes(_rnd.Next(60, 121));

            return TimeSpan.FromMinutes(10);
        }

        // =============================
        // CHECK ĐẾN GIỜ CHƯA
        // =============================
        private bool IsStartTimeReached(TimeSpan startTime)
        {
            var now = DateTime.Now.TimeOfDay;
            return now >= startTime;
        }

        // =============================
        // DEMO NÚT START
        // =============================
        //Chọn ProFile
        private bool SelectProfilesForScan(out List<ProfileDB> profiles)
        {
            profiles = null;

            try
            {
                using (var frm = new SelectProfileDB("auto"))
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
                        $"Đã chọn {profiles.Count} profile để chạy Auto"
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
        // load page
        private void LoadMonitor()
        {
            var data = SQLDAO.Instance.GetPageMonitorVM();

            int stt = 1;
            foreach (var item in data)
            {
                item.STT = stt++;
                item.Select = false;
            }

            _monitorList = new BindingList<PageMonitorViewModel>(data);
            gridControl1.DataSource = _monitorList;

            InitGrid();
        }
        private void InitGrid()
        {
            var gv = gridView1;

            gv.PopulateColumns();

            UIGridHelper.EnableRowIndicatorSTT(gv);
            UIGridHelper.ApplySelect(gv, gridControl1);

            UIGridHelper.ShowOnlyColumns(
                gv,
                "Select",
                "PageName",
                "Status",
                "PostScan",
                "PostSaved",
                "Countdown",
                "LastScanTime"
            );

            UIGridHelper.ApplyVietnameseCaption(gv);

            gv.Columns["PostScan"].Caption = "Post Scan";
            gv.Columns["PostSaved"].Caption = "Đã lưu";
            gv.Columns["LastScanTime"].Caption = "Lần quét cuối";
            gv.Columns["Countdown"].Caption = "Chờ (s)";
            gv.BestFitColumns();
        }

        // ================= UI UPDATE =================
        private void UpdateStatus(string pageId, AutoService.PageStatus status)
        {
            PageMonitorViewModel vm;
            if (!_monitorDict.TryGetValue(pageId, out vm))
                return;

            this.BeginInvoke((Action)(() =>
            {
                if (status == AutoService.PageStatus.Running)
                    vm.Status = "Đang chạy";
                else if (status == AutoService.PageStatus.StopScroll)
                    vm.Status = "Dừng scroll";
                else if (status == AutoService.PageStatus.Resting)
                    vm.Status = "Nghỉ";
                else
                    vm.Status = "Idle";
            }));
        }
        private void UpdateResult(string pageId, int newPosts, int savedPosts)
        {
            if (!_monitorDict.TryGetValue(pageId, out var vm))
                return;

            this.BeginInvoke((Action)(() =>
            {
                vm.PostScan += newPosts;     // ✅ cộng dồn đúng số crawl
                vm.PostSaved += savedPosts;  // giữ nguyên
                vm.LastScanTime = DateTime.Now;
            }));
        }

        private void btn_Addpage_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var f = new FAddPageAuto())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    var pages = f.SelectedPages;

                    foreach (var p in pages)
                    {
                        // ❗ tránh add trùng
                        if (_monitorList.Any(x => x.PageID == p.PageID))
                            continue;

                        _monitorList.Add(new PageMonitorViewModel
                        {
                            STT = _monitorList.Count + 1,
                            Select = false,
                            PageID = p.PageID,
                            PageName = p.PageName,
                            Status = "Nghỉ",
                            PostScan = 0,
                            LastScanTime = null
                        });
                    }

                    gridView1.RefreshData();
                }
            }
        }

        private int _totalNew = 0;
        private int _totalSaved = 0;
        private int _runningTabs = 0;

        private async void btn_StartAuto_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                // ===================== CONFIG =====================
                var (startTime, delayStr) = GetAutoConfig();
                _countdownTimer?.Start();

                var now = DateTime.Now.TimeOfDay;
                TimeSpan wait = startTime > now ? startTime - now : TimeSpan.Zero;

                // ===================== SELECT PROFILE =====================
                if (!SelectProfilesForScan(out _selectedProfiles))
                    return;

                // ===================== SELECT PAGE =====================
                var selectedPages = _monitorList
                    .Where(x => x.Select)
                    .Select(x => SQLDAO.Instance.GetPageByID(x.PageID))
                    .ToList();

                if (selectedPages.Count == 0)
                {
                    MessageBox.Show("❌ Chưa chọn page!");
                    return;
                }

                // ===================== INIT STATE =====================
                _monitorDict = _monitorList.ToDictionary(x => x.PageID);

                _pageContexts = selectedPages.ToDictionary(
                    p => p.PageID,
                    p => new PageRuntimeContext
                    {
                        PageID = p.PageID,
                        PageName = p.PageName
                    });

                Libary.Instance.LogForm(module, $"📄 Đã nạp {selectedPages.Count} page để chạy Auto");
                Libary.Instance.LogForm(module, $"👤 Sử dụng {_selectedProfiles.Count} profile");
                Libary.Instance.LogForm(module, $"⚙ AutoSave: {(_isAutoSave ? "ON" : "OFF")}");

                // ===================== RESET COUNTER =====================
                _totalNew = 0;
                _totalSaved = 0;

                lock (_runningLock)
                {
                    _runningPages.Clear();
                    _runningTabs = 0;
                }

                // ===================== POPUP =====================
                var popup = PopupAuto.Ensure();
                popup.InitEmpty();
                popup.Show();

                // ===================== CREATE SCHEDULER =====================
                _scheduler?.Stop();
                _scheduler = new AutoSchedulerService();

                // ===================== DISTRIBUTE PAGE =====================
                var distributor = new PageDistributionService();

                var pageByProfile = distributor.Distribute(
                    _selectedProfiles,
                    selectedPages,
                    x => x.PageName,
                    "page"
                );

                // ===================== CREATE TAB =====================
                foreach (var kv in pageByProfile)
                {
                    var profile = kv.Key;
                    var pages = kv.Value;

                    // 🔥 reset profile 1 lần (OK giữ)
                    var mainPage = await Ads.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
                    if (mainPage == null)
                        continue;

                    int createdTabs = 0;

                    foreach (var page in pages)
                    {
                        if (createdTabs >= MaxTabsPerProfile)
                            break;

                        try
                        {
                            // 🔥 mở tab + vào page NGAY
                            var tab = await Ads.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                            

                            await tab.GotoAsync(page.PageLink,
                              new Microsoft.Playwright.PageGotoOptions
                              {
                                  WaitUntil = Microsoft.Playwright.WaitUntilState.DOMContentLoaded,
                                  Timeout = 60000
                              }
                          );

                            var runtime = new PageRuntime
                            {
                                PageId = page.PageID,
                                PageName = page.PageName,
                                PageInfo = page,
                                Page = tab,
                                NextRunTime = DateTime.Now,
                                ProfileId = profile.IDAdbrowser,      // 🔥 THÊM
                                ProfileName = profile.ProfileName
                            };

                            _scheduler.AddPage(runtime);
                            if (tab != null)
                                _runningTabs++;
                            createdTabs++;

                            Libary.Instance.LogForm(module,$"🧩 [{profile.ProfileName}] Tab {createdTabs} → {page.PageName}");
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.LogForm(module,$"❌ Lỗi mở tab {page.PageName}: {ex.Message}");
                        }
                    }
                }

                // ===================== EVENT =====================
                _scheduler.OnProgress += (pageId, status, newPosts, savedPosts) =>
                {
                    if (!_monitorDict.TryGetValue(pageId, out var vm))
                        return;

                    var name = vm?.PageName ?? pageId;

                    this.Invoke(new Action(() =>
                    {
                        UpdateStatus(pageId, status);

                        int saved = _isAutoSave ? savedPosts : 0;
                        UpdateResult(pageId, newPosts, saved);

                        if (newPosts > 0) _totalNew += newPosts;


                        lock (_runningLock)
                        {
                            if (status == AutoService.PageStatus.Running)
                                _runningPages.Add(pageId);
                            else
                                _runningPages.Remove(pageId);
                        }

                        PopupAuto.Ensure().UpdateProgress(
                            _pageContexts.Count,     
                            _runningTabs,            
                            _totalNew,
                            _totalSaved
                        );

                        // countdown fix
                        if (status == AutoService.PageStatus.StopScroll && vm != null)
                        {
                            if (vm.Countdown <= 0)
                            {
                                var delay = RandomDelay(delayStr);
                                int seconds = (int)delay.TotalSeconds;

                                if (newPosts == 0)
                                    vm.DelayExtra += 600;
                                else
                                    vm.DelayExtra = 0;

                                vm.Countdown = seconds + vm.DelayExtra;
                            }
                        }
                    }));
                };

                _scheduler.OnNewPosts += (pageId, posts, shares) =>
                {
                    if (!_monitorDict.TryGetValue(pageId, out var vmMonitor))
                        return;

                    var name = vmMonitor.PageName;

                    this.Invoke(new Action(() =>
                    {
                        // ================= AUTO SAVE =================
                        if (_isAutoSave)
                        {
                            try
                            {
                                int inserted = SQLDAO.Instance.InsertPostBatchAuto_V3(
                                    posts,
                                    shares ?? new List<ShareItem>(),
                                    pageId,
                                    posts.FirstOrDefault()?.PageLink
                                );

                                _totalSaved += inserted;

                                Libary.Instance.LogForm(module,$"💾 AUTO SAVE [{name}] +{inserted}/{posts.Count}");
                            }
                            catch (Exception ex)
                            {
                                Libary.Instance.LogForm(module,$"❌ SAVE ERROR [{name}] {ex.Message}");
                            }

                            return; // 🔥 không cần add RAM nữa
                        }

                        // ================= RAM MODE =================
                        if (!_pageContexts.TryGetValue(pageId, out var ctx))
                            return;

                        var list = posts.Select(p => p.ToViewModel()).ToList();

                        lock (ctx.LockObj)
                        {
                            var existingLinks = new HashSet<string>(
                                ctx.Posts.Select(x => x.PostLink));

                            var newItems = list
                                .Where(p => !existingLinks.Contains(p.PostLink))
                                .ToList();

                            ctx.Posts.AddRange(newItems);

                            if (ctx.Posts.Count > 300)
                                ctx.Posts.RemoveRange(0, 100);
                        }

                        Libary.Instance.LogForm(module, $"📥 {name}: +{posts.Count} post");

                        foreach (var post in posts)
                        {
                            LogPostResult(module, post);
                        }
                    }));
                };

                // ===================== START =====================
                _ = Task.Run(async () =>
                {
                    if (wait.TotalSeconds > 0)
                    {
                        for (int i = (int)wait.TotalSeconds; i >= 0; i--)
                        {
                            int seconds = i;

                            this.Invoke(new Action(() =>
                            {
                                this.Text = $"⏳ Auto sau: {seconds}s";

                                foreach (var vm in _monitorList)
                                    vm.Countdown = seconds;

                                gridView1.RefreshData();
                                PopupAuto.Ensure().UpdateStartCountdown(seconds);
                            }));

                            await Task.Delay(1000);
                        }
                    }

                    _scheduler.Start();
                });

                MessageBox.Show("🚀 Auto đã bắt đầu!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi: " + ex.Message);
            }
        }
        private void LogPostResult(string module, PostPage post)
        {
            var sb = new StringBuilder();

            sb.AppendLine("📝 POST RESULT");
            sb.AppendLine($"Page Chứa: {post.PageName}");
            sb.AppendLine($"PageLink: {post.PageLink ?? "N/A"}");
            sb.AppendLine($"PageID: {post.PageID ?? "N/A"}");
            sb.AppendLine($"ContainerIdFB: {post.ContainerIdFB ?? "N/A"}");
            sb.AppendLine($"Container: {post.ContainerType}");
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
        private void btn_AutoSave_ItemClick(object sender, ItemClickEventArgs e)
        {
           
        }
        private void btn_AutoSave_EditValueChanged(object sender, EventArgs e)
        {
            if (_isInit) return; // 🔥 chặn lúc load

            bool isChecked = Convert.ToBoolean(btn_AutoSave.EditValue);

            int selectedCount = _monitorList.Count(x => x.Select);

            if (selectedCount >= 5)
            {
                _isAutoSave = true;

                btn_AutoSave.EditValue = true; // 🔥 ép lại UI

                MessageBox.Show("⚠ >5 page bắt buộc AutoSave ON");

                return;
            }

            _isAutoSave = isChecked;

            Libary.Instance.LogForm(module, $"AutoSave: {(_isAutoSave ? "ON" : "OFF")}");
        }
        private async void btn_StopAuto_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 🔥 stop scheduler
            _scheduler?.Stop();

            // 🔥 đóng toàn bộ tab
            if (_scheduler != null)
            {
                foreach (var p in _scheduler.GetAllRuntimes())
                {
                    try
                    {
                        if (p.Page != null)
                            await p.Page.CloseAsync();
                    }
                    catch { }
                }
            }

            _scheduler = null;

            // 🔥 dừng countdown timer
            _countdownTimer?.Stop();

            // 🔥 reset UI
            foreach (var vm in _monitorList)
            {
                vm.Countdown = 0;
                vm.Status = "Đã dừng";
            }

            lock (_runningLock)
            {
                _runningPages.Clear();
                _runningTabs = 0;
            }

            gridView1.RefreshData();

            // 🔥 ẩn popup
            PopupAuto.Ensure().Hide();

            // =============================
            // ✅ UPDATE TIME TỪ DB
            // =============================
            if (_isAutoSave)
            {
                foreach (var pageInfo in _monitorList)
                {
                    var newestTime = SQLDAO.Instance.GetNewestPostTime(pageInfo.PageID);

                    if (newestTime.HasValue)
                    {
                        if (!pageInfo.TimeLastPost.HasValue || newestTime > pageInfo.TimeLastPost)
                        {
                            SQLDAO.Instance.UpdatePageLastPostTime(pageInfo.PageID, newestTime);
                            pageInfo.TimeLastPost = newestTime;
                        }
                    }
                }
            }

            MessageBox.Show("⛔ Đã dừng Auto");
        }

        private void btn_ShowPopup_ItemClick(object sender, ItemClickEventArgs e)
        {
            PopupAuto.Ensure().Show();
        }
        private void btn_ViewPost_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selected = _monitorList.Where(x => x.Select).ToList();

            if (selected.Count != 1)
            {
                MessageBox.Show("⚠ Chỉ chọn 1 page!");
                return;
            }

            var pageId = selected[0].PageID;

            if (!_pageContexts.TryGetValue(pageId, out var ctx))
            {
                MessageBox.Show("❌ Chưa có dữ liệu!");
                return;
            }

            var bind = new BindingList<PostInfoViewModel>(ctx.Posts);
            new FViewPostAuto(bind).Show();
        }

        private void btn_SavePost_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_scheduler != null)
            {
                MessageBox.Show("⚠ Đang chạy Auto, hãy Stop trước khi lưu!");
                return;
            }
            if (_isAutoSave)
            {
                MessageBox.Show("⚠ Đang bật AutoSave, không cần lưu thủ công!");
                return;
            }
            
            var selectedIds = _monitorList
                .Where(x => x.Select)
                .Select(x => x.PageID)
                .ToList();

            if (selectedIds.Count == 0)
            {
                MessageBox.Show("⚠ Vui lòng chọn Page cần lưu!");
                return;
            }

            if (MessageBox.Show(
                $"Lưu dữ liệu cho {selectedIds.Count} page?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int totalPages = 0;
            int totalPostsInserted = 0;
            int totalPostsAll = 0;
            int totalShares = 0;

            foreach (var pageId in selectedIds)
            {
                if (!_pageContexts.TryGetValue(pageId, out var ctx))
                    continue;

                if (ctx.Posts == null || ctx.Posts.Count == 0)
                    continue;

                totalPages++;

                List<PostPage> postList;
                List<ShareItem> shares;

                lock (ctx.LockObj)
                {

                foreach (var vm in ctx.Posts)
                    {
                        Libary.Instance.LogForm("DEBUG",
                            $"[BEFORE_SAVE] {vm.PostLink} | Container={vm.ContainerType}");
                    }
                    postList = ctx.Posts
                        .Select(x => x.ToPostPage())
                        .ToList();

                    shares = ctx.Shares?.ToList() ?? new List<ShareItem>();

                    ctx.Posts.Clear();
                    ctx.Shares.Clear();
                }

                // 🔥 CHÈN Ở ĐÂY (QUAN TRỌNG)
                totalPostsAll += postList.Count;

                // 🔥 SAVE
                int inserted = SQLDAO.Instance.InsertPostBatchAuto_V3(
                    postList,
                    shares,
                    pageId,
                    postList.FirstOrDefault()?.PageLink
                );

                int skipped = postList.Count - inserted;

                totalPostsInserted += inserted;
                totalShares += shares?.Count ?? 0;

                Libary.Instance.LogForm("SAVE",
                    $"📄 {ctx.PageName} | Tổng: {postList.Count} | Mới: {inserted} | Trùng: {skipped}");
            }

            // =========================
            // 📊 RESULT
            // =========================
            int totalSkipped = totalPostsAll - totalPostsInserted;

            MessageBox.Show(
                $"✅ Lưu thành công!\n\n" +
                $"Page   : {totalPages}\n" +
                $"Post   : {totalPostsInserted}/{totalPostsAll}\n" +
                $"Bỏ qua : {totalSkipped}\n" +
                $"Share  : {totalShares}",
                "Save DB",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void CbEdit_TimeDelay_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
