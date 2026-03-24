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
using CrawlFB_PW._1._0.DAO;
namespace CrawlFB_PW._1._0.Page
{
    public partial class FSelectPageUpdate : Form
    {
        DataTable table = new DataTable();
        public List<PageInfo> SelectedPages { get; private set; } = new List<PageInfo>();
        public FSelectPageUpdate()
        {
            InitializeComponent();
            InitGrid();
            LoadPageUpdate();
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

            // màu theo tuổi
            gv.RowStyle += (s, e) =>
            {
                if (e.RowHandle < 0) return;

                int days = Convert.ToInt32(gv.GetRowCellValue(e.RowHandle, "DaysNotScan"));

                if (days < 7)
                {
                    e.Appearance.BackColor = Color.FromArgb(220, 255, 220); // xanh
                }
                else if (days <= 10)
                {
                    e.Appearance.BackColor = Color.FromArgb(255, 245, 200); // vàng
                }
                else
                {
                    e.Appearance.BackColor = Color.FromArgb(255, 220, 180); // cam
                }
            };

            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
                col.OptionsColumn.AllowEdit = false;

            gv.Columns["Select"].OptionsColumn.AllowEdit = true;
        }
        private void LoadPageUpdate()
        {
            table.Rows.Clear();
            int stt = 1;

            DataTable notes = SQLDAO.Instance.GetAllPageNote();

            foreach (DataRow n in notes.Rows)
            {
                string pageId = n["PageID"]?.ToString();

                int isScan = SQLDAO.Instance.GetIsScanned(pageId);

                // chỉ lấy page đã scan
                if (isScan != 1)
                    continue;

                var pi = SQLDAO.Instance.GetPageByID(pageId);

                DateTime? lastPost = SQLDAO.Instance.GetTimeLastPost(pageId);

                if (pi == null || !lastPost.HasValue)
                    continue;

                int days = (DateTime.Now - lastPost.Value).Days;

                table.Rows.Add(
                    stt++,
                    false,
                    pi.PageID,
                    pi.PageName,
                    pi.PageLink,
                    lastPost.Value,
                    days
                );
            }
        }
        private void SetSelectAll(bool val)
        {
            foreach (DataRow r in table.Rows)
                r["Select"] = val;
        }
        private void btn_SelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetSelectAll(true);
        }

        private void btn_Reset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetSelectAll(false);
        }
        // lưu chọn
        private void ConfirmSelection()
        {
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();

            SelectedPages.Clear();

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
                MessageBox.Show("⚠ Chưa chọn Page nào!");
                return;
            }

            this.DialogResult = DialogResult.OK;
        }
        private void btn_SelectPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ConfirmSelection();
        }
    }
}
