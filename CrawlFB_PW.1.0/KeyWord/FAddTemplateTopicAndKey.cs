using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.KeyWord
{
    public partial class FAddTemplateTopicAndKey : Form
    {
        private BindingList<TopicKeywordTemplateVM> _data;

        public FAddTemplateTopicAndKey()
        {
            InitializeComponent();
            this.Load += FAddTemplateTopicAndKey_Load;
        }

        private void FAddTemplateTopicAndKey_Load(object sender, EventArgs e)
        {
            InitGrid();
        }

        private void InitGrid()
        {
            _data = new BindingList<TopicKeywordTemplateVM>();
            gridControl1.DataSource = _data;

            var gv = gridView1;
            gv.OptionsBehavior.AutoPopulateColumns = false;
            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsBehavior.Editable = false;
            gv.Columns.Clear();

            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.STT), "STT");
            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.TopicName), "Topic");
            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.KeywordName), "Keyword");
            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.Type), "Loại");
            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.Level), "Level");
            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.Score), "Điểm");
            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.IsCritical), "Quan Trọng");
            gv.Columns.AddVisible(nameof(TopicKeywordTemplateVM.Note), "Ghi chú");


            gv.BestFitColumns();
        }

        private void btn_loadTemple_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel (*.xlsx)|*.xlsx";
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                LoadTemplateExcel(ofd.FileName);
            }
        }
        private void LoadTemplateExcel(string filePath)
        {
            _data.Clear();

            using (var wb = new XLWorkbook(filePath))
            {
                var ws = wb.Worksheets.First();
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                for (int row = 2; row <= lastRow; row++)
                {
                    string topic = ws.Cell(row, 1).GetString().Trim();
                    string keyword = ws.Cell(row, 2).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(topic) &&
                        string.IsNullOrWhiteSpace(keyword))
                        continue;

                    var vm = new TopicKeywordTemplateVM
                    {
                        STT = ws.Cell(row, 1).GetValue<int>(),   // ✅ lấy STT

                        TopicName = ws.Cell(row, 2).GetString().Trim(),
                        KeywordName = ws.Cell(row, 3).GetString().Trim(),

                        Type = ws.Cell(row, 4).GetString().Trim(),
                        Level = ws.Cell(row, 5).IsEmpty()
            ? (int?)null
            : ws.Cell(row, 5).GetValue<int>(),


                        Score = ws.Cell(row, 6).GetValue<int>(),
                        IsCritical = ws.Cell(row, 7).GetValue<int>() == 1,
                        Note = ws.Cell(row, 8).GetString()
                    };


                    _data.Add(vm);
                }
            }

            MessageBox.Show($"✔ Đã nạp {_data.Count} dòng từ template Topic + Keyword");
        }

        private void btn_Save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để lưu");
                return;
            }

            int added = 0;

            foreach (var r in _data)
            {
                if (string.IsNullOrWhiteSpace(r.KeywordName))
                    continue;

                // 1️⃣ Ensure keyword
                int keywordId = SQLDAO.Instance.EnsureKeyword(r.KeywordName);

                // 2️⃣ Topic (nếu có)
                if (!string.IsNullOrWhiteSpace(r.TopicName))
                {
                    int topicId = SQLDAO.Instance.EnsureTopic(r.TopicName);
                    SQLDAO.Instance.AddKeywordToTopic(keywordId, topicId);
                }

                // 3️⃣ Save theo TYPE
                SaveKeywordByType(keywordId, r);

                added++;
            }

            MessageBox.Show($"✔ Đã import {added} keyword vào hệ thống");
        }
        private void SaveKeywordByType(int keywordId, TopicKeywordTemplateVM r)
        {
            // Nếu chưa có điểm hoặc chưa có level → chỉ tạo keyword/topic
            if (r.Score <= 0 || !r.Level.HasValue)
                return;

            switch (r.Type)
            {
                case "Theo dõi":
                    SQLDAO.Instance.UpsertAttentionScore(
                        keywordId,
                        r.Score,
                        r.Level.Value,
                        r.Note
                    );
                    break;

                case "Tiêu cực":
                    SQLDAO.Instance.UpsertNegativeScore(
                        keywordId,
                        r.Score,
                        r.Level.Value,
                        r.IsCritical,
                        r.Note
                    );
                    break;
                case "Loại trừ":
                    SQLDAO.Instance.InsertOrUpdateExcludeKeyword(
                        keywordId,
                        r.Level,    // ✔ có thể null
                        r.Note
                    );
                    break;

            }
        }


        private void btn_Cannel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }
    }
}
