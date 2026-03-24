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
using DevExpress.XtraEditors;
using DevExpress.Utils;
using CrawlFB_PW._1._0.ViewModels.phan_tich;
using CrawlFB_PW._1._0.DAO.phantich;
using DevExpress.XtraGrid.Views.Grid;
using System.Text.RegularExpressions;
using CrawlFB_PW._1._0.ViewModels.Keyword;
using CrawlFB_PW._1._0.Dashbroad;
using CrawlFB_PW._1._0.Helper.Mapper;
namespace CrawlFB_PW._1._0.UC.phantich
{
    public partial class UCViewAnalyzeNegative : UserControl
    {
        private Dictionary<int, string> _keywordCache;
        public UCViewAnalyzeNegative()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.UCViewAnalyzeNegative_Load);
        }
        private void UCViewAnalyzeNegative_Load(object sender, EventArgs e)
        {
            // mặc định
            btn_Maxday.EditValue = 36500;
            txb_Maxpost.EditValue = 100;
            LoadKeywordCache();

            LoadGridData();
          
        }
        private void LoadKeywordCache()
        {
                _keywordCache = new Dictionary<int, string>();

                var dt = SQLDAO.Instance.ExecuteQuery(@"
            SELECT KeywordId, KeywordName
            FROM TableKeyword
        ");

                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["KeywordId"]);
                    string name = row["KeywordName"].ToString();
                    _keywordCache[id] = name;
                }
        }
        private void SetupGrid()
        {
            var gv = gridView1;
            var grid = gridControl1;

            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsBehavior.Editable = true;
            gv.OptionsView.RowAutoHeight = true;

            gv.OptionsSelection.EnableAppearanceFocusedRow = false;
            gv.OptionsSelection.EnableAppearanceFocusedCell = false;
            gv.OptionsSelection.EnableAppearanceHideSelection = false;

            UIGridHelper.EnableRowIndicatorSTT(gv);
            UIGridHelper.ApplySelect(gv, grid);
            UIGridHelper.EnableRowClickToggleSelect(gv);
            UIGridHelper.LockAllColumnsExceptSelect(gv);
          
            UIGridHelper.ApplyVietnameseCaption(gv);

            UIGridHelper.ShowOnlyColumns(gv,
                "Select",
                "PostContent",
                "RealPostTime",
                "AttentionScore",
                "AttentionLevel",
                "NegativeScore",
                "NegativeLevel",
                "ResultLevel",
                "ViewDetail"
            );

            // ====== CỘT NỘI DUNG ======
            var col = gv.Columns["PostContent"];
            if (col != null)
            {
                col.Caption = "Nội dung";
                col.Width = 500;

                var memo = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
                memo.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True; // 👈 ĐÚNG CHỖ
                grid.RepositoryItems.Add(memo);

                col.ColumnEdit = memo;

                col.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
                col.AppearanceCell.Options.UseTextOptions = true;
            }

            // ====== FORMAT THỜI GIAN ======
            if (gv.Columns["RealPostTime"] != null)
            {
                gv.Columns["RealPostTime"].Caption = "Thời gian";
                gv.Columns["RealPostTime"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gv.Columns["RealPostTime"].DisplayFormat.FormatString = "dd/MM/yyyy HH:mm";
                gv.Columns["RealPostTime"].Width = 120;
            }

            CenterColumn(gv, "AttentionScore");
            CenterColumn(gv, "AttentionLevel");
            CenterColumn(gv, "NegativeScore");
            CenterColumn(gv, "NegativeLevel");
            CenterColumn(gv, "ResultLevel");

            // ====== CỘT CHI TIẾT ======
            if (gv.Columns["ViewDetail"] != null)
            {
                gv.Columns["ViewDetail"].Caption = "Chi tiết";
                gv.Columns["ViewDetail"].Width = 70;

                // 🔓 MỞ KHÓA CỘT
                gv.Columns["ViewDetail"].OptionsColumn.AllowEdit = true;
                gv.Columns["ViewDetail"].OptionsColumn.ReadOnly = false;
                gv.Columns["ViewDetail"].OptionsColumn.AllowFocus = true;

                var linkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit
                {
                    SingleClick = true,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
                };

                linkEdit.OpenLink += (s, e) =>
                {
                    var row = gv.GetFocusedRow() as NegativeMonitorViewModel;
                    if (row == null) return;

                    var dto = AnalyzeSQLDAO.Instance.GetFullPostById(row.PostID);
                    if (dto == null)
                    {
                        XtraMessageBox.Show("Không tìm thấy bài viết.");
                        return;
                    }

                    var vm = dto.ToViewModel();
                    new FPostDashboardHTML(vm).ShowDialog();
                };

                grid.RepositoryItems.Add(linkEdit);
                gv.Columns["ViewDetail"].ColumnEdit = linkEdit;
            }
            gv.CellValueChanged += GridView_CellValueChanged;
        }
        private void GridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Select")
            {
                gridView1.RefreshRow(e.RowHandle);
            }
        }
        private void CenterColumn(GridView gv, string field)
        {
            if (gv.Columns[field] != null)
            {
                gv.Columns[field].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }
        }

        private void LoadGridData()
        {
            if (!AnalyzeSQLDAO.Instance.HasAnyEvaluation())
            {
                XtraMessageBox.Show("Chưa convert dữ liệu.");
                gridControl1.DataSource = null;
                return;
            }

            int days = GetDaysFromUI();
            int maxPost = GetMaxPostFromUI();

            DateTime fromDate;

            if (days <= 0 || days > 30000)
                fromDate = new DateTime(1753, 1, 1);
            else
                fromDate = DateTime.Now.AddDays(-days);

            var dt = AnalyzeSQLDAO.Instance.GetNegativeMonitorPosts(fromDate, maxPost);
            var list = new List<NegativeMonitorViewModel>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new NegativeMonitorViewModel
                {
                    Select = false,
                    PostID = row["PostID"]?.ToString(),
                    PostContent = row["PostContent"]?.ToString(),
                    RealPostTime = row["RealPostTime"] as DateTime?,
                    AttentionScore = Convert.ToInt32(row["AttentionScore"]),
                    AttentionLevel = Convert.ToInt32(row["AttentionLevel"]),
                    NegativeScore = Convert.ToInt32(row["NegativeScore"]),
                    NegativeLevel = Convert.ToInt32(row["NegativeLevel"]),
                    ResultLevel = Convert.ToInt32(row["ResultLevel"]),
                    // 🔥 QUAN TRỌNG
                    AttentionKeywordIdsJson = row["AttentionKeywordIds"]?.ToString(),
                    NegativeKeywordIdsJson = row["NegativeKeywordIds"]?.ToString()
                });
            }

            gridControl1.DataSource = list;
            SetupGrid();
        }
        private int GetDaysFromUI()
        {
            int defaultDays = 7;

            if (btn_Maxday?.EditValue == null)
                return defaultDays;

            if (int.TryParse(btn_Maxday.EditValue.ToString(), out int days) && days > 0)
                return days;

            btn_Maxday.EditValue = defaultDays;
            return defaultDays;
        }

        private int GetMaxPostFromUI()
        {
            int defaultMax = 20;

            if (txb_Maxpost?.EditValue == null)
                return defaultMax;

            if (int.TryParse(txb_Maxpost.EditValue.ToString(), out int max) && max > 0)
                return max;

            txb_Maxpost.EditValue = defaultMax;
            return defaultMax;
        }
        private void btn_Maxday_EditValueChanged(object sender, EventArgs e)
        {
            LoadGridData();
        }

        private void txb_Maxpost_EditValueChanged(object sender, EventArgs e)
        {
            LoadGridData();
        }

        private void btn_CheckKeyword_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            var list = gridControl1.DataSource as List<NegativeMonitorViewModel>;
            if (list == null) return;

            var selectedPosts = list
                .Where(x => x.Select)
                .ToList();

            if (selectedPosts.Count == 0)
            {
                XtraMessageBox.Show("Chưa chọn bài nào.");
                return;
            }

            var postList = new List<PostHighlightDTO>();

            foreach (var row in selectedPosts)
            {
                var attentionMatches =
                    string.IsNullOrEmpty(row.AttentionKeywordIdsJson)
                    ? new List<KeywordMatchDTO>()
                    : JsonHelper.Deserialize<List<KeywordMatchDTO>>(row.AttentionKeywordIdsJson);

                var negativeMatches =
                    string.IsNullOrEmpty(row.NegativeKeywordIdsJson)
                    ? new List<KeywordMatchDTO>()
                    : JsonHelper.Deserialize<List<KeywordMatchDTO>>(row.NegativeKeywordIdsJson);

                postList.Add(new PostHighlightDTO
                {
                    PostId = row.PostID,
                    Content = row.PostContent,
                    Attention = attentionMatches,
                    Negative = negativeMatches
                });
            }

            var form = new FcheckkeywordPost(postList);
            form.ShowDialog();
        }

        private void btn_SelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var list = gridControl1.DataSource as List<NegativeMonitorViewModel>;
            if (list == null) return;

            bool isAllSelected = list.All(x => x.Select);

            foreach (var item in list)
            {
                item.Select = !isAllSelected;
            }

            gridView1.RefreshData();
        }
    }

}
