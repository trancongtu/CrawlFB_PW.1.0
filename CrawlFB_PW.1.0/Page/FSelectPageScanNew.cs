using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.DAO;
using DocumentFormat.OpenXml.Vml;
using CrawlFB_PW._1._0.Helper;

namespace CrawlFB_PW._1._0.Page
{
    public partial class FSelectPageScanNew : Form
    {
        DataTable table = new DataTable();
        public List<PageInfo> SelectedPages { get; private set; } = new List<PageInfo>();

        public FSelectPageScanNew()
        {
            InitializeComponent();
            InitGrid();
            LoadPageNote();
            BindBarEvents();
        }
        // ================= GRID INIT =====================
        private void InitGrid()
        {
            table.Columns.Add("STT", typeof(int));
            table.Columns.Add("Select", typeof(bool));
            table.Columns.Add("PageID", typeof(string));
            table.Columns.Add("PageName", typeof(string));
            table.Columns.Add("PageLink", typeof(string));
            table.Columns.Add("TimeLastPost", typeof(string));
            table.Columns.Add("IsScanned", typeof(string));

            gridControl1.DataSource = table;

            var gv = gridView1;
            gv.OptionsBehavior.Editable = true;
            gv.OptionsView.ShowGroupPanel = false;

            gv.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gv.Appearance.Row.Font = new Font("Segoe UI", 9F);

            gv.OptionsView.ColumnAutoWidth = false;
            gv.OptionsView.RowAutoHeight = true;

            gv.Columns["PageID"].Visible = false;
            gv.Columns["Select"].Width = 60;
            gv.Columns["PageName"].Width = 260;
            gv.Columns["PageLink"].Width = 260;
            gv.Columns["TimeLastPost"].Width = 150;
            gv.Columns["IsScanned"].Width = 150;

            gv.Columns["STT"].Caption = "STT";
            gv.Columns["Select"].Caption = "Chọn";
            gv.Columns["PageName"].Caption = "Tên Page";
            gv.Columns["PageLink"].Caption = "Link";
            gv.Columns["TimeLastPost"].Caption = "T/G Bài cuối";
            gv.Columns["IsScanned"].Caption = "Trạng thái";

            // Icon trạng thái
            gv.CustomDrawCell += (s, e) =>
            {
                if (e.Column.FieldName != "IsScanned") return;

                string txt = e.CellValue?.ToString() ?? "";
                string icon = txt.Contains("Đã quét") ? "✔" : "⚠";

                e.DisplayText = $"{icon}   {txt}";
            };

            // Tô màu dòng
            gv.RowStyle += (s, e) =>
            {
                if (e.RowHandle < 0) return;

                string status = gv.GetRowCellValue(e.RowHandle, "IsScanned")?.ToString() ?? "";
                if (status.Contains("Đã quét"))
                    e.Appearance.BackColor = Color.FromArgb(230, 255, 230);
                else
                    e.Appearance.BackColor = Color.FromArgb(255, 240, 230);
            };
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
                col.OptionsColumn.AllowEdit = false;

            // ✔ Cho phép sửa duy nhất cột Select
            gv.Columns["Select"].OptionsColumn.AllowEdit = true;
        }

        // ================= LOAD PAGE NOTE =====================
        private void LoadPageNote()
        {
            table.Rows.Clear();
            int stt = 1;
            DataTable notes = SQLDAO.Instance.GetAllPageNote(); // ✔ notes là DataTable

            foreach (DataRow n in notes.Rows)   // ✔ FIX LỖI foreach
            {
                string pageId = n["PageID"]?.ToString();
                DateTime? timeLastPost = SQLDAO.Instance.GetTimeLastPost(pageId);
                string TimeLastPost = TimeHelper.NormalizeTime(timeLastPost);
                var pi = SQLDAO.Instance.GetPageByID(pageId);
                int scanned = SQLDAO.Instance.GetIsScanned(pageId);

                if (pi != null)
                {
                    table.Rows.Add(
                        stt++,
                        false,
                        pi.PageID,
                        pi.PageName,
                        pi.PageLink,
                        TimeLastPost,
                        scanned == 1 ? "✔ Đã quét lần đầu" : "⏳ Chưa quét"
                    );
                }
            }
        }
        // ================= EVENT BINDING =====================
        private void BindBarEvents()
        {
            btnAdd.ItemClick += (s, e) => ConfirmSelection();
            btnCannel.ItemClick += (s, e) => Close();
            btnOnlyNotScan.ItemClick += (s, e) => FilterNotScanned();
            btnOnlyScan.ItemClick += (s, e) => FilterScanned();
            btn_Reset.ItemClick += (s, e) => { gridView1.ActiveFilterString = ""; };

            txtShearch.EditValueChanged += (s, e) =>
            {
                string kw = txtShearch.EditValue?.ToString()?.Trim() ?? "";
                gridView1.ActiveFilterString =
                    $"[PageName] LIKE '%{kw}%' OR [PageLink] LIKE '%{kw}%'";
            };
        }

        // ========================= SELECT =========================
        private void SetSelectAll(bool val)
        {
            foreach (DataRow r in table.Rows)
                r["Select"] = val;
        }

        private void ConfirmSelection()
        {
            // ❗ commit checkbox edit
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();

            SelectedPages.Clear();

            // ✅ CHỈ DUYỆT ROW ĐANG HIỂN THỊ (ĐÃ APPLY FILTER)
            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                DataRow row = gridView1.GetDataRow(i);
                if (row == null) continue;

                bool isSelected = row.Field<bool>("Select");
                if (!isSelected) continue;

                string pageId = row["PageID"]?.ToString();
                if (string.IsNullOrEmpty(pageId)) continue;

                var info = SQLDAO.Instance.GetPageByID(pageId);
                if (info != null)
                    SelectedPages.Add(info);
            }

            if (SelectedPages.Count == 0)
            {
                MessageBox.Show("⚠ Chưa chọn Page nào (theo filter hiện tại)!");
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        // ========================= FILTER =========================
        private void FilterNotScanned()
        {
            gridView1.ActiveFilterString = "[IsScanned] LIKE '%Chưa quét%'";
        }

        private void FilterScanned()
        {
            gridView1.ActiveFilterString = "[IsScanned] LIKE '%Đã quét%'";
        }

        private void barButtonItemUpdateIscan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();

            var gv = gridView1;
            int updated = 0;

            foreach (DataRow r in table.Rows)
            {
                bool isSelected = r.Field<bool>("Select");
                if (!isSelected)
                    continue;

                string pageId = r["PageID"]?.ToString();
                if (string.IsNullOrEmpty(pageId))
                    continue;

                // 1️⃣ Kiểm tra có bài viết không
                int postCount = SQLDAO.Instance.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TablePost WHERE PageIDContainer=@id",
                    new Dictionary<string, object> { { "@id", pageId } }
                );

                int isScan = postCount > 0 ? 1 : 0;
                string txt = isScan == 1 ? "✔ Đã quét lần đầu" : "⏳ Chưa quét";

                // 2️⃣ Update DB
                SQLDAO.Instance.UpdatePageIsScanned(pageId, isScan);

                // 3️⃣ Update Grid
                r["IsScanned"] = txt;

                updated++;
            }

            gv.RefreshData();

            MessageBox.Show($"✔ Đã cập nhật IsScan cho {updated} page!");
        }

        private void btn_SelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetSelectAll(true);
        }
    }
}
