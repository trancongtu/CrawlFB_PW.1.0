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
namespace CrawlFB_PW._1._0.Auto
{
    public partial class FAddPageAuto : Form
    {
        DataTable table = new DataTable();
        public List<PageInfo> SelectedPages { get; private set; } = new List<PageInfo>();
        public FAddPageAuto()
        {
            InitializeComponent();
            InitGrid();
            LoadPage();
        }
        private void InitGrid()
        {
            table.Columns.Add("STT", typeof(int));
            table.Columns.Add("Select", typeof(bool));
            table.Columns.Add("PageID", typeof(string));
            table.Columns.Add("PageName", typeof(string));
            table.Columns.Add("PageLink", typeof(string));
            table.Columns.Add("TimeLastPost", typeof(DateTime));
            table.Columns.Add("DaysNotScan", typeof(int));
            table.Columns.Add("IsAdded", typeof(bool));
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
            gv.Columns["DaysNotScan"].Width = 120;

            gv.Columns["STT"].Caption = "STT";
            gv.Columns["Select"].Caption = "Chọn";
            gv.Columns["PageName"].Caption = "Tên Page";
            gv.Columns["PageLink"].Caption = "Link";
            gv.Columns["TimeLastPost"].Caption = "Bài cuối";
            gv.Columns["DaysNotScan"].Caption = "Chưa quét (ngày)";
            gv.Columns["IsAdded"].Visible = false;
           
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
                col.OptionsColumn.AllowEdit = false;

            gv.Columns["Select"].OptionsColumn.AllowEdit = true;
            gv.RowStyle += (s, e) =>
            {
                if (e.RowHandle < 0) return;

                bool isAdded = Convert.ToBoolean(
                    gv.GetRowCellValue(e.RowHandle, "IsAdded")
                );

                if (isAdded)
                {
                    e.Appearance.BackColor = Color.OrangeRed; // 🔥 nổi bật hơn
                    e.Appearance.ForeColor = Color.White;
                    return;
                }

                int days = Convert.ToInt32(gv.GetRowCellValue(e.RowHandle, "DaysNotScan"));

                if (days < 7)
                    e.Appearance.BackColor = Color.FromArgb(220, 255, 220);
                else if (days <= 10)
                    e.Appearance.BackColor = Color.FromArgb(255, 245, 200);
                else
                    e.Appearance.BackColor = Color.FromArgb(255, 220, 180);
            };
            gv.ShowingEditor += (s, e) =>
            {
                var view = s as DevExpress.XtraGrid.Views.Grid.GridView;

                bool isAdded = Convert.ToBoolean(
                    view.GetRowCellValue(view.FocusedRowHandle, "IsAdded")
                );

                if (isAdded)
                    e.Cancel = true;
            };
        }
        private void LoadPage()
        {
            table.Rows.Clear();
            int stt = 1;

            var monitorSet = SQLDAO.Instance.GetAllPageInMonitor(); // 🔥 load 1 lần

            DataTable notes = SQLDAO.Instance.GetAllPageNote();

            foreach (DataRow n in notes.Rows)
            {
                string pageId = n["PageID"]?.ToString();

                int isScan = SQLDAO.Instance.GetIsScanned(pageId);
                if (isScan != 1) continue;

                var pi = SQLDAO.Instance.GetPageByID(pageId);
                DateTime? lastPost = SQLDAO.Instance.GetTimeLastPost(pageId);

                if (pi == null || !lastPost.HasValue) continue;

                int days = (DateTime.Now - lastPost.Value).Days;

                bool isAdded = monitorSet.Contains(pageId); // 🔥 O(1)

                table.Rows.Add(
                    stt++,
                    false,
                    pi.PageID,
                    pi.PageName,
                    pi.PageLink,
                    lastPost.Value,
                    days,
                    isAdded
                );
            }
        }
        private void SetSelectAll(bool val)
        {
            foreach (DataRow r in table.Rows)
                r["Select"] = val;
        }

        private void btn_SelecAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetSelectAll(true);
        }

        private void btn_Reset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetSelectAll(false);
        }
        private void ConfirmSelection()
        {
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();

            SelectedPages.Clear();

            var addedNames = new List<string>();
            var duplicate = new List<string>();

            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                DataRow row = gridView1.GetDataRow(i);
                if (row == null) continue;

                if (!row.Field<bool>("Select")) continue;

                string pageId = row["PageID"]?.ToString();
                if (string.IsNullOrEmpty(pageId)) continue;

                bool isAdded = row.Field<bool>("IsAdded");

                if (isAdded)
                {
                    duplicate.Add(row["PageName"]?.ToString());
                    continue;
                }

                var info = SQLDAO.Instance.GetPageByID(pageId);
                if (info == null) continue;

                // ✅ INSERT DB
                SQLDAO.Instance.InsertPageMonitor(pageId);

                // ✅ UPDATE UI NGAY (🔥 quan trọng)
                row["IsAdded"] = true;
                row["Select"] = false;

                addedNames.Add(info.PageName);

                SelectedPages.Add(info);
            }

            // ❌ có trùng
            if (duplicate.Count > 0)
            {
                MessageBox.Show(
                    "❌ Các page đã tồn tại:\n\n" + string.Join("\n", duplicate),
                    "Trùng Page",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            // ✅ add thành công
            if (addedNames.Count > 0)
            {
                MessageBox.Show(
                    "✅ Đã thêm thành công:\n\n" + string.Join("\n", addedNames),
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            if (SelectedPages.Count > 0)
            {
                this.DialogResult = DialogResult.OK;
            }

            gridView1.RefreshData(); // 🔥 cập nhật màu ngay
        }

        private void btn_SelectPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ConfirmSelection();
        }
    }
}
