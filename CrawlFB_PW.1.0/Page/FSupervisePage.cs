using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using DevExpress.ClipboardSource.SpreadsheetML;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Text;

namespace CrawlFB_PW._1._0.Page
{
    public partial class FSupervisePage : Form
    {
        
        public FSupervisePage()
        {
            InitializeComponent();
            this.Load += FAutoScanPage_Load;
            this.FormClosing += FAutoScanPage_FormClosing;
        }
        private List<ProfileInfo> _profiles;
        private ProfileInfo _selectedProfile;
        private List<ProfileInfo> allProfiles = new List<ProfileInfo>();
       
        private void LoadProfiles()
        {
            try
            {
                string profileFile = PathHelper.Instance.GetProfilesFilePath();
                if (!System.IO.File.Exists(profileFile))
                {
                    MessageBox.Show("Chưa có file profiles.json");
                    return;
                }

                string json = System.IO.File.ReadAllText(profileFile);
                _profiles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProfileInfo>>(json) ?? new List<ProfileInfo>();
                var available = _profiles.Where(p => p.IsActive == 1 && p.CurrentTabs < p.MaxTabs).ToList();
                cbSelectProfile.DataSource = available;
                cbSelectProfile.DisplayMember = "FacebookName";
                cbSelectProfile.ValueMember = "ProfileId";
                if (available.Count > 0)
                {
                    cbSelectProfile.SelectedIndex = 0;
                    _selectedProfile = available[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load profiles: " + ex.Message);
            }
        }
        private async Task<List<PostPage>> SuperviseOnePageAsync(string url, int days, int maxPosts, ProfileInfo profile)
        {
            var listPosts = new List<PostPage>();
            var uniqueLinks = new HashSet<string>();
            try
            {
                Libary.Instance.CreateLog($"[SuperviseOnePageAsync] 🚀 Bắt đầu giám sát profile {profile.ProfileId}");
                labelStatus.Text = "⏳ Đang mở profile và tải trang...";
                // 1️⃣ Mở profile qua AdsPower
                var page = await AdsPowerPlaywrightManager.Instance.GetPageAsync(profile.ProfileId);
                if (page == null)
                {
                    Libary.Instance.CreateLog("[SuperviseOnePageAsync] ❌ Không mở được profile");
                    labelStatus.Text = "❌ Không mở được profile AdsPower.";
                    return listPosts;
                }
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
                labelStatus.Text = "📜 Đang load bài viết...";
                string PageName =await PageDAO.Instance.GetPageNameAsync(page);
                // 3️⃣ Cuộn để load thêm bài
                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 15);

                // 4️⃣ Lấy vùng feed ổn định
                var feed = await PageDAO.Instance.GetFeedContainerAsync(page);
                if (feed == null)
                {
                    MessageBox.Show("❌ Không thể lấy vùng feed chính của trang.\n\nVui lòng kiểm tra log để xem chi tiết lỗi.", "Lỗi lấy feed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                if (feed == null)
                {
                    Libary.Instance.CreateLog("⚠️ Không tìm thấy feed chính, thử lấy layout khác.");
                    feed = await page.QuerySelectorAsync("div[class*='x1yztbdb'][role='main']");
                }
                if (feed == null)
                {
                    labelStatus.Text = "❌ Không tìm thấy feed.";
                    return listPosts;
                }

                // 5️⃣ Lấy các node bài viết trong feed
                var nodes = await feed.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");
                Libary.Instance.CreateLog($"[SuperviseOnePageAsync] 📄 Phát hiện {nodes.Count} node bài trong feed.");

                //int count = 0;
                var watch = System.Diagnostics.Stopwatch.StartNew();
               // int index = 0;
                // 6️⃣ Duyệt từng node và gọi GetPostGroupsAsync()
                foreach (var node in nodes)
                {
                  
                }

                watch.Stop();
                labelStatus.Text = $"✅ Hoàn tất ({listPosts.Count}/{maxPosts} bài) – {watch.Elapsed.TotalSeconds:0.0}s";

                Libary.Instance.CreateLog($"[SuperviseOnePageAsync] ✅ Đã lấy {listPosts.Count}/{maxPosts} bài trong {watch.Elapsed.TotalSeconds:0.0}s.");

                // 7️⃣ Hiển thị dữ liệu lên grid
                gridControl1.DataSource = null;
                gridControl1.DataSource = listPosts;
                try { gridView1.RefreshData(); } catch { }
                ConfigureGridView();
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[SuperviseOnePageAsync] ❌ Exception: " + ex.Message);
                labelStatus.Text = "❌ Lỗi khi lấy bài: " + ex.Message;
            }

            return listPosts;
        }
      
        // hàm bổ trợ cho lấy đủ bài viết trong 1 lần cuộn, click xem thêm
        
        private void CbTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = CbTime.SelectedItem?.ToString() ?? "1 tuần";
            int days = ConvertToDays(selected);
        }
        // Convert combobox selection to days (simple implementation)
        private int ConvertToDays(string sel)
        {
            if (string.IsNullOrEmpty(sel)) return 7;
            sel = sel.ToLower();
            if (sel.Contains("hôm nay") || sel.Contains("1 ngày")) return 1;
            if (sel.Contains("1 tuần") || sel.Contains("7 ngày")) return 7;
            if (sel.Contains("1 tháng") || sel.Contains("30 ngày")) return 30;
            int v;
            if (int.TryParse(new string(sel.Where(char.IsDigit).ToArray()), out v)) return v;
            return 7;
        }

        private void cbSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedProfile = cbSelectProfile.SelectedItem as ProfileInfo;
        }
        private void FAutoScanPage_Load(object sender, EventArgs e)
        {
            LoadProfiles();
        }
        private void FAutoScanPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // cleanup sessions (fire and forget)
            AdsPowerPlaywrightManager.Instance.CleanupAsync().GetAwaiter().GetResult();
        }
        private void ConfigureGridView()
        {
            gridView1.Columns.Clear();
            gridView1.PopulateColumns(); // Tự tạo tất cả property của DTO
            var captions = new Dictionary<string, string>
              {
                { "PosterName", "Người đăng" },
                { "PosterLink", "Link người đăng" },
                { "PostTime", "Thời gian đăng" },
                { "PostLink", "Link bài viết" },
                { "Content", "Nội dung bài viết" },
                { "CommentCount", "Bình luận" },
                { "ShareCount", "Chia sẻ" },
                {"LikeCount","lượt like" },
                { "PostStatus", "Trạng thái" },
                { "OriginalPosterName", "Người đăng gốc" },
                { "OriginalPostLink", "Link bài gốc" },
                { "OriginalContent", "Nội dung gốc" }
              };
            foreach (var col in gridView1.Columns)
            {
                var column = (DevExpress.XtraGrid.Columns.GridColumn)col;
                if (captions.ContainsKey(column.FieldName))
                    column.Caption = captions[column.FieldName];
            }
            // Ẩn cột Topic nếu có
            if (gridView1.Columns["Topic"] != null)
                gridView1.Columns["Topic"].Visible = false;
            if (gridView1.Columns["PageLink"] != null)
                gridView1.Columns["PageLink"].Visible = false;
            // Header: vừa phải, hiển thị đủ chữ
            gridView1.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            gridView1.Appearance.HeaderPanel.ForeColor = Color.Black;
            gridView1.Appearance.HeaderPanel.Options.UseFont = true;
            gridView1.Appearance.HeaderPanel.Options.UseForeColor = true;
            gridView1.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            gridView1.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            gridView1.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;

            // Nội dung: wrap + tự giãn dòng
            gridView1.OptionsView.RowAutoHeight = true;
            gridView1.Appearance.Row.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            gridView1.OptionsBehavior.Editable = false;

            // Ngắt auto width để mình tự điều chỉnh

            gridView1.OptionsView.ColumnAutoWidth = false;
            // Độ rộng các cột
            // Cấu hình độ rộng chi tiết
            var widths = new Dictionary<string, int>
        {
            { "PosterName", 100 },
            { "PosterLink", 100 },
            { "PostTime", 130 },
            { "PostLink", 200 },
            { "Content", 150 },
            { "CommentCount", 40 },
            { "ShareCount", 40 },
            { "LikeCount", 40 },
            { "PostStatus", 70 },
            { "OriginalPosterName", 150 },
            { "OriginalPostLink", 150 },
            { "OriginalContent", 150 }
        };
            foreach (var col in gridView1.Columns)
            {
                var column = (DevExpress.XtraGrid.Columns.GridColumn)col;
                if (widths.TryGetValue(column.FieldName, out int w))
                    column.Width = w;
                else
                    column.Width = 50; // mặc định
            }
            gridView1.BestFitColumns();
            foreach (var col in gridView1.Columns)
            {
                var column = (DevExpress.XtraGrid.Columns.GridColumn)col;
                if (column.Width > 150)
                    column.Width = 150;
            }
            gridView1.OptionsView.RowAutoHeight = true;
            gridView1.Appearance.Row.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            // STT tự động
            gridView1.CustomDrawRowIndicator += (s, e) =>
            {
                if (e.Info.IsRowIndicator && e.RowHandle >= 0)
                    e.Info.DisplayText = (e.RowHandle + 1).ToString();
            };
            gridView1.IndicatorWidth = 40;

            // Sự kiện double-click xem chi tiết
            gridView1.DoubleClick -= GridView1_DoubleClick;
            gridView1.DoubleClick += GridView1_DoubleClick;
        }
        private void GridView1_DoubleClick(object sender, EventArgs e)
        {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            int rowHandle = view.FocusedRowHandle;
            if (rowHandle < 0) return;

            var dataRow = view.GetRow(rowHandle);
            if (dataRow == null) return;

            // Lấy nội dung Content
            string contentText = "";
            var propContent = dataRow.GetType().GetProperty("Content");
            if (propContent != null)
                contentText = Convert.ToString(propContent.GetValue(dataRow)) ?? "";

            // 🔹 Form chi tiết
            Form frm = new Form
            {
                Text = "📋 Chi tiết bài viết",
                Size = new Size(950, 650),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 1,
                Padding = new Padding(20)
            };

            // 🔹 Phần hiển thị Content chiếm nhiều diện tích hơn
            var grpContent = new GroupBox
            {
                Text = "📝 Nội dung bài viết",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10),
                AutoSize = true
            };

            var txtContent = new TextBox
            {
                Text = contentText,
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10),
                Height = 300, // to hơn các field khác
                BackColor = Color.White
            };

            grpContent.Controls.Add(txtContent);
            mainPanel.Controls.Add(grpContent);

            // 🔹 Các trường còn lại
            var detailTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoScroll = true,
                ColumnCount = 2,
                Padding = new Padding(10, 10, 10, 10)
            };
            detailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            detailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            foreach (var prop in dataRow.GetType().GetProperties())
            {
                if (prop.Name.Equals("Topic", StringComparison.OrdinalIgnoreCase)) continue;
                if (prop.Name.Equals("Content", StringComparison.OrdinalIgnoreCase)) continue;

                string fieldName = prop.Name;
                string value = Convert.ToString(prop.GetValue(dataRow)) ?? "";

                var lbl = new Label
                {
                    Text = fieldName,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    AutoSize = true,
                    Padding = new Padding(0, 5, 0, 5)
                };

                var txt = new TextBox
                {
                    Text = value,
                    Multiline = true,
                    ReadOnly = true,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Segoe UI", 9),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };

                detailTable.Controls.Add(lbl);
                detailTable.Controls.Add(txt);
            }

            mainPanel.Controls.Add(detailTable);
            frm.Controls.Add(mainPanel);
            frm.ShowDialog();
        }        
        private async void btnShearch_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (_selectedProfile == null)
                {
                    MessageBox.Show("Vui lòng chọn profile");
                    return;
                }

                string url = txbLinkPage.Text.Trim();
                if (string.IsNullOrEmpty(url))
                {
                    MessageBox.Show("Vui lòng nhập link Page/Group");
                    return;
                }

                btnShearch.Enabled = false;
                labelStatus.Text = "⏳ Đang giám sát...";
                Libary.Instance.CreateLog($"[BtnShearch_Click] Start profile {_selectedProfile.ProfileId} url={url}");

                int days = ConvertToDays(CbTime.SelectedItem != null ? CbTime.SelectedItem.ToString() : "1 tuần");
                int maxPosts = AppConfig.MAX_POSTS_DEFAULT;

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var posts = await SuperviseOnePageAsync(url, days, maxPosts, _selectedProfile);
                watch.Stop();

                if (posts != null && posts.Count > 0)
                {
                    // Bind list<PostPage> thẳng lên GridControl
                    gridControl1.DataSource = null;
                    gridControl1.DataSource = posts;
                    try { gridView1.RefreshData(); } catch { }
                    ConfigureGridView();
                    labelStatus.Text = $"✅ Hoàn tất - lấy được {posts.Count} bài. ({watch.Elapsed.TotalSeconds:0.0}s)";
                    Libary.Instance.CreateLog($"[BtnShearch_Click] Đã lấy {posts.Count} bài. ({watch.Elapsed.TotalSeconds:0.0}s)");
                }
                else
                {
                    labelStatus.Text = $"⚠️ Không có bài viết. ({watch.Elapsed.TotalSeconds:0.0}s)";
                    Libary.Instance.CreateLog("[BtnShearch_Click] Không có bài.");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[BtnShearch_Click] Exception: " + ex.Message);
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            finally
            {
                btnShearch.Enabled = true;
            }
        }
    }
}

