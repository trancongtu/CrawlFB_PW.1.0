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
using UIHelper = CrawlFB_PW._1._0.Helper.UICommercialHelper;
using DevExpress.XtraGrid.Columns;
namespace CrawlFB_PW._1._0.UC
{
    public partial class UCPageDB : UserControl
    {
        private UIPagerBarHelper pager;
        PageSourceType _currentSourcetype;
        private Dictionary<int, bool> _uiSelect = new Dictionary<int, bool>();
        private DataTable currentTable = new DataTable();
        //=== hàm Style chia 3 page 1 grid, post 1 grid, perosn 1 grid     
        public UCPageDB()
        {
            InitializeComponent();
            this.Load += UCPageDB_Load;
            gridView1.CustomUnboundColumnData += GridView1_CustomUnboundColumnData;
            gridView1.CustomColumnDisplayText += GridView1_CustomColumnDisplayText;
        }
        private void UCPageDB_Load(object sender, EventArgs e)
        {
            InitPager();
        }
        //setup Grid
        private void InitGrid()
        {
            var gv = gridView1;

            gv.OptionsBehavior.Editable = true;   // bật tạm để checkbox
                                                  //gv.RowCellClick += GridView1_RowCellClick;

            // gv.RowCellStyle += GridView1_RowCellStyle;
            // gv.CustomColumnDisplayText += GridView1_CustomColumnDisplayText;
            UIHelper.StyleAllControls(this);
            gv.OptionsBehavior.Editable = false;
            UICommercialHelper.StyleGrid(gridView1);// app style chung của grid          
            UIPageInfoGridHelper.ApplyAll(gridView1); //app style của pagegrid         
            // ===== LOG GRID SAU KHI INIT =====
            WriteGridLog(gv);
        }
        // ==== CÁC HÀM XUẤT DỮ LIỆU RA PageGRID
        public void LoadSource(PageSourceType type)
        {
            _currentSourcetype = type;
            LoadPage(1, pager.PageSize); // luôn load trang 1
            //LoadSoure lấy type và gọi loadpage
        }
        void LoadPage(int pageIndex, int pageSize)
        {
            //load page gọi getSoure là lấy DB với indexpage, pagesiz số hàng 1 page
            int totalRows;

            DataTable dt = GetSourcePage(
                _currentSourcetype,
                pageIndex,
                pageSize,
                out totalRows
            );
            if (dt != null && dt.Rows.Count > 0)
            {
                Libary.Instance.CreateLog(
                    "UCPageDB",
                    $"✅ Lấy dữ liệu từ Database thành công | Rows = {dt.Rows.Count}"
                );
            }
            else
            {
                Libary.Instance.CreateLog(
                    "UCPageDB",
                    "⚠ Lấy dữ liệu thành công nhưng KHÔNG có dòng nào"
                );
            }
            BindGrid(dt);
            pager.Update(totalRows);
        }
        public DataTable GetSourcePage(PageSourceType type, int pageIndex,int pageSize, out int totalRows)
        {
            switch (type)
            {
                case PageSourceType.PageInfo:
                    return SQLDAO.Instance.GetPageInfoPage(pageIndex, pageSize, out totalRows);

                case PageSourceType.PageNote:
                    return SQLDAO.Instance.GetPageNotePage(pageIndex, pageSize, out totalRows);

                case PageSourceType.PageMonitor:
                    return SQLDAO.Instance.GetPageMonitorPage(pageIndex, pageSize, out totalRows);
                default:
                    totalRows = 0;
                    return new DataTable();
            }
        }

        private void BindGrid(DataTable dt)
        {
            gridControl1.DataSource = dt;
            CheckDataTableColumns(dt, _currentSourcetype);
            gridView1.PopulateColumns();
            if (gridView1.Columns["STT"] == null)
            {
                GridColumn sttCol = gridView1.Columns.AddVisible("STT", "STT");
                sttCol.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                sttCol.VisibleIndex = 0;
                sttCol.Width = 25;
            }

            gridView1.CustomUnboundColumnData += (s, e) =>
            {
                if (e.Column.FieldName == "STT" && e.IsGetData)
                    e.Value = e.ListSourceRowIndex + 1;
            };
            if (gridView1.Columns["Select"] == null)
            {
                GridColumn colSelect = gridView1.Columns.AddVisible("Select", "✓");
                colSelect.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                colSelect.VisibleIndex = 0;
                colSelect.Width = 35;
                colSelect.OptionsColumn.AllowEdit = true;
                colSelect.OptionsColumn.AllowFocus = true;
            }

            InitGrid();   // gọi helper style + column
        }
        //===============HẾT XUẤT DỮ LIỆU
        //===========HÀM ĐỂ PHÂN TRANG
        void InitPager()
        {
            pager = new UIPagerBarHelper();

            pager.OnPageChanged = (pageIndex, pageSize) =>
            {
                LoadPage(pageIndex, pageSize);
            };

            // ⭐ DÙNG barManager1 + bar3 (Status bar)
            pager.Init(barManager1, bar3);
        }

        //======================
        //=============HỖ TRỢ GHI LOG ĐỂ DEBUG
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
        private void CheckDataTableColumns(DataTable dt, PageSourceType type)
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
        private void GridView1_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.Column.FieldName == "Select")
            {
                if (e.IsGetData)
                {
                    _uiSelect.TryGetValue(e.ListSourceRowIndex, out bool val);
                    e.Value = val;
                }
                if (e.IsSetData)
                {
                    _uiSelect[e.ListSourceRowIndex] = Convert.ToBoolean(e.Value);
                }
            }
        }
        private void GridView1_CustomColumnDisplayText(  object sender,DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "TimeLastPost" && e.Value != null)
            {
                if (DateTime.TryParse(e.Value.ToString(), out DateTime dt))
                {
                    e.DisplayText = dt.ToString("dd/MM/yyyy HH:mm");
                }
            }
        }
      
        private void btnRemoveRow_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (_uiSelect == null || _uiSelect.Count == 0)
            {
                MessageBox.Show("⚠ Chưa chọn dòng nào!");
                return;
            }

            // =========================
            // 1️⃣ LẤY DANH SÁCH PageID ĐƯỢC SELECT
            // =========================
            List<string> pageIds = new List<string>();

            foreach (var kv in _uiSelect)
            {
                if (!kv.Value) continue;

                int rowIndex = kv.Key;
                if (rowIndex < 0 || rowIndex >= gridView1.DataRowCount)
                    continue;

                string pageId = gridView1
                    .GetRowCellValue(rowIndex, "PageID")
                    ?.ToString();

                if (!string.IsNullOrWhiteSpace(pageId))
                    pageIds.Add(pageId);
            }

            if (pageIds.Count == 0)
            {
                MessageBox.Show("⚠ Không lấy được PageID để xóa!");
                return;
            }

            // =========================
            // 2️⃣ CONFIRM
            // =========================
            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa {pageIds.Count} Page đã chọn?\n(Toàn bộ dữ liệu liên quan sẽ bị xóa)",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (confirm == DialogResult.No) return;

            try
            {
                // =========================
                // 3️⃣ XÓA TOÀN BỘ PAGE
                // =========================
                foreach (string pageId in pageIds)
                {
                    SQLDAO.Instance.DeleteAllPostsOfPage(pageId);
                    SQLDAO.Instance.DeletePageNote(pageId);
                    SQLDAO.Instance.DeletePageMonitor(pageId);

                    SQLDAO.Instance.ExecuteNonQuery(
                        "DELETE FROM TablePageInfo WHERE PageID=@id",
                        new Dictionary<string, object> { { "@id", pageId } }
                    );
                }

                MessageBox.Show($"✔ Đã xóa {pageIds.Count} Page!");

                // =========================
                // 4️⃣ RESET SELECT + RELOAD PAGE
                // =========================
                _uiSelect.Clear();

                LoadPage(1, pager.PageSize);   // ⭐ LOAD LẠI TRANG ĐẦU
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi xóa Page:\n" + ex.Message);
            }
        }

        private void btnDeleteAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            // =========================
            // 1️⃣ CONFIRM
            // =========================
            var confirm = MessageBox.Show(
                "⚠ Bạn có chắc muốn XÓA TOÀN BỘ PAGE?\nToàn bộ bài viết, note, monitor liên quan sẽ bị xóa!",
                "Xác nhận xóa toàn bộ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (confirm == DialogResult.No) return;

            try
            {
                // =========================
                // 2️⃣ LẤY TOÀN BỘ PageID TỪ DB (KHÔNG DỰA GRID)
                // =========================
                DataTable dt = SQLDAO.Instance.GetAllPagesDB();

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("⚠ Không có Page nào để xóa!");
                    return;
                }

                List<string> pageIds = new List<string>();
                foreach (DataRow r in dt.Rows)
                {
                    string pageId = r["PageID"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(pageId))
                        pageIds.Add(pageId);
                }

                if (pageIds.Count == 0)
                {
                    MessageBox.Show("⚠ Không lấy được PageID!");
                    return;
                }

                // =========================
                // 3️⃣ XÓA TOÀN BỘ PAGE
                // =========================
                foreach (string pageId in pageIds)
                {
                    SQLDAO.Instance.DeleteAllPostsOfPage(pageId);
                    SQLDAO.Instance.DeletePageNote(pageId);
                    SQLDAO.Instance.DeletePageMonitor(pageId);

                    SQLDAO.Instance.ExecuteNonQuery(
                        "DELETE FROM TablePageInfo WHERE PageID=@id",
                        new Dictionary<string, object> { { "@id", pageId } }
                    );
                }

                MessageBox.Show($"✔ Đã xóa TOÀN BỘ {pageIds.Count} Page!");

                // =========================
                // 4️⃣ RESET SELECT + RELOAD
                // =========================
                _uiSelect.Clear();
                LoadPage(1, pager.PageSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi xóa toàn bộ Page:\n" + ex.Message);
            }
        }
    }
}
