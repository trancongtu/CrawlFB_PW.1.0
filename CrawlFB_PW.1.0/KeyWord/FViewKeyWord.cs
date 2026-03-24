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
using CrawlFB_PW._1._0.Helper.Text;
using CrawlFB_PW._1._0.Topic;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.ViewModels.Keyword;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;

namespace CrawlFB_PW._1._0.KeyWord
{
    public partial class FViewKeyWord : Form
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        private List<KeywordViewModel> _allData;
        private KeywordFilterModel _filter = new KeywordFilterModel();

        public FViewKeyWord()
        {
            InitializeComponent();
            this.Load += FViewKeyWord_Load;
        
            // ===== GẮN EVENT CHO BAR EDIT =====          
        }
        public void LoadByTopic()
        {
            LoadKeywordByTopic(TopicId);
            InitKeywordGrid(); // ✅ BẮT BUỘC
            this.Text = $"Keyword - {TopicName}";
        }
        private void InitKeywordGrid()
        {
            var gv = gridView1;

            gv.BeginUpdate();
            try
            {
                gv.OptionsBehavior.AutoPopulateColumns = false;
                gv.Columns.Clear();
                gv.IndicatorWidth = 25;
                gv.Columns.AddVisible(nameof(KeywordViewModel.STT), "STT");
                // ===== SELECT =====
                var colSelect = gv.Columns.AddVisible(nameof(KeywordViewModel.Select), "✓");
                colSelect.OptionsColumn.AllowEdit = true;
                colSelect.Width = 40;

                var chkSelect = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                gridControl1.RepositoryItems.Add(chkSelect);
                colSelect.ColumnEdit = chkSelect;

                // ===== KEYWORD =====
                gv.Columns.AddVisible(nameof(KeywordViewModel.KeywordName), "Từ khóa");

                // ===== TOPIC COUNT =====
                gv.Columns.AddVisible(nameof(KeywordViewModel.CountTopic), "Số chủ đề");

                // ===== ATTENTION =====
                var colAttScore = gv.Columns.AddVisible(
                    nameof(KeywordViewModel.AttentionScore),
                    "Điểm theo dõi"
                );
                colAttScore.OptionsColumn.AllowEdit = true;

                var colAttLevel = gv.Columns.AddVisible(
                    nameof(KeywordViewModel.TrackingLevel),
                    "Level theo dõi"
                );
                colAttLevel.OptionsColumn.AllowEdit = true;

                // ===== NEGATIVE =====
                var colNegScore = gv.Columns.AddVisible(
                    nameof(KeywordViewModel.NegativeScore),
                    "Điểm tiêu cực"
                );
                colNegScore.OptionsColumn.AllowEdit = true;

                var colNegLevel = gv.Columns.AddVisible(
                    nameof(KeywordViewModel.NegativeLevel),
                    "Level tiêu cực"
                );
                colNegLevel.OptionsColumn.AllowEdit = true;

                var colCritical = gv.Columns.AddVisible(
                    nameof(KeywordViewModel.IsCritical),
                    "Xấu độc"
                );
                colCritical.OptionsColumn.AllowEdit = true;

                var chkCritical = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                gridControl1.RepositoryItems.Add(chkCritical);
                colCritical.ColumnEdit = chkCritical;

                // ===============================
                // 🔥 LOẠI TRỪ (SỬA QUAN TRỌNG)
                // ===============================
                var colExclude = gv.Columns.AddVisible(
                    nameof(KeywordViewModel.IsExcluded),
                    "Loại Trừ"
                );

                colExclude.OptionsColumn.AllowEdit = true;
                colExclude.Width = 70;

                var chkExclude = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                gridControl1.RepositoryItems.Add(chkExclude);
                colExclude.ColumnEdit = chkExclude;

                // ===== NOTE =====
                gv.Columns.AddVisible(nameof(KeywordViewModel.Note), "Ghi chú");

                // ===== ẨN ID =====
                var colId = gv.Columns[nameof(KeywordViewModel.KeywordId)];
                if (colId != null)
                    colId.Visible = false;

                gv.BestFitColumns();
            }
            finally
            {
                gv.EndUpdate();
            }
        }

        private void LoadKeywordGrid()
        {
            InitKeywordGrid();

            _allData = KeywordDAO.Instance.GetKeywordViewModels();

            ApplyFilter(); // CHỈ GỌI CÁI NÀY
        }
        private void InitFilterUI()
        {
            // ===== GROUP FILTER =====
            var repoGroup = cb_Filter.Edit as RepositoryItemComboBox;
            if (repoGroup != null)
            {
                repoGroup.Items.Clear();
                repoGroup.Items.Add("Tất cả");
                repoGroup.Items.Add("Theo dõi");
                repoGroup.Items.Add("Tiêu cực");
                repoGroup.Items.Add("Loại trừ");

                cb_Filter.EditValue = "Tất cả";
                _filter.Group = "Tất cả";

                cb_Filter.EditValueChanged += (s, e) =>
                {
                    _filter.Group = cb_Filter.EditValue?.ToString();
                    ApplyFilter();
                };
            }

            // ===== LEVEL FILTER =====
            var repoLevel = cb_LevelFilter.Edit as RepositoryItemComboBox;
            if (repoLevel != null)
            {
                repoLevel.Items.Clear();
                repoLevel.Items.Add("Tất cả");
                for (int i = 1; i <= 7; i++)
                    repoLevel.Items.Add(i);

                cb_LevelFilter.EditValue = "Tất cả";
            }

            cb_LevelFilter.EditValueChanged += cb_LevelFilter_EditValueChanged;

            // ===== CHECK CHƯA ĐIỂM =====
            cb_CheckScore.EditValue = false;
            cb_CheckScore.EditValueChanged += cb_CheckScore_EditValueChanged;
        }

        private void ApplyFilter()
        {
            if (_allData == null)
                return;

            IEnumerable<KeywordViewModel> query = _allData;

            // =========================
            // 1️⃣ Group filter
            // =========================
            switch (_filter.Group)
            {
                case "Theo dõi":
                    query = query.Where(x => x.AttentionScore > 0);
                    break;

                case "Tiêu cực":
                    query = query.Where(x => x.NegativeScore > 0);
                    break;

                case "Loại trừ":
                    query = query.Where(x => x.IsExcluded);
                    break;
            }

            // =========================
            // 2️⃣ Level filter
            // =========================
            // =========================
            // 2️⃣ Lọc theo Level
            // =========================
            if (_filter.Level.HasValue)
            {
                int lv = _filter.Level.Value;

                if (_filter.Group == "Theo dõi")
                {
                    query = query.Where(x => x.TrackingLevel == lv);
                }
                else if (_filter.Group == "Tiêu cực")
                {
                    query = query.Where(x => x.NegativeLevel == lv);
                }
                else if (_filter.Group == "Loại trừ")
                {
                    query = query.Where(x => x.ExcludeLevel == lv);
                }
                else
                {
                    // 🔥 TẤT CẢ
                    query = query.Where(x =>
                        x.TrackingLevel == lv ||
                        x.NegativeLevel == lv ||
                        x.ExcludeLevel == lv
                    );
                }
            }


            // =========================
            // 3️⃣ Chưa thêm điểm
            // =========================
            if (_filter.OnlyNoScore)
            {
                query = query.Where(x =>
                    !x.IsExcluded &&
                    x.AttentionScore == 0 &&
                    x.NegativeScore == 0
                );
            }

            // =========================
            // 4️⃣ Search text (🔥 QUAN TRỌNG)
            // =========================
            if (!string.IsNullOrWhiteSpace(_filter.SearchText))
            {
                string kw = _filter.SearchText;

                query = query
                    .Select(x => new
                    {
                        Item = x,
                        Score = TextSimilarity.MatchScoreVietnameseAdvanced(
                                    x.KeywordName, kw)
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Item);
            }
            var list = query.ToList();
            // 🔥 GÁN STT SAU KHI FILTER XONG
            for (int i = 0; i < list.Count; i++)
            {
                list[i].STT = i + 1;
            }
            gridControl1.DataSource = list;


        }

        private void cb_LevelFilter_EditValueChanged(object sender, EventArgs e)
        {
            if (int.TryParse(cb_LevelFilter.EditValue?.ToString(), out int lv))
                _filter.Level = lv;
            else
                _filter.Level = null;

            ApplyFilter();
        }
        private void cb_CheckScore_EditValueChanged(object sender, EventArgs e)
        {
            _filter.OnlyNoScore =
                Convert.ToBoolean(cb_CheckScore.EditValue ?? false);

            ApplyFilter();
        }
        private void cb_Filter_EditValueChanged(object sender, EventArgs e)
        {
            _filter.Group = cb_Filter.EditValue?.ToString();

            // Reset level khi đổi nhóm
            _filter.Level = null;
            cb_LevelFilter.EditValue = null;

            ApplyFilter();
        }


        private void LoadKeywordByTopic(int topicId)
        {
            var data = SQLDAO.Instance.GetKeywordsByTopic(topicId);

            gridControl1.DataSource = data;
        }

        private void FViewKeyWord_Load(object sender, EventArgs e)
        {
            InitFilterUI();
            LoadKeywordGrid();
            barEditItem1.EditWidth = 300;
            cb_Filter.EditWidth = 100;
            gridView1.ShowingEditor += gridView1_ShowingEditor;

            cb_Filter.EditValueChanged += cb_Filter_EditValueChanged;
            cb_LevelFilter.EditValueChanged += cb_LevelFilter_EditValueChanged;
            cb_CheckScore.EditValueChanged += cb_CheckScore_EditValueChanged;


            // Hiện filter panel (nếu thích)
            gridView1.OptionsView.ShowFilterPanelMode =
                DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.ShowAlways;

            // Highlight VIP
            gridView1.CustomDrawCell += gridView1_CustomDrawCell;

            // 🔥 CHỈ GẮN 1 EVENT SEARCH – Y HỆT SELECTPAGE
            barEditItem1.EditValueChanged += BarSearch_EditValueChanged;
        }
       
        // loại trừ


        // tìm kiếm
        private void barEditItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
           
        }
        private void gridView1_CustomDrawCell(object sender,DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            string keyword = barEditItem1.EditValue?.ToString();
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            if (e.RowHandle < 0 || e.CellValue == null)
                return;

            string cellText = e.CellValue.ToString();

            if (!TextNormalizeHelper.ContainsIgnoreCaseAndAccent(cellText, keyword))
                return;

            int index = IndexOfIgnoreCaseAndAccent(cellText, keyword);
            if (index < 0)
                return;

            e.Handled = true;

            var g = e.Graphics;
            var bounds = e.Bounds;
            var font = e.Appearance.Font;
            var normalBrush = e.Appearance.GetForeBrush(e.Cache);

            string before = cellText.Substring(0, index);
            string match = cellText.Substring(index, keyword.Length);
            string after = cellText.Substring(index + keyword.Length);

            SizeF sizeBefore = g.MeasureString(before, font);
            SizeF sizeMatch = g.MeasureString(match, font);

            g.DrawString(before, font, normalBrush, bounds.X, bounds.Y);

            var highlightRect = new RectangleF(
                bounds.X + sizeBefore.Width,
                bounds.Y,
                sizeMatch.Width,
                bounds.Height
            );

            g.FillRectangle(Brushes.Gold, highlightRect);
            g.DrawString(match, font, Brushes.Black, highlightRect.Location);

            g.DrawString(
                after,
                font,
                normalBrush,
                bounds.X + sizeBefore.Width + sizeMatch.Width,
                bounds.Y
            );
        }

        private int IndexOfIgnoreCaseAndAccent(string source, string keyword)
        {
            string normSource = TextNormalizeHelper.Normalize(source);
            string normKeyword = TextNormalizeHelper.Normalize(keyword);

            return normSource.IndexOf(normKeyword, StringComparison.Ordinal);
        }
        private void BarSearch_EditValueChanged(object sender, EventArgs e)
        {
            _filter.SearchText = barEditItem1.EditValue?.ToString()?.Trim();
            ApplyFilter();
        }

        //HẾT TÌM KIẾM THEO TỪ
        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            var gv = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (gv == null) return;

            string field = gv.FocusedColumn.FieldName;

            // Chỉ cho sửa điểm
            if (field != "AttentionScore" && field != "NegativeScore")
                return;

            bool isSelected = Convert.ToBoolean(gv.GetFocusedRowCellValue("Select"));

            // ❌ chưa tick Select → không cho edit
            if (!isSelected)
                e.Cancel = true;
        }
        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Select")
            {
                gridView1.RefreshRow(e.RowHandle);
            }
        }
        private void btn_Update_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            gridView1.CloseEditor();
            gridView1.UpdateCurrentRow();

            var list = gridControl1.DataSource as BindingList<KeywordViewModel>;
            if (list == null) return;

            var selected = list.Where(x => x.Select).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn keyword cần cập nhật");
                return;
            }

            // =========================
            // 1️⃣ VALIDATE
            // =========================
            foreach (var k in selected)
            {
                if (k.AttentionScore < 0 || k.AttentionScore > 30)
                {
                    MessageBox.Show($"Keyword '{k.KeywordName}'\nĐiểm theo dõi phải 0–30");
                    return;
                }

                if (k.NegativeScore < 0 || k.NegativeScore > 50)
                {
                    MessageBox.Show($"Keyword '{k.KeywordName}'\nĐiểm tiêu cực phải 0–50");
                    return;
                }

                if (k.TrackingLevel.HasValue && (k.TrackingLevel < 1 || k.TrackingLevel > 5))
                {
                    MessageBox.Show($"Keyword '{k.KeywordName}'\nLevel theo dõi phải 1–5");
                    return;
                }

                if (k.NegativeLevel.HasValue && (k.NegativeLevel < 1 || k.NegativeLevel > 5))
                {
                    MessageBox.Show($"Keyword '{k.KeywordName}'\nLevel tiêu cực phải 1–5");
                    return;
                }
            }

            // =========================
            // 2️⃣ SAVE DB
            // =========================
            int updated = 0;

            foreach (var k in selected)
            {
                // ===== ATTENTION =====
                if (k.AttentionScore > 0 && k.TrackingLevel.HasValue)
                {
                    if (SQLDAO.Instance.UpsertAttentionScore(
                            k.KeywordId,
                            k.AttentionScore,
                            k.TrackingLevel.Value,
                            k.Note))
                    {
                        updated++;
                    }
                }

                // ===== NEGATIVE =====
                if (k.NegativeScore > 0 && k.NegativeLevel.HasValue)
                {
                    if (SQLDAO.Instance.UpsertNegativeScore(
                            k.KeywordId,
                            k.NegativeScore,
                            k.NegativeLevel.Value,
                            k.IsCritical,
                            k.Note))
                    {
                        updated++;
                    }
                }
            }

            MessageBox.Show($"✔ Đã cập nhật {updated} keyword");
        }
        private void btn_addTopic_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var list = gridControl1.DataSource as List<KeywordViewModel>;
            if (list == null) return;

            var selectedKeywords = list.Where(x => x.Select).ToList();

            if (selectedKeywords.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn keyword trước");
                return;
            }

            using (var f = new FSelectTopic(selectedKeywords))
            {
                f.ShowDialog(); // ❗ modal
            }

            // sau khi form con đóng → reload lại grid nếu cần
            LoadKeywordGrid();
        }

        private void btn_Delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var row = gridView1.GetFocusedRow() as KeywordViewModel;
            if (row == null)
            {
                XtraMessageBox.Show("Chọn keyword cần xóa", "Thông báo");
                return;
            }

            var confirm = XtraMessageBox.Show(
                $"Xóa keyword '{row.KeywordName}'?\n" +
                "Sẽ xóa luôn:\n" +
                "- Mapping topic\n" +
                "- Điểm theo dõi\n" +
                "- Điểm tiêu cực",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            // 🔥 XÓA ĐẦY ĐỦ
            SQLDAO.Instance.DeleteKeywordFull(row.KeywordId);

            // reload lại grid
            LoadKeywordGrid(); 

            XtraMessageBox.Show("Đã xóa keyword", "OK");
        }
    }
}
