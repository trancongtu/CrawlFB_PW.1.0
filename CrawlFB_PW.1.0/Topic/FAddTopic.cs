using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DTO;
using System.IO;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Helper;
using DevExpress.Utils.Extensions;
namespace CrawlFB_PW._1._0.Topic
{
    public partial class FAddTopic : Form
    {
        private BindingList<TopicViewModel> _data;
        public FAddTopic()
        {
            InitializeComponent();
            this.Load += FAddTopic_Load;
            barEditItem1.EditWidth = 200;
        }
        private void FAddTopic_Load(object sender, EventArgs e)
        {
            InitGrid();
        }
        private void InitGrid()
        {
            _data = new BindingList<TopicViewModel>();
            gridControl1.DataSource = _data;
            gridView1.OptionsBehavior.AutoPopulateColumns = false;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsView.ShowIndicator = false;
            gridView1.OptionsBehavior.Editable = true;
            UIGridHelper.EnableRowIndicatorSTT(gridView1);
            gridView1.Columns.Clear();
            gridView1.Columns.AddVisible(nameof(TopicViewModel.STT), "STT").OptionsColumn.AllowEdit = false;
            gridView1.Columns.AddVisible(nameof(TopicViewModel.TopicName), "Tên chủ đề");
        }
        private void btn_Save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để lưu");
                return;
            }

            int added = 0;
            int skipped = 0;

            foreach (var t in _data)
            {
                string name = t.TopicName?.Trim();
                if (string.IsNullOrEmpty(name))
                    continue;

                bool ok = SQLDAO.Instance.AddTopicIfNotExists(name);

                if (ok) added++;
                else skipped++;
            }

            MessageBox.Show(
                $"✔ Lưu thành công\n" +
                $"• Thêm mới: {added}\n" +
                $"• Bỏ qua: {skipped}"
            );
        }
        private void btn_reset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data == null) return;
            _data.Clear();          // 🔥 xóa toàn bộ dữ liệu trên grid
            barEditItem1.EditValue = "";   // clear ô nhập (nếu có)
        }
        private void btn_Cannel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }
        private void btn_AddTable_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string topicName =
        barEditItem1.EditValue?.ToString().Trim() ?? "";

            if (string.IsNullOrEmpty(topicName))
            {
                MessageBox.Show("Vui lòng nhập tên chủ đề");
                return;
            }

            _data.Add(new TopicViewModel
            {
                STT = _data.Count + 1,
                TopicName = topicName
            });

            barEditItem1.EditValue = "";
        }
        private void btn_LoadFile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text file (*.txt)|*.txt";
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var list = new List<TopicDTO>();

                var lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length < 2) continue;

                    string topicName = parts[1].Trim();
                    if (string.IsNullOrEmpty(topicName)) continue;

                    list.Add(new TopicDTO
                    {
                        TopicId = 0,
                        TopicName = topicName
                    });
                }

                gridControl1.DataSource = list;
            }
        }
        private void CreateTopicTemplate(string filePath)
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Topic");

                ws.Cell("A1").Value = "STT";
                ws.Cell("B1").Value = "TopicName";

                ws.Range("A1:B1").Style.Font.Bold = true;
                ws.Range("A1:B1").Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Range("A1:B1").Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightGray;

                ws.Cell("A2").Value = 1;
                ws.Cell("B2").Value = "Kinh tế";

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                wb.SaveAs(filePath);
            }
        }

        private void btn_CreatTemple_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel file (*.xlsx)|*.xlsx";
                sfd.FileName = "Template_Topic.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                CreateTopicTemplate(sfd.FileName);

                MessageBox.Show("✔ Đã tạo template Topic");
            }
        }
    }
}
