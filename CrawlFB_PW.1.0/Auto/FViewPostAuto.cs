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

namespace CrawlFB_PW._1._0.Auto
{
    public partial class FViewPostAuto : Form
    {
        private BindingList<PostInfoViewModel> _postList;

        public FViewPostAuto(BindingList<PostInfoViewModel> postList)
        {
            InitializeComponent();
            _postList = postList;
            this.Load += FViewPostAuto_Load;
        }
        private void FViewPostAuto_Load(object sender, EventArgs e)
        {
            gridControl1.DataSource = _postList;

            gridControl1.ForceInitialize();
            gridView1.RefreshData();

            gridView1.Tag = null;

            InitGridPost();
        }
        private void InitGridPost()
        {
            var gv = gridView1;

            if (gv.Tag as string == "INIT_DONE")
                return;

            gv.BeginUpdate();
            try
            {
                gv.PopulateColumns();

                gv.OptionsBehavior.Editable = true;

                // ===== STT =====
                UIGridHelper.EnableRowIndicatorSTT(gv);

                // ===== Caption =====
                UIGridHelper.ApplyVietnameseCaption(gv);

                // ===== CHỈ HIỆN CỘT CẦN =====
                UIGridHelper.ShowOnlyColumns(
                    gv,
                    "PostLink",
                    "TimeView",
                    "Content",
                    "AttachmentView",
                    "Like",
                    "Share",
                    "Comment",
                    "PostType",
                    "PosterName",
                    "PosterLink",
                    "PosterNote",
                    "PageName",
                    "PageLink",
                    "ContainerType"
                );

                // ===== LINK CLICK =====
                UIGridHelper.ApplyHyperlinkColumn(gridView1, gridControl1, "PostLink", "🔗 Mở Bài");
                UIGridHelper.ApplyHyperlinkColumn(gridView1, gridControl1, "PageLink", "📄 Mở Page");
                UIGridHelper.ApplyHyperlinkColumn(gridView1, gridControl1, "PosterLink", "👤 Mở Người đăng");

                // ===== ATTACHMENT =====
                UIGridHelper.ApplyAttachmentLink(gridView1, gridControl1, "AttachmentView");

                // ===== TOOLTIP =====
                UIGridHelper.ApplyLinkTooltip(gridView1, gridControl1);

                // ===== LOCK =====
                UIGridHelper.LockAllColumnsExceptLinks(gv);

                gv.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
                gv.OptionsSelection.EnableAppearanceFocusedCell = false;
                gv.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;

                gv.Tag = "INIT_DONE";
            }
            finally
            {
                gv.EndUpdate();
            }
        }
    }
}
