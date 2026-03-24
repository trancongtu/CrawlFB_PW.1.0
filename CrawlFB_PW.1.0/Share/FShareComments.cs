using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.Share
{
    public partial class FShareComments : Form
    {
        public FShareComments(List<CommentGridRow> comments)
        {
            InitializeComponent();

            gridControl1.DataSource = comments;

            gridView1.PopulateColumns();
            if (gridView1.Columns[nameof(CommentGridRow.RealPostTime)] != null)
                gridView1.Columns[nameof(CommentGridRow.RealPostTime)].Visible = false;
            // ===== CAPTION =====
           
            gridView1.Columns[nameof(CommentGridRow.Select)].Caption = "";
            gridView1.Columns[nameof(CommentGridRow.ActorName)].Caption = "Người bình luận";
            gridView1.Columns[nameof(CommentGridRow.IDFBPerson)].Caption = "ID FB";
            gridView1.Columns[nameof(CommentGridRow.PosterFBType)].Caption = "Loại";
            gridView1.Columns[nameof(CommentGridRow.Time)].Caption = "Thời gian";
            gridView1.Columns[nameof(CommentGridRow.LinkView)].Caption = "Link";
            gridView1.Columns[nameof(CommentGridRow.Content)].Caption = "Nội dung";

            // ===== THỨ TỰ CỘT =====
            gridView1.Columns[nameof(CommentGridRow.Select)].VisibleIndex = 0;
            gridView1.Columns[nameof(CommentGridRow.ActorName)].VisibleIndex = 1;
            gridView1.Columns[nameof(CommentGridRow.IDFBPerson)].VisibleIndex = 2;
            gridView1.Columns[nameof(CommentGridRow.PosterFBType)].VisibleIndex = 3;
            gridView1.Columns[nameof(CommentGridRow.Time)].VisibleIndex = 4;
            gridView1.Columns[nameof(CommentGridRow.LinkView)].VisibleIndex = 5;
            gridView1.Columns[nameof(CommentGridRow.Content)].VisibleIndex = 6;

            UIGridHelper.EnableRowIndicatorSTT(gridView1);
            UIGridHelper.ApplyCommentGridStyle(gridView1);
            UIGridHelper.ApplySelect(gridView1, gridControl1);
            UIGridHelper.ApplyCommentLink(gridView1, gridControl1);
            gridView1.Columns[nameof(CommentGridRow.Link)].Visible = false;
            UIGridHelper.LockAllColumnsExceptLinks(gridView1);

            gridView1.RefreshData();
        }
    }
}
