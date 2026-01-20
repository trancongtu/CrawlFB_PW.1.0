using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using DevExpress.XtraGrid.Views.Grid;

namespace CrawlFB_PW._1._0.Page
{
    public partial class FViewPage : Form
    {
        private DataTable currentTable = new DataTable();

        public FViewPage()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            cbSelectSource.Items.Add("PageInfo");
            cbSelectSource.Items.Add("PageNote");
            cbSelectSource.Items.Add("PageMonitor");

            cbSelectSource.SelectedIndexChanged += CbSelectSource_SelectedIndexChanged;
            btnAddPageNote.Click += BtnAddPageNote_Click;          
            InitGrid();
        }

        private void InitGrid()
        {
            var gv = gridView1;
            gv.OptionsBehavior.Editable = false;
            gv.OptionsView.ShowGroupPanel = false;
        }

        // =============================
        //   SỰ KIỆN COMBOBOX: LOAD DỮ LIỆU
        // =============================
        private void CbSelectSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            string type = cbSelectSource.SelectedItem.ToString();

            switch (type)
            {
                case "PageInfo":
                    LoadPageInfo();
                    break;

                case "PageNote":
                    LoadPageNote();
                    break;

                case "PageMonitor":
                    LoadPageMonitor();
                    break;
            }
        }

        // =============================
        //   LOAD TABLE PAGEINFO (DB1)
        // =============================
        private void LoadPageInfo()
        {
            currentTable = new DataTable();
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("PageName");
            currentTable.Columns.Add("PageLink");

            string dbPath = PathHelper.Instance.GetMainDatabasePath();
            var dt = DatabaseDAO.Instance.GetAllPageInfo(); // cần hàm này

            int stt = 1;
            foreach (DataRow r in dt.Rows)
            {
                currentTable.Rows.Add(stt++, r["PageID"], r["PageName"], r["PageLink"]);
            }

            gridControl1.DataSource = currentTable;
            gridView1.BestFitColumns();
        }

        // =============================
        //   LOAD TABLE PAGENOTE (DB2)
        // =============================
        private void LoadPageNote()
        {
            currentTable = new DataTable();
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("PageName");
            currentTable.Columns.Add("PageLink");
            currentTable.Columns.Add("TimeSave");

            var notes = DatabaseDAO.Instance.GetAllPageNote();
            // notes = List<(PageInfo Info, string TimeSave)>

            int stt = 1;

            foreach (var item in notes)
            {
                var pi = item.Info;
                string timeSave = item.TimeSave;

                currentTable.Rows.Add(
                    stt++,
                    pi.PageID,
                    pi.PageName,
                    pi.PageLink,
                    timeSave
                );
            }

            gridControl1.DataSource = currentTable;
            gridView1.BestFitColumns();
        }


        // =============================
        //   LOAD TABLE PAGEMONITOR (DB1)
        // =============================
        private void LoadPageMonitor()
        {
            currentTable = new DataTable();
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("PageName");
            currentTable.Columns.Add("Status");
            currentTable.Columns.Add("LastScanTime");

            var dt = DatabaseDAO.Instance.GetMonitoredPages();
            int stt = 1;

            foreach (DataRow r in dt.Rows)
            {
                currentTable.Rows.Add(
                    stt++,
                    r["PageID"],
                    r["PageName"],
                    r["Status"],
                    r["LastScanTime"]
                );
            }

            gridControl1.DataSource = currentTable;
            gridView1.BestFitColumns();
        }

        // =============================
        //   NÚT THÊM VÀO PAGENOTE
        // =============================
        private void BtnAddPageNote_Click(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0)
            {
                MessageBox.Show("⚠ Hãy chọn 1 dòng!", "Thông báo");
                return;
            }

            string pageID = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "PageID").ToString();

            DatabaseDAO.Instance.InsertPageNote(pageID);

            MessageBox.Show("✔ Đã thêm vào PageNote!");

            LoadPageNote();
        }

        // =============================
        //   NÚT XÓA KHỎI PAGENOTE
        // =============================


        private void BtnDeletePageNote_Click_1(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0)
            {
                MessageBox.Show("⚠ Hãy chọn 1 dòng để xóa!");
                return;
            }

            string pageID = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "PageID").ToString();

            if (MessageBox.Show("Bạn chắc muốn xóa page này khỏi PageNote?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            DatabaseDAO.Instance.DeletePageNote(pageID);

            MessageBox.Show("✔ Đã xóa khỏi PageNote!");

            LoadPageNote(); // reload
        }

        private void btnAddPageMoniter_Click(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0)
            {
                MessageBox.Show("⚠ Hãy chọn 1 dòng!", "Thông báo");
                return;
            }

            // Lấy PageID từ dòng được chọn
            string pageID = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "PageID")?.ToString();

            if (string.IsNullOrEmpty(pageID))
            {
                MessageBox.Show("❌ PageID không hợp lệ!", "Lỗi");
                return;
            }

            // ➤ Thêm vào TablePageMonitor (MainDatabase)
            DatabaseDAO.Instance.InsertPageMonitor(pageID);

            MessageBox.Show("✔ Đã thêm vào danh sách giám sát!");

            // ➤ Reload lại Monitor Grid
            LoadPageMonitor();
        }
    }
}
