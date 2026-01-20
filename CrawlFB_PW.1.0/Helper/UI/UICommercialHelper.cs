using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
namespace CrawlFB_PW._1._0.Helper
{
    public static class UICommercialHelper
    {
        // =========================
        // LABEL
        // =========================
        public static void StyleLabel(Label lbl)
        {
            lbl.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lbl.ForeColor = Color.FromArgb(50, 50, 50);
        }

        // =========================
        // TEXTBOX
        // =========================
        public static void StyleTextBox(TextBox tb)
        {
            tb.Font = new Font("Segoe UI", 9f);
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = Color.White;
            tb.ForeColor = Color.Black;
        }

        // =========================
        // COMBOBOX
        // =========================
        public static void StyleComboBox(ComboBox cb)
        {
            cb.Font = new Font("Segoe UI", 9f);
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = Color.White;
            cb.ForeColor = Color.Black;
        }

        // =========================
        // BUTTON (CƠ BẢN – NHẸ)
        // =========================
        public static void StyleButton(Button btn)
        {
            btn.Font = new Font("Segoe UI", 9f);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.BackColor = Color.White;
            btn.ForeColor = Color.Black;
            btn.Cursor = Cursors.Hand;

            // Hover nhẹ
            btn.MouseEnter += (s, e) =>
                btn.BackColor = Color.FromArgb(245, 245, 245);

            btn.MouseLeave += (s, e) =>
                btn.BackColor = Color.White;
        }

        // =========================
        // GRID THEME CHUNG (KHÔNG NGHIỆP VỤ)
        // =========================
        public static void StyleGrid(GridView gv)
        {
            if (gv == null) return;

            gv.PaintStyleName = "Flat";

            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsView.EnableAppearanceOddRow = true;//xen kẽ màu
            gv.OptionsView.EnableAppearanceEvenRow = true;
            gv.OptionsView.RowAutoHeight = true;// tự co dãn chiều cao để xuống dòng được

            // ===== HEADER =====
            gv.Appearance.HeaderPanel.Font =
                new Font("Segoe UI", 9f, FontStyle.Bold);
            gv.Appearance.HeaderPanel.ForeColor = Color.Black;
            gv.Appearance.HeaderPanel.BackColor =
                Color.FromArgb(235, 240, 245);

            gv.Appearance.HeaderPanel.Options.UseFont = true;// bắt buộc để nhận style
            gv.Appearance.HeaderPanel.Options.UseForeColor = true;// bắt buộc để nhận style
            gv.Appearance.HeaderPanel.Options.UseBackColor = true;// bắt buộc để nhận style

            gv.Appearance.HeaderPanel.TextOptions.HAlignment =
                DevExpress.Utils.HorzAlignment.Center;
            gv.Appearance.HeaderPanel.TextOptions.WordWrap =
            DevExpress.Utils.WordWrap.Wrap;              // ⭐ BẮT BUỘC
            // ===== ROW =====
            gv.Appearance.Row.Font =
                new Font("Segoe UI", 9f);
            gv.Appearance.Row.Options.UseFont = true;

            gv.Appearance.OddRow.BackColor =
                Color.FromArgb(250, 250, 250);
            gv.Appearance.OddRow.Options.UseBackColor = true;

            gv.Appearance.EvenRow.BackColor = Color.White;
            gv.Appearance.EvenRow.Options.UseBackColor = true;

            // ===== SELECT / FOCUS =====
            gv.Appearance.FocusedRow.BackColor =
                Color.FromArgb(220, 230, 245);
            gv.Appearance.FocusedRow.ForeColor = Color.Black;

            gv.Appearance.SelectedRow.BackColor =
                Color.FromArgb(200, 220, 240);
            gv.Appearance.SelectedRow.ForeColor = Color.Black;

            gv.Appearance.FocusedRow.Options.UseBackColor = true;
            gv.Appearance.SelectedRow.Options.UseBackColor = true;

            gv.OptionsBehavior.Editable = false;
        }
        
        public static void ResetGridState(GridView gv)
        {
            if (gv == null) return;

            // Reset column state
            foreach (GridColumn col in gv.Columns)
            {
                col.OptionsColumn.FixedWidth = false;
                col.MinWidth = 0;
                col.Width = 100;
                col.Visible = true;
            }

            // Reset appearance
            gv.Appearance.Reset();

            // Reset row
            gv.RowHeight = 25;
            gv.OptionsView.RowAutoHeight = false;

            // Reset filter & sort
            gv.ActiveFilter.Clear();
            gv.ClearSorting();
        }

        // =========================
        // APPLY TO ALL CONTROLS
        // =========================
        public static void StyleAllControls(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Label lbl)
                    StyleLabel(lbl);
                else if (c is TextBox tb)
                    StyleTextBox(tb);
                else if (c is ComboBox cb)
                    StyleComboBox(cb);
                else if (c is Button btn)
                    StyleButton(btn);

                if (c.HasChildren)
                    StyleAllControls(c);
            }
        }
    }
}
