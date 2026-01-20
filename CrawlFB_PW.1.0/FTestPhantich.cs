using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using DevExpress.XtraGrid.Views.Grid;

namespace CrawlFB_PW._1._0
{
    public partial class FTestPhantich : Form
    {
        int currentPage = 1;
        int pageSize = 50;

        public FTestPhantich()
        {
            InitializeComponent();
            gridView1.CustomColumnDisplayText += gridView1_CustomColumnDisplayText;       
            LoadGrid();     // load trang đầu tiên
            gridView1.MasterRowGetChildList += gridControl1_MasterRowGetChildList;
            gridView1.MasterRowGetRelationCount += gridControl1_MasterRowGetRelationCount;
            gridView1.MasterRowGetRelationName += gridControl1_MasterRowGetRelationName;


        }

        private void LoadGrid()
        {
            var dt = PostCategoryDAO.Instance.GetPage(currentPage, pageSize);
            gridControl1.DataSource = dt;

            int totalRow = PostCategoryDAO.Instance.CountAll();
            //lblPage.Text = $"{currentPage} / {Math.Ceiling(totalRow / (double)pageSize)}";
        }

        // =====================
        //     NÚT CONVERT
        // =====================
        private void barButtonItemconvert_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // 1) Insert chủ đề + keyword vào DB
                PostCategoryDAO.Instance.InsertTopicRulesToDB();

                // 2) Phân loại toàn bộ bài viết → lưu TableTopicPost
                PostCategoryDAO.Instance.ConvertAllPosts();

                // 3) Load lại Grid
                LoadGrid();

                MessageBox.Show("✔ Đã nạp topic/keyword & phân loại bài viết!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi Convert: " + ex.Message);
            }
        }
 

        private void UpdateTopicEngine_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // Xoá dữ liệu cũ
            PostCategoryDAO.Instance.ClearTopicData();

            // Insert topic + keyword
            PostCategoryDAO.Instance.InsertTopicRulesToDB();

            // Phân loại lại toàn bộ bài
            PostCategoryDAO.Instance.ConvertAllPosts();
        }

        private void btnPrev_Click_1(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadGrid();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            currentPage++;
            LoadGrid();

        }
        private void gridView1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "STT")
            {
                int rowIndex = e.ListSourceRowIndex;
                e.DisplayText = ((currentPage - 1) * pageSize + rowIndex + 1).ToString();
            }
        }
        private void ExportExcelTopics()
        {
            var topics = PostCategoryDAO.Instance.TopicRules.Keys.ToList();

            using (var sfd = new SaveFileDialog { Filter = "Excel File (*.xlsx)|*.xlsx", FileName = "Topic_Analysis.xlsx" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var wb = new ClosedXML.Excel.XLWorkbook())
                {
                    foreach (var topic in topics)
                    {
                        var dt = PostCategoryDAO.Instance.GetPostsByTopic(topic);

                        string sheetName = topic.Length > 31 ? topic.Substring(0, 31) : topic;
                        var ws = wb.AddWorksheet(sheetName);

                        string[] header = new[] { "STT", "Địa chỉ", "Nội dung", "Page đăng (chứa)", "Thời gian đăng", "Like", "Share", "Comment" };
                        for (int i = 0; i < header.Length; i++)
                        {
                            var cell = ws.Cell(1, i + 1);
                            cell.Value = header[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.DarkBlue;
                            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(235, 241, 255);
                        }

                        int row = 2;
                        int stt = 1;
                        foreach (DataRow r in dt.Rows)
                        {
                            ws.Cell(row, 1).Value = stt++;

                            // Hyperlink
                            string link = r["PostLink"]?.ToString() ?? "";
                            ws.Cell(row, 2).FormulaA1 = $"HYPERLINK(\"{link}\", \"Link\")";

                            ws.Cell(row, 3).Value = r["PostContent"]?.ToString() ?? "";
                            ws.Cell(row, 4).Value = r["PageNameContainer"]?.ToString() ?? "";
                            DateTime.TryParse(r["PostTime"]?.ToString(), out DateTime t);
                            ws.Cell(row, 5).Value = t;
                            ws.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                            // Convert numeric fields explicitly
                            ws.Cell(row, 6).Value = Convert.ToInt32(r["LikeCount"] ?? 0);
                            ws.Cell(row, 7).Value = Convert.ToInt32(r["ShareCount"] ?? 0);
                            ws.Cell(row, 8).Value = Convert.ToInt32(r["CommentCount"] ?? 0);

                            row++;
                        }

                        var used = ws.RangeUsed();
                        if (used != null)
                        {
                            used.Style.Font.FontName = "Times New Roman";
                            used.Style.Font.FontSize = 9;
                            used.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                            used.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                            ws.Columns().AdjustToContents();
                        }
                    }

                    wb.SaveAs(sfd.FileName);
                }

                MessageBox.Show("✔ Xuất Excel theo chủ đề thành công!");
            }
        }

        private void barButtonItemExportTopic_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ExportExcelTopics();
        }

        private void barButtonItemXuhuong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                // 1) Hỏi chọn chủ đề (topic)
                var topics = PostCategoryDAO.Instance.TopicRules.Keys.ToList();
                string topicName = topics.First(); // default

               // using (var f = new SelectTopicForm(topics))   // bạn có thể tạo form nhỏ chọn topic
               // {
                //    if (f.ShowDialog() == DialogResult.OK)
                 //       topicName = f.SelectedTopic;
                 //   else
                  //      return;
             //   }

                // 2) Lấy TopicId
                int topicId = Convert.ToInt32(SQLDAO.Instance.ExecuteScalar(
                    "SELECT TopicId FROM TableTopic WHERE TopicName=@t",
                    new Dictionary<string, object> { { "@t", topicName } }));

                // 3) Lấy danh sách bài để gom nhóm (mặc định 2 ngày gần nhất)
                DataTable dt = PostCategoryDAO.Instance.GetPostsForClustering(topicId, days: 2);

                // 4) Gom nhóm cluster (vụ việc)
                var rawClusters = PostCategoryDAO.Instance.ClusterPosts(dt);

                // 5) Chuyển về dạng hiển thị
                var clusterGroups = PostCategoryDAO.Instance.BuildClusterDisplay(rawClusters, topicName);

                // 6) Gán vào master Grid
                gridControl1.DataSource = clusterGroups;

                // 7) Setup Master → Detail
                SetupClusterDetailGrid();

                MessageBox.Show("✔ Đã phân tích xu hướng cho chủ đề: " + topicName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xu hướng: " + ex.Message);
            }
        }
        private void SetupClusterDetailGrid()
        {
            gridView1.OptionsDetail.EnableMasterViewMode = true;
            gridView1.OptionsDetail.ShowDetailTabs = false;

            gridControl1.LevelTree.Nodes.Clear();
            gridControl1.LevelTree.Nodes.Add("Posts", gridView1);

            gridView1.Columns.Clear();
            gridView1.Columns.AddVisible("PostID", "Post ID");
            gridView1.Columns.AddVisible("Content", "Nội dung");
            gridView1.Columns.AddVisible("Time", "Thời gian đăng");

            gridView1.OptionsBehavior.Editable = false;
        }
        private void gridControl1_MasterRowGetChildList(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowGetChildListEventArgs e)
        {
            GridView view = sender as GridView;
            var parent = view.GetRow(e.RowHandle) as PostCategoryDAO.ClusterGroup;
            e.ChildList = parent?.Posts;
        }

        private void gridControl1_MasterRowGetRelationCount(object sender, MasterRowGetRelationCountEventArgs e)
        {
            e.RelationCount = 1;
        }

        private void gridControl1_MasterRowGetRelationName(object sender, MasterRowGetRelationNameEventArgs e)
        {
            e.RelationName = "Posts";
        }

    }
}
