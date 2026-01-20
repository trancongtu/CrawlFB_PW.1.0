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
using CrawlFB_PW._1._0.Helper;
using DevExpress.XtraBars;
using CrawlFB_PW._1._0.Enums;
using DevExpress.XtraGrid.Columns;
using UIHelper = CrawlFB_PW._1._0.Helper.UICommercialHelper;
using ClosedXML.Excel;

namespace CrawlFB_PW._1._0.UC
{
    public partial class UCPostMainDBcs : UserControl
    {
        private UIPagerBarHelper pager;
        public UCPostMainDBcs()
        {
            InitializeComponent();
            this.Load += UCPostMainDBcs_Load;
        }
        private void UCPostMainDBcs_Load(object sender, EventArgs e)
        {
            InitPager();
            //InitGrid();
        }
        private void InitGrid()
        {
            var gv = gridView1;
            gv.OptionsBehavior.Editable = true;   // bật tạm để checkbox                                                
            //gv.RowCellClick += GridView1_RowCellClick;
            // gv.RowCellStyle += GridView1_RowCellStyle;
            gv.CustomColumnDisplayText += GridView_CustomColumnDisplayText;
            UIHelper.StyleAllControls(this);
            gv.OptionsBehavior.Editable = false;
            UICommercialHelper.StyleGrid(gridView1);// app style chung của grid          
            UIPostGridHelper.ApplyAllPostGridUCDBMain(gv);
            // ===== LOG GRID SAU KHI INIT =====
            WriteGridLog(gv);
        }
        // ==== CÁC HÀM XUẤT DỮ LIỆU RA PageGRID
        public void LoadSource()
        {
            if (pager == null)
                InitPager();
            LoadPage(1, pager.PageSize); // luôn load trang 1            
        }
        // Load Page kiểu này để xuất theo trang được
        void LoadPage(int pageIndex, int pageSize)
        {
            //load page gọi getSoure là lấy DB với indexpage, pagesiz số hàng 1 page
            if (pageSize <= 0)
                pageSize = 20; // mặc định an toàn (20 dòng 1 trang)
            int totalRows;
            DataTable dt = SQLDAO.Instance.GetPostsForPagesDB(daysFilter: null, pageIndex, pageSize, out totalRows);
            // có nghĩa là hiện thị theo trang là k getall mà get theo khúc, từ pageindex đến tổng pagesize, 
            if (dt != null && dt.Rows.Count > 0)
            {
                Libary.Instance.CreateLog("UCPostDB", $"✅ Lấy dữ liệu từ Database thành công | Rows = {dt.Rows.Count}");
            }
            else
            {
                Libary.Instance.CreateLog("UCPostDB", "⚠ Lấy dữ liệu thành công nhưng KHÔNG có dòng nào");
                MessageBox.Show("Không có dữ liệu");
            }
            BindGrid(dt);
            pager.Update(totalRows);
        }
        private void BindGrid(DataTable dt)
        {

            gridControl1.DataSource = dt; // lấy dữ liệu từ database dữ liệu tĩnh
            CheckDataTableColumns(dt, "PostInfo");
            gridView1.PopulateColumns();   // hiện cột      
            InitGrid();// khởi tạo grid
        }
        void InitPager()
        {
            pager = new UIPagerBarHelper();

            pager.OnPageChanged = (pageIndex, pageSize) =>
            {
                LoadPage(pageIndex, pageSize);
            };
            // ⭐ DÙNG barManager1 + bar3 (Status bar) // tắt bar khi chưa có dữ liệu
            pager.Init(barManager1, bar3);
        }
        private void WriteGridLog(DevExpress.XtraGrid.Views.Grid.GridView gv)
        {
            const string module = "UCPageDB";

            if (gv == null)
            {
                Libary.Instance.CreateLog(module, "❌ GridView = null");
                return;
            }

            int colCount = gv.Columns.Count;
            Libary.Instance.CreateLog(module, $"✅ InitGrid OK | Columns = {colCount}");

            if (colCount == 0)
            {
                Libary.Instance.CreateLog(module, "⚠ Grid chưa có cột (PopulateColumns chưa chạy)");
                return;
            }

            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
            {
                Libary.Instance.CreateLog(
                    module,
                    $"🧱 Col: FieldName='{col.FieldName}', Caption='{col.Caption}', Width={col.Width}, Visible={col.Visible}"
                );
            }
        }
        private void CheckDataTableColumns(DataTable dt, string type)
        {
            const string module = "UCPageDB";

            if (dt.Columns.Count == 0)
            {
                Libary.Instance.CreateLog(module, $"❌ [{type}] DataTable KHÔNG có cột nào");
                return;
            }

            Libary.Instance.CreateLog(
                module,
                $"✅ [{type}] DataTable OK | Columns = {dt.Columns.Count}"
            );

            foreach (DataColumn col in dt.Columns)
            {
                Libary.Instance.CreateLog(
                    module,
                    $"📦 DT Col: '{col.ColumnName}' ({col.DataType.Name})"
                );
            }
        }
        // định dạng giờ cho đẹp
        private void GridView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == null || e.Value == null)
                return;

            if (e.Column.FieldName == "RealPostTime" && e.Value is DateTime dt)
            {
                e.DisplayText = dt.TimeOfDay == TimeSpan.Zero
                    ? dt.ToString("dd/MM/yyyy")
                    : dt.ToString("dd/MM/yyyy HH:mm");
            }
        }

        private void btn_ExportAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                // 🔹 Lấy toàn bộ post (không phân trang)
                int totalRows;
                var dt = SQLDAO.Instance.GetPostsForPagesDB(
                    daysFilter: null,
                    pageIndex: 1,
                    pageSize: int.MaxValue,
                    out totalRows
                );

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("⚠ Không có dữ liệu để xuất!");
                    return;
                }

                // 🔹 Bỏ cột ID
                string[] removeCols =
                {
            "PostID",
            "PageIDContainer",
            "PageIDCreate",
            "PersonIDCreate"
                };

                foreach (var col in removeCols)
                {
                    if (dt.Columns.Contains(col))
                        dt.Columns.Remove(col);
                }

                // 🔹 Chọn nơi lưu
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Excel (*.xlsx)|*.xlsx",
                    FileName = $"Post_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                // 🔹 Ghi Excel
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Posts");

                    // 1️⃣ Đổ dữ liệu
                    ws.Cell(1, 1).InsertTable(dt, "PostData", true);

                    // 2️⃣ 🔥 GỌI STYLE Ở ĐÂY (CHỖ DUY NHẤT)
                    ExcellHelper.StylePostWorksheet(ws, dt);

                    // 3️⃣ Lưu file
                    wb.SaveAs(sfd.FileName);
                }

                MessageBox.Show("✅ Xuất Excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xuất Excel: " + ex.Message);
            }
        }
    }
}
