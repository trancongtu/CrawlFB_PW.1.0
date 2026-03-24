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
using CrawlFB_PW._1._0.KeyWord;
using CrawlFB_PW._1._0.ViewModels;
using System.IO;
using DevExpress.XtraBars;
namespace CrawlFB_PW._1._0.Topic
{
    public partial class FTopicView : Form
    {
        private List<TopicViewModel> _data;
        public FTopicView()
        {
            InitializeComponent();
            this.Load += FTopicView_Load;
        }

        private void FTopicView_Load(object sender, EventArgs e)
        {
            LoadTopicGrid();
            InitTopicGrid();
            Edit_ShearchTopic.EditWidth = 300;
        }

        private void LoadTopicGrid()
        {
            _data = TopicDAO.Instance.GetTopicViewModels();
            gridControl1.DataSource = _data;
        }

        private void InitTopicGrid()
        {
            var gv = gridView1;
            gv.BeginUpdate();
            try
            {
                gv.PopulateColumns();

                UIGridHelper.EnableRowIndicatorSTT(gv);
                UIGridHelper.ApplySelect(gv, gridControl1);
                UIGridHelper.LockAllColumnsExceptSelect(gv);
                UIGridHelper.ApplyVietnameseCaption(gv);

                UIGridHelper.ShowOnlyColumns(
                    gv,
                    "Select",
                    "TopicName",
                    "CountKeyword"
                );

                if (gv.Columns["TopicId"] != null)
                    gv.Columns["TopicId"].Visible = false;

                gv.IndicatorWidth = 30;
                gv.BestFitColumns();
            }
            finally
            {
                gv.EndUpdate();
            }
        }

        private void btn_Delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selected = _data.Where(x => x.Select).ToList();
            if (!selected.Any())
            {
                MessageBox.Show("Vui lòng chọn chủ đề cần xóa!");
                return;
            }

            if (MessageBox.Show(
                $"Xóa {selected.Count} chủ đề?\n(Sẽ xóa mapping keyword)",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            ) != DialogResult.Yes)
                return;

            foreach (var t in selected)
                SQLDAO.Instance.DeleteTopicFull(t.TopicId);

            LoadTopicGrid();
        }

        private void btn_Edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selected = _data.Where(x => x.Select).ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn 1 chủ đề!");
                return;
            }

            if (selected.Count > 1)
            {
                MessageBox.Show("Chỉ sửa từng chủ đề!");
                return;
            }

            var topic = selected.First();
            using (var f = new FUpdateTopic(topic.TopicId, topic.TopicName))
            {
                if (f.ShowDialog() == DialogResult.OK)
                    LoadTopicGrid();
            }
        }

        private void btn_ViewKeyWord_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selected = _data.Where(x => x.Select).ToList();

            if (selected.Count != 1)
            {
                MessageBox.Show("Vui lòng chọn đúng 1 chủ đề!");
                return;
            }

            var topic = selected.First();
            var main = MdiParent as FMain;
            if (main == null) return;

            var f = main.OpenMdiForm<FViewKeyWord>();
            f.TopicId = topic.TopicId;
            f.TopicName = topic.TopicName;
            f.LoadByTopic();
        }

        private void btn_addKeyword_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var f = new FAddKeyword())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadTopicGrid(); // reload để cập nhật CountKeyword nếu có
                }
            }
        }

        private void btn_loadFileKeyword_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file keyword";
                ofd.Filter = "Text file (*.txt)|*.txt|CSV (*.csv)|*.csv|All files (*.*)|*.*";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                int added = 0;
                int existed = 0;

                try
                {
                    var lines = File.ReadAllLines(ofd.FileName);

                    foreach (var line in lines)
                    {
                        string keyword = line.Trim();

                        if (string.IsNullOrWhiteSpace(keyword))
                            continue;

                        bool isNew = SQLDAO.Instance.AddKeywordIfNotExists(
                            keyword,
                            out int keywordId
                        );

                        if (isNew)
                            added++;
                        else
                            existed++;
                    }

                    MessageBox.Show(
                        $"Import hoàn tất!\n\n" +
                        $"➕ Thêm mới: {added}\n" +
                        $"⚠️ Đã tồn tại: {existed}",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    LoadTopicGrid(); // reload nếu CountKeyword phụ thuộc keyword
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Lỗi khi đọc file:\n" + ex.Message,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
        
    }
}

