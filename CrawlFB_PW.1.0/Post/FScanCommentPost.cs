using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DAO.Post;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Person;
using CrawlFB_PW._1._0.Profile;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace CrawlFB_PW._1._0.Post
{
    public partial class FScanCommentPost : Form
    {
        const string module = nameof(FScanCommentPost);
        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>();
        private List<string> _ListPostLink = new List<string>();
        public FScanCommentPost()
        {
            InitializeComponent();
        }
        public class CommentGridRow
        {
            public bool Select { get; set; }
            public string STT { get; set; }             
            public string CommentId { get; set; }         
          
            public string PosterName { get; set; }
            public string PosterLink { get; set; }
            public string Time { get; set; }
            public string Content { get; set; }
            public string Status { get; set; }
        }
     
        void InitGrid()
        {
            var gv = gridView1;

            // ===== CHUNG =====
            gv.OptionsView.RowAutoHeight = true;
            gv.OptionsView.ColumnAutoWidth = false;
            gv.OptionsBehavior.Editable = true;

            gv.OptionsSelection.MultiSelect = true;
            gv.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect;
           
            gv.OptionsView.EnableAppearanceEvenRow = false;
            gv.OptionsView.EnableAppearanceOddRow = false;

            gv.OptionsView.ShowIndicator = true;

            gv.Appearance.Row.Font = new Font("Segoe UI", 9f);
            gv.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            gv.Columns.Clear();

            // ===== CỘT =====
            gv.Columns.AddVisible("STT", "STT");
            gv.Columns.AddVisible("Select", "Chọnn");
            gv.Columns.AddVisible("PosterName", "Người đăng");
            gv.Columns.AddVisible("PosterLink", "Link");
            gv.Columns.AddVisible("Time", "Thời gian");
            gv.Columns.AddVisible("Content", "Nội dung");
            gv.Columns.AddVisible("Status", "Ghi Chú");

            gv.Columns["STT"].Width = 50;
            gv.Columns["Select"].Width = 40;
            gv.Columns["PosterName"].Width = 160;
            gv.Columns["PosterLink"].Width = 80;
            gv.Columns["Time"].Width = 90;
            gv.Columns["Content"].Width = 520;
            gv.Columns["Status"].Width = 100;

            gv.Columns["STT"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gv.Columns["Select"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            // ===== CHECKBOX =====
            var chk = new RepositoryItemCheckEdit();
            gv.Columns["Select"].ColumnEdit = chk;
            gv.Columns["Select"].OptionsColumn.AllowEdit = true;

            // ===== LINK =====
            var linkEdit = new RepositoryItemHyperLinkEdit
            {
                Caption = "Xem link",
                SingleClick = true
            };
            linkEdit.OpenLink += (s, e) =>
            {
                if (e.EditValue == null) return;
                try { System.Diagnostics.Process.Start(e.EditValue.ToString()); } catch { }
            };
            gv.Columns["PosterLink"].ColumnEdit = linkEdit;

            gridControl1.RepositoryItems.AddRange(
                new RepositoryItem[] { chk, linkEdit }
            );
           
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
        private void LoadPostLinkFromText()
        {
            string rawUrl = txd_PostLink.EditValue?.ToString()?.Trim();

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
            _ListPostLink.Add(url);
  

            Libary.Instance.LogForm(
                module,
                $"📌 Nhận URL từ textbox: {rawUrl}"
            );
        }
        private async void btn_RunScan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!SelectProfilesForScan(out _selectedProfiles))
            {
                MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txd_PostLink.EditValue?.ToString()))
            {
                LoadPostLinkFromText();
            }

            if (_ListPostLink == null || _ListPostLink.Count == 0)
            {
                MessageBox.Show("⚠ Bạn chưa nhập URL hợp lệ!");
                return;
            }

          
            var profile = _selectedProfiles.First();

            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);
            string reelLink = _ListPostLink[0];

            if (!reelLink.Contains("/reel/"))
            {
                MessageBox.Show("⚠ URL không phải Reel!");
                return;
            }

            var reelPage =
                await AdsPowerPlaywrightManager.Instance.OpenNewTabAsync(
                    profile.IDAdbrowser,
                    applyViewport: false
                );

            var comments =await PostReelCommentDAO.Instance.ScanReelCommentsAsync( reelPage, reelLink );

            if (comments.Count == 0)
            {
                Libary.Instance.LogForm(module, "❌ Không lấy được comment Reel");
                return;
            }
            var gridRows = comments
             .Select((c, index) => new CommentGridRow
            {
                Select = false,
                STT = (index + 1).ToString(),
                CommentId = c.CommentId,
                PosterName = c.PosterName,
                PosterLink = c.PosterLink,
                Time = c.RealCommentTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A",
                Content = c.Content,
                Status = c.Status
            })
            .ToList();

            gridControl1.DataSource = gridRows;
            InitGrid();
            //log
            foreach (var cmt in comments)
            {              
                    Libary.Instance.LogForm(
                        module,
                        $"🧱 COMMENT CHA\n" +
                        $"  🆔 {cmt.CommentId}\n" +
                        $"  👤 {cmt.PosterName}\n" +
                        $"  Link {cmt.PosterLink}\n" +
                        $"  🔗 {cmt.PosterLink}\n" +
                        $"  ⏰ {cmt.RealCommentTime}\n" +
                        $"  status {cmt.Status}\n" +
                        $"  📝 {ProcessingDAO.Instance.PreviewText(cmt.Content)}"
                    );               
            }
            Libary.Instance.LogForm(
                module,
                $"✅ Tổng comment lấy được: {comments.Count}"
            );

        }

        private void btn_ExportFull_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // 1️⃣ Lấy dữ liệu từ Grid
                var gridRows = gridControl1.DataSource as List<CommentGridRow>;

                if (gridRows == null || gridRows.Count == 0)
                {
                    MessageBox.Show(
                        "⚠ Không có dữ liệu bình luận để xuất!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // 2️⃣ Chọn nơi lưu file
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                    sfd.FileName = $"BinhLuan_Reel_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;

                    // 3️⃣ Lấy link bài viết (đang quét)
                    string postLink = _ListPostLink != null && _ListPostLink.Count > 0
                        ? _ListPostLink[0]
                        : "N/A";

                    // 4️⃣ Xuất Excel
                    ExcellHeper.ExportCommentsToExcel(
                        gridRows,
                        postLink,
                        sfd.FileName
                    );

                    MessageBox.Show(
                        "✅ Xuất Excel thành công!",
                        "Hoàn tất",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    Libary.Instance.LogForm(
                        module,
                        $"📤 Xuất Excel: {sfd.FileName} ({gridRows.Count} comment)"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ Lỗi khi xuất Excel:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                Libary.Instance.LogForm(
                    module,
                    "❌ Export Excel lỗi: " + ex.Message
                );
            }
        }
    }
 }

