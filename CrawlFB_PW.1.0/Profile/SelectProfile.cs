using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Repository;

namespace CrawlFB_PW._1._0.Profile
{
    public partial class SelectProfile : Form
    {
        private List<ProfileInfo> profiles = new List<ProfileInfo>();
        private readonly string profileFile = PathHelper.Instance.GetProfilesFilePath();

        private ProgressBarControl progressBar;
        private Label lblProgress;
        private SimpleButton btnSelect;
        private SimpleButton btnRefresh;
        private Label lblSlotNeeded;

        // 🔹 slotNeeded được truyền từ FScanPostPage (bằng số URL)
        private int _slotNeeded = 0;

        // 🔹 property public để trả kết quả về FScanPostPage
        public List<ProfileInfo> SelectedProfiles { get; private set; } = new List<ProfileInfo>();


        public SelectProfile()
        {
            InitializeComponent();
            BuildHeaderUI();
            InitGrid();
            this.Load += SelectProfile_Load;
        }

        public SelectProfile(int slotNeeded) : this()
        {
            _slotNeeded = slotNeeded;
        }

        private async void SelectProfile_Load(object sender, EventArgs e)
        {
            lblSlotNeeded.Text = _slotNeeded > 0
                ? $"🔸 Số slot cần: {_slotNeeded} (mỗi URL = 1 slot)"
                : "🔸 Chưa có URL được nạp.";
            await LoadProfilesAsync();
        }

        private void BuildHeaderUI()
        {
            progressBar = new ProgressBarControl
            {
                Location = new Point(20, 25),
                Size = new Size(300, 25),
                Properties = { Minimum = 0, Maximum = 100, ShowTitle = true, PercentView = true }
            };
            panelControlSetup.Controls.Add(progressBar);

            lblProgress = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                Location = new Point(340, 30),
                ForeColor = Color.Gray,
                Text = "Đang tải..."
            };
            panelControlSetup.Controls.Add(lblProgress);

            lblSlotNeeded = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(lblProgress.Left, lblProgress.Bottom + 6),
                ForeColor = Color.DarkBlue,
                Text = ""
            };
            panelControlSetup.Controls.Add(lblSlotNeeded);

            btnSelect = new SimpleButton
            {
                Text = "Chọn Profile",
                Location = new Point(750, 20),
                Width = 120
            };
            btnSelect.Click += BtnSelect_Click;
            panelControlSetup.Controls.Add(btnSelect);

            btnRefresh = new SimpleButton
            {
                Text = "Làm mới",
                Location = new Point(880, 20),
                Width = 100
            };
            btnRefresh.Click += async (s, e) => await LoadProfilesAsync();
            panelControlSetup.Controls.Add(btnRefresh);
        }

        private void InitGrid()
        {
            gridView1.OptionsBehavior.Editable = true;
            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsView.ShowIndicator = false;
            gridView1.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gridView1.Appearance.Row.Font = new Font("Segoe UI", 9);

            // cho double-click chọn nhanh
            gridView1.DoubleClick += GridView1_DoubleClick;
        }

        private async Task LoadProfilesAsync()
        {
            try
            {
                btnSelect.Enabled = false;
                btnRefresh.Enabled = false;
                progressBar.Position = 0;
                lblProgress.Text = "Đang tải danh sách profile...";

                if (!File.Exists(profileFile))
                {
                    lblProgress.Text = "⚠️ Không tìm thấy profiles.json!";
                    return;
                }

                string json = File.ReadAllText(profileFile);
                profiles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProfileInfo>>(json) ?? new List<ProfileInfo>();

                var adsMgr = AdsPowerPlaywrightManager.Instance;

                int total = profiles.Count;
                int count = 0;
                var data = new List<ProfileDisplay>();

                foreach (var p in profiles)
                {
                    count++;
                    double percent = (double)count / total * 100;
                    progressBar.Position = (int)percent;
                    lblProgress.Text = $"Đang tải {count}/{total} profiles ({(int)percent}%)...";
                    Application.DoEvents();

                    if (p.CurrentTabs < 1)
                        p.CurrentTabs = 1;

                    int usedSlots = Math.Max(0, p.CurrentTabs - 1);
                    int maxTabs = Math.Max(1, p.MaxTabs);
                    int remaining = Math.Max(0, maxTabs - usedSlots);

                    string tab1 = usedSlots < 1 ? "✅" : "❌";
                    string tab2 = usedSlots < 2 ? "✅" : "❌";
                    string tab3 = usedSlots < 3 ? "✅" : "❌";

                    bool isRunning = false;
                    try
                    {
                        var page = await adsMgr.GetPageAsync(p.ProfileId);
                        if (page != null)
                            isRunning = true;
                    }
                    catch { isRunning = false; }

                    data.Add(new ProfileDisplay
                    {
                        STT = count,
                        Name = p.Name ?? p.FacebookName,
                        MaxTab = maxTabs,
                        UsedTab = usedSlots,
                        Tab1 = tab1,
                        Tab2 = tab2,
                        Tab3 = tab3,
                        SlotCon = remaining,
                        TrangThai = isRunning ? "🟢 Đang chạy" : "⚪ Rảnh",
                        Chon = false
                    });

                    await Task.Delay(50);
                }

                // ✅ BIND dữ liệu thật (model có property)
                gridControl1.DataSource = data;
                gridView1.PopulateColumns();

                // chỉnh lại caption
                gridView1.Columns["UsedTab"].Caption = "Đang dùng";
                gridView1.Columns["TrangThai"].Caption = "Trạng thái";
                gridView1.Columns["SlotCon"].Caption = "Slot còn";
                gridView1.Columns["Chon"].Caption = "Chọn";

                // ✅ Bật checkbox tick
                var colChon = gridView1.Columns["Chon"];
                if (colChon != null)
                {
                    var checkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                    checkEdit.ValueChecked = true;
                    checkEdit.ValueUnchecked = false;
                    gridControl1.RepositoryItems.Add(checkEdit);

                    colChon.ColumnEdit = checkEdit;
                    colChon.OptionsColumn.AllowEdit = true;
                }

                // ✅ Cho phép tick trực tiếp bằng click
                gridView1.OptionsBehavior.Editable = true;

                progressBar.Position = 100;
                lblProgress.Text = $"✅ Đã tải {total} profile.";
            }
            catch (Exception ex)
            {
                lblProgress.Text = "❌ Lỗi khi tải: " + ex.Message;
            }
            finally
            {
                btnSelect.Enabled = true;
                btnRefresh.Enabled = true;
            }
        }

        // double click chọn nhanh
        private void GridView1_DoubleClick(object sender, EventArgs e)
        {
            var view = sender as GridView;
            if (view == null || view.FocusedRowHandle < 0) return;

            bool isChecked = (bool)(view.GetRowCellValue(view.FocusedRowHandle, "Chon") ?? false);
            view.SetRowCellValue(view.FocusedRowHandle, "Chon", !isChecked);
        }

        // khi bấm "Chọn Profile"
        // 🟢 Khi nhấn nút chọn profile
        private void BtnSelect_Click(object sender, EventArgs e)
        {
            // Lấy dữ liệu hiển thị (ProfileDisplay)
            var data = gridControl1.DataSource as List<ProfileDisplay>;
            if (data == null)
            {
                XtraMessageBox.Show("⚠️ Không có dữ liệu hiển thị!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lấy danh sách ProfileDisplay đã tick chọn
            var selectedDisplays = data.Where(x => x.Chon).ToList();
            if (selectedDisplays.Count == 0)
            {
                XtraMessageBox.Show("⚠️ Bạn chưa chọn profile nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Map lại sang danh sách ProfileInfo thật
            var selectedProfiles = new List<ProfileInfo>();
            foreach (var disp in selectedDisplays)
            {
                var profile = profiles.FirstOrDefault(p => (p.Name ?? p.FacebookName) == disp.Name);
                if (profile != null)
                    selectedProfiles.Add(profile);
            }

            if (selectedProfiles.Count == 0)
            {
                XtraMessageBox.Show("⚠️ Không tìm thấy thông tin Profile tương ứng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ✅ Trả về cho form gọi (FScanPostPage)
            this.Tag = selectedProfiles;   // 👈 Cách an toàn, để FScanPostPage đọc lại
            this.DialogResult = DialogResult.OK;

            string msg = string.Join(Environment.NewLine, selectedProfiles.Select(p => p.Name ?? p.FacebookName));
            XtraMessageBox.Show($"✅ Đã chọn {selectedProfiles.Count} profile:\n\n{msg}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }


        // ✅ Cho phép sửa cột "Chon"
        private void GridView1_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var view = sender as GridView;
            if (view.FocusedColumn.FieldName != "Chon")
                e.Cancel = true; // chỉ cho edit cột "Chon"
        }

        // ✅ Xử lý tick/untick trực tiếp
        private void GridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var view = sender as GridView;
            if (e.Column.FieldName == "Chon")
            {
                bool current = (bool)(view.GetRowCellValue(e.RowHandle, e.Column) ?? false);
                view.SetRowCellValue(e.RowHandle, e.Column, !current);
            }
        }
        public class ProfileDisplay
        {
            public int STT { get; set; }
            public string Name { get; set; }
            public int MaxTab { get; set; }
            public int UsedTab { get; set; }
            public string Tab1 { get; set; }
            public string Tab2 { get; set; }
            public string Tab3 { get; set; }
            public int SlotCon { get; set; }
            public string TrangThai { get; set; }
            public bool Chon { get; set; }   // ✅ cho phép tick chọn
        }

    }
}
