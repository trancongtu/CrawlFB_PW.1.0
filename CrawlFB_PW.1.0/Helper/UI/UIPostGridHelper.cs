using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid;
using DevExpress.Utils;
using System.Linq;

public static class UIPostGridHelper
{
    //=============TẠO CỘT CỦA UCPAGEMAIN
    public static void ApplyPostUCDatatableMainColumnWidth(GridView gv)
    {
        if (gv == null || gv.Columns.Count == 0) return;

        gv.OptionsView.ColumnAutoWidth = false;

        // ===== 1️⃣ RESET AN TOÀN =====
        foreach (GridColumn col in gv.Columns)
        {
            col.OptionsColumn.FixedWidth = false;
            col.MinWidth = 30;
            col.Width = 50; // base width
        }

        // ===== 2️⃣ CỘT NHỎ – CHẶN TRƯỚC =====
        AutoFitWithMax(gv, "STT", 30, 40);
        AutoFitWithMax(gv, "LikeCount", 30, 40);
        AutoFitWithMax(gv, "ShareCount", 30, 40);
        AutoFitWithMax(gv, "CommentCount", 30, 40);

        // ===== 3️⃣ CỘT TRUNG BÌNH =====
        AutoFitWithMax(gv, "RealPostTime", 80, 120);
        AutoFitWithMax(gv, "PostStatus", 80, 120);

        // ===== 4️⃣ CỘT ƯU TIÊN (AUTO SAU CÙNG) =====
        AutoFitWithMax(gv, "PostContent", 80, 120);
        AutoFitWithMax(gv, "ContainerPageName", 80, 120);
        AutoFitWithMax(gv, "PosterPageName", 80, 120);
        AutoFitWithMax(gv, "PosterPersonName", 80, 120);
    }

    public static void ApplyPostHeaderCaptionUCDBMain(GridView gv)
    {
        SetCaption(gv, "STT", "STT");
        SetCaption(gv, "PostContent", "Nội dung");
        SetCaption(gv, "RealPostTime", "Thời gian");
        SetCaption(gv, "LikeCount", "Like");
        SetCaption(gv, "CommentCount", "Bình luận");
        SetCaption(gv, "ShareCount", "Chia sẻ");
        SetCaption(gv, "PostStatus", "Trạng thái");
        SetCaption(gv, "PostTimeSave", "T/G Lưu");
        SetCaption(gv, "ContainerPageName", "Page đăng");
        SetCaption(gv, "PosterPageName", "Page tạo");
        SetCaption(gv, "PosterPersonName", "Người đăng");
        SetCaption(gv, "PosterPersonNote", "Ghi chú");
    }
   
    public static void ApplyAllPostGridUCDBMain(GridView gv)
    {
        if (gv == null) return;
        ApplyPostGridStyle(gv);
        ApplyPostUCDatatableMainColumnWidth(gv);
        ApplyPostHeaderCaptionUCDBMain(gv);     
        ApplyHyperlinkBehavior(gv);
    }
    
    //=======================================================
    //============================================UCPAGEVIEW
    public static void ApplyPostGridColumnWidth(GridView gv)
    {
        if (gv.Columns.Count == 0) return;

        gv.OptionsView.ColumnAutoWidth = false;

        // Set FixedWidth trước để DevExpress không override width
        foreach (GridColumn col in gv.Columns)
          col.OptionsColumn.FixedWidth = true;

        SafeWidth(gv, "STT", 40);
        SafeWidth(gv, "Địa chỉ", 80);
        SafeWidth(gv, "Thời gian", 120);

        // Nội dung auto-fit + nới rộng
        var c = gv.Columns.ColumnByFieldName("Nội dung");
        if (c != null)
        {
            c.BestFit();
            c.Width += 40;
        }

        SafeWidth(gv, "Like", 50);
        SafeWidth(gv, "Share", 50);
        SafeWidth(gv, "Comment", 60);

        // Ẩn link thật
        var hidden = gv.Columns.ColumnByFieldName("LinkThật");
        if (hidden != null) hidden.Visible = false;
    }
    public static void ApplyPostGridStyleUCPageView(GridView gv)
    {
        gv.OptionsView.ColumnAutoWidth = false;
        gv.OptionsBehavior.Editable = false;
        gv.OptionsSelection.EnableAppearanceFocusedCell = false;

    }
    // =============================
    // 1) HAMD CHUNG
    // =============================
    public static void ApplyPostGridStyle(GridView gv)
    {
        gv.OptionsView.ColumnAutoWidth = false;
        gv.OptionsBehavior.Editable = true;
        gv.OptionsSelection.EnableAppearanceFocusedCell = false;

    }
    private static void AutoFitWithMax(GridView gv, string field, int minWidth, int maxWidth)
    {
        var col = gv.Columns.ColumnByFieldName(field);
        if (col == null) return;

        col.OptionsColumn.FixedWidth = false;
        col.MinWidth = minWidth;
        col.MaxWidth = maxWidth;

        // ⭐ QUAN TRỌNG: BestFit bao gồm Header
        col.BestFit();

        // Chặn max
        if (col.Width > maxWidth)
            col.Width = maxWidth;

        // Đảm bảo không nhỏ hơn min
        if (col.Width < minWidth)
            col.Width = minWidth;
    }

    // =============================
    // 2) SET WIDTH CỐ ĐỊNH CHO POST GRID
    // =============================
    private static void SetCaption(GridView gv, string field, string caption)
    {
        var col = gv.Columns[field];
        if (col != null)
            col.Caption = caption;
    }
    private static void SafeWidth(GridView gv, string field, int width)
    {
        var col = gv.Columns.ColumnByFieldName(field);
        if (col != null)
        {
            col.OptionsColumn.FixedWidth = true; // ⭐ chỉ fixed cột này
            col.Width = width;
        }
    }


    // =============================
    // 3) ÁP STYLE HYPERLINK "Xem link"
    // =============================
    public static void ApplyHyperlinkBehavior(DevExpress.XtraGrid.Views.Grid.GridView gv)
    {
        if (gv == null) return;

        var grid = gv.GridControl;
        if (grid == null) return;

        // ===== LẤY TẤT CẢ CỘT CÓ FIELDNAME KẾT THÚC BẰNG "Link" =====
        var linkColumns = gv.Columns
            .Where(c => c.FieldName.EndsWith("Link", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (linkColumns.Count == 0)
            return;
        // ===== Repository Hyperlink =====
        var linkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit
        {
            SingleClick = true
        };
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

        // ===== GÁN COLUMNEDIT =====
        foreach (var col in linkColumns)
        {
            col.ColumnEdit = linkEdit;
        }

        // ===== HIỂN THỊ TEXT NGẮN =====
        gv.CustomColumnDisplayText += (s, e) =>
        {
            if (e.Value == null) return;

            if (e.Column.FieldName.EndsWith("Link", StringComparison.OrdinalIgnoreCase))
                e.DisplayText = "🔗 Mở link";
        };

        // ===== TOOLTIP HIỆN LINK THẬT =====
        if (grid.ToolTipController == null)
            grid.ToolTipController = new DevExpress.Utils.ToolTipController();

        grid.ToolTipController.GetActiveObjectInfo += (s, e) =>
        {
            var view = grid.FocusedView as GridView;
            if (view == null) return;

            var hit = view.CalcHitInfo(e.ControlMousePosition);
            if (!hit.InRowCell) return;

            if (!hit.Column.FieldName.EndsWith("Link", StringComparison.OrdinalIgnoreCase))
                return;

            string link = view.GetRowCellValue(hit.RowHandle, hit.Column) as string;
            if (!string.IsNullOrWhiteSpace(link))
            {
                object key = hit.RowHandle + "_" + hit.Column.FieldName;
                e.Info = new DevExpress.Utils.ToolTipControlInfo(key, link);
            }
        };
        foreach (var col in linkColumns)
        {
            col.ColumnEdit = linkEdit;

            // ⭐ BẮT BUỘC
            col.OptionsColumn.AllowEdit = true;
            col.OptionsColumn.ReadOnly = false;
        }
    }
        /// <summary>
        /// Dùng cho grid dashboard:
        /// - Cột hiển thị: text giả (VD: "Địa chỉ")
        /// - Cột link thật: URL (VD: "LinkThật")
        /// </summary>
        public static void ApplyFakeLink(GridView gv,string displayField,string linkField)
        {
            if (gv == null) return;

            // Style giống hyperlink
            gv.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == displayField)
                {
                    e.Appearance.ForeColor = Color.Blue;
                    e.Appearance.Font = new Font("Segoe UI", 9, FontStyle.Underline);
                }
            };

            // Cursor tay
            gv.MouseMove += (s, e) =>
            {
                var hit = gv.CalcHitInfo(e.Location);
                gv.GridControl.Cursor =
                    hit.InRowCell && hit.Column.FieldName == displayField
                    ? Cursors.Hand
                    : Cursors.Default;
            };

            // Click mở link
            gv.RowCellClick += (s, e) =>
            {
                if (e.Column.FieldName != displayField) return;
                if (e.RowHandle < 0) return;

                DataRow row = gv.GetDataRow(e.RowHandle);
                if (row == null) return;

                string url = row[linkField]?.ToString();
                if (string.IsNullOrWhiteSpace(url)) return;

                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        }
                    );
                }
                catch { }
            };
        
    }
}
