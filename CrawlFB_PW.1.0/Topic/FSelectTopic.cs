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
using CrawlFB_PW._1._0.ViewModels;

namespace CrawlFB_PW._1._0.Topic
{
    public partial class FSelectTopic : Form
    {
        private readonly List<KeywordViewModel> _keywords;

        public FSelectTopic(List<KeywordViewModel> keywords)
        {
            InitializeComponent();
            _keywords = keywords;
            this.Load += FSelectTopic_Load;
        }
        private void FSelectTopic_Load(object sender, EventArgs e)
        {
            var topics = SQLDAO.Instance.GetAllTopic();

            checkedListBox1.DataSource = topics;
            checkedListBox1.DisplayMember = "TopicName";
            checkedListBox1.ValueMember = "TopicId";
        }

        private void btn_thucthi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var selectedTopicIds = checkedListBox1.CheckedItems
                .Cast<dynamic>()
                .Select(x => (int)x.TopicId)
                .ToList();

            if (selectedTopicIds.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 topic");
                return;
            }

            int added = 0;
            int skipped = 0;
            foreach (var kw in _keywords)
            {
                foreach (var topicId in selectedTopicIds)
                {
                    bool ok = SQLDAO.Instance.AddKeywordToTopic(
                        kw.KeywordId,
                        topicId
                    );

                    if (ok)
                        added++;
                    else
                        skipped++;
                }
            }
            MessageBox.Show(
                $"✔ Hoàn tất\n" +
                $"• Thêm mới: {added}\n" +
                $"• Bỏ qua (đã tồn tại): {skipped}"
            );

            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        private void btn_Canncel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
