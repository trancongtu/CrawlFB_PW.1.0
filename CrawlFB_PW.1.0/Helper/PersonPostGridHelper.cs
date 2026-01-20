using System;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using CrawlFB_PW._1._0.ViewModels;
public static class PersonPostGridHelper
{
    public static void Apply(GridView gv, GridControl grid)
    {
        gv.OptionsView.ColumnAutoWidth = false;
        gv.OptionsView.RowAutoHeight = true;
        gv.OptionsBehavior.Editable = true;
        gv.OptionsSelection.EnableAppearanceFocusedCell = false;

        gv.Appearance.HeaderPanel.Font =
            new Font("Segoe UI", 9, FontStyle.Bold);

        gv.Appearance.Row.Font =
            new Font("Segoe UI", 9);

        ApplyColumns(gv);
        ApplyHyperLink(gv, grid);
    }

    private static void ApplyColumns(GridView gv)
    {
        gv.Columns["Select"].Width = 50;

        gv.Columns["PersonName"].Caption = "Người đăng";
        gv.Columns["PostTime"].Caption = "Thời gian";
        gv.Columns["PostStatus"].Caption = "Trạng thái";

        gv.Columns["Like"].Caption = "👍";
        gv.Columns["Comment"].Caption = "💬";
        gv.Columns["Share"].Caption = "🔁";

        gv.Columns["Content"].Caption = "Nội dung";
        gv.Columns["Content"].Width = 350;
    }

    private static void ApplyHyperLink(GridView gv, GridControl grid)
    {
        var repo = new RepositoryItemHyperLinkEdit
        {
            Caption = "Xem",
            SingleClick = true
        };

        grid.RepositoryItems.Add(repo);
        gv.Columns["PostLink"].ColumnEdit = repo;
    }
}
