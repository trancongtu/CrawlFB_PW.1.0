using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid;
using System.Security.Cryptography.X509Certificates;
using DevExpress.XtraEditors.Repository;
using CrawlFB_PW._1._0.DTO;
namespace CrawlFB_PW._1._0.Helper
{
    public class UIPageInfoGridHelper
    {
        // style tổng để gọi
        public static void ApplyAll(GridView gv)
        {
            if (gv == null) return;          
            ApplyPostGridColumnWidth(gv);
            ApplyPageHeaderCaption(gv);
           ApplyHyperlinkBehavior(gv);
            ApplyPostGridStyle(gv);
        }
        //cài đặt griview
        public static void ApplyPostGridStyle(GridView gv)
        {
            if (gv == null) return;

            // ===== GRID CHUNG =====
            gv.OptionsBehavior.Editable = true; // ⭐ bật edit toàn grid cho đơn giản
           
            // ===== MỞ RIÊNG CỘT PageLink ĐỂ CLICK =====
            var colLink = gv.Columns["PageLink"];
            if (colLink != null)
            {
                colLink.OptionsColumn.AllowEdit = true;
                colLink.OptionsColumn.ReadOnly = false;
                colLink.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
            }
        }

        // cài đặt cột
        public static void ApplyPostGridColumnWidth(GridView gv)
        {
            if (gv.Columns.Count == 0) return;

            gv.OptionsView.ColumnAutoWidth = false;

            // 1️⃣ Mặc định FIXED hết
            foreach (GridColumn col in gv.Columns)
                col.OptionsColumn.FixedWidth = true;

            // 2️⃣ Set MAX width (anh đang set)
            SafeWidth(gv, "STT", 80);
            SafeWidth(gv, "PageID", 140);
            SafeWidth(gv, "PageName", 120);        // 👈 MAX
            SafeWidth(gv, "PageLink", 100);
            SafeWidth(gv, "IDFBPage", 120);
            SafeWidth(gv, "PageType", 100);
            SafeWidth(gv, "PageMembers", 100);
            SafeWidth(gv, "TimeLastPost", 120);
            SafeWidth(gv, "PageInteraction", 80);
            SafeWidth(gv, "PageEvaluation", 130);
            SafeWidth(gv, "PageInfoText", 250);    // 👈 MAX
            SafeWidth(gv, "IsScanned", 90);
            SafeWidth(gv, "PageTimeSave", 150);

            // 3️⃣ Cho phép CO THEO DỮ LIỆU (nhưng không vượt MAX)
            AutoFitWithMax(gv, "PageName", 80, 120);
            AutoFitWithMax(gv, "PageInfoText", 120, 250);
        }

        // cho co dữ liệu
        private static void AutoFitWithMax(GridView gv,string field,int minWidth,int maxWidth)
        {
            var col = gv.Columns[field];
            if (col == null) return;

            col.OptionsColumn.FixedWidth = false; // ⭐ cho phép co
            col.MinWidth = minWidth;

            col.BestFit();                        // co theo dữ liệu

            if (col.Width > maxWidth)             // chặn MAX
                col.Width = maxWidth;
        }

        private static void SafeWidth(GridView gv, string field, int width)
        {
            var col = gv.Columns.ColumnByFieldName(field);
            if (col != null)
                col.Width = width;
        }
        // cài đặt Header
        public static void ApplyPageHeaderCaption(GridView gv)
        {
            SetCaption(gv, "STT", "STT");
            SetCaption(gv, "PageID", "ID Page");
            SetCaption(gv, "PageName", "Tên Page");
            SetCaption(gv, "PageLink", "Link Page");
            SetCaption(gv, "IDFBPage", "ID Facebook");
            SetCaption(gv, "PageType", "Loại Page");
            SetCaption(gv, "PageMembers", "Followers");
            SetCaption(gv, "TimeLastPost", "Bài gần nhất");
            SetCaption(gv, "PageInteraction", "Tương tác");
            SetCaption(gv, "PageEvaluation", "Đánh giá");
            SetCaption(gv, "PageInfoText", "Thông tin");
            SetCaption(gv, "IsScanned", "Đã quét");
            SetCaption(gv, "PageTimeSave", "Thời gian lưu");
        }
        private static void SetCaption(GridView gv, string field, string caption)
        {
            var col = gv.Columns[field];
            if (col != null)
                col.Caption = caption;
        }
        //========= dẫn link page
        public static void ApplyHyperlinkBehavior(DevExpress.XtraGrid.Views.Grid.GridView gv)
        {
            if (gv == null) return;

            var grid = gv.GridControl;   // ⭐ LẤY GRID TỪ ĐÂY
            if (grid == null) return;

            if (gv.Columns["PageLink"] == null)
                return;

            var linkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            linkEdit.SingleClick = true;

            linkEdit.OpenLink += (s, e) =>
            {
                try
                {
                    string url = e.EditValue as string;
                    if (!string.IsNullOrWhiteSpace(url))
                        System.Diagnostics.Process.Start(url);
                }
                catch { }
            };

            grid.RepositoryItems.Add(linkEdit);
            gv.Columns["PageLink"].ColumnEdit = linkEdit;

            // Không hiện link dài
            gv.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Column.FieldName == "PageLink" && e.Value != null)
                    e.DisplayText = "🔗 Mở link";
            };

            // Tooltip
            if (grid.ToolTipController == null)
                grid.ToolTipController = new DevExpress.Utils.ToolTipController();

            grid.ToolTipController.GetActiveObjectInfo += (s, e) =>
            {
                var view = grid.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView;
                if (view == null) return;

                var hit = view.CalcHitInfo(e.ControlMousePosition);
                if (hit.InRowCell && hit.Column.FieldName == "PageLink")
                {
                    string link = view.GetRowCellValue(hit.RowHandle, hit.Column) as string;
                    if (!string.IsNullOrEmpty(link))
                    {
                        object key = hit.RowHandle + "_" + hit.Column.FieldName;
                        e.Info = new DevExpress.Utils.ToolTipControlInfo(key, link);
                    }
                }
            };
        }

       
        //===== TÔ MÀU CỘT THEO TRẠNG THÁI
        public static void ApplyRowColorByColumn(GridView gv,string statusColumnName)
        {
            if (gv == null || string.IsNullOrWhiteSpace(statusColumnName))
                return;

            gv.RowStyle -= Grid_RowStyle_ByStatus;
            gv.RowStyle += Grid_RowStyle_ByStatus;
            void Grid_RowStyle_ByStatus(object sender, RowStyleEventArgs e)
            {
                if (e.RowHandle < 0) return;

                var view = sender as GridView;
                if (view == null) return;

                if (view.Columns[statusColumnName] == null)
                    return;

                var value = view.GetRowCellValue(e.RowHandle, statusColumnName);
                if (value == null) return;

                string status = value.ToString().ToLowerInvariant();
                e.HighPriority = true;
                switch (status)
                {
                    case "pending":
                        e.Appearance.BackColor = Color.FromArgb(255, 240, 200);
                        break;
                    case "running":
                        e.Appearance.BackColor = Color.FromArgb(200, 220, 255);
                        break;
                    case "done":
                        e.Appearance.BackColor = Color.FromArgb(200, 255, 200);
                        break;
                    case "error":
                        e.Appearance.BackColor = Color.FromArgb(255, 210, 210);
                        break;
                }

                e.Appearance.Options.UseBackColor = true;
            }
        }
        //===============hết tô màu
        // kiểu mới tí trên bỏ
        public static void ApplyPageColumnAdjust(GridView gv)
        {
            SetWidth(gv, "Select", 50);
            SetWidth(gv, "PageName", 150);
            SetWidth(gv, "PageLink", 200);
            SetWidth(gv, "Status", 90);
        }

        static void SetWidth(GridView gv, string field, int width)
        {
            var col = gv.Columns[field];
            if (col != null)
                col.Width = width;
        }

        static void Hide(GridView gv, string field)
        {
            var col = gv.Columns[field];
            if (col != null)
                col.Visible = false;
        }
    }
}
