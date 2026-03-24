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
namespace CrawlFB_PW._1._0.Auto
{
    public partial class FAutoCrawlPageMonitor : Form
    {
        const string module = nameof(FAutoCrawlPageMonitor);
        const int MaxTabsPerProfile = 3;
        private List<PageInfoDTO> _selectedPages = new List<PageInfoDTO>();
        private BindingList<PageMonitorViewModel> _monitorList;
        private BindingList<PostInfoViewModel> _postList = new BindingList<PostInfoViewModel>();
        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>(); //lưu danh sách profile
        private List<string> _listUrls = new List<string>();// lưu danh sách page
        private Dictionary<string, List<ShareItem>> _shareMap = new Dictionary<string, List<ShareItem>>();// lưa để thêm bảng share
        // Lock để tránh race condition khi chạy đa profile
        private readonly object _shareMapLock = new object();
        public FAutoCrawlPageMonitor()
        {
            InitializeComponent();
            this.Load += FAutoCrawlPageMonitor_Load;
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

            var rnd = new Random();

            if (range.Contains("5-10"))
                return TimeSpan.FromMinutes(rnd.Next(5, 11));

            if (range.Contains("10-20"))
                return TimeSpan.FromMinutes(rnd.Next(10, 21));

            if (range.Contains("30-60"))
                return TimeSpan.FromMinutes(rnd.Next(30, 61));

            if (range.Contains("1-2"))
                return TimeSpan.FromMinutes(rnd.Next(60, 121));

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
        // load page
        public List<PageMonitorViewModel> GetPageMonitorVM()
        {
            var list = new List<PageMonitorViewModel>();

            using (var conn = SQLDAO.Instance.OpenConnection())
            {
                string sql = @"
        SELECT m.PageID, p.PageName, m.Status, m.LastScanTime
        FROM TablePageMonitor m
        LEFT JOIN TablePageInfo p ON m.PageID = p.PageID
        ";

                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                {
                    int stt = 1;

                    while (rd.Read())
                    {
                        list.Add(new PageMonitorViewModel
                        {
                            STT = stt++,
                            Select = false,
                            PageID = rd["PageID"].ToString(),
                            PageName = rd["PageName"]?.ToString(),
                            Status = rd["Status"]?.ToString(),
                            PostScan = 0,
                            LastScanTime = rd["LastScanTime"] as DateTime?
                        });
                    }
                }
            }

            return list;
        }
        // Phần chia tab chia page chia profile
       
        private void LoadMonitor()
        {
            var data = GetPageMonitorVM();

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
                "LastScanTime"
            );

            UIGridHelper.ApplyVietnameseCaption(gv);

            gv.Columns["PostScan"].Caption = "Post Scan";
            gv.Columns["LastScanTime"].Caption = "Lần quét cuối";

            gv.BestFitColumns();
        }
        private void btnStart_ItemClick(object sender, ItemClickEventArgs e)
        {
            var (startTime, delayStr) = GetAutoConfig();

            if (!IsStartTimeReached(startTime))
            {
                MessageBox.Show($"⏳ Chưa đến giờ chạy: {startTime}");
                return;
            }

            if (!SelectProfilesForScan(out _selectedProfiles))
                return;

            var selectedPages = _monitorList
                .Where(x => x.Select)
                .Select(x => SQLDAO.Instance.GetPageByID(x.PageID))
                .ToList();

            if (selectedPages.Count == 0)
            {
                MessageBox.Show("❌ Chưa chọn page!");
                return;
            }

            var service = new PageDistributionService();

            var profileQueues = service.Distribute(
                _selectedProfiles,
                selectedPages,
                x => x.PageName,
                "Auto"
            );
            foreach (var kv in profileQueues)
            {
                var profile = kv.Key;
                var queue = new Queue<PageInfo>(kv.Value); // 🔥 FIX

                _ = Task.Run(() => ProfileWorker(profile, queue, delayStr));
            }

            MessageBox.Show("🚀 Auto đã bắt đầu!");
        }
        // ================= WORKER =================
        private async Task ProfileWorker(ProfileDB profile, Queue<PageInfo> queue, string delayRange)
        {
            var managerDao = new ManagerProfileDAO();

            while (true)
            {
                while (queue.Count > 0)
                {
                    int used = managerDao.CountMappingByProfile(profile.ID);

                    if (used < MaxTabsPerProfile)
                    {
                        var pageInfo = queue.Dequeue();
                        LaunchPageTask(profile, pageInfo, delayRange);
                        await Task.Delay(200);
                    }
                    else
                    {
                        await Task.Delay(300);
                    }
                }

                while (managerDao.CountMappingByProfile(profile.ID) > 0)
                {
                    await Task.Delay(500);
                }

                var delay = RandomDelay(delayRange);
                await Task.Delay(delay);

                RefillProfileQueue(profile, queue);
            }
        }

        private void RefillProfileQueue(ProfileDB profile, Queue<PageInfo> queue)
        {
            var pages = _monitorList
                .Where(x => x.Select)
                .Select(x => SQLDAO.Instance.GetPageByID(x.PageID))
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            foreach (var p in pages)
                queue.Enqueue(p);
        }

        // ================= LAUNCH TASK =================
        private void LaunchPageTask(ProfileDB profile, PageInfo pageInfo, string delayRange)
        {
            var profileDao = new ProfileInfoDAO();
            var managerDao = new ManagerProfileDAO();

            profileDao.ChangeRuntimeUseTab(profile.ID, +1);

            managerDao.InsertMapping(new ManagerProfileDTO
            {
                IDProfile = profile.ID,
                PageIDCrawl = pageInfo.PageID,
                LinkFBCrawl = pageInfo.PageLink
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    var page = await AdsPowerPlaywrightManager.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                    if (page == null) return;

                    UpdateStatus(pageInfo.PageID, "Đang chạy");

                    var result = await AutoPageDAO.Instance.RunAutoAsync(page, pageInfo);

                    UpdateResult(pageInfo.PageID, result.NewPosts);
                }
                catch
                {
                    UpdateStatus(pageInfo.PageID, "Lỗi");
                }
                finally
                {
                    profileDao.ChangeRuntimeUseTab(profile.ID, -1);
                }
            });
        }

        // ================= UI UPDATE =================
        private void UpdateStatus(string pageId, string status)
        {
            var vm = _monitorList.FirstOrDefault(x => x.PageID == pageId);
            if (vm == null) return;

            this.BeginInvoke((Action)(() =>
            {
                vm.Status = status;
            }));
        }

        private void UpdateResult(string pageId, int newPosts)
        {
            var vm = _monitorList.FirstOrDefault(x => x.PageID == pageId);
            if (vm == null) return;

            this.BeginInvoke((Action)(() =>
            {
                vm.PostScan += newPosts;
                vm.LastScanTime = DateTime.Now;
                vm.Status = "Nghỉ";
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

        private void btn_StartAuto_ItemClick(object sender, ItemClickEventArgs e)
        {
            var (startTime, delayStr) = GetAutoConfig();

            if (!IsStartTimeReached(startTime))
            {
                MessageBox.Show($"⏳ Chưa đến giờ chạy: {startTime}");
                return;
            }

            if (!SelectProfilesForScan(out _selectedProfiles))
                return;

            var selectedPages = _monitorList
                .Where(x => x.Select)
                .Select(x => SQLDAO.Instance.GetPageByID(x.PageID))
                .ToList();

            if (selectedPages.Count == 0)
            {
                MessageBox.Show("❌ Chưa chọn page!");
                return;
            }

            var service = new PageDistributionService();

            var profileQueues = service.Distribute(
                _selectedProfiles,
                selectedPages,
                x => x.PageName,
                "Auto"
            );
            foreach (var kv in profileQueues)
            {
                var profile = kv.Key;
                var queue = new Queue<PageInfo>(kv.Value); // 🔥 FIX

                _ = Task.Run(() => ProfileWorker(profile, queue, delayStr));
            }

            MessageBox.Show("🚀 Auto đã bắt đầu!");
        }
    }
}
