using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO.Post;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Page;
using CrawlFB_PW._1._0.Profile;
using CrawlFB_PW._1._0.ViewModels;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Playwright;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using System.IO;
using CrawlFB_PW._1._0.DAO;
namespace CrawlFB_PW._1._0.Share
{
    public partial class FCrawlPostShare : Form
    {
        const string module = nameof(FCrawlPostShare);
        private ProfileDB _profile;
        public FCrawlPostShare()
        {
            InitializeComponent();
        }
        private void InitShareGrid()
        {
            var gv = gridView1;

            if (gv.Tag as string == "INIT_DONE")
                return;

            UIGridHelper.EnableRowIndicatorSTT(gv);
            UIGridHelper.ApplyVietnameseCaption(gv);

            UIGridHelper.ShowOnlyColumns(
                gv,
                nameof(SharePostVM.Select),
                nameof(SharePostVM.SharerName),
                nameof(SharePostVM.SharerLinkView),
                nameof(SharePostVM.TargetName),
                nameof(SharePostVM.TargetLinkView),
                nameof(SharePostVM.TimeShare),
                nameof(SharePostVM.PostLinkShareView),
                nameof(SharePostVM.TotalComment),
                nameof(SharePostVM.ViewComments)
            );      
            // ❗ Ẩn link thật
            UIGridHelper.HideIfExists(gv, nameof(SharePostVM.SharerLink));
            UIGridHelper.HideIfExists(gv, nameof(SharePostVM.TargetLink));
            UIGridHelper.HideIfExists(gv, nameof(SharePostVM.PostLinkShare));

            // 🔥 ÁP HYPERLINK CHO RIÊNG SHARE POST
            ApplyShareHyperlink(gv);
            UIGridHelper.LockAllColumnsExceptLinks(gv);

            gv.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            gv.Tag = "INIT_DONE";
        }

        private async Task AskAndPrepareShareAsync(IPage page)
        {
            var ask = MessageBox.Show(
                "Trang đã sẵn sàng.\nHãy bật popup chia sẻ bài viết rồi nhấn OK.",
                "Bật chia sẻ",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            );

            if (ask != DialogResult.OK)
            {
                Close();
                return;
            }

            MessageBox.Show(
                "⏳ Đang chờ popup chia sẻ xuất hiện...",
                "Chờ người dùng thao tác",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // 👉 CHỜ popup thật sự xuất hiện
            bool popupDetected = await WaitForSharePopupAsync(page);

            if (!popupDetected)
            {
                MessageBox.Show(
                    "❌ Không phát hiện popup chia sẻ!",
                    "Thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                Close();
                return;
            }

            MessageBox.Show(
                "✅ Đã thấy popup chia sẻ.\nBắt đầu quét!",
                "Sẵn sàng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            var feed = await SharePostDAO.Instance.GetShareFeedAsync(page);

            if (feed == null)
            {
                MessageBox.Show("❌ Không lấy được feed chia sẻ!");
                return;
            }
            var shareList = await SharePostDAO.Instance.CrawlShareAsync(page, feed);
            gridControl1.DataSource = shareList;   // 1️⃣ GÁN DATA
            gridView1.PopulateColumns();           // 2️⃣ SINH CỘT
            InitShareGrid();                        // 3️⃣ APPLY HELPER

        }
        private async Task<bool> WaitForSharePopupAsync( IPage page,int timeoutMs = 15000,int pollMs = 500)
        {
            int waited = 0;

            while (waited < timeoutMs)
            {
                if (await SharePostDAO.Instance.IsShareEnabledAsync(page))
                {
                    return true; // 🎯 đã thấy popup / span
                }

                await Task.Delay(pollMs);
                waited += pollMs;
            }

            return false;
        }

        private bool SelectProfileForScan(out ProfileDB profile)
        {
            profile = null;

            try
            {
                using (var frm = new SelectProfileDB())
                {
                    if (frm.ShowDialog() != DialogResult.OK)
                        return false;

                    var selected = frm.Tag as List<ProfileDB>;
                    if (selected == null || selected.Count != 1)
                    {
                        MessageBox.Show("⚠ Vui lòng chọn đúng 1 profile!");
                        return false;
                    }

                    var p = selected[0];

                    if (p.UseTab >= 3)
                    {
                        MessageBox.Show(
                            $"❌ Profile {p.ProfileName} đã đạt giới hạn 3 tab!",
                            "Không thể chọn",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return false;
                    }

                    profile = p;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi chọn profile: " + ex.Message);
                return false;
            }
        }

        private async void btn_SharePost_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!SelectProfileForScan(out _profile))
            {
                MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                Close();
                return;
            }

            // 👉 đảm bảo chỉ 1 profile
            var mainPage = await Ads.Instance.GetPageEnsureSingleTabAsync(_profile.IDAdbrowser);
            if (mainPage == null)
            {
                MessageBox.Show("❌ Không mở được tab chính của profile!");
                Close();
                return;
            }
            Libary.Instance.SetProfileContext(
                _profile.IDAdbrowser,
                _profile.ProfileName
            );
            await AskAndPrepareShareAsync(mainPage);

        }

        private void btn_SaveJson_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var data = gridControl1.DataSource as List<SharePostVM>;
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("❌ Không có dữ liệu để lưu");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON file (*.json)|*.json";
                sfd.FileName = $"share_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                // 🔥 LUÔN SAVE LIST – KỂ CẢ 1 PHẦN TỬ
                JsonHelper.Save<SharePostVM>(data, sfd.FileName);
                MessageBox.Show("✅ Đã lưu JSON (chuẩn list)");
            }
        }
        private void btn_Import_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON file (*.json)|*.json";
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    var data = JsonHelper.LoadSharePostJson(ofd.FileName);

                    if (data == null || data.Count == 0)
                    {
                        MessageBox.Show("⚠️ File JSON rỗng");
                        return;
                    }

                    gridControl1.DataSource = data;
                    gridView1.PopulateColumns();
                    InitShareGrid();
                    gridView1.RefreshData();

                    MessageBox.Show($"✅ Import {data.Count} share");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ Import JSON lỗi:\n" + ex.Message);
                }
            }
        }
        private void gridView1_RowCellClick( object sender,DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            if (e.RowHandle < 0) return;

            var gv = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            var row = gv?.GetRow(e.RowHandle) as SharePostVM;
            if (row == null) return;

            OpenSharePostLink(row, e.Column.FieldName);
        }

        // hàm mở comment
        private void OpenSharePostLink(SharePostVM row, string fieldName)
        {
            if (row == null) return;

            string url = null;

            switch (fieldName)
            {
                case nameof(SharePostVM.SharerLinkView):
                    url = row.SharerLink;
                    break;

                case nameof(SharePostVM.TargetLinkView):
                    url = row.TargetLink;
                    break;

                case nameof(SharePostVM.PostLinkShareView):
                    url = row.PostLinkShare;
                    break;

                case nameof(SharePostVM.ViewComments):
                    OpenShareComments(row);
                    return;
            }

            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Không mở được link:\n" + ex.Message);
            }
        }
        private void OpenShareComments(SharePostVM row)
        {
            if (row.Comments == null || row.Comments.Count == 0)
            {
                MessageBox.Show("⚠ Bài chia sẻ chưa có bình luận");
                return;
            }

            using (var f = new FShareComments(row.Comments))
            {
                f.ShowDialog();
            }
        }
        // hàm mở link
        private void ApplyShareHyperlink(GridView gv)
        {
            var linkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit
            {
                SingleClick = true,
                TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard
            };

            linkEdit.OpenLink += (s, e) =>
            {
                var view = gv;
                var row = view.GetFocusedRow() as SharePostVM;
                if (row == null) return;

                string field = view.FocusedColumn.FieldName;
                string url = null;

                switch (field)
                {
                    case nameof(SharePostVM.SharerLinkView):
                        url = row.SharerLink;
                        break;

                    case nameof(SharePostVM.TargetLinkView):
                        url = row.TargetLink;
                        break;

                    case nameof(SharePostVM.PostLinkShareView):
                        url = row.PostLinkShare;
                        break;

                    case nameof(SharePostVM.ViewComments):
                        OpenShareComments(row);
                        return;
                }

                if (!string.IsNullOrWhiteSpace(url))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(url);
                    }
                    catch { }
                }
            };

            // 👉 GÁN CHỈ CHO CÁC CỘT CẦN CLICK
            string[] cols =
            {
        nameof(SharePostVM.SharerLinkView),
        nameof(SharePostVM.TargetLinkView),
        nameof(SharePostVM.PostLinkShareView),
        nameof(SharePostVM.ViewComments)
    };

            foreach (var colName in cols)
            {
                var col = gv.Columns[colName];
                if (col == null) continue;

                col.ColumnEdit = linkEdit;
                col.OptionsColumn.AllowEdit = true;   // chỉ cột này
                col.OptionsColumn.ReadOnly = false;
            }
        }
        private void barButtonItem3_ItemClick(
     object sender,
     DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var data = gridControl1.DataSource as List<SharePostVM>;
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("❌ Không có dữ liệu để xuất");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                sfd.FileName = $"Share_Comment_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // 👉 GỌI HELPER MỚI
                    ExcellHelper.Instance.ExportSharePostFullExcel(data, sfd.FileName);
                    MessageBox.Show(
                        "✅ Xuất file thành công",
                        "Hoàn tất",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "❌ Lỗi xuất Excel:\n" + ex.Message,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

    }
}
