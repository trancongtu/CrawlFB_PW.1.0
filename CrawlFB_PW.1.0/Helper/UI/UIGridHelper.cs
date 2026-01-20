using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Repository;
using System.Collections;
using System;
using System.Drawing;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Enums;
using System.Collections.Generic;
using DevExpress.Utils;

namespace CrawlFB_PW._1._0.Helper
{
    public static class UIGridHelper
    {
        public static void ApplySelect(GridView gv, GridControl grid)
        {
            var col = gv.Columns["Select"];
            if (col == null) return;

            col.Width = 40;
            col.OptionsColumn.FixedWidth = true;

            var chk = new RepositoryItemCheckEdit
            {
                AllowGrayed = false
            };
            // 🔥 QUAN TRỌNG: commit ngay khi click
            chk.EditValueChanged += (s, e) =>
            {
                gv.PostEditor();        // đẩy editor về datasource
                gv.UpdateCurrentRow(); // update object trong list
            };

            grid.RepositoryItems.Add(chk);
            col.ColumnEdit = chk;
        }
        public static void LockAllColumnsExceptSelect(GridView gv)
        {
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
            {
                if (col.FieldName != "Select")
                {
                    // 🔒 KHÓA CÁC CỘT KHÁC
                    col.OptionsColumn.AllowEdit = false;
                    col.OptionsColumn.ReadOnly = true;
                    col.OptionsColumn.AllowFocus = true; // cho focus row
                }
            }
        }

        public static void SelectAll(GridControl grid, bool value)
        {
            var data = grid.DataSource as IEnumerable;
            if (data == null) return;

            foreach (var item in data)
            {
                var prop = item.GetType().GetProperty("Select");
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(item, value);
                }
            }

            grid.MainView?.RefreshData();
        }

        // 1️⃣ STT – dùng RowIndicator (CHUNG mọi form)
        // ==================================================
        public static void EnableRowIndicatorSTT(GridView gv, int width = 45)
        {
            gv.IndicatorWidth = width;

            gv.CustomDrawRowIndicator += (s, e) =>
            {
                if (!e.Info.IsRowIndicator) return;

                if (e.RowHandle < 0)
                {
                    e.Info.DisplayText = "STT";
                    return;
                }

                e.Info.DisplayText = (e.RowHandle + 1).ToString();
            };
        }
        // ==================================================
        // 2️⃣ Click dòng → toggle Select
        // (ViewModel phải có property bool Select)
        // ==================================================
        public static void EnableRowClickToggleSelect(GridView gv, string selectField = "Select")
        {
            gv.RowCellClick += (s, e) =>
            {
                if (!gv.IsDataRow(e.RowHandle)) return;

                var row = gv.GetRow(e.RowHandle);
                if (row == null) return;

                // ❌ Click vào chính checkbox thì bỏ qua
                if (e.Column?.FieldName == selectField)
                    return; 

                var prop = row.GetType().GetProperty("Select");
                if (prop == null || prop.PropertyType != typeof(bool))
                    return;

                bool current = (bool)prop.GetValue(row);
                prop.SetValue(row, !current);

                gv.RefreshRow(e.RowHandle);
            };
        }
        // ==================================================
        // 3️⃣ Hiển thị Status (pending / running / done / error)
        // ==================================================
        public static void EnableStatusDisplay(GridView gv, string fieldName = "Status")
        {
            gv.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Column.FieldName != fieldName || e.Value == null)
                    return;

                if (!(e.Value is UIStatus))
                    return;

                var status = (UIStatus)e.Value;

                switch (status)
                {
                    case UIStatus.Added:
                        e.DisplayText = "➕ Đã thêm";
                        break;
                    case UIStatus.Pending:
                        e.DisplayText = "⏳ Chờ";
                        break;
                    case UIStatus.Running:
                        e.DisplayText = "▶️ Đang chạy";
                        break;
                    case UIStatus.Done:
                        e.DisplayText = "✅ Xong";
                        break;
                    case UIStatus.Skip:
                        e.DisplayText = "⏭ Bỏ qua";
                        break;
                    case UIStatus.Error:
                        e.DisplayText = "❌ Lỗi";
                        break;
                }
            };
        }
        // việt hóa cột   
        public static void ApplyVietnameseCaption(GridView gv)
    {
        // ===== CỘT CHUNG =====
        SetIfExists(gv, "Select", "Chọn");
        SetIfExists(gv, "Status", "Trạng thái");
        SetIfExists(gv, "TimeView", "Thời gian");
            // ===== PAGE =====
        SetIfExists(gv, "PageName", "Tên Page");
        SetIfExists(gv, "PageLink", "Link Page");
        SetIfExists(gv, "PageType", "Loại");
        SetIfExists(gv, "PageMembers", "Thành viên");
        SetIfExists(gv, "TimeLastPostView", "T/g bài cuối");
        // ===== POST =====
        SetIfExists(gv, "PostLink", "Link bài viết");
        SetIfExists(gv, "Content", "Nội dung");     
        // ===== PERSON =====
        SetIfExists(gv, "PosterName", "Người đăng");
        SetIfExists(gv, "PosterLink", "Link Ng đăng");
    }

        private static void SetIfExists(GridView gv, string fieldName, string caption)
        {
            var col = gv.Columns[fieldName];
            if (col != null)
                col.Caption = caption;
        }
        // màu trạng thái 
    public static void ApplyRowColorByStatus(GridView gv, string statusColumnName)
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
            if (value == null || !(value is UIStatus))
                return;

            var status = (UIStatus)value;

            e.HighPriority = true;

            switch (status)
            {
                case UIStatus.Pending:
                    e.Appearance.BackColor = Color.FromArgb(255, 240, 200);
                    break;

                case UIStatus.Running:
                    e.Appearance.BackColor = Color.FromArgb(200, 220, 255);
                    break;

                case UIStatus.Done:
                    e.Appearance.BackColor = Color.FromArgb(200, 255, 200);
                    break;

                case UIStatus.Error:
                    e.Appearance.BackColor = Color.FromArgb(255, 210, 210);
                    break;
            }

            e.Appearance.Options.UseBackColor = true;
        }
    }
        // ẩn nếu tồn tại
        public static void HideIfExists(GridView gv, string field)
        {
            var col = gv.Columns[field];
            if (col != null)
                col.Visible = false;
        }
        // chỉ hiện một số cột

        public static void ShowOnlyColumns(GridView gv, params string[] visibleFields)
        {
            if (gv == null) return;

            var set = new HashSet<string>(visibleFields);
            int index = 0;
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
            {
                if (set.Contains(col.FieldName))
                {
                    col.Visible = true;
                    col.VisibleIndex = index++;
                }
                else
                {
                    col.Visible = false;
                }
            }
        }
        // Grid Comment bài viết
        public static void ApplyCommentGridStyle(GridView gv)
        {
            gv.Appearance.Row.Font = new Font("Segoe UI", 9);
            gv.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            gv.Columns[nameof(CommentGridRow.Select)].Width = 40;
            gv.Columns[nameof(CommentGridRow.STT)].Width = 40;
            gv.Columns[nameof(CommentGridRow.ActorName)].Width = 100;
            gv.Columns[nameof(CommentGridRow.PosterFBType)].Width = 60;
            gv.Columns[nameof(CommentGridRow.PosterFBType)].OptionsColumn.AllowEdit = false;
            gv.Columns[nameof(CommentGridRow.Time)].Width = 100;
            gv.Columns[nameof(CommentGridRow.LinkView)].Width =50;
            gv.Columns[nameof(CommentGridRow.IDFBPerson)].Width = 80;
            gv.Columns[nameof(CommentGridRow.Content)].Width = 300;
            gv.Columns[nameof(CommentGridRow.Level)].Width = 20;
            gv.Columns[nameof(CommentGridRow.Content)]
                .AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            
            gv.RowHeight = 24;
        }
    public static void ApplyCommentLink(GridView gv, GridControl grid)
        {
            var col = gv.Columns[nameof(CommentGridRow.LinkView)];
            if (col == null) return;

            var linkEdit = new RepositoryItemHyperLinkEdit
            {
                SingleClick = true,
                TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
            };

            linkEdit.OpenLink += (s, e) =>
            {
                var row = gv.GetFocusedRow() as CommentGridRow;
                if (row == null) return;

                if (!string.IsNullOrWhiteSpace(row.Link))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(row.Link);
                    }
                    catch { }
                }
            };

            grid.RepositoryItems.Add(linkEdit);
            col.ColumnEdit = linkEdit;
        }
        /// Hyper chung chuẩn
     public static void ApplyHyperlinkColumn( GridView gv,GridControl grid,string fieldName,
         string displayText = "🔗 Xem link",bool hideIfEmpty = false)
        {
          
            if (gv == null || grid == null) return;
            if (gv.Columns[fieldName] == null) return;

            var col = gv.Columns[fieldName];

            // ===== Repository =====
            var linkEdit = new RepositoryItemHyperLinkEdit
            {
                SingleClick = true,
                TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
            };

            linkEdit.OpenLink += (s, e) =>
            {
                string url = e.EditValue as string;
                if (string.IsNullOrWhiteSpace(url)) return;

                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch { }
            };

            grid.RepositoryItems.Add(linkEdit);
            col.ColumnEdit = linkEdit;

            // ===== Display text =====
            gv.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Column.FieldName != fieldName) return;

                string url = e.Value as string;
                if (string.IsNullOrWhiteSpace(url))
                {
                    e.DisplayText = hideIfEmpty ? "" : "";
                }
                else
                {
                    e.DisplayText = displayText;
                }
            };

            // ===== Tooltip =====
            if (grid.ToolTipController == null)
                grid.ToolTipController = new ToolTipController();

            grid.ToolTipController.GetActiveObjectInfo += (s, e) =>
            {
                var view = grid.FocusedView as GridView;
                if (view == null) return;

                var hit = view.CalcHitInfo(e.ControlMousePosition);
                if (!hit.InRowCell) return;
                if (hit.Column.FieldName != fieldName) return;

                string url = view.GetRowCellValue(hit.RowHandle, hit.Column) as string;
                if (string.IsNullOrWhiteSpace(url)) return;

                object key = hit.RowHandle + "_" + fieldName;
                e.Info = new ToolTipControlInfo(key, url);
            };

            // ===== Ẩn row nếu link rỗng (optional) =====
            if (hideIfEmpty)
            {
                gv.CustomRowFilter += (s, e) =>
                {
                    if (e.ListSourceRow < 0) return;

                    var view = s as GridView;
                    string url = view.GetListSourceRowCellValue(
                        e.ListSourceRow, fieldName
                    ) as string;

                    if (string.IsNullOrWhiteSpace(url))
                    {
                        e.Visible = false;
                        e.Handled = true;
                    }
                };
            }
        }
        public static void LockAllColumnsExceptLinks(GridView gv)
        {
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
            {
                bool isInteractive =
                    col.FieldName.Contains("Link") ||
                    col.FieldName.Equals("AttachmentView", StringComparison.OrdinalIgnoreCase);

                if (isInteractive)
                {
                    // 🔓 CỘT TƯƠNG TÁC (Link + Attachment)
                    col.OptionsColumn.AllowEdit = true;
                    col.OptionsColumn.ReadOnly = false;
                    col.OptionsColumn.AllowFocus = true;
                }
                else
                {
                    // 🔒 CỘT CHỈ XEM
                    col.OptionsColumn.AllowEdit = false;
                    col.OptionsColumn.ReadOnly = true;
                    col.OptionsColumn.AllowFocus = true;
                }
            }
        }

        // ROWCELL CLICK
        public static void EnableLinkClickByRowCell(GridView gv)
        {
            gv.RowCellClick += (s, e) =>
            {
                if (e.RowHandle < 0) return;
                if (!e.Column.FieldName.EndsWith("Link")) return;

                var url = gv.GetRowCellValue(e.RowHandle, e.Column) as string;
                if (string.IsNullOrWhiteSpace(url)) return;

                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch { }
            };
        }
        public static void ApplyLinkDisplayText(GridView gv,string displayText = "🔗 Mở link")
        {
            gv.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Value == null) return;
                if (!e.Column.FieldName.EndsWith("Link")) return;

                var url = e.Value as string;
                if (string.IsNullOrWhiteSpace(url))
                {
                    e.DisplayText = "";
                }
                else
                {
                    e.DisplayText = displayText;
                }
            };
        }
        public static void ApplyLinkTooltip(GridView gv, GridControl grid)
        {
            if (grid.ToolTipController == null)
                grid.ToolTipController = new DevExpress.Utils.ToolTipController();

            grid.ToolTipController.GetActiveObjectInfo += (s, e) =>
            {
                var view = grid.FocusedView as GridView;
                if (view == null) return;

                var hit = view.CalcHitInfo(e.ControlMousePosition);
                if (!hit.InRowCell) return;
                if (!hit.Column.FieldName.EndsWith("Link")) return;

                var url = view.GetRowCellValue(hit.RowHandle, hit.Column) as string;
                if (string.IsNullOrWhiteSpace(url)) return;

                e.Info = new DevExpress.Utils.ToolTipControlInfo(
                    hit.RowHandle + "_" + hit.Column.FieldName,
                    url
                );
            };
        }

        // ==================================================
        // 📎 ATTACHMENT LINK (ICON + CLICK + TOOLTIP)
        // YÊU CẦU ViewModel:
        //   string AttachmentView  (link thật)
        //   bool   HasReel
        //   bool   HasVideo
        //   bool   HasPhoto
        // ==================================================
        public static void ApplyAttachmentLink(GridView gv,GridControl grid,string fieldName = "AttachmentView")
        {
            if (gv == null || grid == null) return;
            var col = gv.Columns[fieldName];
            if (col == null) return;

            // ===============================
            // 🔓 MỞ CỘT ATTACHMENT (QUAN TRỌNG)
            // ===============================
            col.OptionsColumn.AllowEdit = true;
            col.OptionsColumn.ReadOnly = false;
            col.OptionsColumn.AllowFocus = true;

            col.Width = 40;
            col.OptionsColumn.FixedWidth = true;

            // ===============================
            // Repository HyperLink
            // ===============================
            var linkEdit = new RepositoryItemHyperLinkEdit
            {
                SingleClick = true,
                TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
            };

            linkEdit.OpenLink += (s, e) =>
            {
                string url = e.EditValue as string;

                if (string.IsNullOrWhiteSpace(url) || url == "N/A")
                    return;

                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch { }
            };

            grid.RepositoryItems.Add(linkEdit);
            col.ColumnEdit = linkEdit;

            // ===============================
            // ICON theo loại attachment
            // ===============================
            gv.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Column.FieldName != fieldName) return;

                string url = e.Value as string;

                // 🔥 CHẶN "N/A"
                if (string.IsNullOrWhiteSpace(url) || url == "N/A")
                {
                    e.DisplayText = "";
                    return;
                }

                var view = s as GridView;
                if (view == null || e.ListSourceRowIndex < 0)
                {
                    e.DisplayText = "📎";
                    return;
                }

                var row = view.GetRow(e.ListSourceRowIndex);
                if (row == null)
                {
                    e.DisplayText = "📎";
                    return;
                }

                bool hasReel = false, hasVideo = false, hasPhoto = false;

                var pReel = row.GetType().GetProperty("HasReel");
                var pVideo = row.GetType().GetProperty("HasVideo");
                var pPhoto = row.GetType().GetProperty("HasPhoto");

                if (pReel != null && (bool)pReel.GetValue(row)) hasReel = true;
                if (pVideo != null && (bool)pVideo.GetValue(row)) hasVideo = true;
                if (pPhoto != null && (bool)pPhoto.GetValue(row)) hasPhoto = true;

                if (hasReel)
                    e.DisplayText = "🎬";
                else if (hasVideo)
                    e.DisplayText = "🎥";
                else if (hasPhoto)
                    e.DisplayText = "📷";
                else
                    e.DisplayText = "📎";
            };

            // ===============================
            // Tooltip hiện link thật
            // ===============================
            if (grid.ToolTipController == null)
                grid.ToolTipController = new DevExpress.Utils.ToolTipController();

            grid.ToolTipController.GetActiveObjectInfo += (s, e) =>
            {
                var view = grid.FocusedView as GridView;
                if (view == null) return;

                var hit = view.CalcHitInfo(e.ControlMousePosition);
                if (!hit.InRowCell) return;
                if (hit.Column.FieldName != fieldName) return;

                string url = view.GetRowCellValue(hit.RowHandle, hit.Column) as string;
                if (string.IsNullOrWhiteSpace(url) || url == "N/A") return;

                e.Info = new DevExpress.Utils.ToolTipControlInfo(
                    hit.RowHandle + "_" + fieldName,
                    url
                );
            };
        }


    }
}
