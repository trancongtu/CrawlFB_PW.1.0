// FAutoSupervisePage.cs - Clean final version
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Profile;
using DevExpress.XtraGrid.Views.Grid;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;

namespace CrawlFB_PW._1._0.Page
{
    public partial class FAutoSupervisePage : Form
    {
        private const int MaxTabsPerProfile = 3;
        private int globalTotalPosts = 0;
        private int totalAutoPages = 0;

        // editors for checkbox enabling/disabling
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repoCheck;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repoCheckDisabled;

        // scheduler data
        private Dictionary<ProfileDB, Queue<PageInfo>> profileQueues = new Dictionary<ProfileDB, Queue<PageInfo>>();
        // master groups to refill per profile after each round
        private Dictionary<ProfileDB, List<PageInfo>> originalGroups = new Dictionary<ProfileDB, List<PageInfo>>();
        private List<ProfileDB> selectedProfiles = new List<ProfileDB>();
       
        public FAutoSupervisePage()
        {
            InitializeComponent();

            this.Load += FAutoSupervisePage_Load;
        }

        private void FAutoSupervisePage_Load(object sender, EventArgs e)
        {         
            SetupCheckboxEditors();
            LoadPageMonitor();
            gridView1.OptionsBehavior.Editable = true;

        }

        // ---------------- UI & Grid Setup ----------------
        private void SetupCheckboxEditors()
        {
            repoCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit()
            {
                ValueChecked = true,
                ValueUnchecked = false,
                ReadOnly = false
            };
            repoCheckDisabled = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit()
            {
                ValueChecked = true,
                ValueUnchecked = false,
                ReadOnly = true
            };

            if (!gridControl1.RepositoryItems.Contains(repoCheck))
                gridControl1.RepositoryItems.Add(repoCheck);
            if (!gridControl1.RepositoryItems.Contains(repoCheckDisabled))
                gridControl1.RepositoryItems.Add(repoCheckDisabled);
        }

        private void ConfigureGridView()
        {
           
            var gv = gridView1;

            gv.PopulateColumns();

            // add STT if missing (unbound)
            if (!gv.Columns.Any(c => c.FieldName == "STT"))
            {
                var col = gv.Columns.AddField("STT");
                col.VisibleIndex = 0;
                col.Caption = "STT";
                col.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                col.OptionsColumn.AllowEdit = false;
            }

            // add Selected checkbox column if missing
            if (!gv.Columns.Any(c => c.FieldName == "Selected"))
            {
                var col = gv.Columns.AddField("Selected");
                col.VisibleIndex = 1;
                col.Caption = "Chọn";
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 40;
                col.ColumnEdit = repoCheck; // default editor
            }

            SafeCaption(gv, "PageID", "Page ID");
            SafeCaption(gv, "PageName", "Tên Page");
            SafeCaption(gv, "Status", "Trạng thái");
            SafeCaption(gv, "LastScanTime", "Lần cuối quét");

            // ensure we don't attach duplicates
            gv.CustomUnboundColumnData -= Gv_CustomUnboundColumnData;
            gv.CustomUnboundColumnData += Gv_CustomUnboundColumnData;

            gv.CustomRowCellEdit -= Gv_CustomRowCellEdit;
            gv.CustomRowCellEdit += Gv_CustomRowCellEdit;

            gv.RowStyle -= Gv_RowStyle;
            gv.RowStyle += Gv_RowStyle;
            UICommercialHelper.StyleGrid(gridView1);
            gv.BestFitColumns();
        }
        private void SafeCaption(GridView gv, string field, string caption)
        {
            var col = gv.Columns[field];
            if (col != null)
                col.Caption = caption;
        }

        private void Gv_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.Column.FieldName == "STT" && e.IsGetData)
            {
                e.Value = e.ListSourceRowIndex + 1;
            }
        }

        private void Gv_CustomRowCellEdit(object sender, DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "Selected") return;

            var gv = sender as GridView;
            string status = string.Empty;
            try { status = gv.GetRowCellValue(e.RowHandle, "Status")?.ToString() ?? ""; } catch { }

            if (!string.IsNullOrEmpty(status) && status.StartsWith("Running"))
                e.RepositoryItem = repoCheckDisabled;
            else
                e.RepositoryItem = repoCheck;
        }

        private void Gv_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle < 0) return;
            var gv = sender as GridView;
            string status = string.Empty;
            try { status = gv.GetRowCellValue(e.RowHandle, "Status")?.ToString() ?? ""; } catch { }

            if (!string.IsNullOrEmpty(status) && status.StartsWith("Running"))
            {
                e.Appearance.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
                e.Appearance.ForeColor = System.Drawing.Color.Black;
            }
        }

        // ---------------- Monitor loading ----------------
        private void LoadPageMonitor()
        {
            try
            {
                var dt = DatabaseDAO.Instance.GetMonitoredPages();

                // Thêm cột Selected nếu chưa có
                if (!dt.Columns.Contains("Selected"))
                {
                    dt.Columns.Add("Selected", typeof(bool));
                    foreach (DataRow r in dt.Rows)
                        r["Selected"] = false;
                }

                // Bind lại vào grid
                gridControl1.DataSource = dt;
                ConfigureGridView();
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("LoadPageMonitor ERROR: " + ex.Message);
            }
        }
        //========
        private async Task<IPage> GetTabForProfile(ProfileDB profile)
        {
            // Slot đã được reserve trước khi gọi hàm này (tại LaunchPageTask)
            // → GetPageAsync sẽ mở 1 TAB MỚI trong AdsPower
            var page = await AdsPowerPlaywrightManager.Instance.GetPageAsync(profile.IDAdbrowser);

            if (page == null)
            {
                Libary.Instance.CreateLog($"[ERROR] Không mở được tab mới cho profile {profile.ProfileName}");
            }

            return page;
        }


        // ---------------- Scheduler helpers ----------------

        private Dictionary<ProfileDB, Queue<PageInfo>> DistributePages(List<PageInfo> pages, List<ProfileDB> profiles)
        {
            
            var dict = new Dictionary<ProfileDB, Queue<PageInfo>>();
            for (int i = 0; i < profiles.Count; i++) dict[profiles[i]] = new Queue<PageInfo>();

            int idx = 0;
            foreach (var pg in pages)
            {
                var prof = profiles[idx % profiles.Count];
                dict[prof].Enqueue(pg);
                idx++;
            }
            Libary.Instance.CreateLog("===== DISTRIBUTE PAGES START =====");
            foreach (var kv in dict)
            {
                foreach (var p in kv.Value)
                {
                    Libary.Instance.CreateLog($"Profile [{kv.Key.ProfileName}] nhận Page [{p.PageName}] - ID={p.PageID}");
                }
            }
            Libary.Instance.CreateLog("===== DISTRIBUTE PAGES END =====");
            return dict;
        }

        // refill from originalGroups[profile]
        private void RefillProfileQueueFromOriginal(ProfileDB profile, Queue<PageInfo> queue)
        {
            queue.Clear();
            if (!originalGroups.ContainsKey(profile)) return;
            var list = originalGroups[profile].OrderBy(x => new Random().Next()).ToList();
            foreach (var p in list) queue.Enqueue(p);
        }

        // ---------------- Worker ----------------

        private async Task ProfileWorker(ProfileDB profile, Queue<PageInfo> queue, string delayRange)
        {
            var managerDao = new ManagerProfileDAO();

            while (true)
            {
                // run until the queue is empty (fill is from originalGroups)
                while (queue.Count > 0)
                {
                    int used = managerDao.CountMappingByProfile(profile.ID);

                    if (used < MaxTabsPerProfile)
                    {
                        var pageInfo = queue.Dequeue();
                        Libary.Instance.CreateLog($"[WORKER] Profile={profile.ProfileName} chuẩn bị chạy Page={pageInfo.PageName} | PageID={pageInfo.PageID}");

                        LaunchPageTask(profile, pageInfo, delayRange); // reserve inside
                        await Task.Delay(200);
                    }
                    else
                    {
                        await Task.Delay(300);
                    }
                }

                // wait all tabs release for this profile
                while (managerDao.CountMappingByProfile(profile.ID) > 0)
                {
                    await Task.Delay(500);
                }

                // profile finished a round -> rest then refill
                var delay = RandomDelay(delayRange);
                Libary.Instance.CreateLog($"[Profile {profile.ProfileName}] sleeping {delay.TotalMinutes} minutes before next round");
                await Task.Delay(delay);

                // refill queue from originalGroups (shuffled)
                RefillProfileQueueFromOriginal(profile, queue);
            }
        }

        // ---------------- Launch task (single page) ----------------
        // bước 2 lauchpagetask
        private void LaunchPageTask(ProfileDB profile, PageInfo pageInfo, string delayRange)
        {
            var profileDao = new ProfileInfoDAO();
            var managerDao = new ManagerProfileDAO();

            // ======== RESERVE SLOT ========
            int currentTab = profileDao.GetUseTab(profile.ID);
            profileDao.ChangeRuntimeUseTab(profile.ID, +1);

            int newTabCount = profileDao.GetUseTab(profile.ID);
            ProfileSlotManager.Instance.SetUsedSlots(profile.IDAdbrowser, newTabCount);

            // mapping runtime
            managerDao.InsertMapping(new ManagerProfileDTO
            {
                IDProfile = profile.ID,
                PageIDCrawl = pageInfo.PageID,
                LinkFBCrawl = pageInfo.PageLink
            });
            RefreshGridSafe();
            UpdateSlotUI();

            // ======== CHẠY TASK TRONG BACKGROUND ========
            _ = Task.Run(async () =>
            {
                try
                {
                    Libary.Instance.CreateLog($"[OPEN-TAB] Mở tab mới cho profile={profile.ProfileName}");
                    IPage page = await AdsPowerPlaywrightManager.Instance.OpenNewTabAsync(profile.IDAdbrowser);

                    if (page == null)
                    {
                        Libary.Instance.CreateLog($"[LaunchPageTask] KHÔNG mở tab → huỷ job");

                        // rollback
                        managerDao.RemoveMappingByPageID(profile.ID, pageInfo.PageID);
                        profileDao.ChangeRuntimeUseTab(profile.ID, -1);
                        UpdateSlotUI();
                        return;
                    }

                    // ======== CHẠY AUTO ========
                    var result = await RunAutoPageTask(profile, page, pageInfo);

                    Interlocked.Add(ref globalTotalPosts, result.NewPosts);

                    DatabaseDAO.Instance.UpdateMonitorStatus(
                        pageInfo.PageID,
                        result.NewPosts > 0 ? $"DoneAndWait ({result.NewPosts} new)" : "DoneAndWait (0 new)"
                    );
                    RefreshGridSafe();
                    // ======== TÍNH TIẾN ĐỘ ========
                    int totalPages = totalAutoPages;      // page user tick để quét
                    int completed = GetCompletedPagesCount();

                    // ======== UPDATE POPUP 1 LẦN DUY NHẤT ========
                    this.BeginInvoke((Action)(() =>
                    {
                        PopupAuto.Instance.ShowPopup();
                        PopupAuto.Instance.UpdateProgress(
                            pageInfo.PageName,
                            totalPages,      // tổng
                            completed,       // đã xong
                            globalTotalPosts // tổng bài mới
                        );
                    }));

                    // ======== BẮT ĐẦU ĐẾM LÙI ========
                    var delay = RandomDelay(delayRange);
                    this.BeginInvoke((Action)(() =>
                    {
                        PopupAuto.Instance.StartCountdown((int)delay.TotalSeconds);
                    }));
                }
                catch (Exception ex)
                {
                    Libary.Instance.CreateLog($"[TASK ERROR] {ex}");
                    DatabaseDAO.Instance.UpdateMonitorStatus(pageInfo.PageID, "Error");
                }
                finally
                {
                    // ======== RELEASE SLOT ========
                    profileDao.ChangeRuntimeUseTab(profile.ID, -1);

                    int remainTab = profileDao.GetUseTab(profile.ID);
                    ProfileSlotManager.Instance.SetUsedSlots(profile.IDAdbrowser, remainTab);

                    UpdateSlotUI();

                    // Refresh grid an toàn UI thread
                    this.BeginInvoke((Action)(() => LoadPageMonitor()));
                }
            });
        }

        private void UpdateSlotUI()
        {
            try
            {
                // Tìm form quản lý Slot
                var frm = Application.OpenForms["FSlotManagerProfile"] as FSlotManagerProfile;
                if (frm == null) return;

                // Đảm bảo chạy trên UI thread
                if (frm.InvokeRequired)
                {
                    frm.BeginInvoke((Action)(() => frm.ForceReload()));
                }
                else
                {
                    frm.ForceReload();
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[UpdateSlotUI ERROR] " + ex.Message);
            }
        }

        // ---------------- Auto crawl per page ----------------
        // bước 3 chạy runauto
        private async Task<AutoResult> RunAutoPageTask(ProfileDB profile, IPage page, PageInfo pageInfo)
        {
            AutoResult result = new AutoResult();

            try
            {
                Libary.Instance.CreateLog($"[CRAWL-START] Profile={profile.ProfileName} | Page={pageInfo.PageName}");

                await page.GotoAsync(pageInfo.PageLink,
                    new PageGotoOptions { Timeout = 60000, WaitUntil = WaitUntilState.DOMContentLoaded });

                await page.WaitForTimeoutAsync(1500);

                DateTime lastScan = DatabaseDAO.Instance.GetPageLastScan(pageInfo.PageID);

                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, AppConfig.scrollCount);
                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 1);

                var feed = await PageDAO.Instance.GetFeedContainerAsync(page);
                if (feed == null) return result;

                var nodes = await feed.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");
                HashSet<string> unique = new HashSet<string>();
                bool stop = false;

                foreach (var node in nodes)
                {
                    var posts = await PageDAO.Instance.GetPostAutoPageAsyncV3(page, node, pageInfo.PageName, pageInfo.PageLink);

                    foreach (var post in posts)
                    {
                        result.TotalRead++;

                        if (DatabaseDAO.Instance.ExistPostByLink(post.PostLink)) { stop = true; break; }

                        var realTime = TimeHelper.ParseFacebookTime(post.PostTime);
                        if (realTime <= lastScan) { stop = true; break; }

                        if (unique.Contains(post.PostLink)) continue;

                        unique.Add(post.PostLink);
                        result.Posts.Add(post);
                        result.NewPosts++;
                    }
                    if (stop) break;
                }

                foreach (var p in result.Posts)
                    DatabaseDAO.Instance.InsertOrIgnorePost(p);

                DatabaseDAO.Instance.UpdatePageLastScan(pageInfo.PageID);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("AUTO ERROR: " + ex.Message);
            }

            return result;
        }

        // ---------------- Tab management ----------------

        private void ReleaseTab(ProfileDB profile)
        {
            var managerDao = new ManagerProfileDAO();
            var profileDao = new ProfileInfoDAO();

            int usingTabs = managerDao.CountMappingByProfile(profile.ID);

            managerDao.UpdateProfileUseTab(profile.ID, Math.Max(0, usingTabs - 1));
            profileDao.ChangeRuntimeUseTab(profile.ID, -1);
            ProfileSlotManager.Instance.SetUsedSlots(profile.IDAdbrowser, Math.Max(0, usingTabs - 1));

            Libary.Instance.CreateLog($"[TAB] Release {profile.ProfileName}, {Math.Max(0, usingTabs - 1)}");
        }

        // ---------------- Start flow / distribute ----------------

        private void btnStart_Click(object sender, EventArgs e)
        {
            string delayRange = cbSelectTime.SelectedItem?.ToString();
            string startTime = cbStartTime.SelectedItem?.ToString();
          
            if (selectedProfiles == null || selectedProfiles.Count == 0)
            {
                MessageBox.Show("⚠ Vui lòng chọn profile trước!");
                return;
            }

            var pages = GetSelectedPages();
            if (pages.Count == 0)
            {
                MessageBox.Show("⚠ Hãy chọn ít nhất 1 page!", "Thông báo");
                return;
            }
            foreach (var pg in pages)
            {
                DatabaseDAO.Instance.UpdateMonitorIsAuto(pg.PageID, 1);
            }
            // ⭐ GÁN SỐ PAGE CHO POPUP
            totalAutoPages = pages.Count;
            var popup = PopupAuto.Ensure();
            popup.InitEmpty();
            popup.UpdateProgress(
                "Đang chuẩn bị...",
                totalAutoPages,
                0,
                0
            );
            popup.Show();
            // distribute round-robin
            profileQueues = DistributePages(pages, selectedProfiles);

            // store original groups for refill
            originalGroups.Clear();
            foreach (var kv in profileQueues)
            {
                originalGroups[kv.Key] = kv.Value.ToList();
            }

            // start workers
            foreach (var prof in selectedProfiles)
            {
                var q = profileQueues[prof];
                _ = Task.Run(() => ProfileWorker(prof, q, delayRange));
            }

            LoadPageMonitor();
            MessageBox.Show("✔ Đã bắt đầu giám sát theo Queue Scheduler!");
        }

        private List<PageInfo> GetSelectedPages()
        {
            var list = new List<PageInfo>();
            var gv = gridView1;
            for (int i = 0; i < gv.DataRowCount; i++)
            {
                bool sel = false;
                try { sel = Convert.ToBoolean(gv.GetRowCellValue(i, "Selected")); } catch { }
                if (!sel) continue;
                DataRow r = gv.GetDataRow(i);
                var pi = DatabaseDAO.Instance.GetPageByID(r["PageID"].ToString());
                if (pi != null) list.Add(pi);
            }
            return list;
        }

        // ---------------- Select All / Deselect All ----------------

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            var gv = gridView1;
            gv.BeginUpdate();
            try
            {
                for (int i = 0; i < gv.DataRowCount; i++)
                {
                    string status = gv.GetRowCellValue(i, "Status")?.ToString() ?? "";
                    if (status.StartsWith("Running")) continue;
                    gv.SetRowCellValue(i, "Selected", true);
                }
            }
            finally { gv.EndUpdate(); }
        }

        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            var gv = gridView1;
            gv.BeginUpdate();
            try
            {
                for (int i = 0; i < gv.DataRowCount; i++)
                    gv.SetRowCellValue(i, "Selected", false);
            }
            finally { gv.EndUpdate(); }
        }

        // ---------------- Utilities ----------------

        public int GetCompletedPagesCount()
        {
            var dt = DatabaseDAO.Instance.GetMonitoredPages();
            int count = 0;
            foreach (DataRow r in dt.Rows)
            {
                string st = r["Status"]?.ToString() ?? "";
                if (st.StartsWith("DoneAndWait")) count++;
            }
            return count;
        }

        private TimeSpan RandomDelay(string range)
        {
            if (string.IsNullOrEmpty(range)) return TimeSpan.FromMinutes(60);
            range = range.ToLower().Trim();
            var rnd = new Random();
            if (range.Contains("1-2h")) return TimeSpan.FromMinutes(rnd.Next(60, 121));
            if (range.Contains("30phut") || range.Contains("30 phút")) return TimeSpan.FromMinutes(rnd.Next(30, 61));
            if (range.Contains("5 - 10") || range.Contains("5-10")) return TimeSpan.FromMinutes(rnd.Next(5, 11));
            return TimeSpan.FromMinutes(60);
        }
        private void btnSelectProfile_Click(object sender, EventArgs e)
        {
            var popup = new SelectProfileDB();
            if (popup.ShowDialog() == DialogResult.OK)
            {
                selectedProfiles = popup.Tag as List<ProfileDB>;
                if (selectedProfiles == null || selectedProfiles.Count == 0)
                {
                    MessageBox.Show("Không chọn profile nào!");
                    return;
                }
                MessageBox.Show($"Đã chọn {selectedProfiles.Count} profile");
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            var gv = gridView1;

            // lấy tất cả row đang tick
            List<string> selectedPageIds = new List<string>();

            for (int i = 0; i < gv.DataRowCount; i++)
            {
                bool isChecked = Convert.ToBoolean(gv.GetRowCellValue(i, "Selected"));
                if (isChecked)
                {
                    string pageId = gv.GetRowCellValue(i, "PageID")?.ToString();
                    string isAuto = gv.GetRowCellValue(i, "IsAuto")?.ToString();

                    if (pageId != null && isAuto == "1")
                        selectedPageIds.Add(pageId);
                }
            }

            if (selectedPageIds.Count == 0)
            {
                MessageBox.Show("Không có Page nào đang Auto để dừng!");
                return;
            }

            var managerDao = new ManagerProfileDAO();
            var profileDao = new ProfileInfoDAO();

            foreach (var pageId in selectedPageIds)
            {
                var mappings = managerDao.GetMappingByPageID(pageId);

                foreach (var m in mappings)
                {
                    // xóa mapping theo ID
                    managerDao.RemoveMappingByID(m.ID);

                    var p = new ProfileInfoDAO().GetProfileByID(m.IDProfile);
                    if (p != null)
                    {
                        ProfileSlotManager.Instance.SetUsedSlots(
                            p.IDAdbrowser,
                            profileDao.GetUseTab(m.IDProfile)
                        );
                    }
                }

                // cập nhật monitor
                DatabaseDAO.Instance.UpdateMonitorIsAuto(pageId, 0);
                DatabaseDAO.Instance.UpdateMonitorStatus(pageId, "Stopped");
            }

            LoadPageMonitor();
            UpdateSlotUI();

            MessageBox.Show("Đã dừng thành công!");
        }

        private void btnStopAll_Click(object sender, EventArgs e)
        {
            var dt = DatabaseDAO.Instance.GetMonitoredPages();
            var managerDao = new ManagerProfileDAO();
            var profileDao = new ProfileInfoDAO();

            foreach (DataRow row in dt.Rows)
            {
                if (row["IsAuto"].ToString() == "1")
                {
                    string pageId = row["PageID"].ToString();

                    var mappings = managerDao.GetMappingByPageID(pageId);

                    foreach (var m in mappings)
                    {
                        managerDao.RemoveMappingByID(m.ID);
                        profileDao.ChangeRuntimeUseTab(m.IDProfile, -1);

                        var p = new ProfileInfoDAO().GetProfileByID(m.IDProfile);
                        if (p != null)
                        {
                            ProfileSlotManager.Instance.SetUsedSlots(
                                p.IDAdbrowser,
                                profileDao.GetUseTab(m.IDProfile)
                            );
                        }
                    }
                    DatabaseDAO.Instance.UpdateMonitorIsAuto(pageId, 0);
                    DatabaseDAO.Instance.UpdateMonitorStatus(pageId, "Stopped");
                }
            }

            LoadPageMonitor();
            UpdateSlotUI();

            MessageBox.Show("Đã dừng tất cả Auto!");
        }
        private void RefreshGridSafe()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => LoadPageMonitor()));
            }
            else LoadPageMonitor();
        }

    }
}
