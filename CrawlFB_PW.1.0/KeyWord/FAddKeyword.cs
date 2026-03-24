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
using DevExpress.XtraBars;
using System.IO;    
namespace CrawlFB_PW._1._0.KeyWord
{
    public partial class FAddKeyword : Form
    {
        private BindingList<KeywordViewModel> _data;
      
        public FAddKeyword()
        {
            InitializeComponent();
            this.Load += FAddKeyword_Load;
            barEditItem1.EditWidth = 100;
        }
        private void FAddKeyword_Load(object sender, EventArgs e)
        {   
            InitGrid();
            InitLevelCombo(cb_AttentionLevel);          // AttentionLevel
            InitLevelCombo(cb_NegativeLevel);  // NegativeLevel
            InitExcludeLevelCombo(cb_ExcludeLevel);

            barEditItemtheodoi.EditValueChanged += (s, e1) => UpdateUIState();
            barEditItemtieucuc.EditValueChanged += (s, e2) => UpdateUIState();
            checkbox_Loaitru.EditValueChanged += (s, e3) => UpdateUIState();

            gridView1.CellValueChanged += gridView1_CellValueChanged;

        }
        private void UpdateSaveButtonState()
        {
            btn_Result.Enabled = _data != null && _data.Any(x => x.Select);
        }

        private void InitGrid()
        {
            _data = new BindingList<KeywordViewModel>();
            gridControl1.DataSource = _data;

            gridView1.OptionsBehavior.AutoPopulateColumns = false;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsView.ShowIndicator = true;
            gridView1.OptionsBehavior.Editable = true;

            UIGridHelper.EnableRowIndicatorSTT(gridView1);
            UIGridHelper.ApplySelect(gridView1, gridControl1);
            UIGridHelper.EnableRowClickToggleSelect(gridView1);

            gridView1.Columns.Clear();

            gridView1.Columns.AddVisible(nameof(KeywordViewModel.Select), "Chọn");
            gridView1.Columns.AddVisible(nameof(KeywordViewModel.KeywordName), "Keyword");

            gridView1.Columns.AddVisible(nameof(KeywordViewModel.AttentionScore), "Điểm theo dõi");
            gridView1.Columns.AddVisible(nameof(KeywordViewModel.TrackingLevel), "Level theo dõi");

            gridView1.Columns.AddVisible(nameof(KeywordViewModel.NegativeScore), "Điểm tiêu cực");
            gridView1.Columns.AddVisible(nameof(KeywordViewModel.NegativeLevel), "Level tiêu cực");
            // 🔥 CỘT LOẠI TRỪ
            var colExclude = gridView1.Columns.AddVisible(
                nameof(KeywordViewModel.IsExcluded),
                "Loại trừ"
            );

            // Gán checkbox editor
            var repoCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            gridControl1.RepositoryItems.Add(repoCheck);
            colExclude.ColumnEdit = repoCheck;
            // ===== THỨ TỰ =====
            gridView1.Columns[nameof(KeywordViewModel.Select)].VisibleIndex = 0;
            gridView1.Columns[nameof(KeywordViewModel.KeywordName)].VisibleIndex = 1;
        }     
        private void InitLevelCombo(BarEditItem cb)
        {
            var repo = cb.Edit as DevExpress.XtraEditors.Repository.RepositoryItemComboBox;
            if (repo == null) return;

            repo.Items.Clear();
            repo.Items.Add(""); // chưa chọn

            for (int i = 1; i <= 7; i++)
                repo.Items.Add(i);

            cb.EditValue = null;
            cb.Enabled = false; // 🔥 ban đầu disable
        }
        private void InitExcludeLevelCombo(BarEditItem cb)
        {
            var repo = cb.Edit as DevExpress.XtraEditors.Repository.RepositoryItemComboBox;
            if (repo == null) return;

            repo.Items.Clear();
            repo.Items.Add("");   // chưa chọn

            for (int i = 1; i <= 7; i++)
                repo.Items.Add(i);

            cb.EditValue = null;
            cb.Enabled = false;   // ban đầu disable
        }

        private void btn_Result_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để lưu");
                return;
            }

            var selectedItems = _data?.Where(x => x.Select).ToList();

            if (selectedItems == null || selectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Không có keyword nào được chọn để lưu",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }


            int added = 0;
            int updated = 0;
            int skipped = 0;

            try
            {
                SQLDAO.Instance.ExecuteInTransaction(() =>
                {
                    foreach (var k in selectedItems)
                    {
                        if (string.IsNullOrWhiteSpace(k.KeywordName))
                        {
                            skipped++;
                            continue;
                        }

                        int keywordId = SQLDAO.Instance.EnsureKeyword(k.KeywordName.Trim());

                        // 🚫 KEYWORD LOẠI TRỪ
                        if (k.IsExcluded)
                        {
                            SQLDAO.Instance.UpsertExcludeKeyword(
                             keywordId,
                             k.ExcludeLevel,
                             "Exclude from FAdd"
                         );
                            added++;
                            continue;
                        }

                        bool hasAnyScore = false;

                        // Attention
                        if (k.AttentionScore > 0 && k.TrackingLevel.HasValue)
                        {
                            if (SQLDAO.Instance.UpsertAttentionScore(
                                keywordId,
                                k.AttentionScore,
                                k.TrackingLevel.Value))
                            {
                                updated++;
                                hasAnyScore = true;
                            }
                        }

                        // Negative
                        if (k.NegativeScore > 0 && k.NegativeLevel.HasValue)
                        {
                            if (SQLDAO.Instance.UpsertNegativeScore(
                                keywordId,
                                k.NegativeScore,
                                k.NegativeLevel.Value,
                                k.IsCritical))
                            {
                                updated++;
                                hasAnyScore = true;
                            }
                        }

                        if (!hasAnyScore)
                        {
                            added++;
                        }
                    }
                });

                MessageBox.Show(
                    $"✔ Lưu xong\n" +
                    $"• Dòng đã chọn: {selectedItems.Count}\n" +
                    $"• Keyword mới / chưa chấm: {added}\n" +
                    $"• Gán / cập nhật điểm: {updated}\n" +
                    $"• Bỏ qua: {skipped}",
                    "Hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ Lưu thất bại, dữ liệu đã được rollback\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            btn_reset_ItemClick(null, null);

        }
        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data == null) return;

            _data.Clear();

            barEditItem1.EditValue = "";
            barEditItemtheodoi.EditValue = 0;
            barEditItemtieucuc.EditValue = 0;
        }
        private bool ValidateScore( string keyword,int attention,int negative)
        {
            if (attention < 0 || attention > 30)
            {
                MessageBox.Show(
                    $"Keyword: {keyword}\n" +
                    $"Điểm theo dõi phải từ 0 – 30",
                    "Điểm không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            if (negative < 0 || negative > 50)
            {
                MessageBox.Show(
                    $"Keyword: {keyword}\n" +
                    $"Điểm tiêu cực phải từ 0 – 50",
                    "Điểm không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            return true;
        }
        private void btn_createTemple_ItemClick(object sender, ItemClickEventArgs e)
        {
            var result = MessageBox.Show(
       "Chọn loại template cần tạo:\n\nYES = Keyword thường\nNO = Keyword loại trừ",
       "Tạo template keyword",
       MessageBoxButtons.YesNoCancel,
       MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
                return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel file (*.xlsx)|*.xlsx";
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                if (result == DialogResult.Yes)
                    ExcelTemplateHelper.CreateKeywordNormalTemplate(sfd.FileName);
                else
                    ExcelTemplateHelper.CreateKeywordExcludeTemplate(sfd.FileName);
            }
        }      
        private void btn_LoadKeywordFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel file (*.xlsx)|*.xlsx";
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                _data.Clear();

                using (var wb = new ClosedXML.Excel.XLWorkbook(ofd.FileName))
                {
                    var ws = wb.Worksheet(1);
                    int row = 2;

                    while (true)
                    {
                        if (ws.Cell(row, 2).IsEmpty())
                            break;

                        string keyword = ws.Cell(row, 2).GetString().Trim();

                        int attention = ws.Cell(row, 3).TryGetValue<int>(out var a) ? a : 0;
                        int attLevelRaw = ws.Cell(row, 4).TryGetValue<int>(out var al) ? al : 0;
                        int negative = ws.Cell(row, 5).TryGetValue<int>(out var n) ? n : 0;
                        int negLevelRaw = ws.Cell(row, 6).TryGetValue<int>(out var nl) ? nl : 0;

                        string excludeRaw = ws.Cell(row, 7).GetString().Trim();
                        bool isExcluded = excludeRaw.Equals("Yes", StringComparison.OrdinalIgnoreCase);

                        if (string.IsNullOrWhiteSpace(keyword))
                        {
                            row++;
                            continue;
                        }

                        // 🔥 validate điểm
                        if (!ValidateScore(keyword, attention, negative))
                        {
                            MessageBox.Show(
                                $"Lỗi tại dòng {row}\nDòng này sẽ bị bỏ qua",
                                "File không hợp lệ",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                            row++;
                            continue;
                        }

                        _data.Add(new KeywordViewModel
                        {
                            Select = false,
                            KeywordName = keyword,
                            AttentionScore = attention,
                            TrackingLevel = attLevelRaw > 0 ? attLevelRaw : (int?)null,
                            NegativeScore = negative,
                            NegativeLevel = negLevelRaw > 0 ? negLevelRaw : (int?)null,
                            IsExcluded = isExcluded
                        });

                        row++;
                    }
                }
            }
            UpdateSaveButtonState();
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }
        private void btn_AddkeywordToTable_ItemClick(object sender, ItemClickEventArgs e)
        {
            string keyword = barEditItem1.EditValue?.ToString().Trim() ?? "";
            bool isExcluded = Convert.ToBoolean(checkbox_Loaitru.EditValue ?? false);

            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Vui lòng nhập keyword");
                return;
            }

            // ❌ Chặn trùng trong danh sách
            if (_data.Any(x => x.KeywordName.Equals(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Keyword đã tồn tại trong danh sách");
                return;
            }

            int attention = Convert.ToInt32(barEditItemtheodoi.EditValue ?? 0);
            int negative = Convert.ToInt32(barEditItemtieucuc.EditValue ?? 0);

            int? attLevel = null;
            int? negLevel = null;
            int? excludeLevel = null;

            if (!isExcluded)
            {
                // ===== KEYWORD THƯỜNG =====
                if (!ValidateScore(keyword, attention, negative))
                    return;

                if (cb_AttentionLevel.Enabled &&
                    cb_AttentionLevel.EditValue != null &&
                    int.TryParse(cb_AttentionLevel.EditValue.ToString(), out int al))
                    attLevel = al;

                if (cb_NegativeLevel.Enabled &&
                    cb_NegativeLevel.EditValue != null &&
                    int.TryParse(cb_NegativeLevel.EditValue.ToString(), out int nl))
                    negLevel = nl;
            }
            else
            {
                if (isExcluded)
                {
                    if (cb_ExcludeLevel.EditValue == null)
                    {
                        MessageBox.Show("Vui lòng chọn Level loại trừ");
                        return;
                    }

                    excludeLevel = Convert.ToInt32(cb_ExcludeLevel.EditValue);
                }

            }

            _data.Add(new KeywordViewModel
            {
                Select = true,   // vẫn auto select để tránh quên lưu
                KeywordName = keyword,

                IsExcluded = isExcluded,
                ExcludeLevel = excludeLevel,   // ✅ CHO PHÉP NULL

                AttentionScore = isExcluded ? 0 : attention,
                TrackingLevel = isExcluded ? null : attLevel,

                NegativeScore = isExcluded ? 0 : negative,
                NegativeLevel = isExcluded ? null : negLevel
            });

            // ===== RESET UI =====
            barEditItem1.EditValue = "";
            barEditItemtheodoi.EditValue = 0;
            barEditItemtieucuc.EditValue = 0;

            checkbox_Loaitru.EditValue = false;
            cb_ExcludeLevel.EditValue = null;
            UpdateSaveButtonState();
        }
        private void btn_SelectAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_data == null || _data.Count == 0)
                return;

            bool selectAll = _data.Any(x => !x.Select);

            foreach (var item in _data)
            {
                item.Select = selectAll;
            }
            btn_Result.Enabled = Enabled;
            gridView1.RefreshData();
        }

        private void btn_reset_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_data == null) return;

            // 1️⃣ Clear toàn bộ grid
            _data.Clear();

            // 2️⃣ Reset UI nhập liệu
            barEditItem1.EditValue = "";
            barEditItemtheodoi.EditValue = 0;
            barEditItemtieucuc.EditValue = 0;

            cb_AttentionLevel.EditValue = null;
            cb_NegativeLevel.EditValue = null;
            cb_ExcludeLevel.EditValue = null;

            checkbox_Loaitru.EditValue = false;

            // 3️⃣ Disable lại các level (vì mặc định chưa có điểm)
            cb_AttentionLevel.Enabled = false;
            cb_NegativeLevel.Enabled = false;
        }
        private void gridView1_CellValueChanged( object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var row = gridView1.GetRow(e.RowHandle) as KeywordViewModel;
            if (row == null) return;

            if (e.Column.FieldName == "IsExcluded")
            {
                if (row.IsExcluded)
                {
                    row.Select = true;

                    row.AttentionScore = 0;
                    row.TrackingLevel = null;

                    row.NegativeScore = 0;
                    row.NegativeLevel = null;
                }
            }
            if (e.Column.FieldName == "Select")
            {
                UpdateSaveButtonState();
            }

        }

        private void UpdateUIState()
        {
            int attention = Convert.ToInt32(barEditItemtheodoi.EditValue ?? 0);
            int negative = Convert.ToInt32(barEditItemtieucuc.EditValue ?? 0);
            bool isExcluded = Convert.ToBoolean(checkbox_Loaitru.EditValue ?? false);

            bool hasScore = attention > 0 || negative > 0;

            // ===== TRẠNG THÁI LOẠI TRỪ =====
            checkbox_Loaitru.Enabled = !hasScore;

            if (hasScore)
            {
                checkbox_Loaitru.EditValue = false;
            }

            // ===== TRẠNG THÁI CHO ĐIỂM =====
            barEditItemtheodoi.Enabled = !isExcluded;
            barEditItemtieucuc.Enabled = !isExcluded;

            cb_AttentionLevel.Enabled = !isExcluded && attention > 0;
            cb_NegativeLevel.Enabled = !isExcluded && negative > 0;

            // ===== LEVEL LOẠI TRỪ =====
            cb_ExcludeLevel.Enabled = isExcluded;

            if (isExcluded)
            {
                barEditItemtheodoi.EditValue = 0;
                barEditItemtieucuc.EditValue = 0;
                cb_AttentionLevel.EditValue = null;
                cb_NegativeLevel.EditValue = null;
            }
            else
            {
                cb_ExcludeLevel.EditValue = null;
            }
        }

    }
}
