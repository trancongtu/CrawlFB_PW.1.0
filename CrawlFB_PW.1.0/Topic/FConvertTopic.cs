using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Helper.Text;
using CrawlFB_PW._1._0.ViewModels;

namespace CrawlFB_PW._1._0.Topic
{
    public partial class FConvertTopic : Form
    {
        private List<string> _highlightKeywords = new List<string>();
        private bool _isCheckKeywordMode = false;
        public FConvertTopic()
        {
            InitializeComponent();
            this.Load += FConvertTopic_Load;       
            cb_SelectTopic.EditValueChanged += Cb_SelectTopic_EditValueChanged;


        }
        private void FConvertTopic_Load(object sender, EventArgs e)
        {
            try
        {
                    LoadGrid();   // có thể rỗng nhưng KHÔNG lỗi
                    InitGrid();
                LoadTopicToBar();              

            }
            catch (Exception ex)
        {
                    MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }
        private void LoadGrid()
        {
            var dt = SQLDAO.Instance.GetPostTopicForView();
            var list = new List<PostTopicViewModel>();

            int stt = 1;
            foreach (DataRow r in dt.Rows)
            {
                DateTime? realPostTime = null;
                DateTime convertTime = DateTime.MinValue;

                realPostTime = r["RealPostTime"] == DBNull.Value? (DateTime?)null : (DateTime)r["RealPostTime"];
                convertTime = (DateTime)r["ConvertTime"];             
                list.Add(new PostTopicViewModel
                {
                    STT = stt++,
                    Select = false,
                    PostId = r["PostId"].ToString(),
                    TopicName = r["TopicName"].ToString(),
                    PostContent = r["PostContent"].ToString(),
                    PostContentRaw = r["PostContent"].ToString(),
                    RealPostTime = realPostTime,
                    ConvertTime = convertTime
                });
            }

            gridControl1.DataSource = list;
        }

        private void InitGrid()
        {
            var gv = gridView1;

            gv.BeginUpdate();
            try
            {
                // 🔥 QUAN TRỌNG: sinh cột từ ViewModel
                gv.PopulateColumns();

                // ===== STT (row indicator) =====
                UIGridHelper.EnableRowIndicatorSTT(gv);

                // ===== SELECT (checkbox) =====
                UIGridHelper.ApplySelect(gv, gridControl1);

             
                // ✅ Chỉ Select được edit
                if (gv.Columns["Select"] != null)
                    gv.Columns["Select"].OptionsColumn.AllowEdit = true;
                // ===== VIỆT HÓA CAPTION =====
                UIGridHelper.ApplyVietnameseCaption(gv);
                        // ===== CHỈ HIỆN CỘT CẦN =====
                        UIGridHelper.ShowOnlyColumns(
            gridView1,
            "Select",
            "STT",
            "TopicName",
            "PostContent",
            "TimeView",     // 🔥 hiển thị
            "ConvertTime",
            "ViewDetail"
        );               
                gridView1.OptionsView.RowAutoHeight = true;
                gridView1.Columns["STT"]?.BestFit();
                gridView1.Columns["TopicName"]?.BestFit();
                gridView1.Columns["TimeView"]?.BestFit();
                gridView1.Columns["ConvertTime"]?.BestFit();
                var colContent = gridView1.Columns["PostContent"];
                if (colContent != null)
                {
                    colContent.OptionsColumn.AllowSize = true;
                    colContent.MinWidth = 300;   // không khóa cứng, chỉ là minimum
                }
            }
            finally
            {
                gv.EndUpdate();
            }
        }
        private void LoadTopicToBar()
        {
            var dt = SQLDAO.Instance.GetPostTopicForView();

            var topics = dt.AsEnumerable()
                           .Select(r => r["TopicName"].ToString())
                           .Distinct()
                           .OrderBy(x => x)
                           .ToList();

            repositoryItemComboBox1.Items.Clear();
            repositoryItemComboBox1.Items.Add("Tất cả");
            repositoryItemComboBox1.Items.AddRange(topics);

            cb_SelectTopic.EditValue = "Tất cả";
        }

        private void Cb_SelectTopic_EditValueChanged(object sender, EventArgs e)
        {
            string topic = cb_SelectTopic.EditValue?.ToString();

            if (string.IsNullOrEmpty(topic) || topic == "Tất cả")
            {
                gridView1.ActiveFilter.Clear();
                return;
            }

            gridView1.ActiveFilterString =
                $"[TopicName] = '{topic.Replace("'", "''")}'";
        }

        private void btn_ConvertAgainAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var confirm = MessageBox.Show(
        "Thao tác này sẽ phân loại LẠI TOÀN BỘ bài viết.\nBạn có chắc chắn?",
        "Xác nhận",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning
    );

            if (confirm != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;

            PostCategoryDAO.Instance.RebuildAllTopicPost();

            LoadGrid();

            Cursor.Current = Cursors.Default;

            MessageBox.Show("✔ Đã phân loại lại toàn bộ bài viết");
        }

        private void btn_UpdatePostTopic_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            // chỉ update những mapping chưa có
            PostCategoryDAO.Instance.RebuildByNewKeywordOrTopic();

            LoadGrid();

            Cursor.Current = Cursors.Default;

            MessageBox.Show("✔ Đã cập nhật phân loại cho bài / keyword mới");
        }

        private void btn_Export_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                sfd.FileName = $"PostTopic_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                gridView1.ExportToXlsx(sfd.FileName);

                MessageBox.Show("✔ Đã xuất Excel thành công");
            }
        }

        private void btn_checkkeyword_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 1️⃣ Lấy danh sách bài đang được select
            var checkedPosts = ((List<PostTopicViewModel>)gridControl1.DataSource)
       .Where(p => p.Select)
       .ToList();

            if (checkedPosts.Count == 0)
            {
                MessageBox.Show("Vui lòng tick chọn ít nhất 1 bài viết");
                return;
            }


            // 2️⃣ Lấy TopicName (từ combobox đang chọn)
            string topicName = cb_SelectTopic.EditValue?.ToString();

            if (string.IsNullOrWhiteSpace(topicName) || topicName == "Tất cả")
            {
                MessageBox.Show("Vui lòng chọn 1 Topic cụ thể để kiểm tra keyword");
                return;
            }

            // 3️⃣ Đổi TopicName → TopicId
            var topicDto = SQLDAO.Instance.GetTopicByName(topicName);
            if (topicDto == null)
            {
                MessageBox.Show($"Không tìm thấy Topic: {topicName}");
                return;
            }

            // 4️⃣ Lấy keyword theo TopicId
            var keywords = SQLDAO.Instance
                .GetKeywordsByTopic(topicDto.TopicId)
                .Select(x => x.KeywordName)
                .ToList();

            if (keywords.Count == 0)
            {
                MessageBox.Show("Topic này chưa có keyword");
                return;
            }

            // 5️⃣ Mở form check
            var frm = new FCheckKeywordOnPost();
            frm.InitData(
                checkedPosts.Select(p => new PostInfoViewModel
                {
                    Content = p.PostContent
                }).ToList(),
                keywords
            );
            frm.ShowDialog();
        }

        private void btn_selectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                int rowHandle = gridView1.GetVisibleRowHandle(i);
                var row = gridView1.GetRow(rowHandle) as PostTopicViewModel;
                if (row != null)
                {
                    row.Select = true;
                }
            }

            gridView1.RefreshData();
        }
    }
}
