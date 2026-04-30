using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Profile;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Playwright;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.Page
{
    public partial class FFindPage : Form
    {
        private string SelectedProfileId = "";
        private List<string> SelectedProfiles = new List<string>();
        private FBType SearchType = FBType.Unknown;   // "Fanpage" hoặc "Group"
        private bool _isRunning = false;     // đang chạy job
        private bool _hasGridData = false;   // grid có dữ liệu hay chưa
        public class PageInfoGrid
        {
            public bool Select { get; set; } = false;
            public int STT { get; set; }
            public string PageName { get; set; }
            public string PageLink { get; set; }
            public string PageMembers { get; set; }
            public FBType PageType { get; set; }

            // 👉 GIÁ TRỊ THẬT (LOGIC)
            public DateTime? TimeLastPost { get; set; }

            // 👉 HIỂN THỊ UI
            public string LastPostTime { get; set; } = "N/A";
        }

        public FFindPage()
        {
            InitializeComponent();
            textEditMaxPage.Text = "10";   // đặt mặc định
            this.Load += FFindPage_Load;
        }
        private void FFindPage_Load(object sender, EventArgs e)
        {
            // Gắn sự kiện CheckedChanged cho 2 check item
            barCheckItemFanPage.CheckedChanged += barCheckItemFanPage_CheckedChanged;
            barCheckItemGroups.CheckedChanged += barCheckItemGroups_CheckedChanged;

            // Nếu chưa chọn lần nào → set mặc định Fanpage
            if (!barCheckItemFanPage.Checked && !barCheckItemGroups.Checked)
            {
                barCheckItemFanPage.Checked = true; // mặc định tìm Fanpage
            }
            InitGrid();
            UICommercialHelper.StyleGrid(gridView1);
            _isRunning = false;
            _hasGridData = false;
            UpdateBarState();   // 🔒 bar chắc chắn TẮT
            gridView1.RowCellClick += GridView1_RowCellClick;         
        }
        private void InitGrid()
        {
            var gv = gridView1;
            gv.Columns.Clear();
            gv.OptionsBehavior.Editable = true;   // cho editor hoạt động
            gv.OptionsBehavior.ReadOnly = true;   // nhưng vẫn readonly mặc định
            gv.OptionsSelection.MultiSelect = true;
            gv.OptionsSelection.MultiSelectMode =
                DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CellSelect; // ⭐ CELL
            gv.OptionsClipboard.AllowCopy = DevExpress.Utils.DefaultBoolean.True;

            // Cột SELECT (checkbox)
            var colSelect = gv.Columns.AddVisible("Select", "Chọn");
            colSelect.Width = 50;
            colSelect.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            colSelect.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            colSelect.OptionsColumn.FixedWidth = true;
            // Cột STT
            var colSTT = gv.Columns.AddVisible("STT", "STT");
            colSTT.Width = 50;
            colSTT.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            colSTT.OptionsColumn.AllowEdit = false;
            // Tên Page
            var colName = gv.Columns.AddVisible("PageName", "Tên Page");
            colName.Width = 200;
            colName.OptionsColumn.AllowEdit = false;
            // Link
            var colLink = gv.Columns.AddVisible("PageLink", "Địa Chỉ");
            colLink.Width = 250;
            colLink.OptionsColumn.AllowEdit = false;
            // Member / Follower
            var colMembers = gv.Columns.AddVisible("PageMembers", "Theo dõi / Thành viên");
            colMembers.Width = 120;
            colMembers.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            colMembers.OptionsColumn.AllowEdit = false;
            // Type
            var colType = gv.Columns.AddVisible("PageType", "Type");
            colType.Width = 80;
            colType.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            colType.OptionsColumn.AllowEdit = false;

            // Last Post Time
            var colLastPost = gv.Columns.AddVisible("LastPostTime", "Time Last Post");
            colLastPost.Width = 120;
            colLastPost.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            colLastPost.OptionsColumn.AllowEdit = false;
            //
            var colOpen = gv.Columns.AddVisible("OpenLink", "Mở");
            colOpen.UnboundType = DevExpress.Data.UnboundColumnType.String;
            colOpen.Width = 60;
            colOpen.OptionsColumn.AllowEdit = false;
            colOpen.ShowButtonMode =
                DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
            gridView1.CustomUnboundColumnData += (s, e) =>
            {
                if (e.Column.FieldName == "OpenLink" && e.IsGetData)
                {
                    e.Value = "Xem"; // ⭐ chữ hiển thị trong cell
                }
            };

            var linkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            linkEdit.Caption = "Mở";
            linkEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;

            linkEdit.OpenLink += (s, e) =>
            {
                var view = gridView1;
                int rowHandle = view.FocusedRowHandle;
                if (rowHandle < 0) return;

                string link = view.GetRowCellValue(rowHandle, "PageLink")?.ToString();
                if (string.IsNullOrWhiteSpace(link)) return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                });
            };
            gridControl1.RepositoryItems.Add(linkEdit);
            colOpen.ColumnEdit = linkEdit;

        }
        private void SetBarEnabled(bool enabled)
        {
            foreach (DevExpress.XtraBars.BarItemLink link in bar1.ItemLinks)
            {
                link.Item.Enabled = enabled;
            }
        }
        //================ dòng status trên panel===================
        private void ShowLoading(string text = "Đang chạy...")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowLoading(text)));
                return;
            }

            picLoading.Visible = true;
            lblStatus.Text = text;
            lblStatus.Visible = true;
        }
        private void HideLoading(string text = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HideLoading(text)));
                return;
            }

            picLoading.Visible = false;
            lblStatus.Visible = false;

            if (!string.IsNullOrEmpty(text))
            {
                lblStatus.Text = text;
                lblStatus.Visible = true;

                // tự ẩn sau 2 giây
                var t = new System.Windows.Forms.Timer();
                t.Interval = 2000;
                t.Tick += (s, e) =>
                {
                    lblStatus.Visible = false;
                    t.Stop();
                };
                t.Start();
            }
        }
        private void UpdateBarState()
        {
            bool enable = !_isRunning && _hasGridData;
            SetBarEnabled(enable);
        }
        //===================== HẾT CODE STATUS
        private void barEditItemMinFlow_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }
        private void GridView1_RowCellClick( object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            var gv = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (e.RowHandle < 0) return;

            // ⭐ CLICK CỘT "Mở" → MỞ LINK
            if (e.Column.FieldName == "OpenLink")
            {
                string link = gv.GetRowCellValue(e.RowHandle, "PageLink")?.ToString();
                if (!string.IsNullOrWhiteSpace(link))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link,
                        UseShellExecute = true
                    });
                }
                return; // ❗ KHÔNG ĐỤNG SELECT
            }

            // ⭐ CLICK BẤT KỲ CỘT NÀO KHÁC → TOGGLE SELECT
            var row = gv.GetRow(e.RowHandle) as PageInfoGrid;
            if (row == null) return;

            row.Select = !row.Select;
            gv.RefreshRow(e.RowHandle);
        }
        private void barCheckItemFanPage_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItemFanPage.Checked)
            {
                SearchType = FBType.Fanpage;
                barCheckItemGroups.Checked = false;
                Libary.Instance.LogForm(nameof(FFindPage), "Shearch: Fanpage");
            }
        }
        private void barCheckItemGroups_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItemGroups.Checked)
            {
                SearchType = FBType.GroupOn;
                barCheckItemFanPage.Checked = false;
                Libary.Instance.LogForm(nameof(FFindPage), "Shearch: Groups");
            }
        }
        private async void barButtonItemRun_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // ===== 0️⃣ CHECK INPUT CƠ BẢN (KHÔNG KHÓA BAR) =====
            if (SearchType == FBType.Unknown)
            {
                MessageBox.Show(
                    "Bạn chưa chọn loại tìm kiếm (Fanpage hoặc Groups)!",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            string keyword = textEditKeyword.Text.Trim();
            Libary.Instance.LogForm(nameof(FFindPage), $"👤 Từ khóa: {keyword}");

            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Vui lòng nhập từ khóa!");
                return;
            }

            // ===== 1️⃣ LẤY THAM SỐ USER CHỌN =====
            int maxPage = 50;
            if (!string.IsNullOrWhiteSpace(textEditMaxPage.Text))
                int.TryParse(textEditMaxPage.Text.Trim(), out maxPage);

            decimal userVal = 0;
            try
            {
                userVal = Convert.ToDecimal(barEditItemMinFlow.EditValue ?? 0);
            }
            catch { userVal = 0; }

            int minFlow = (int)(userVal * 1000);

            Libary.Instance.LogForm(
                nameof(FFindPage),
                $"👤 Tham số: MaxPage={maxPage}, MinFlow={minFlow}"
            );

            // ===== 2️⃣ CHỌN / AUTO PROFILE =====
            if (string.IsNullOrEmpty(SelectedProfileId))
            {
                var allProfiles = new ProfileInfoDAO().GetAllProfiles();
                if (allProfiles.Count == 0)
                {
                    MessageBox.Show("⚠ Không có profile nào trong DB. Hãy thêm profile trước!");
                    return;
                }

                SelectedProfileId = allProfiles[0].IDAdbrowser;
                Libary.Instance.LogForm(
                    nameof(FFindPage),
                    $"⚙️ Auto chọn profile: {SelectedProfileId}"
                );
            }

            // ===== 3️⃣ BẮT ĐẦU RUN → KHÓA BAR =====
            _isRunning = true;
            UpdateBarState(); // 🔒 khóa bar
            ShowLoading("🔄 Đang tìm kiếm Page Facebook...");
            try
            {
                // ===== 4️⃣ INIT PLAYWRIGHT =====
                bool ok = await FindPageDAO.Instance.InitAsync(SelectedProfileId);
                if (!ok)
                {
                    MessageBox.Show("Không thể khởi tạo Playwright từ profile này!");
                    return;
                }

                // ===== 5️⃣ MỞ TRANG SEARCH =====
                bool opened = false;
                if (SearchType == FBType.Fanpage)
                    opened = await FindPageDAO.Instance.OpenSearchPageAsync(keyword, "pages");
                else if (SearchType == FBType.GroupOn)
                    opened = await FindPageDAO.Instance.OpenSearchPageAsync(keyword, "groups");

                if (!opened)
                {
                    MessageBox.Show("❌ Không mở được trang tìm kiếm Facebook!");
                    return;
                }

                Libary.Instance.LogForm(
                    nameof(FFindPage),
                    "🌐 Đã mở trang tìm kiếm Facebook"
                );

                // ===== 6️⃣ RUN FAST SEARCH =====
                var list = await FindPageDAO.Instance
                    .RunFastSearchAsync(SearchType, maxPage, minFlow);
                Libary.Instance.LogForm(
                    nameof(FFindPage),
                    $"🔎 RunFast hoàn tất: {list.Count} kết quả"
                );
                // ===== 7️⃣ ĐỔ GRID =====
                var gridList = list.Select((p, index) => new PageInfoGrid
                {
                    Select = false,
                    STT = index + 1,
                    PageName = p.PageName,
                    PageLink = p.PageLink,
                    PageMembers = p.PageMembers,
                    PageType = p.PageType,
                    LastPostTime = p.TimeLastPost == null? "N/A": p.TimeLastPost.Value.ToString("dd/MM/yyyy HH:mm"),
                    TimeLastPost = p.TimeLastPost
                }).ToList();
                gridControl1.DataSource = gridList;
                gridView1.RefreshData();
                _hasGridData = gridList.Count > 0;

                MessageBox.Show("✔ Hoàn tất tìm kiếm!");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogForm(
                    nameof(FFindPage),
                    "❌ Lỗi RunFast: " + ex.Message
                );
                MessageBox.Show("Có lỗi khi chạy tìm kiếm!");
            }
            finally
            {
                // ===== 8️⃣ KẾT THÚC RUN → MỞ BAR LẠI =====
                _isRunning = false;
                UpdateBarState();
                HideLoading("✔ Hoàn tất");// 🔓 mở bar nếu có data
            }
        }
        private void barButtonItemSeleProfile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Libary.Instance.LogForm(nameof(FManagerProfile),"▶ Click chọn profile");
            // 1️⃣ Mở form chọn profile
            using (var f = new CrawlFB_PW._1._0.Profile.SelectProfileDB())
            {
                var result = f.ShowDialog();

                // Nếu user chọn profile
                if (result == DialogResult.OK &&
                    f.Tag is List<ProfileDB> selected &&
                    selected.Count > 0)
                {
                    SelectedProfileId = selected[0].IDAdbrowser;

                    Libary.Instance.LogForm(
                        nameof(FManagerProfile),
                        $"✅ User chọn profile: {SelectedProfileId}"
                    );

                    MessageBox.Show($"Đã chọn profile: {SelectedProfileId}");
                    return;
                }
            }
            // 2️⃣ Nếu user KHÔNG chọn nhưng đã có profile trước đó → giữ nguyên
            if (!string.IsNullOrEmpty(SelectedProfileId))
            {
                Libary.Instance.LogForm(
                    nameof(FManagerProfile),
                    $"ℹ️ User không chọn, giữ profile cũ: {SelectedProfileId}"
                );

                MessageBox.Show($"Đang dùng profile mặc định: {SelectedProfileId}");
                return;
            }
            // 3️⃣ Nếu chưa chọn lần nào → tự động lấy profile đầu tiên trong DB
            var allProfiles = new ProfileInfoDAO().GetAllProfiles();
            if (allProfiles.Count > 0)
            {
                SelectedProfileId = allProfiles[0].IDAdbrowser;

                Libary.Instance.LogForm(
                    nameof(FManagerProfile),
                    $"⚙️ Auto chọn profile đầu DB: {SelectedProfileId}"
                );
                MessageBox.Show($"Tự động chọn profile đầu tiên: {SelectedProfileId}");
                return;
            }
            // 4️⃣ Không có profile → thông báo bắt buộc chọn
            Libary.Instance.LogForm(
                nameof(FManagerProfile),
                "❌ Không có profile nào trong DB"
            );
            MessageBox.Show("⚠ Không có profile nào trong database! Hãy thêm profile trước.");
        }
        private async void barButtonItemRunAndCheck_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // ===== 0️⃣ CHECK INPUT (KHÔNG KHÓA BAR) =====
            string keyword = textEditKeyword.Text.Trim();
            int maxPost = GetMaxPost();
            int minFlow = GetMinFlow();

            if (!CheckInputBeforeRun(keyword))
                return;

            // ===== 1️⃣ BẮT CHỌN PROFILE =====
            var frm = new CrawlFB_PW._1._0.Profile.SelectProfileDB();
            if (frm.ShowDialog() != DialogResult.OK || frm.Tag == null)
            {
                MessageBox.Show("Bạn phải chọn ít nhất 1 profile!");
                return;
            }

            var selectedProfiles = (frm.Tag as List<ProfileDB>)
                ?.Select(p => p.IDAdbrowser)
                .ToList();

            if (selectedProfiles == null || selectedProfiles.Count == 0)
            {
                MessageBox.Show("Bạn phải chọn ít nhất 1 profile!");
                return;
            }

            Libary.Instance.LogForm(
                nameof(FFindPage),
                $"▶ Run&Check: keyword='{keyword}', profiles={selectedProfiles.Count}, maxPost={maxPost}, minFlow={minFlow}"
            );

            // ===== 2️⃣ BẮT ĐẦU RUN → KHÓA BAR =====
            _isRunning = true;
            UpdateBarState();
            ShowLoading("⏳ Đang chạy & kiểm tra...");// 🔒 khóa bar

            try
            {
                // ===== 3️⃣ INIT PROFILE ĐẦU TIÊN (BẮT BUỘC) =====
                if (!await FindPageDAO.Instance.InitAsync(selectedProfiles[0]))
                {
                    MessageBox.Show("Không mở được profile!");
                    return;
                }

                // ===== 4️⃣ MỞ TRANG TÌM KIẾM =====
                if (!await OpenSearchPage(keyword))
                {
                    MessageBox.Show("Không mở được trang tìm kiếm!");
                    return;
                }

                // ===== 5️⃣ RUN FAST =====
                var list = await FindPageDAO.Instance
                    .RunFastSearchAsync(SearchType, maxPost, minFlow);

                Libary.Instance.LogForm(
                    nameof(FFindPage),
                    $"🔎 RunFast xong: {list.Count} page"
                );

                // ===== 6️⃣ CHIA PAGE THEO PROFILE =====
                var batches = SplitPagesByProfiles(list, selectedProfiles.Count);

                // ===== 7️⃣ CHẠY SONG SONG CHECK TIME =====
                List<Task> tasks = new List<Task>();

                for (int i = 0; i < selectedProfiles.Count; i++)
                {
                    string pid = selectedProfiles[i];
                    var batch = batches[i];

                    Libary.Instance.LogForm(
                        nameof(FFindPage),
                        $"[PROFILE] ▶ {pid} xử lý {batch.Count} page"
                    );

                    tasks.Add(Task.Run(async () =>
                    {
                        await RunCheckLastPost_ByProfile(pid, batch);
                    }));
                }

                await Task.WhenAll(tasks);

                var grid = list.Select((p, index) => new PageInfoGrid
                {
                    Select = false,
                    STT = index + 1,
                    PageName = p.PageName,
                    PageLink = p.PageLink,
                    PageMembers = p.PageMembers,
                    PageType = p.PageType,
                    LastPostTime = p.TimeLastPost == null? "N/A": p.TimeLastPost.Value.ToString("dd/MM/yyyy HH:mm"),
                    TimeLastPost = p.TimeLastPost

                }).ToList();
                _hasGridData = grid.Count > 0;

                MessageBox.Show("✔ Run & Check hoàn tất!");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogForm(
                    nameof(FFindPage),
                    "❌ RunAndCheck lỗi: " + ex.Message
                );

                MessageBox.Show("Có lỗi xảy ra khi Run & Check!");
            }
            finally
            {
                // ===== 9️⃣ KẾT THÚC RUN → MỞ BAR LẠI =====
                _isRunning = false;
                UpdateBarState();
                HideLoading("✔ Xong");// 🔓 mở bar nếu có data
            }
        }
        private int GetMaxPost()
        {
            int maxDefault = 50;
            string raw = textEditMaxPage.Text?.Trim();
            if (!int.TryParse(raw, out int max))
            {
                max = maxDefault;
                Libary.Instance.LogForm(nameof(FManagerProfile), $"⚙️ MaxPost không hợp lệ ('{raw}') → dùng mặc định {maxDefault}");
            }
            else
            {
                Libary.Instance.LogForm(nameof(FManagerProfile),$"👤 User chọn MaxPost = {max}"
                );
            }
            return max;
        }
        private int GetMinFlow()
        {
            decimal userVal = 0;

            try
            {
                userVal = Convert.ToDecimal(barEditItemMinFlow.EditValue ?? 0);
            }
            catch
            {
                Libary.Instance.LogForm(nameof(FManagerProfile),"⚠️ MinFlow đọc giá trị lỗi → dùng 0");
                return 0;
            }
            int flow = (int)(userVal * 1000);Libary.Instance.LogForm(nameof(FManagerProfile),
                $"👤 User chọn MinFlow = {userVal}K ({flow})"
            );
            return flow;
        }
        private List<List<PageInfo>> SplitPagesByProfiles(List<PageInfo> pages, int profileCount)
        {
            List<List<PageInfo>> result = new List<List<PageInfo>>();

            if (profileCount <= 0) profileCount = 1;

            int batchSize = (int)Math.Ceiling((double)pages.Count / profileCount);

            var batches = pages
                .Select((p, idx) => new { p, idx })
                .GroupBy(x => x.idx / batchSize)
                .Select(g => g.Select(x => x.p).ToList())
                .ToList();

            // Đảm bảo đủ batch = profileCount
            for (int i = 0; i < profileCount; i++)
            {
                if (i < batches.Count) result.Add(batches[i]);
                else result.Add(new List<PageInfo>()); // batch rỗng
            }
            return result;
        }
        private async Task RunCheckLastPost_ByProfile(string profileId, List<PageInfo> pages)
        {
            var adsMgr = AdsPowerPlaywrightManager.Instance;

            Libary.Instance.LogForm(nameof(FFindPage), $"[Profile] ▶ {profileId} bắt đầu xử lý {pages.Count} page.");

            IPage mainPage = await adsMgr.GetPageAsync(profileId);
            if (mainPage == null)
            {
                Libary.Instance.LogForm(nameof(FFindPage), $"[Profile] ❌ Không lấy được main page cho {profileId}");
                return;
            }

            foreach (var p in pages)
            {
                try
                {
                    var tab = await adsMgr.OpenNewTabAsync(profileId);
                    if (tab == null)
                    {
                        Libary.Instance.LogForm(nameof(FFindPage), $"❌ Không mở tab mới cho {profileId}");
                        continue;
                    }

                    Libary.Instance.LogForm(nameof(FFindPage), $"[CHECK] Mở: {p.PageLink}");

                    await tab.GotoAsync(p.PageLink, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = 25000
                    });

                    DateTime? lastTime = await ScanCheckPageDAO.Instance.GetPostTimeAsync(tab);
                    p.TimeLastPost = lastTime == DateTime.MinValue
                        ? (DateTime?)null
                        : lastTime;
                    Libary.Instance.LogForm(nameof(FFindPage), $"[CHECK] {p.PageName} → {p.TimeLastPost}");

                    await adsMgr.ClosePageAsync(tab);
                }
                catch (Exception ex)
                {
                    p.TimeLastPost = null;
                    Libary.Instance.LogForm(nameof(FFindPage), $"[CHECK ERROR] {p.PageLink} | {ex.Message}");
                }
            }

            Libary.Instance.LogForm(nameof(FFindPage), $"[Profile] ✔ Xong profile {profileId}");
        }
        private bool CheckInputBeforeRun(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Vui lòng nhập từ khóa!");
                return false;
            }

            if (SearchType == FBType.Unknown)
            {
                MessageBox.Show("Chưa chọn Fanpage / Group!");
                return false;
            }

            return true;
        }
        private async Task<bool> InitProfileIfNeeded()
        {
            if (string.IsNullOrWhiteSpace(SelectedProfileId))
            {
                var allProfiles = new ProfileInfoDAO().GetAllProfiles();
                if (allProfiles.Count == 0)
                {
                    MessageBox.Show("Không có profile trong DB!");
                    return false;
                }
                SelectedProfileId = allProfiles[0].IDAdbrowser;
            }

            bool ok = await FindPageDAO.Instance.InitAsync(SelectedProfileId);
            return ok;
        }
        private async Task<bool> OpenSearchPage(string keyword)
        {
            string typePath;

            switch (SearchType)
            {
                case FBType.Fanpage:
                    typePath = "pages";
                    break;

                case FBType.GroupOn:
                case FBType.GroupOff:
                    typePath = "groups";
                    break;

                default:
                    throw new InvalidOperationException("SearchType chưa hợp lệ");
            }

            return await FindPageDAO.Instance.OpenSearchPageAsync(keyword, typePath);
        }
        private void barButtonItemSaveDB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();
            var gridList = gridControl1.DataSource as List<PageInfoGrid>;
            if (gridList == null || gridList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để lưu!");
                return;
            }

            var selected = gridList.Where(x => x.Select == true).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Bạn chưa chọn dòng nào để lưu!");
                return;
            }

            int saveCount = 0;

            foreach (var item in selected)
            {
                // kiểm tra trùng link trước khi lưu
                if (SQLDAO.Instance.CheckPageExistsByLink(item.PageLink))
                {
                    Libary.Instance.LogForm(nameof(FFindPage), $"[DB] Bỏ qua (đã tồn tại): {item.PageLink}");
                    continue;
                }
      
                // tạo DTO PageInfo đầy đủ
                PageInfo dto = new PageInfo
                {
                    PageName = item.PageName,
                    PageLink = item.PageLink,
                    PageMembers = item.PageMembers,
                    PageType = item.PageType,
                    TimeLastPost = item.TimeLastPost, // ✅ DateTime? thật
                    PageTimeSave = DateTime.Now.ToString("dd:MM:yyyy HH:00")
                };

                // INSERT
                SQLDAO.Instance.InsertOrIgnorePageInfo(dto);

                saveCount++;
                Libary.Instance.LogForm(nameof(FFindPage), $"[DB] Đã lưu: {item.PageName}");
            }

            MessageBox.Show($"Đã lưu {saveCount} dòng vào Database!");
        }
        private async void barButtonItemCheckTimeLastPost_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // ===== 0️⃣ LẤY DATA TỪ GRID (KHÔNG KHÓA BAR) =====
            var gridList = gridControl1.DataSource as List<PageInfoGrid>;
            if (gridList == null || gridList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu trong Grid!");
                return;
            }

            var selectedPages = gridList.Where(x => x.Select).ToList();
            if (selectedPages.Count == 0)
            {
                MessageBox.Show("Bạn chưa chọn Page nào!");
                return;
            }

            Libary.Instance.LogForm(
                nameof(FFindPage),
                $"▶ CheckTimeLastPost: chọn {selectedPages.Count} page"
            );

            // ===== 1️⃣ BẮT CHỌN PROFILE =====
            var frm = new CrawlFB_PW._1._0.Profile.SelectProfileDB();
            if (frm.ShowDialog() != DialogResult.OK || frm.Tag == null)
            {
                MessageBox.Show("Bạn phải chọn profile!");
                return;
            }

            var selectedProfiles = (frm.Tag as List<ProfileDB>)
                ?.Select(p => p.IDAdbrowser)
                .ToList();

            if (selectedProfiles == null || selectedProfiles.Count == 0)
            {
                MessageBox.Show("Bạn phải chọn profile!");
                return;
            }

            Libary.Instance.LogForm(
                nameof(FFindPage),
                $"👤 CheckTime: profiles={selectedProfiles.Count}"
            );

            // ===== 2️⃣ CHIA CÔNG VIỆC =====
            int batchSize = (int)Math.Ceiling(
                (double)selectedPages.Count / selectedProfiles.Count
            );

            var batches = selectedPages
                .Select((page, index) => new { page, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.page).ToList())
                .ToList();

            // ===== 3️⃣ BẮT ĐẦU RUN → KHÓA BAR =====
            _isRunning = true;
            UpdateBarState(); // 🔒 khóa bar

            try
            {
                List<Task> tasks = new List<Task>();

                for (int i = 0; i < selectedProfiles.Count; i++)
                {
                    string pid = selectedProfiles[i];
                    var batch = (i < batches.Count)
                        ? batches[i]
                        : new List<PageInfoGrid>();

                    Libary.Instance.LogForm(
                        nameof(FFindPage),
                        $"[PROFILE] ▶ {pid} check {batch.Count} page"
                    );

                    tasks.Add(RunCheckLastPostByProfile(pid, batch));
                }

                await Task.WhenAll(tasks);

                // ===== 4️⃣ REFRESH GRID =====
                gridView1.RefreshData();
                _hasGridData = true;

                MessageBox.Show("✔ Đã kiểm tra xong Time Last Post!");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogForm(
                    nameof(FFindPage),
                    "❌ CheckTimeLastPost lỗi: " + ex.Message
                );

                MessageBox.Show("Có lỗi khi kiểm tra Time Last Post!");
            }
            finally
            {
                // ===== 5️⃣ KẾT THÚC RUN → MỞ BAR =====
                _isRunning = false;
                UpdateBarState(); // 🔓 mở lại bar
            }

        }
        private async Task RunCheckLastPostByProfile(string profileId, List<PageInfoGrid> pages)
        {
            var adsMgr = AdsPowerPlaywrightManager.Instance;

            Libary.Instance.LogForm(nameof(FFindPage), $"[PROFILE] ▶ Profile {profileId} đang xử lý {pages.Count} page...");
            // Lấy MAIN PAGE (y như FAddPage)
            IPage mainPage = await adsMgr.GetPageAsync(profileId);
            if (mainPage == null)
            {
                Libary.Instance.LogForm(nameof(FFindPage), $"[PROFILE] ❌ Không lấy được main page cho {profileId}");
                return;
            }

            foreach (var item in pages)
            {
                try
                {
                    // ⭐ MỞ TAB MỚI GIỐNG FAddPagecs
                    var tab = await adsMgr.OpenNewTabAsync(profileId);
                    if (tab == null)
                    {
                        Libary.Instance.LogForm(nameof(FFindPage), $"[PROFILE] ❌ Không mở được tab mới cho {profileId}");
                        continue;
                    }

                    // ⭐ BẮT BUỘC GỌI GOTO — KHÔNG GOTO SẼ BỊ about:blank
                    Libary.Instance.LogForm(nameof(FFindPage), $"[CHECK] Đang mở: {item.PageLink}");

                    await tab.GotoAsync(item.PageLink, new PageGotoOptions
                    {
                        Timeout = 25000,
                        WaitUntil = WaitUntilState.NetworkIdle
                    });

                    // ⭐ LẤY TIME LAST POST
                   DateTime? lastTime = await ScanCheckPageDAO.Instance.GetPostTimeAsync(tab);
                    item.TimeLastPost = lastTime == DateTime.MinValue? (DateTime?)null : lastTime;
                    item.LastPostTime = item.TimeLastPost == null
                        ? "N/A"
                        : item.TimeLastPost.Value.ToString("dd/MM/yyyy HH:mm");


                    Libary.Instance.LogForm(nameof(FFindPage), $"[CHECK DONE] {item.PageName} = {item.LastPostTime}");

                    // ⭐ ĐÓNG TAB (giống FAddPagecs)
                    await adsMgr.ClosePageAsync(tab);
                }
                catch (Exception ex)
                {
                    item.LastPostTime = "N/A";
                    Libary.Instance.LogForm(nameof(FFindPage), $"[CHECK ERROR] {item.PageLink}: {ex.Message}");
                }
            }

            Libary.Instance.LogForm(nameof(FFindPage), $"[PROFILE] ✔ Xong profile {profileId}");
        }

        private void barButtonItemSelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var gridList = gridControl1.DataSource as List<PageInfoGrid>;
            if (gridList == null || gridList.Count == 0) return;

            foreach (var item in gridList)
                item.Select = true;

            gridView1.RefreshData();
        }

        private void barButtonItemReset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var gridList = gridControl1.DataSource as List<PageInfoGrid>;
            if (gridList == null || gridList.Count == 0) return;

            foreach (var item in gridList)
                item.Select = false;

            gridView1.RefreshData();
        }
    }
}
