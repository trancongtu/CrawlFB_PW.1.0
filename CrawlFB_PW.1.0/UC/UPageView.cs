using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using System.Globalization;
using ClosedXML.Excel;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using FBType = CrawlFB_PW._1._0.Enums.FBType;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Page;
using CrawlFB_PW._1._0.Service;
using CrawlFB_PW._1._0.Helper.Mapper;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.UC
{
    public partial class UPageView : UserControl
    {
        private DataTable currentTable = new DataTable();
        private string currentViewType = "";
        string iconDir = Application.StartupPath + @"\Icons\";
        private Panel cardLikes;
        private Panel cardShares;
        private Panel cardComments;
        private Panel cardPosts;
        private Panel cardFollowers;
        private UIPagerBarHelper pager;
        private int _postPageIndex = 1;
        private int _postPageSize = 50;
        private string _currentPostPageId;
        //-== khai báo grid post kèm paging
        private Panel panelPostContainer;
        private GridControl gridPost;
        private GridView gvPost;
        private Panel panelPaging;
        private int _postTotalRows = 0;
        private TextBox txtPageJump;
        private Label lblPostPage;
        private Button btnFirst, btnPrev, btnNext, btnLast;
        public UPageView()
        {
            InitializeComponent();
            InitGrid();
            gridView1.FocusedRowChanged += GridView1_FocusedRowChanged;
            gridView1.RowClick += GridView1_RowClick;
            txb_PageShearch.EditValueChanged += Txb_PageShearch_EditValueChanged;


            panelControlDetail.AutoScroll = true;
            this.Load += (s, e) =>
            {
                InitPager();
                InitPostUI();
            };
        }     
        // ==================== KHỞI TẠO GRID ======================
        private void InitGrid()
        {
            var gv = gridView1;
            gv.OptionsBehavior.Editable = true; // bật chế độ edit cho checkbox
            gv.RowCellClick += GridView1_RowCellClick;
            gv.OptionsSelection.MultiSelect = true;
            gv.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect;
            gv.OptionsBehavior.Editable = false;
            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsView.ColumnAutoWidth = false;
            gv.OptionsView.RowAutoHeight = true;
            gv.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gv.Appearance.Row.Font = new Font("Segoe UI", 9);
            gv.RowCellStyle += GridView1_RowCellStyle;
            gv.CustomColumnDisplayText += GridView1_CustomColumnDisplayText;

        }
        private void InitPager()
        {
            pager = new UIPagerBarHelper();

            pager.OnPageChanged = (pageIndex, pageSize) =>
            {
                if (!string.IsNullOrEmpty(_currentPostPageId))
                {
                    _postPageIndex = pageIndex;
                    _postPageSize = pageSize;

                    LoadPostPaging(_currentPostPageId, pageIndex, pageSize);
                }
                else
                {
                    LoadPage(pageIndex, pageSize);
                }
            };

            pager.Init(barManager1, bar3); // ⚠ bạn đã có sẵn bar3 rồi
        }
        private void InitPostUI()
        {
            // ===== container =====
            panelPostContainer = new Panel()
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            // ===== grid =====
            gridPost = new GridControl() { Dock = DockStyle.Fill };
            gvPost = new GridView();
            gridPost.MainView = gvPost;
            gridPost.ViewCollection.Add(gvPost);

            // ===== paging panel =====
            panelPaging = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.WhiteSmoke
            };

            // ===== tạo control trước =====
            lblPostPage = new Label()
            {
                Text = "Trang 1 / 1",
                AutoSize = true,
                Top = 8,
                Left = 150
            };

            btnFirst = new Button()
            {
                Text = "<<",
                Width = 40,
                Left = 10,
                Top = 5
            };

            btnPrev = new Button()
            {
                Text = "<",
                Width = 40,
                Left = 60,
                Top = 5
            };

            btnNext = new Button()
            {
                Text = ">",
                Width = 40,
                Left = 260,
                Top = 5
            };

            btnLast = new Button()
            {
                Text = ">>",
                Width = 40,
                Left = 310,
                Top = 5
            };
            txtPageJump = new TextBox()
            {
                Width = 50,
                Top = 5,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtPageJump.Left = panelPaging.Width - txtPageJump.Width - 10;
            // ===== event =====
            btnFirst.Click += (s, e) =>
            {
                _postPageIndex = 1;
                LoadPostPaging(_currentPostPageId, _postPageIndex, _postPageSize);
            };

            btnPrev.Click += (s, e) =>
            {
                if (_postPageIndex > 1)
                {
                    _postPageIndex--;
                    LoadPostPaging(_currentPostPageId, _postPageIndex, _postPageSize);
                }
            };

            btnNext.Click += (s, e) =>
            {
                int totalPages = (int)Math.Ceiling((double)_postTotalRows / _postPageSize);

                if (_postPageIndex < totalPages)
                {
                    _postPageIndex++;
                    LoadPostPaging(_currentPostPageId, _postPageIndex, _postPageSize);
                }
            };

            btnLast.Click += (s, e) =>
            {
                int totalPages = Math.Max(1,
                    (int)Math.Ceiling((double)_postTotalRows / _postPageSize));

                _postPageIndex = totalPages;

                LoadPostPaging(_currentPostPageId, _postPageIndex, _postPageSize);
            };
            // Enter để jump
            txtPageJump.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    JumpToPage();
                    e.SuppressKeyPress = true;
                }
            };
            txtPageJump.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                    e.Handled = true;
            };
            panelPaging.Resize += (s, e) =>
            {
                txtPageJump.Left = panelPaging.Width - txtPageJump.Width - 10;
            };
            // ===== add sau khi tạo =====
            panelPaging.Controls.Add(btnFirst);
            panelPaging.Controls.Add(btnPrev);
            panelPaging.Controls.Add(lblPostPage);
            panelPaging.Controls.Add(btnNext);
            panelPaging.Controls.Add(btnLast);
            panelPaging.Controls.Add(txtPageJump);
            // ===== add vào container =====
            panelPostContainer.Controls.Add(gridPost);
            panelPostContainer.Controls.Add(panelPaging);

            panelControlDetail.Controls.Add(panelPostContainer);
            UIPostGridHelper.ApplyFakeLink(gvPost, "Địa chỉ", "LinkThật");
        }
        private void JumpToPage()
        {
            if (!int.TryParse(txtPageJump.Text, out int page))
                return;

            int totalPages = Math.Max(1,
                (int)Math.Ceiling((double)_postTotalRows / _postPageSize));

            // clamp
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            _postPageIndex = page;

            LoadPostPaging(_currentPostPageId, _postPageIndex, _postPageSize);
        }
        private void GridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            var gv = sender as DevExpress.XtraGrid.Views.Grid.GridView;

            // ❗ Chống crash nếu không phải dòng hợp lệ
            if (e.RowHandle < 0) return;

            // ❗ Lấy row tương ứng
            DataRow row = gv.GetDataRow(e.RowHandle);
            if (row == null) return;

            // ❗ Lật trạng thái Select
            bool current = row["Select"] != DBNull.Value && (bool)row["Select"];
            row["Select"] = !current;   // → Toggle: true <-> false

            gv.RefreshRow(e.RowHandle);
        }
        private void GridView1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName != "TimeLastPost")
                return;

            int days = GetDaysDiff(e.Value);

            // các trường hợp đặc biệt
            if (days == 0)
            {
                e.DisplayText = "Today";
                return;
            }
            if (days == 1)
            {
                e.DisplayText = "Yesterday";
                return;
            }

            if (days >= 9999)
            {
                e.DisplayText = "N/A";
                return;
            }

            // còn lại: hiển thị dạng "x days ago"
            e.DisplayText = $"{days} days ago";
        }
        private void GridView1_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            var gv = sender as GridView;

            // Chỉ xử lý cột TimeLastPost
            if (e.Column.FieldName != "TimeLastPost")
                return;

            DataRow row = gv.GetDataRow(e.RowHandle);
            if (row == null) return;

            int days = GetDaysDiff(row["TimeLastPost"]);

            if (days <= 10)
                e.Appearance.BackColor = Color.LightGreen;         // xanh
            else if (days <= 30)
                e.Appearance.BackColor = Color.Khaki;              // vàng
            else if (days <= 90)
                e.Appearance.BackColor = Color.Orange;             // cam
            else
                e.Appearance.BackColor = Color.LightCoral;         // đỏ
        }
        //============= HÀM TRÊN BẮT SỰ KIỆN CLICK
        private int GetDaysDiff(object value)
        {
            if (value == null || value == DBNull.Value)
                return 9999;   // coi như N/A

            if (value is DateTime dt)
                return (DateTime.Now.Date - dt.Date).Days;

            return 9999;
        }

        // ======================= LOAD DATA ========================
        
        private void LoadPage(int pageIndex, int pageSize)
        {
            switch (currentViewType)
            {
                // ===== PAGE =====
                case "PageAdded":
                case "PageCrawl":
                    LoadPagePagingByIscan(pageIndex, pageSize);
                    break;
                case "PageInfo":
                    LoadPageInfoPaging(pageIndex, pageSize);
                    break;
                case "PageNote":
                    LoadPageNotePaging(pageIndex, pageSize);
                    break;
                case "PageMonitor":
                    LoadPageMonitorPaging(pageIndex, pageSize);
                    break;

                // ===== POST (giữ nguyên) =====
                case "PostPageNote":
                    LoadPostPageNotePaging(pageIndex, pageSize);
                    break;

                case "PostPageMonitor":
                    LoadPostPageMonitorPaging(pageIndex, pageSize);
                    break;

                case "PostAllPage":
                    LoadPostAllPagePaging(pageIndex, pageSize);
                    break;
            }
        }
        //1. Load theo trang của pagenote và pagemonitor
        private void LoadPageNotePaging(int pageIndex, int pageSize)
        {
            int totalRows;

            var dt = SQLDAO.Instance.GetPageNotePaging(
                pageIndex,
                pageSize,
                out totalRows
            );

            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Địa chỉ");
            currentTable.Columns.Add("Thông tin");
            currentTable.Columns.Add("Thời gian lưu");
            currentTable.Columns.Add("TimeLastPost", typeof(DateTime));

            // dashboard
            currentTable.Columns.Add("IDFBPage");
            currentTable.Columns.Add("PageMembers");
            currentTable.Columns.Add("PageInteraction");
            currentTable.Columns.Add("PageEvaluation");
            currentTable.Columns.Add("PageInfoText");
            currentTable.Columns.Add("PageType");

            int stt = (pageIndex - 1) * pageSize + 1;

            foreach (DataRow r in dt.Rows)
            {
                string pageId = r["PageID"]?.ToString();
                object timeSave = r["TimeSave"];

                var info = PageInfoCacheService.Get(pageId);

                if (info != null)
                {
                    currentTable.Rows.Add(
                        false,
                        stt++,
                        info.PageID,
                        info.PageName,
                        info.PageLink,
                        info.IsScanned ? "Đã quét" : "Chưa quét",
                        info.PageTimeSave,
                        info.TimeLastPost.HasValue
                            ? (object)info.TimeLastPost.Value
                            : DBNull.Value,

                        info.IDFBPage,
                        info.PageMembers,
                        info.PageInteraction,
                        info.PageEvaluation,
                        info.PageInfoText,
                        info.PageType
                    );
                }
                else
                {
                    currentTable.Rows.Add(
                        false,
                        stt++,
                        pageId,
                        "(Không tìm thấy PageInfo)",
                        "",
                        "Chưa quét",
                        timeSave,
                        DBNull.Value,
                        "", "", "", "", "", ""
                    );
                }
            }

            // UI giữ nguyên
            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;

            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }

            gridView1.Columns["PageID"].Visible = false;
            gridView1.Columns["PageMembers"].Visible = false;
            gridView1.Columns["PageInteraction"].Visible = false;
            gridView1.Columns["PageEvaluation"].Visible = false;
            gridView1.Columns["PageInfoText"].Visible = false;
            gridView1.Columns["Thời gian lưu"].Visible = false;

            gridView1.BestFitColumns();

            if (currentTable.Rows.Count > 0)
                ShowPageDashboard(currentTable.Rows[0]);

            pager.Update(totalRows);
        }
        private void LoadPageMonitorPaging(int pageIndex, int pageSize)
        {
            int totalRows;

            var dt = SQLDAO.Instance.GetPageMonitorPaging(
                pageIndex,
                pageSize,
                out totalRows
            );

            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Địa chỉ");
           
            // 🔥 monitor riêng
            currentTable.Columns.Add("Trạng thái");
            currentTable.Columns.Add("Auto");
            currentTable.Columns.Add("Tổng bài đã quét");
            currentTable.Columns.Add("Lần quét đầu");
            currentTable.Columns.Add("Lần quét gần nhất");
            currentTable.Columns.Add("TimeLastPost", typeof(DateTime));
            // dashboard
            currentTable.Columns.Add("IDFBPage");
            currentTable.Columns.Add("PageMembers");
            currentTable.Columns.Add("PageInteraction");
            currentTable.Columns.Add("PageEvaluation");
            currentTable.Columns.Add("PageInfoText");
            currentTable.Columns.Add("PageType");

            int stt = (pageIndex - 1) * pageSize + 1;

            foreach (DataRow r in dt.Rows)
            {
                string pageId = r["PageID"]?.ToString();

                var info = PageInfoCacheService.Get(pageId);

                if (info != null)
                {
                  currentTable.Rows.Add(
                        false,
                        stt++,
                        pageId,
                        info.PageName,
                        info.PageLink,

                        r["Status"]?.ToString(),
                        Convert.ToInt32(r["IsAuto"]) == 1 ? "Bật" : "Tắt",
                        r["TotalPostsScanned"],

                        r["FirstScanTime"] == DBNull.Value ? "" : r["FirstScanTime"],
                        r["LastScanTime"] == DBNull.Value ? "" : r["LastScanTime"],

                        // 🔥 thêm dòng này
                        info.TimeLastPost.HasValue
                            ? (object)info.TimeLastPost.Value
                            : DBNull.Value,

                        // dashboard
                        info.IDFBPage,
                        info.PageMembers,
                        info.PageInteraction,
                        info.PageEvaluation,
                        info.PageInfoText,
                        info.PageType
                    );
                }
            }

            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;

            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }

            gridView1.Columns["PageID"].Visible = false;

            gridView1.BestFitColumns();

            if (currentTable.Rows.Count > 0)
                ShowPageDashboard(currentTable.Rows[0]);

            pager.Update(totalRows);
        }
        //2. Load theo trang của đã quét và chưa quét
        private void LoadPagePagingByIscan(int pageIndex, int pageSize)
        {
            int totalRows = 0;
            DataTable dt = null;

            if (currentViewType == "PageAdded" || currentViewType == "PageCrawl")
            {
                // 🔥 dùng hàm cũ (có isScan)
                bool isScan = currentViewType == "PageAdded";

                dt = SQLDAO.Instance.GetPageInfoPage_ByType(
                    pageIndex,
                    pageSize,
                    isScan,
                    out totalRows
                );
            }
            else
            {
                return; // không xử lý ở đây (Post để chỗ khác)
            }
            // 👉 UI giữ nguyên
            LoadPageFromDataTable(dt);
            pager.Update(totalRows);
        }
        //3. loag page info theo trang
        private void LoadPageInfoPaging(int pageIndex, int pageSize)
        {
            int totalRows;

            // 🔥 dùng hàm DB riêng (KHÔNG isScan)
            var dt = SQLDAO.Instance.GetPageInfoPage(
                pageIndex,
                pageSize,
                out totalRows
            );

            object timeLastPostValue = DBNull.Value;

            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Địa chỉ");
            currentTable.Columns.Add("Thông tin");
            currentTable.Columns.Add("Thời gian lưu");
            currentTable.Columns.Add("TimeLastPost", typeof(DateTime));

            // dashboard
            currentTable.Columns.Add("IDFBPage");
            currentTable.Columns.Add("PageMembers");
            currentTable.Columns.Add("PageInteraction");
            currentTable.Columns.Add("PageEvaluation");
            currentTable.Columns.Add("PageInfoText");
            currentTable.Columns.Add("PageType");

            int stt = (pageIndex - 1) * pageSize + 1;

            foreach (DataRow r in dt.Rows)
            {
                var rawTime = r["TimeLastPost"];

                if (rawTime != DBNull.Value)
                {
                    if (rawTime is string s && s.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase))
                        timeLastPostValue = DBNull.Value;
                    else if (rawTime is DateTime date)
                        timeLastPostValue = date;
                    else if (DateTime.TryParse(rawTime.ToString(), out DateTime parsed))
                        timeLastPostValue = parsed;
                    else
                        timeLastPostValue = DBNull.Value;
                }
                else
                {
                    timeLastPostValue = DBNull.Value;
                }

                currentTable.Rows.Add(
                    false,
                    stt++,
                    Safe(r, "PageID"),
                    Safe(r, "PageName"),
                    Safe(r, "PageLink"),
                    Convert.ToInt32(Safe(r, "IsScanned")) == 1 ? "Đã quét" : "Chưa quét",
                    Safe(r, "PageTimeSave"),
                    timeLastPostValue,

                    Safe(r, "IDFBPage"),
                    Safe(r, "PageMembers"),
                    Safe(r, "PageInteraction"),
                    Safe(r, "PageEvaluation"),
                    Safe(r, "PageInfoText"),
                    Safe(r, "PageType")
                );
            }

            // 👉 UI giữ nguyên
            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;

            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }

            gridView1.Columns["PageID"].Visible = false;
            gridView1.Columns["PageMembers"].Visible = false;
            gridView1.Columns["PageInteraction"].Visible = false;
            gridView1.Columns["PageEvaluation"].Visible = false;
            gridView1.Columns["PageInfoText"].Visible = false;
            gridView1.Columns["Thời gian lưu"].Visible = false;

            gridView1.BestFitColumns();

            if (currentTable.Rows.Count > 0)
                ShowPageDashboard(currentTable.Rows[0]);

            // 🔥 paging
            pager.Update(totalRows);
        }

        private void LoadPageFromDataTable(DataTable dt)
        {
            var list = new List<PageInfo>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new PageInfo
                {
                    PageID = r["PageID"] == DBNull.Value ? null : r["PageID"].ToString(),
                    PageName = r["PageName"] == DBNull.Value ? null : r["PageName"].ToString(),
                    PageLink = r["PageLink"] == DBNull.Value ? null : r["PageLink"].ToString(),
                    IDFBPage = r["IDFBPage"] == DBNull.Value ? null : r["IDFBPage"].ToString(),
                    PageMembers = r["PageMembers"] == DBNull.Value ? null : r["PageMembers"].ToString(),
                    PageInteraction = r["PageInteraction"] == DBNull.Value ? null : r["PageInteraction"].ToString(),
                    PageEvaluation = r["PageEvaluation"] == DBNull.Value ? null : r["PageEvaluation"].ToString(),
                    PageInfoText = r["PageInfoText"] == DBNull.Value ? null : r["PageInfoText"].ToString(),

                    PageType = r["PageType"] == DBNull.Value
                        ? FBType.Unknown
                        : Enum.TryParse<FBType>(r["PageType"].ToString(), true, out var t)
                            ? t
                            : FBType.Unknown,

                    PageTimeSave = r["PageTimeSave"] == DBNull.Value ? null : r["PageTimeSave"].ToString(),

                    IsScanned = r.Table.Columns.Contains("IsScanned") &&
                                r["IsScanned"] != DBNull.Value &&
                                Convert.ToInt32(r["IsScanned"]) == 1,

                    TimeLastPost = r["TimeLastPost"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(r["TimeLastPost"])
                });
            }

            LoadPageBase(list);
        }
        private void BindPostGrid(DataTable dt)
        {
            panelPostContainer.Visible = true;

            gridPost.DataSource = dt;

            gvPost.PopulateColumns();
            gvPost.OptionsView.ColumnAutoWidth = false;
            // ===== FONT =====
            gvPost.Appearance.Row.Font = new Font("Segoe UI", 9);
            gvPost.RowHeight = 22;

            // ===== WIDTH =====
            SetColWidth("STT", 50);
            SetColWidth("Địa chỉ", 80);
            SetColWidth("Thời gian", 140);
            SetColWidth("Like", 40);
            SetColWidth("Share", 40);
            SetColWidth("Comment", 40);

            // 🔥 Nội dung chiếm hết phần còn lại
            if (gvPost.Columns["Nội dung"] != null)
            {
                gvPost.Columns["Nội dung"].Width = 400;
                gvPost.Columns["Nội dung"].OptionsColumn.AllowSize = true;
            }

            // ===== ALIGN =====
            Center("STT");
            Center("Like");
            Center("Share");
            Center("Comment");
            Center("Thời gian");

            // ===== WRAP TEXT =====
            var col = gvPost.Columns.ColumnByFieldName("Nội dung");
            if (col != null)
            {
                col.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
                gvPost.OptionsView.RowAutoHeight = true;
            }

            // ===== HELPER =====
            void SetColWidth(string name, int w)
            {
                var column = gvPost.Columns.ColumnByFieldName(name); // 🔥 đổi tên
                if (column != null)
                {
                    column.Width = w;
                    column.OptionsColumn.FixedWidth = true;
                }
            }

            void Center(string name)
            {
                var column = gvPost.Columns.ColumnByFieldName(name); // 🔥 đổi tên
                if (column != null)
                {
                    column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                }
            }
            gvPost.OptionsBehavior.Editable = false;
            gvPost.OptionsSelection.EnableAppearanceFocusedCell = false;

         

            var col2 = gvPost.Columns.ColumnByFieldName("LinkThật");
            if (col2 != null)
                col2.Visible = false;
          
        }
        public void LoadSource(string type)
        {
            currentViewType = type;

            // giữ nguyên layout
            if (type == "PostPageNote"|| type == "PostPageMonitor" || type == "PostAllPage") // 🔥 thêm dòng này
            {
                panelControlGrid.Width = (int)(this.Width * 0.50);
                panelControlDetail.Width = (int)(this.Width * 0.50);
            }
            else
            {
                panelControlGrid.Width = (int)(this.Width * 0.60);
                panelControlDetail.Width = (int)(this.Width * 0.40);
            }

            // 👉 load trang 1
            LoadPage(1, pager.PageSize);
        }
        private void LoadPageBase(List<PageInfo> pages)
        {
            currentTable = new DataTable();

            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Địa chỉ");
            currentTable.Columns.Add("Thông tin");
            currentTable.Columns.Add("Thời gian lưu");
            currentTable.Columns.Add("TimeLastPost", typeof(DateTime));

            currentTable.Columns.Add("IDFBPage");
            currentTable.Columns.Add("PageMembers");
            currentTable.Columns.Add("PageInteraction");
            currentTable.Columns.Add("PageEvaluation");
            currentTable.Columns.Add("PageInfoText");
            currentTable.Columns.Add("PageType");

            int stt = 1;

            foreach (var p in pages)
            {
                currentTable.Rows.Add(
                    false,
                    stt++,
                    p.PageID,
                    p.PageName,
                    p.PageLink,
                    p.IsScanned ? "Đã quét" : "Chưa quét",
                    p.PageTimeSave,
                    p.TimeLastPost ?? (object)DBNull.Value,
                    p.IDFBPage,
                    p.PageMembers,
                    p.PageInteraction,
                    p.PageEvaluation,
                    p.PageInfoText,
                    p.PageType
                );
            }

            // ✅ FIX
            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;
            gridView1.BestFitColumns();
        }
        // ==================== PAGE INFO ======================     
        private void LoadPageInfo()
        {
            var dt = SQLDAO.Instance.GetAllPagesDB();   // SQL version
            object timeLastPostValue = DBNull.Value;
          
            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Địa chỉ");
            currentTable.Columns.Add("Thông tin");
            currentTable.Columns.Add("Thời gian lưu");
            currentTable.Columns.Add("TimeLastPost", typeof(DateTime));
            // Phần ẩn dùng dashboard
            currentTable.Columns.Add("IDFBPage");
            currentTable.Columns.Add("PageMembers");
            currentTable.Columns.Add("PageInteraction");
            currentTable.Columns.Add("PageEvaluation");
            currentTable.Columns.Add("PageInfoText");
            currentTable.Columns.Add("PageType");
 
            foreach (DataRow r in dt.Rows)
            {
                var rawTime = r["TimeLastPost"];
                if (rawTime != DBNull.Value)
                {
                    // Nếu còn dữ liệu legacy "N/A"
                    if (rawTime is string s && s.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase))
                    {
                        timeLastPostValue = DBNull.Value;
                    }
                    // Nếu là DateTime thật
                    else if (rawTime is DateTime date)
                    {
                        timeLastPostValue = date;
                    }
                    // Trường hợp khác (string nhưng parse được)
                    else if (DateTime.TryParse(rawTime.ToString(), out DateTime parsed))
                    {
                        timeLastPostValue = parsed;
                    }
                    else
                    {
                        timeLastPostValue = DBNull.Value;
                    }
                }
                currentTable.Rows.Add(
                     false,   // Select = false mặc định
                    Safe(r, "STT"),
                    Safe(r, "PageID"),
                    Safe(r, "PageName"),
                    Safe(r, "PageLink"),
                    Convert.ToInt32(Safe(r, "IsScanned")) == 1 ? "Đã quét" : "Chưa quét",
                    Safe(r, "PageTimeSave"),
                     timeLastPostValue,  
                    Safe(r, "IDFBPage"),
                    Safe(r, "PageMembers"),
                    Safe(r, "PageInteraction"),
                    Safe(r, "PageEvaluation"),
                    Safe(r, "PageInfoText"),
                    Safe(r, "PageType")
                );
            }
            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;
            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }
            gridView1.Columns["PageID"].Visible = false;
            gridView1.Columns["PageMembers"].Visible = false;
            gridView1.Columns["PageInteraction"].Visible = false;
            gridView1.Columns["PageEvaluation"].Visible = false;
            gridView1.Columns["PageInfoText"].Visible = false;
            gridView1.Columns["Thời gian lưu"].Visible = false;
            gridView1.BestFitColumns();
            if (currentTable.Rows.Count > 0)
                ShowPageDashboard(currentTable.Rows[0]);
        }
    
        // ==================== PAGE NOTE ======================
        private void LoadPageNote()
        {
            var notes = SQLDAO.Instance.GetAllPageNote();   // dạng DataTable
            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Địa chỉ");
            currentTable.Columns.Add("Thông tin");
            currentTable.Columns.Add("Thời gian lưu");
            currentTable.Columns.Add("TimeLastPost", typeof(DateTime));  // giống LoadPageInfo

            // dashboard ẩn
            currentTable.Columns.Add("IDFBPage");
            currentTable.Columns.Add("PageMembers");
            currentTable.Columns.Add("PageInteraction");
            currentTable.Columns.Add("PageEvaluation");
            currentTable.Columns.Add("PageInfoText");
            currentTable.Columns.Add("PageType");

            int stt = 1;

            foreach (DataRow r in notes.Rows)
            {
                string pageId = r["PageID"]?.ToString();
                string timeSave = r["TimeSave"]?.ToString();

                // 1. LẤY PAGEINFO TỪ PAGEID
                var info = SQLDAO.Instance.GetPageByID(pageId);

                if (info != null)
                {
                        currentTable.Rows.Add(
                        false,// cột select
                        stt++,
                        info.PageID,
                        info.PageName,
                        info.PageLink,
                        info.IsScanned ? "Đã quét" : "Chưa quét",
                        info.PageTimeSave,        // ⭐ HIỂN THỊ THỜI GIAN LƯU PAGEINFO
                         info.TimeLastPost.HasValue
                         ? (object)info.TimeLastPost.Value
                        : DBNull.Value,
                        // ⭐ lấy đúng TimeLastPost

                        info.IDFBPage,
                        info.PageMembers,
                        info.PageInteraction,
                        info.PageEvaluation,
                        info.PageInfoText,
                        info.PageType
                        );

                }
                else
                {
                    // Trường hợp pageInfo NULL
                    currentTable.Rows.Add(
                         false,
                        stt++,
                        pageId,
                        "(Không tìm thấy PageInfo)",
                        "",
                        "Chưa quét",
                        timeSave,
                        "",

                        "", "", "", "", "", ""
                    );
                }
            }
            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;
            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }
            // ẨN các cột dashboard giống LoadPageInfo
            gridView1.Columns["PageID"].Visible = false;
            gridView1.Columns["PageMembers"].Visible = false;
            gridView1.Columns["PageInteraction"].Visible = false;
            gridView1.Columns["PageEvaluation"].Visible = false;
            gridView1.Columns["PageInfoText"].Visible = false;
            gridView1.Columns["Thời gian lưu"].Visible = false;

            gridView1.BestFitColumns();

            if (currentTable.Rows.Count > 0)
                ShowPageDashboard(currentTable.Rows[0]);
        }

        // ==================== PAGE MONITOR ======================
        private void LoadPageMonitor()
        {
            var dt = SQLDAO.Instance.GetMonitoredPages();  // SQL version

            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Trạng thái");
            currentTable.Columns.Add("Lần quét gần nhất");

            // dashboard
            currentTable.Columns.Add("Địa chỉ");
            currentTable.Columns.Add("IDFBPage");
            currentTable.Columns.Add("PageMembers");
            currentTable.Columns.Add("PageInteraction");
            currentTable.Columns.Add("PageEvaluation");
            currentTable.Columns.Add("PageInfoText");
            currentTable.Columns.Add("PageType");
            currentTable.Columns.Add("IsScanned");
            currentTable.Columns.Add("PageTimeSave");

            int stt = 1;

            foreach (DataRow r in dt.Rows)
            {
                string pageId = r["PageID"].ToString();
                var info = SQLDAO.Instance.GetPageByID(pageId);

                if (info == null)
                {
                    currentTable.Rows.Add(
                        false,
                        stt++,
                        pageId,
                        r["PageName"],
                        r["Status"],
                        r["LastScanTime"],

                        "", "", "", "", "", "", "0", ""
                    );
                }
                else
                {
                    currentTable.Rows.Add(
                        stt++,
                        info.PageID,
                        info.PageName,
                        r["Status"],
                        r["LastScanTime"],

                        info.PageLink,
                        info.IDFBPage,
                        info.PageMembers,
                        info.PageInteraction,
                        info.PageEvaluation,
                        info.PageInfoText,
                        info.PageType,
                        info.IsScanned ? "1" : "0",
                        info.PageTimeSave
                    );
                }
            }
            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;
            // Thêm cột SELECT (nếu chưa có)
            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }
            gridView1.BestFitColumns();

            if (currentTable.Rows.Count > 0)
                ShowPageDashboard(currentTable.Rows[0]);
        }
        //phần post
        private void LoadPostAllPagePaging(int pageIndex, int pageSize)
        {
            int totalRows;

            var dt = SQLDAO.Instance.GetPageInfoPage(pageIndex, pageSize, out totalRows);

            BuildPostTable(dt, pageIndex, pageSize, totalRows);
        }
        private void LoadPostPageNotePaging(int pageIndex, int pageSize)
        {
            int totalRows;

            var dt = SQLDAO.Instance.GetPageNotePaging(pageIndex, pageSize, out totalRows);

            BuildPostTable(dt, pageIndex, pageSize, totalRows);
        }
        private void LoadPostPageMonitorPaging(int pageIndex, int pageSize)
        {
            int totalRows;

            var dt = SQLDAO.Instance.GetPageMonitorPaging(pageIndex, pageSize, out totalRows);

            BuildPostTable(dt, pageIndex, pageSize, totalRows);
        }
        private void BuildPostTable(DataTable dt, int pageIndex, int pageSize, int totalRows)
        {
            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Tổng bài viết");
            currentTable.Columns.Add("Lần quét gần nhất");

            int stt = (pageIndex - 1) * pageSize + 1;

            // 🔥 lấy list PageID
            var pageIds = dt.AsEnumerable()
                .Select(r => r["PageID"].ToString())
                .ToList();

            // 🔥 batch 1 query
            var postStats = SQLDAO.Instance.GetPostStatsBatch(pageIds);

            foreach (DataRow r in dt.Rows)
            {
                string pageId = r["PageID"].ToString();

                var info = PageInfoCacheService.Get(pageId);

                int totalPost = 0;
                DateTime? lastPost = null;

                if (postStats.ContainsKey(pageId))
                {
                    totalPost = postStats[pageId].TotalPost;
                    lastPost = postStats[pageId].LastPostTime;
                }

                string timeText = TimeHelper.NormalizeTime(lastPost);

                if (info != null)
                {
                    currentTable.Rows.Add(
                        false,
                        stt++,
                        pageId,
                        info.PageName,
                        totalPost,
                        timeText
                    );
                }
            }

            // UI
            gridView1.Columns.Clear();
            gridControl1.DataSource = currentTable;

            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }

            gridView1.BestFitColumns();

            if (currentTable.Rows.Count > 0)
                ShowPostPageDashbroad(currentTable.Rows[0]);

            pager.Update(totalRows);
        }
        private void LoadPostPageNote()
        {
            var dt = SQLDAO.Instance.GetAllPageNote();  // SQL version
            currentTable = new DataTable();
            currentTable.Columns.Add("Select", typeof(bool));
            currentTable.Columns.Add("STT");
            currentTable.Columns.Add("PageID");
            currentTable.Columns.Add("Tên Page");
            currentTable.Columns.Add("Tổng bài viết");
            currentTable.Columns.Add("Lần quét gần nhất");          
            int stt = 1;
            foreach (DataRow r in dt.Rows)
            {
                string pageId = r["PageID"].ToString();
                var info = SQLDAO.Instance.GetPageByID(pageId);
                string SumPost = SQLDAO.Instance.CountPostsByPage(pageId).ToString();
                DateTime? TimelastPost = SQLDAO.Instance.GetTimeLastPost(pageId);
                string PostTimeNew = TimeHelper.NormalizeTime(TimelastPost);
                if (info != null)
                {
                    currentTable.Rows.Add(
                        false,
                        stt++,
                        pageId,
                       info.PageName,
                        SumPost,
                        PostTimeNew
                        );
                }               
            }
            gridView1.Columns.Clear();// clear tránh lỗi vì dùng chung gridview bên trái
            // ⭐⭐⭐ QUAN TRỌNG: GÁN DATASOURCE
            gridControl1.DataSource = currentTable;
            // Thêm cột SELECT (nếu chưa có)
            if (!gridView1.Columns.Contains(gridView1.Columns.ColumnByFieldName("Select")))
            {
                var col = gridView1.Columns.AddVisible("Select", "Chọn");
                col.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                col.OptionsColumn.AllowEdit = true;
                col.Width = 50;
            }
            gridView1.BestFitColumns();
           if (currentTable.Rows.Count > 0)
                ShowPostPageDashbroad(currentTable.Rows[0]);
        }
        private void ShowPostPageDashbroad(DataRow r)
        {
            string pageID = r["PageID"].ToString();

            _currentPostPageId = pageID;
            _postPageIndex = 1;

            panelControlDetail.Controls.Clear(); // 🔥 FIX

            panelControlDetail.Controls.Add(panelPostContainer);
            panelPostContainer.Visible = true;

            LoadPostPaging(pageID, _postPageIndex, _postPageSize);
        }
        //pageAdd

        // ==================== DASHBOARD ======================
        private void GridView1_RowClick(object sender, RowClickEventArgs e)
        {
            var row = gridView1.GetDataRow(e.RowHandle);
            if (row == null) return;

            switch (currentViewType)
            {
                case "PostPageNote":
                    ShowPostPageDashbroad(row);
                    break;

                case "PostPageMonitor":
                    ShowPostPageDashbroad(row);
                    break;
                case "PostAllPage":   
                    ShowPostPageDashbroad(row);
                    break;
                default:
                    ShowPageDashboard(row);
                    break;
            }
        }
        private void GridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0) return;

            var row = gridView1.GetDataRow(e.FocusedRowHandle);
            if (row == null) return;

            switch (currentViewType)
            {
                case "PostPageNote":
                    ShowPostPageDashbroad(row);
                    break;

                case "PostPageMonitor":
                    ShowPostPageDashbroad(row);
                    break;
                case "PostAllPage":  
                    ShowPostPageDashbroad(row);
                    break;
                default:
                    ShowPageDashboard(row);
                    break;
            }
        }
       
        // ==================== BUILD DASHBOARD ======================
        private void ShowPageDashboard(DataRow r)
        {
            panelControlDetail.Controls.Clear();
            panelControlDetail.Padding = new Padding(10);
            panelControlDetail.AutoScroll = true;

            // CONTAINER chứa toàn bộ UI
            FlowLayoutPanel container = new FlowLayoutPanel();
            container.FlowDirection = FlowDirection.TopDown;
            container.WrapContents = false;
            container.AutoSize = true;
            container.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            container.Dock = DockStyle.Top;
            container.Padding = new Padding(0);

            string pageID = r["PageID"].ToString();
            var stats = SQLDAO.Instance.GetPageStats(pageID);
            // ========== TITLE ==========
            var title = new Label()
            {
                Text = r["Tên Page"].ToString(),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 5)
            };
            container.Controls.Add(title);

            // ========== CARD DASHBOARD ==========
            FlowLayoutPanel wrap = new FlowLayoutPanel(); 
            wrap.Dock = DockStyle.Top; 
            wrap.FlowDirection = FlowDirection.LeftToRight;
            wrap.WrapContents = true; wrap.AutoSize = true; 
            wrap.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            wrap.AutoScroll = false; 
            wrap.MaximumSize = new Size(panelControlDetail.Width - 20, 0);

            cardFollowers = BuildCard("Theo dõi", stats.Followers.ToString(), iconDir + "folow.png", Color.FromArgb(52, 168, 83));
            cardLikes = BuildCard("Like", stats.TotalLikes.ToString(), iconDir + "like.png", Color.FromArgb(199, 161, 122));
            cardShares = BuildCard("Share", stats.TotalShares.ToString(), iconDir + "share.png", Color.FromArgb(234, 67, 53));
            cardComments = BuildCard("Comment", stats.TotalComments.ToString(), iconDir + "tinnhan.png", Color.FromArgb(142, 36, 170));
            cardPosts = BuildCard("Bài viết", stats.TotalPosts.ToString(), iconDir + "baiviet.png", Color.FromArgb(251, 188, 5));

            var cardLastPost = BuildCard("Last Post",
           CountDays(r["TimeLastPost"]),
            iconDir + "Time_32x32.png",
            Color.FromArgb(66, 133, 244),
            new Font("Segoe UI", 8, FontStyle.Regular));   // ⭐ HERE

            wrap.Controls.Add(cardFollowers);
            wrap.Controls.Add(cardLikes);
            wrap.Controls.Add(cardShares);
            wrap.Controls.Add(cardComments);
            wrap.Controls.Add(cardPosts);
            wrap.Controls.Add(cardLastPost);

            container.Controls.Add(wrap);

            // ========== FILTER BAR (dưới card) ==========
            FlowLayoutPanel filterBar = new FlowLayoutPanel();
            filterBar.FlowDirection = FlowDirection.LeftToRight;
            filterBar.AutoSize = true;
            filterBar.Padding = new Padding(0, 5, 0, 5);

            Button btn7d = new Button() { Text = "1 tuần", Width = 80, Height = 26 };
            Button btn15d = new Button() { Text = "15 ngày", Width = 80, Height = 26 };
            Button btn1m = new Button() { Text = "1 tháng", Width = 80, Height = 26 };
            Button btn3m = new Button() { Text = "3 tháng", Width = 80, Height = 26 };
            Button btn6m = new Button() { Text = "6 tháng", Width = 80, Height = 26 };
            Button btn1y = new Button() { Text = "1 năm", Width = 80, Height = 26 };
            Button btnAll = new Button() { Text = "All time", Width = 80, Height = 26 };

            btn7d.Click += (s, e) => FilterPosts(pageID, 7);
            btn15d.Click += (s, e) => FilterPosts(pageID, 15);
            btn1m.Click += (s, e) => FilterPosts(pageID, 30);
            btn3m.Click += (s, e) => FilterPosts(pageID, 90);
            btn6m.Click += (s, e) => FilterPosts(pageID, 180);
            btn1y.Click += (s, e) => FilterPosts(pageID, 365);
            btnAll.Click += (s, e) => FilterPosts(pageID, 0);

            filterBar.Controls.Add(btn7d);
            filterBar.Controls.Add(btn15d);
            filterBar.Controls.Add(btn1m);
            filterBar.Controls.Add(btn3m);
            filterBar.Controls.Add(btn6m);
            filterBar.Controls.Add(btn1y);
            filterBar.Controls.Add(btnAll);

            container.Controls.Add(filterBar);

            // ========== DETAIL LIST (TableLayout 2 cột) ==========
            TableLayoutPanel detailTable = new TableLayoutPanel();
            detailTable.ColumnCount = 2;
            detailTable.AutoSize = true;
            detailTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            detailTable.Dock = DockStyle.Top;
            detailTable.Padding = new Padding(0, 10, 0, 0);

            detailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            detailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            void AddDetail(string name, string value)
            {
                int row = detailTable.RowCount++;

                detailTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var lbl = new Label()
                {
                    Text = name + ":",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    AutoSize = true,
                    Padding = new Padding(0, 4, 0, 4)
                };

                var val = new Label()
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    MaximumSize = new Size(panelControlDetail.Width - 180, 0),
                    Padding = new Padding(0, 4, 0, 4)
                };

                detailTable.Controls.Add(lbl, 0, row);
                detailTable.Controls.Add(val, 1, row);
            }
            AddDetail("ID FB", Safe(r, "IDFBPage"));
            AddDetail("Trạng thái", Safe(r, "Thông tin"));
            AddDetail("Lưu lúc", Safe(r, "Thời gian lưu"));
            AddDetail("Loại Page", Safe(r, "PageType"));
            AddDetail("Link Page", Safe(r, "Địa chỉ"));
            AddDetail("PageID", Safe(r, "PageID"));
            container.Controls.Add(detailTable);
            // ========== PAGE INFO TEXT ==========
            var desc = new Label()
            {
                Text = Safe(r, "PageInfoText"),
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                MaximumSize = new Size(panelControlDetail.Width - 40, 0),
                Padding = new Padding(0, 10, 0, 0)
            };
            container.Controls.Add(desc);
            // ADD TO PANEL
            panelControlDetail.Controls.Add(container);
        }

        //============SHOW POST

        private void LoadPostPaging(string pageId, int pageIndex, int pageSize)
        {
            int total;

            var dto = SQLDAO.Instance.GetPostsByPagePaging(pageId, pageIndex, pageSize, out total);

            var dt = BuildPostTable(dto, pageIndex, pageSize);

            _postPageSize = pageSize;          // 🔥 PHẢI ĐẶT TRƯỚC

            BindPostGrid(dt);
            UpdatePostPagingUI(total);
            Console.WriteLine($"TOTAL={total}, SIZE={pageSize}");
        }
        private void UpdatePostPagingUI(int totalRows)
        {
            _postTotalRows = totalRows;

            int totalPages = Math.Max(1,
                (int)Math.Ceiling((double)_postTotalRows / _postPageSize));

            if (_postPageIndex > totalPages)
                _postPageIndex = totalPages;

            lblPostPage.Text = $"Trang {_postPageIndex} / {totalPages}";

            // 🔥 sync textbox
            if (txtPageJump != null)
                txtPageJump.Text = _postPageIndex.ToString();

            btnFirst.Enabled = _postPageIndex > 1;
            btnPrev.Enabled = _postPageIndex > 1;

            btnNext.Enabled = _postPageIndex < totalPages;
            btnLast.Enabled = _postPageIndex < totalPages;
        }
        private DataTable BuildPostTable(List<PostPage> posts, int pageIndex, int pageSize)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("STT", typeof(int));
            dt.Columns.Add("Địa chỉ", typeof(string));
            dt.Columns.Add("LinkThật", typeof(string));
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Nội dung", typeof(string));
            dt.Columns.Add("Like", typeof(int));
            dt.Columns.Add("Share", typeof(int));
            dt.Columns.Add("Comment", typeof(int));
            int i = 0;
            foreach (var p in posts)
            {
                dt.Rows.Add(
                    (pageIndex - 1) * pageSize + (++i),
                    "Xem link",
                    p.PostLink,
                    p.RealPostTime,
                    p.Content,
                    p.LikeCount ?? 0,
                    p.ShareCount ?? 0,
                    p.CommentCount ?? 0
                );
            }
            return dt;
        }
        // ==================== UI HELPERS ======================
        // buil đã đẩy dữ liệu vào luôn
        private Panel BuildCard(string title, string value, string icon, Color bg, Font valueFont = null)
        {
            Panel card = new Panel();
            card.Width = 160;
            card.Height = 70;
            card.Margin = new Padding(10);
            card.Padding = new Padding(8);
            card.BackColor = bg;
            card.BorderStyle = BorderStyle.FixedSingle;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.ColumnCount = 2;
            layout.RowCount = 2;
            layout.Dock = DockStyle.Fill;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            PictureBox pic = new PictureBox();
            pic.Image = Image.FromFile(icon);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Dock = DockStyle.Fill;

            // ⭐ FONT VALUE tùy chọn (nếu null thì dùng font mặc định bold 13)
            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = valueFont ?? new Font("Segoe UI", 13, FontStyle.Bold);
            lblValue.ForeColor = Color.White;
            lblValue.Dock = DockStyle.Fill;
            lblValue.TextAlign = ContentAlignment.MiddleLeft;

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 9);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.MaximumSize = new Size(110, 0);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            layout.Controls.Add(pic, 0, 0);
            layout.SetRowSpan(pic, 2);

            layout.Controls.Add(lblValue, 1, 0);
            layout.Controls.Add(lblTitle, 1, 1);

            card.Controls.Add(layout);

            return card;
        }
        private Panel BuildDetail(string label, string value)
        {
            Panel p = new Panel();
            p.Height = 26;
            p.Dock = DockStyle.Top;

            Label lbl = new Label()
            {
                Text = label + ":",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Width = 140,
                Dock = DockStyle.Left
            };

            Label val = new Label()
            {
                Text = value,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill
            };

            p.Controls.Add(val);
            p.Controls.Add(lbl);
            return p;
        }
        private void UpdateCard(Panel card, string newValue)
        {
            foreach (Control c in card.Controls)
            {
                if (c is TableLayoutPanel layout)
                {
                    foreach (Control child in layout.Controls)
                    {
                        if (child is Label lbl && lbl.Font.Size >= 13) // lblValue
                        {
                            lbl.Text = newValue;
                            return;
                        }
                    }
                }
            }
        }
        private void FilterPosts(string pageID, int days)
        {
            DateTime from = DateTime.Now.AddDays(-days);
            var posts = SQLDAO.Instance.GetPostsByPage(pageID);
            List<PostPage> filtered;
            if (days > 0)
            {
                filtered = posts
                    .Where(p => DateTime.TryParse(p.PostTime, out DateTime t) && t >= from)
                    .ToList();
            }
            else
            {
                filtered = posts;   // ALL TIME
            }
            int totalLikes = filtered.Sum(p => p.LikeCount ?? 0);
            int totalShares = filtered.Sum(p => p.ShareCount ?? 0);
            int totalComments = filtered.Sum(p => p.CommentCount ?? 0);
            int totalPosts = filtered.Count(); // ✔ đúng
            // ⭐ UPDATE CARD VALUE
            UpdateCard(cardLikes, totalLikes.ToString());
            UpdateCard(cardShares, totalShares.ToString());
            UpdateCard(cardComments, totalComments.ToString());
            UpdateCard(cardPosts, totalPosts.ToString());
        }
        //====== HẾT PHẦN BUILD GIAO DIỆN CARD
        private string Safe(DataRow r, string col)
        {
            return r.Table.Columns.Contains(col) ? r[col].ToString() : "N/A";
        }
        private string CountDays(object value)
        {
            if (value == null || value == DBNull.Value)
                return "N/A";

            if (!(value is DateTime dt))
                return "N/A";

            int days = (DateTime.Now.Date - dt.Date).Days;
            string dateText = dt.ToString("dd/MM/yyyy");

            if (days <= 0) return $"Hôm nay ({dateText})";
            if (days == 1) return $"Hôm qua ({dateText})";

            return $"{days} ngày trước ({dateText})";
        }
     
        private void btnSelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataTable dt = gridControl1.DataSource as DataTable;
            if (dt == null) return;

            foreach (DataRow r in dt.Rows)
                r["Select"] = true;

            gridView1.RefreshData();
        }
        private void barButtonItem1removeSelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataTable dt = gridControl1.DataSource as DataTable;
            if (dt == null) return;

            foreach (DataRow r in dt.Rows)
                r["Select"] = false;

            gridView1.RefreshData();
        }
        /// <summary>
        /// Thu thập link được chọn + chọn profile + chia link theo profile.
        /// Trả về: Dictionary<ProfileDB, List<(PageID, PageLink)>>
        /// </summary>
        private Dictionary<ProfileDB, List<(string PageID, string PageLink)>> CollectAndDistributePages()
        {
            // ===== 1) THU THẬP PAGE ĐƯỢC CHỌN =====
            if (currentTable == null || !currentTable.Columns.Contains("Select"))
                throw new Exception("Grid chưa có cột Select!");

            var selectedPages = new List<(string PageID, string PageLink)>();

            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                var row = gridView1.GetDataRow(i);
                if (row == null) continue;

                bool selected = row["Select"] != DBNull.Value && (bool)row["Select"];
                if (!selected) continue;

                string pageId = row["PageID"]?.ToString();
                string pageLink = row["Địa chỉ"]?.ToString();

                if (string.IsNullOrEmpty(pageId) && string.IsNullOrEmpty(pageLink))
                    continue;

                selectedPages.Add((pageId, pageLink));
            }

            if (selectedPages.Count == 0)
                throw new Exception("Chưa chọn Page nào!");

            // ===== 2) CHỌN PROFILE =====
            var frm = new CrawlFB_PW._1._0.Profile.SelectProfileDB();
            if (frm.ShowDialog() != DialogResult.OK)
                throw new Exception("Bạn chưa chọn profile!");

            var selectedProfiles = frm.Tag as List<ProfileDB>;
            if (selectedProfiles == null || selectedProfiles.Count == 0)
                throw new Exception("Không nhận được danh sách profile!");

            // Chỉ lấy profile live
            var profiles = selectedProfiles.Where(p => p.ProfileStatus == "Live").ToList();
            if (profiles.Count == 0)
                throw new Exception("Không có profile LIVE!");

            // ===== 3) CHIA PAGE → PROFILE (ROUND ROBIN) =====
            var result = new Dictionary<ProfileDB, List<(string PageID, string PageLink)>>();

            foreach (var p in profiles)
                result[p] = new List<(string, string)>();

            int index = 0;

            foreach (var p in selectedPages)
            {
                result[profiles[index]].Add((p.PageID, p.PageLink));
                index = (index + 1) % profiles.Count;
            }

            return result;
        }

        private async void barButtonItemUpdateLastTimePost_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // ⭐ DÙNG HÀM DUY NHẤT CỦA BẠN
                var buckets = CollectAndDistributePages();

                var tasks = new List<Task>();

                foreach (var kv in buckets)
                {
                    var profile = kv.Key;
                    var pages = kv.Value;

                    tasks.Add(Task.Run(async () =>
                    {
                        var tab = await AdsPowerPlaywrightManager.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                        if (tab == null) return;

                        foreach (var page in pages)
                        {
                            try
                            {
                                await tab.GotoAsync(page.PageLink);
                                await tab.WaitForTimeoutAsync(1200);

                                DateTime? lastPost = await ScanCheckPageDAO.Instance.GetPostTimeAsync(tab);
                                Libary.Instance.CreateLog("LastPost: " +lastPost);
                                // Update DB
                                SQLDAO.Instance.ExecuteNonQuery(
                                    "UPDATE TablePageInfo SET TimeLastPost=@t WHERE PageID=@id",
                                    new Dictionary<string, object>
                                    {
                                { "@t", lastPost },
                                { "@id", page.PageID }
                                    });

                                this.Invoke(new Action(() =>
                                {
                                    foreach (DataRow r in currentTable.Rows)
                                        if (r["PageID"].ToString() == page.PageID)
                                            r["TimeLastPost"] = lastPost;
                                }));

                            }
                            catch { }
                        }

                        try { await tab.CloseAsync(); } catch { }
                    }));
                }
                await Task.WhenAll(tasks);
                gridView1.RefreshData();
                MessageBox.Show("✔ Cập nhật TimeLastPost xong!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi: " + ex.Message);
            }
        }
        private async void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // --- 1. Thu thập hàng được chọn ---
                if (currentTable == null || !currentTable.Columns.Contains("Select"))
                {
                    MessageBox.Show("⚠ Grid chưa có cột Select. Vui lòng bật cột chọn trước.");
                    return;
                }

                var selectedRows = new List<(string PageID, string PageLink, int RowHandle)>();

                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    var row = gridView1.GetDataRow(i);
                    if (row == null) continue;

                    bool isSelected = false;
                    try
                    {
                        isSelected = row.Table.Columns.Contains("Select") && row["Select"] != DBNull.Value && (bool)row["Select"];
                    }
                    catch { isSelected = false; }
                    if (!isSelected) continue;
                    string pageId = row.Table.Columns.Contains("PageID") ? row["PageID"]?.ToString() : null;
                    string pageLink = row.Table.Columns.Contains("Địa chỉ") ? row["Địa chỉ"]?.ToString() : null;
                    // Some tables may use "PageLink" column name; try fallback:
                    if (string.IsNullOrEmpty(pageLink) && row.Table.Columns.Contains("PageLink"))
                        pageLink = row["PageLink"]?.ToString();
                    if (string.IsNullOrEmpty(pageId) && string.IsNullOrEmpty(pageLink))
                        continue;
                    selectedRows.Add((pageId, pageLink, i));
                }
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("⚠ Chưa chọn page nào để cập nhật.");
                    return;
                }
                // --- 2. Chọn profile(s) ---
                var frm = new CrawlFB_PW._1._0.Profile.SelectProfileDB();
                if (frm.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn profile!");
                    return;
                }
                var selectedProfiles = frm.Tag as List<ProfileDB>;
                if (selectedProfiles == null || selectedProfiles.Count == 0)
                {
                    MessageBox.Show("⚠ Không nhận được profile!");
                    return;
                }
                // Lọc profile Live (hoặc lấy tất cả đã chọn nếu bạn muốn). Dùng Live như yêu cầu.
                List<ProfileDB> profiles = selectedProfiles.Where(p => p.ProfileStatus == "Live").ToList();
                if (profiles.Count == 0)
                {
                    MessageBox.Show("⚠ Không có profile LIVE!");
                    return;
                }
                // --- 3. Phân chia selectedRows cho profiles (round-robin) ---
                var buckets = new Dictionary<ProfileDB, List<(string PageID, string PageLink, int RowHandle)>>();
                for (int i = 0; i < profiles.Count; i++) buckets[profiles[i]] = new List<(string, string, int)>();
                int idx = 0;
                foreach (var item in selectedRows)
                {
                    buckets[profiles[idx]].Add(item);
                    idx = (idx + 1) % profiles.Count;
                }
                // --- 4. Tạo task cho mỗi profile (mỗi task mở 1 tab) ---
                var tasks = new List<Task>();
                foreach (var kv in buckets)
                {
                    var profile = kv.Key;
                    var pagesForProfile = kv.Value;

                    if (pagesForProfile.Count == 0) continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        IPage tab = null;
                        try
                        {
                            tab = await AdsPowerPlaywrightManager.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                            if (tab == null)
                            {
                                Libary.Instance.CreateLog($"[UpdateLastPost] ❌ Mở tab thất bại cho profile {profile.IDAdbrowser}");
                                return;
                            }

                            foreach (var pageItem in pagesForProfile)
                            {
                                try
                                {
                                    // mark running on UI thread
                                    this.Invoke((Action)(() =>
                                    {
                                        gridView1.RefreshRow(pageItem.RowHandle);
                                    }));

                                    // Nếu có link -> điều hướng, nếu không có link nhưng có PageID -> load bằng PageID nếu bạn có cách mapping
                                    if (!string.IsNullOrEmpty(pageItem.PageLink))
                                    {
                                        try
                                        {
                                            await tab.GotoAsync(pageItem.PageLink, new Microsoft.Playwright.PageGotoOptions { WaitUntil = Microsoft.Playwright.WaitUntilState.Load, Timeout = 30000 });
                                            await tab.WaitForTimeoutAsync(1500);
                                        }
                                        catch
                                        {
                                            // cố gắng tiếp tục nếu goto fail
                                        }
                                    }

                                    // Gọi hàm GetPostTimeAsync (hàm bạn đã có)
                                    FBType type = await PageDAO.Instance.CheckFBTypeAsync(tab);
                                    string pagetype = type.ToString();

                                    // Nếu trả về "N/A" -> không update DB, nhưng vẫn show
                                    if (!string.IsNullOrEmpty(pagetype) && pagetype != "N/A")
                                    {
                                        // Cập nhật DB: TimeLastPost trong TablePageInfo
                                        try
                                        {
                                            SQLDAO.Instance.ExecuteNonQuery(
                                                "UPDATE TablePageInfo SET PageType=@t WHERE PageID=@id",
                                                new Dictionary<string, object>
                                                {
                                            { "@t", pagetype },
                                            { "@id", pageItem.PageID ?? pageItem.PageLink } // nếu PageID null thì fallback PageLink
                                                }
                                            );
                                        }
                                        catch (Exception exDb)
                                        {
                                            Libary.Instance.CreateLog("[UpdateLastPost DB] " + exDb.Message);
                                        }
                                    }

                                    // Cập nhật UI (on UI thread)
                                    this.Invoke((Action)(() =>
                                    {
                                        // cập nhật cell TimeLastPost nếu tồn tại
                                        if (gridView1.DataRowCount > pageItem.RowHandle && pageItem.RowHandle >= 0)
                                        {
                                            var row = gridView1.GetDataRow(pageItem.RowHandle);
                                            if (row != null && row.Table.Columns.Contains("PageType"))
                                                row["PageType"] = pagetype ?? "N/A";
                                        }
                                        gridView1.RefreshRow(pageItem.RowHandle);
                                    }));
                                }
                                catch (Exception exItem)
                                {
                                    Libary.Instance.CreateLog("[UpdateLastPost item] " + exItem.Message);
                                    this.Invoke((Action)(() =>
                                    {
                                        gridView1.RefreshRow(pageItem.RowHandle);
                                    }));
                                }
                            } // foreach pageItem
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.CreateLog("[UpdateLastPost profile task] " + ex.Message);
                        }
                        finally
                        {
                            // đóng tab nếu bạn muốn (nếu API có hỗ trợ). Nếu không muốn đóng, bỏ qua.
                            try
                            {
                                if (tab != null)
                                    await tab.CloseAsync();
                            }
                            catch { }
                        }
                    }));
                } // foreach bucket

                // --- 5. Chờ tất cả xong ---
                await Task.WhenAll(tasks);
                LoadPageInfo();
                MessageBox.Show("✔ Cập nhật TimeLastPost xong!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi: " + ex.Message);
            }
        }
        private void barButtonItemUpdateNameAndID_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }
        private void barCheckItemTimeUnder10_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ApplyAllFilters();
        }

        private void barCheckItemTimeUnder60_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ApplyAllFilters();
        }
        private void barCheckItemUnder90_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ApplyAllFilters();
        }
        private void FilterGridByLastPost(int maxDays)
        {
            if (currentTable == null) return;

            DataTable filtered = currentTable.Clone();

            foreach (DataRow r in currentTable.Rows)
            {
                int d = GetDaysDiff(r["TimeLastPost"]?.ToString());
                if (d <= maxDays)
                    filtered.ImportRow(r);
            }

            gridControl1.DataSource = filtered;
        }
     
        private void barCheckItemRemoveGroupoff_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ApplyAllFilters();
        }
        //============ ÁP DỤNG THAY ĐỔI CHO THẺ
        private void ApplyAllFilters()
        {
            if (currentTable == null) return;

            DataTable filtered = currentTable.Clone();

            // --- LẤY TRẠNG THÁI CỦA FILTER ---
            bool removeGroupOff = barCheckItemRemoveGroupoff.Checked;
            int maxDays = 99999; // mặc định không giới hạn ngày
            if (barCheckItemTimeUnder10.Checked) maxDays = 10;
            else if (barCheckItemUnder60.Checked) maxDays = 60;
            else if (barCheckItemUnder90.Checked) maxDays = 90;
            else if (barCheckItemTimeUnder30.Checked) maxDays = 30;
            foreach (DataRow r in currentTable.Rows)
            {
                // --- 1. FILTER PAGETYPE ---
                if (removeGroupOff)
                {
                    string pageType = r["PageType"]?.ToString();
                    if (!string.IsNullOrEmpty(pageType) &&
                        pageType.Equals("GroupOff", StringComparison.OrdinalIgnoreCase))
                        continue;   // bỏ qua row GroupOff
                }
                // --- 2. FILTER THEO NGÀY ĐĂNG ---
                int days = GetDaysDiff(r["TimeLastPost"]);
                if (days > maxDays)
                    continue;

                filtered.ImportRow(r);
            }

            gridControl1.DataSource = filtered;
        }

       
        //===============HÀM EXPORT
        private void barButtonItem1_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // ===== 1. LẤY PAGE ĐƯỢC CHỌN =====
                if (currentTable == null)
                {
                    MessageBox.Show("⚠ Không có dữ liệu!");
                    return;
                }

                var selectedPageIDs = new List<string>();

                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    var row = gridView1.GetDataRow(i);
                    if (row == null) continue;

                    bool isChecked = row["Select"] != DBNull.Value && (bool)row["Select"];
                    if (!isChecked) continue;

                    string pid = row["PageID"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(pid))
                        selectedPageIDs.Add(pid);
                }

                if (selectedPageIDs.Count == 0)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn Page nào!");
                    return;
                }

                // ===== 2. CHỌN NƠI LƯU FILE =====
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Excel File (*.xlsx)|*.xlsx";
                sfd.FileName = "PageNote_Export.xlsx";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                string filePath = sfd.FileName;

                // ===== 3. TẠO EXCEL =====
                using (var wb = new ClosedXML.Excel.XLWorkbook())
                {
                    var ws = wb.AddWorksheet("PageNote");

                    // Header
                    string[] header = new[]
                    {
                "STT","Tên Page","Link","ID FB","Tổng bài viết",
                "Từ thời gian","Đến thời gian",
                "Like","Share","Comment","Tổng tương tác"
            };

                    for (int i = 0; i < header.Length; i++)
                    {
                        var cell = ws.Cell(1, i + 1);
                        cell.Value = header[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Blue;
                        cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(235, 241, 255);
                    }

                    int rowIdx = 2;
                    int stt = 1;

                    foreach (string pid in selectedPageIDs)
                    {
                        var info = SQLDAO.Instance.GetPageByID(pid);
                        if (info == null) continue;

                        var posts = SQLDAO.Instance.GetPostsByPage(pid);

                        // Tổng số liệu
                        int totalPosts = posts.Count;
                        int totalLikes = posts.Sum(p => p.LikeCount ?? 0);
                        int totalShares = posts.Sum(p => p.ShareCount ?? 0);
                        int totalComments = posts.Sum(p => p.CommentCount ?? 0);
                        int totalInteract = totalLikes + totalShares + totalComments;

                        // Tính thời gian
                        var validTimes = posts
                            .Where(p => DateTime.TryParse(p.PostTime, out _))
                            .Select(p => DateTime.Parse(p.PostTime))
                            .ToList();

                        DateTime? newest = null;
                        DateTime? oldest = null;

                        if (validTimes.Count > 0)
                        {
                            newest = validTimes.Max();
                            oldest = validTimes.Min();
                        }

                        // ===== 4. GHI DỮ LIỆU =====
                        ws.Cell(rowIdx, 1).Value = stt++;
                        ws.Cell(rowIdx, 2).Value = info.PageName;

                        // Link clickable
                        ws.Cell(rowIdx, 3).FormulaA1 = $"HYPERLINK(\"{info.PageLink}\", \"Link\")";

                        ws.Cell(rowIdx, 4).Value = info.IDFBPage;
                        ws.Cell(rowIdx, 5).Value = totalPosts;

                        ws.Cell(rowIdx, 6).Value = newest?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                        ws.Cell(rowIdx, 7).Value = oldest?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";


                        ws.Cell(rowIdx, 8).Value = totalLikes;
                        ws.Cell(rowIdx, 9).Value = totalShares;
                        ws.Cell(rowIdx, 10).Value = totalComments;
                        ws.Cell(rowIdx, 11).Value = totalInteract;

                        rowIdx++;
                    }

                    // ===== 5. FORMAT TOÀN BỘ SHEET =====
                    var used = ws.RangeUsed();

                    // Font chung
                    used.Style.Font.FontName = "Times New Roman";
                    used.Style.Font.FontSize = 9;

                    // Kẻ viền
                    used.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    used.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                    // Căn giữa header
                    ws.Range(1, 1, 1, header.Length).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    // Căn giữa STT + số liệu
                    ws.Range(2, 1, rowIdx - 1, header.Length)
                        .Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    // Căn trái tên Page
                    ws.Column(2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Left;

                    // Tự fit cột
                    ws.Columns().AdjustToContents();

                    wb.SaveAs(filePath);
                }

                MessageBox.Show("✔ Xuất Excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xuất Excel: " + ex.Message);
            }
        }

        private void barButtonItemExportAllPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (currentTable == null)
                {
                    MessageBox.Show("⚠ Không có dữ liệu!");
                    return;
                }

                // ===== 1. Lấy danh sách Page ID được chọn =====
                var selectedPageIDs = new List<string>();

                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    var row = gridView1.GetDataRow(i);
                    if (row == null) continue;

                    bool isChecked = row["Select"] != DBNull.Value && (bool)row["Select"];
                    if (!isChecked) continue;

                    string pid = row["PageID"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(pid))
                        selectedPageIDs.Add(pid);
                }

                if (selectedPageIDs.Count == 0)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn Page nào!");
                    return;
                }

                // ===== 2. Chọn nơi lưu =====
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Excel File (*.xlsx)|*.xlsx";
                sfd.FileName = "Export_All_Page.xlsx";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                string filePath = sfd.FileName;

                // ===== 3. Tạo workbook =====
                using (var wb = new ClosedXML.Excel.XLWorkbook())
                {
                    var ws = wb.AddWorksheet("Data Tổng");

                    // ===== HEADER SHEET TỔNG =====
                    string[] header = new[]
                    {
                "STT","Tên Page","Link","ID FB","Tổng bài viết",
                "Từ thời gian","Đến thời gian",
                "Like","Share","Comment","Tổng tương tác"
            };

                    for (int i = 0; i < header.Length; i++)
                    {
                        var cell = ws.Cell(1, i + 1);
                        cell.Value = header[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                    }

                    int rowIdx = 2;
                    int stt = 1;

                    // ===== 4. Vòng lặp từng Page =====
                    foreach (string pid in selectedPageIDs)
                    {
                        var info = SQLDAO.Instance.GetPageByID(pid);
                        if (info == null) continue;

                        var posts = SQLDAO.Instance.GetPostsByPage(pid);

                        // ===== Tính tổng =====
                        int totalPosts = posts.Count;
                        int totalLikes = posts.Sum(p => p.LikeCount ?? 0);
                        int totalShares = posts.Sum(p => p.ShareCount ?? 0);
                        int totalComments = posts.Sum(p => p.CommentCount ?? 0);
                        int totalInteract = totalLikes + totalShares + totalComments;

                        // ===== Thời gian =====
                        var validTimes = posts
                            .Where(p => DateTime.TryParse(p.PostTime, out _))
                            .Select(p => DateTime.Parse(p.PostTime))
                            .ToList();

                        DateTime? newest = null;
                        DateTime? oldest = null;

                        if (validTimes.Count > 0)
                        {
                            newest = validTimes.Max();
                            oldest = validTimes.Min();
                        }

                        // ===== Ghi vào sheet tổng =====
                        ws.Cell(rowIdx, 1).Value = stt++;

                        string cleanPageName = RemoveInvalidChars(info.PageName);
                        ws.Cell(rowIdx, 2).Value = cleanPageName;

                        ws.Cell(rowIdx, 3).FormulaA1 = $"HYPERLINK(\"{info.PageLink}\", \"Link\")";

                        ws.Cell(rowIdx, 4).Value = info.IDFBPage;
                        ws.Cell(rowIdx, 5).Value = totalPosts;

                        ws.Cell(rowIdx, 6).Value = newest?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                        ws.Cell(rowIdx, 7).Value = oldest?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                        ws.Cell(rowIdx, 8).Value = totalLikes;
                        ws.Cell(rowIdx, 9).Value = totalShares;
                        ws.Cell(rowIdx, 10).Value = totalComments;
                        ws.Cell(rowIdx, 11).Value = totalInteract;

                        // ===== TẠO SHEET CHI TIẾT =====
                        string rawSheetName = RemoveInvalidChars(info.PageName);
                        string sheetName = MakeValidSheetName(rawSheetName);

                        // ===== FIX TRÙNG TÊN SHEET =====
                        string baseName = sheetName;
                        int index = 1;
                        while (wb.Worksheets.Any(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase)))
                        {
                            string suffix = $" ({index})";

                            if (baseName.Length + suffix.Length > 31)
                                sheetName = baseName.Substring(0, 31 - suffix.Length) + suffix;
                            else
                                sheetName = baseName + suffix;

                            index++;
                        }

                        var wsDetail = wb.Worksheets.Add(sheetName);

                        // ===== HEADER CHI TIẾT =====
                        string[] detailHeader = new[]
                        {
                    "STT","Thời gian","Nội dung","Link bài",
                    "Like","Share","Comment","Tương tác"
                };

                        for (int i = 0; i < detailHeader.Length; i++)
                        {
                            var cell = wsDetail.Cell(1, i + 1);
                            cell.Value = detailHeader[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.AliceBlue;
                        }

                        int dr = 2;
                        int dstt = 1;

                        // ===== Ghi bài viết =====
                        foreach (var p in posts)
                        {
                            wsDetail.Cell(dr, 1).Value = dstt++;
                            wsDetail.Cell(dr, 2).Value = p.PostTime;
                            wsDetail.Cell(dr, 3).Value = RemoveInvalidChars(p.Content);
                            wsDetail.Cell(dr, 4).FormulaA1 = $"HYPERLINK(\"{p.PostLink}\", \"Link\")";
                            wsDetail.Cell(dr, 5).Value = p.LikeCount ?? 0;
                            wsDetail.Cell(dr, 6).Value = p.ShareCount ?? 0;
                            wsDetail.Cell(dr, 7).Value = p.CommentCount ?? 0;
                            wsDetail.Cell(dr, 8).Value =
                                (p.LikeCount ?? 0) + (p.ShareCount ?? 0) + (p.CommentCount ?? 0);

                            dr++;
                        }

                        // ===== Hyperlink quay về Data Tổng =====
                        wsDetail.Cell("A1").FormulaA1 = $"HYPERLINK(\"#'Data Tổng'!A1\", \"← Về Tổng\")";
                        wsDetail.Cell("A1").Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;

                        // ===== Format sheet chi tiết =====
                        var usedDetail = wsDetail.RangeUsed();
                        usedDetail.Style.Font.FontName = "Times New Roman";
                        usedDetail.Style.Font.FontSize = 9;
                        usedDetail.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        usedDetail.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                        wsDetail.Columns().AdjustToContents();

                        // ===== Tạo hyperlink từ sheet tổng sang sheet chi tiết =====
                        string safeSheetName = sheetName.Replace("'", "''");
                        string safeName = RemoveInvalidChars(info.PageName).Replace("'", "''");

                        ws.Cell(rowIdx, 2).FormulaA1 =
                            $"HYPERLINK(\"#'{safeSheetName}'!A1\", \"{safeName}\")";

                        rowIdx++;
                    }

                    // ===== Format sheet tổng =====
                    var used = ws.RangeUsed();
                    used.Style.Font.FontName = "Times New Roman";
                    used.Style.Font.FontSize = 9;
                    used.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    used.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                    ws.Columns().AdjustToContents();

                    wb.SaveAs(filePath);
                }

                MessageBox.Show("✔ Xuất Excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xuất Excel: " + ex.Message);
            }
        }
        private string MakeValidSheetName(string name)
        {
            // Excel sheet name max 31 ký tự và cấm: : \ / ? * [ ]
            char[] invalid = { ':', '\\', '/', '?', '*', '[', ']' };

            foreach (var c in invalid)
                name = name.Replace(c, '_');

            if (name.Length > 31)
                name = name.Substring(0, 31);

            if (string.IsNullOrWhiteSpace(name))
                name = "Sheet";

            return name;
        }
        private string RemoveInvalidChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Loại bỏ emoji / surrogate
            return new string(input.Where(c => !char.IsSurrogate(c)).ToArray());
        }
        //============filter
        private void Txb_PageShearch_EditValueChanged(object sender, EventArgs e)
        {
            ApplyPageNameFilter();
        }
        private void ApplyPageNameFilter()
        {
            if (currentTable == null)
                return;

            string keyword = txb_PageShearch.EditValue == null
                ? ""
                : txb_PageShearch.EditValue.ToString().Trim().ToLower();

            // Không nhập gì → trả lại bảng gốc
            if (string.IsNullOrEmpty(keyword))
            {
                gridControl1.DataSource = currentTable;
                return;
            }

            DataTable filtered = currentTable.Clone();

            foreach (DataRow r in currentTable.Rows)
            {
                string pageName = r.Table.Columns.Contains("Tên Page")
                    ? r["Tên Page"]?.ToString().ToLower()
                    : "";

                if (pageName.Contains(keyword))
                {
                    filtered.ImportRow(r);
                }
            }

            gridControl1.DataSource = filtered;
        }
        private void btn_DeleteAllPostPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var gv = gridView1;

                if (gv.FocusedRowHandle < 0)
                {
                    MessageBox.Show("⚠ Chưa chọn Page!");
                    return;
                }

                DataRow row = gv.GetDataRow(gv.FocusedRowHandle);
                if (row == null)
                {
                    MessageBox.Show("⚠ Không lấy được dữ liệu Page!");
                    return;
                }

                string pageId = row["PageID"]?.ToString();
                if (string.IsNullOrWhiteSpace(pageId))
                {
                    MessageBox.Show("⚠ PageID không hợp lệ!");
                    return;
                }

                // ===== CONFIRM =====
                var confirm = MessageBox.Show(
                    "⚠ XÓA TOÀN BỘ BÀI VIẾT CỦA PAGE NÀY?\n\n" +
                    "✔ Chỉ xóa Post + Share + Comment + Topic\n" +
                    "❌ KHÔNG xóa Page / Person\n\n" +
                    "Hành động này KHÔNG THỂ HOÀN TÁC!",
                    "Xác nhận xóa Post Page",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirm != DialogResult.Yes)
                    return;

                // ===== DELETE POSTS =====
                SQLDAO.Instance.DeleteAllPostsOfPage(pageId);

                MessageBox.Show("✔ Đã xóa toàn bộ bài viết của Page!");

                // ===== REFRESH UI =====
                if (currentViewType == "PostPageNote")
                {
                    LoadPostPageNote();
                }
                else
                {
                    // reload dashboard / page info nếu cần
                    gridView1.RefreshData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xóa bài viết Page: " + ex.Message);
            }
        }

        private void txb_PageShearch_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btn_AddPagenote2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var selectedRows = new List<DataRow>();

                // Lấy toàn bộ row Select = true
                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    DataRow r = gridView1.GetDataRow(i);
                    if (r == null) continue;

                    bool selected = r["Select"] != DBNull.Value && (bool)r["Select"];
                    if (selected)
                        selectedRows.Add(r);
                }

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn page nào!");
                    return;
                }

                int success = 0;
                int skipped = 0;

                foreach (var row in selectedRows)
                {
                    string pageId = row["PageID"]?.ToString();
                    if (string.IsNullOrEmpty(pageId))
                    {
                        skipped++;
                        continue;
                    }

                    // Lấy info từ DB
                    PageInfo info = SQLDAO.Instance.GetPageByID(pageId);
                    if (info == null)
                    {
                        skipped++;
                        continue;
                    }
                    // Chặn GroupOff
                    if (info.PageType == FBType.GroupOff)
                    {
                        skipped++;
                        continue;
                    }
                    // Check trùng PageNote
                    if (SQLDAO.Instance.PageNoteExists(pageId))
                    {
                        skipped++;
                        continue;
                    }

                    // Insert Note
                    SQLDAO.Instance.InsertPageNote(pageId, DateTime.Now);
                    success++;
                }

                MessageBox.Show($"✔ Thêm ghi chú xong!\n" +
                                $"- Thành công: {success}\n" +
                                $"- Bỏ qua: {skipped}");

                LoadPageNote();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi thêm PageNote: " + ex.Message);
            }
        }

        private void btn_AddPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FAddPagecs f = new FAddPagecs();
            f.ShowDialog();
        }

        private void btn_AddPageNote_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var selectedRows = new List<DataRow>();

                // Lấy toàn bộ row Select = true
                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    DataRow r = gridView1.GetDataRow(i);
                    if (r == null) continue;

                    bool selected = r["Select"] != DBNull.Value && (bool)r["Select"];
                    if (selected)
                        selectedRows.Add(r);
                }

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn page nào!");
                    return;
                }

                int success = 0;
                int skipped = 0;

                foreach (var row in selectedRows)
                {
                    string pageId = row["PageID"]?.ToString();
                    if (string.IsNullOrEmpty(pageId))
                    {
                        skipped++;
                        continue;
                    }

                    // Lấy info từ DB
                    PageInfo info = SQLDAO.Instance.GetPageByID(pageId);
                    if (info == null)
                    {
                        skipped++;
                        continue;
                    }
                    // Chặn GroupOff
                    if (info.PageType == FBType.GroupOff)
                    {
                        skipped++;
                        continue;
                    }
                    // Check trùng PageNote
                    if (SQLDAO.Instance.PageNoteExists(pageId))
                    {
                        skipped++;
                        continue;
                    }

                    // Insert Note
                    SQLDAO.Instance.InsertPageNote(pageId, DateTime.Now);
                    success++;
                }

                MessageBox.Show($"✔ Thêm ghi chú xong!\n" +
                                $"- Thành công: {success}\n" +
                                $"- Bỏ qua: {skipped}");

                LoadPageNote();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi thêm PageNote: " + ex.Message);
            }
        }

        private void btn_DeletePage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var gv = gridView1;
                if (gv.FocusedRowHandle < 0)
                {
                    MessageBox.Show("⚠ Chưa chọn dòng!");
                    return;
                }

                DataRow r = gv.GetDataRow(gv.FocusedRowHandle);
                if (r == null)
                {
                    MessageBox.Show("⚠ Không lấy được dữ liệu dòng!");
                    return;
                }

                string pageId = r["PageID"]?.ToString();
                if (string.IsNullOrEmpty(pageId))
                {
                    MessageBox.Show("⚠ Không lấy được PageID!");
                    return;
                }

                // Confirm
                var confirm = MessageBox.Show(
                    "Bạn có chắc muốn xóa?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (confirm == DialogResult.No) return;

                // ===============================================
                // XOÁ THEO CHẾ ĐỘ (currentViewType)
                // ===============================================
                switch (currentViewType)
                {
                    case "PageInfo":
                        // ❗ XÓA HOÀN TOÀN PAGE
                        // ⭐ 1. XÓA BÀI VIẾT LIÊN QUAN PAGE TRƯỚC
                        SQLDAO.Instance.DeleteAllPostsOfPage(pageId);

                        // ⭐ 2. XÓA NOTE
                        SQLDAO.Instance.DeletePageNote(pageId);

                        // ⭐ 3. XÓA MONITOR
                        SQLDAO.Instance.DeletePageMonitor(pageId);

                        // ⭐ 4. CUỐI CÙNG MỚI XÓA PAGEINFO
                        SQLDAO.Instance.ExecuteNonQuery(
                            "DELETE FROM TablePageInfo WHERE PageID=@id",
                            new Dictionary<string, object> { { "@id", pageId } }
                        );

                        MessageBox.Show("✔ Đã xóa toàn bộ dữ liệu Page!");

                        LoadPageInfo();
                        break;

                    case "PageNote":
                        // ❗ CHỈ XÓA GHI CHÚ - KHÔNG ĐỤNG PAGEINFO
                        SQLDAO.Instance.DeletePageNote(pageId);
                        MessageBox.Show("✔ Đã xóa ghi chú Page!");
                        LoadPageNote();
                        break;

                    case "PageMonitor":
                        // ❗ CHỈ XÓA TRONG TABLE PAGEMONITOR
                        SQLDAO.Instance.DeletePageMonitor(pageId);
                        MessageBox.Show("✔ Đã xóa Page khỏi danh sách theo dõi!");
                        LoadPageMonitor();
                        break;

                    default:
                        MessageBox.Show("❗ Chế độ xóa không hợp lệ!");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xóa Page: " + ex.Message);
            }
        }

       
    }
}
