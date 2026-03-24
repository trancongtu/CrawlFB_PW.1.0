using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.Auto;
using CrawlFB_PW._1._0.Check;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DAO.phantich;
using CrawlFB_PW._1._0.DB;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.KeyWord;
using CrawlFB_PW._1._0.Page;
using CrawlFB_PW._1._0.Person;
using CrawlFB_PW._1._0.phantich;
using CrawlFB_PW._1._0.Post;
using CrawlFB_PW._1._0.Profile;
using CrawlFB_PW._1._0.Share;
using CrawlFB_PW._1._0.Topic;
using CrawlFB_PW._1._0.ViewModels;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraTabbedMdi;

namespace CrawlFB_PW._1._0
{
    public partial class FMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private XtraTabbedMdiManager xtraTabbedMdiManager1;
        public FMain()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
            // RESET DB2 trước khi mở bất kỳ form nào
            Libary.Instance.ClearAllLogs();
            new ProfileInfoDAO().ResetAllTabs();
            new ProfileInfoDAO().ResetAllMappings();
            new ProfileInfoDAO().ResetAllMonitorStatus();
            DatabaseDAO.Instance.ResetAllPageMonitorAuto();

            // Khởi tạo tab manager
            xtraTabbedMdiManager1 = new XtraTabbedMdiManager();
            xtraTabbedMdiManager1.MdiParent = this;
            this.Load += new System.EventHandler(this.FormMain_Load);

        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_startupChecked) return;
            _startupChecked = true;

            // 🔥 RESET runtime ngay khi app mở


            // 🔥 Hiển thị popup kiểm tra profile
            /* using (var startup = new CrawlFB_PW._1._0.Profile.ProfileStartup())
               {
                   var result = startup.ShowDialog();
                   if (result == DialogResult.OK)
                   {
                       MessageBox.Show("✅ Dữ liệu profile đã được cập nhật thành công!",
                           "Hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Information);
                   }
               }
           */
        }
        private bool _startupChecked = false;
        public T OpenMdiForm<T>() where T : Form, new()
        {
            foreach (Form f in this.MdiChildren)
            {
                if (f is T existing)
                {
                    existing.Activate();
                    return (T)existing;
                }
            }

            var form = new T();
            form.MdiParent = this;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.WindowState = FormWindowState.Maximized;
            form.Show();

            return form;
        }



        private void btnLogin_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FsetupProfile>();
        }
        private void btnLoadProfile_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FManagerProfile>();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {

            if (!SQLDAO.Instance.TestConnection())
            {
                MessageBox.Show("Không kết nối được SQL Server!");
                return;
            }
            SQLDAO.Instance.CreateTableAuto();
            SQLDAO.Instance.EnsureEvaluationTables();

            // Kết nối OK → tạo bảng
        }
        private void barbtnScanOnePage_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FScanPostPage>();
        }

        private void barbtnGiamsatPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FAutoSupervisePage>();
        }

        private void btnViewDatabase_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FShowDB>();
        }

        private void barButtonItemSlotManager_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FSlotManagerProfile>();
        }

        private void barButtonItemNewAuto_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FAutoCrawlPageMonitor>();
        }

        private void btnAddPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FAddPagecs>();
        }

        private void btnViewPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FViewPagePro>();
        }

        private void btnViewPageGS_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FormTest>();
        }

        private void barButtonItemScanFindPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FFindPage>();
        }

        private void barButtonItemTest_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void barButtonItemUpdatePostPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FUpdatePostPage>();
        }

        private void btnViewDBMain_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FDatabaseMain>();
        }

        private void btn_CrawlPostPerson_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FCawrlPostPerson>();
        }

        private void btn_ScanCommentPost_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FScanCommentPost>();
        }

        private void btn_ScanCommentPostFull_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FscanCommenPostFull>();
        }

        private void btn_ViewTopic_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FTopicView>();
        }

        private void btn_ViewKeyword_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FViewKeyWord>();
        }

        private void btn_SharePost_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FCrawlPostShare>();
        }

        private void btn_DeleteDataTable_ItemClick(object sender, ItemClickEventArgs e)
        {
            var confirm = MessageBox.Show(
       "⚠️ Thao tác này sẽ XÓA TOÀN BỘ dữ liệu đánh giá\n" +
       "và tạo lại bảng khi chạy app.\n\n" +
       "Bạn có chắc chắn không?",
       "Xác nhận",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Warning
   );

            if (confirm != DialogResult.Yes)
                return;

            string sql = @"
        -- =========================
        -- DROP TABLES
        -- =========================
        IF OBJECT_ID('TablePostEvaluation', 'U') IS NOT NULL
            DROP TABLE TablePostEvaluation;

        IF OBJECT_ID('TableAttentionKeywordScore', 'U') IS NOT NULL
            DROP TABLE TableAttentionKeywordScore;

        IF OBJECT_ID('TableNegativeKeywordScore', 'U') IS NOT NULL
            DROP TABLE TableNegativeKeywordScore;
        ";

            SQLDAO.Instance.ExecuteNonQuery(sql);

            MessageBox.Show(
                "✔ Đã xóa toàn bộ bảng đánh giá.\n" +
                "Hãy chạy lại app để tạo lại bảng.",
                "Hoàn tất",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void btn_AddTopic_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FAddTopic>();
        }

        private void btn_AddKeyword_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FAddKeyword>();
        }

        private void btn_ClearData_ItemClick(object sender, ItemClickEventArgs e)
        {
            var confirm = MessageBox.Show(
              "⚠️ CLEAR TOÀN BỘ TOPIC + KEYWORD + PHÂN LOẠI + ĐÁNH GIÁ\n\n" +
              "Dữ liệu sẽ bị XÓA VĨNH VIỄN.\n" +
              "Cấu trúc bảng được GIỮ NGUYÊN.\n\n" +
              "Bạn có chắc chắn không?",
              "Xác nhận CLEAR FULL",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Warning
          );

            if (confirm != DialogResult.Yes)
                return;

            string sql = @"
        -- Tắt FK
        EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

        DELETE FROM TableTopicPost;
        DELETE FROM TableTopicKey;
        DELETE FROM TableKeyword;
        DELETE FROM TableTopic;

        DELETE FROM TablePostEvaluation;
        DELETE FROM TableAttentionKeywordScore;
        DELETE FROM TableNegativeKeywordScore;

        DBCC CHECKIDENT ('TableTopic', RESEED, 0);
        DBCC CHECKIDENT ('TableKeyword', RESEED, 0);
        DBCC CHECKIDENT ('TableTopicPost', RESEED, 0);

        EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
    ";

            try
            {
                SQLDAO.Instance.ExecuteNonQuery(sql);

                MessageBox.Show(
                    "✔ CLEAR HOÀN TẤT\n\n" +
                    "Topic / Keyword / Mapping / Đánh giá đã bị xóa sạch.\n" +
                    "Mở lại View sẽ không còn dữ liệu.",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "❌ Lỗi khi clear dữ liệu:\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btn_Creattemplate_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                sfd.FileName = "Template_Topic_Keyword.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                ExcelTemplateHelper.CreateTopicKeywordTemplate(sfd.FileName);

                MessageBox.Show("✔ Đã tạo template Topic + Keyword");
            }

        }

        private void btn_Loadtemplate_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var f = new FAddTemplateTopicAndKey())
            {
                f.ShowDialog();
            }
        }

        private void btn_ConvertTopic_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FConvertTopic>();
        }

        private void btn_Test_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FTestRule>();
        }

        private void btn_phantichpost_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<fphantichpost>();
        }

        private void btn_GroupsAnalyze_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FSetupAnalyze>();
        }

        private void btn_CreateTemplateKeyword_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                sfd.FileName = "Template_Keyword.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                ExcelTemplateHelper.CreateKeywordNormalTemplate(sfd.FileName);

                MessageBox.Show("✔ Đã tạo template Keyword");
            }
        }

        private void btn_anlyze_ItemClick(object sender, ItemClickEventArgs e)
        {
            DateTime fromDate = new DateTime(1753, 1, 1);
            int maxPost = int.MaxValue;

            var posts = AnalyzeSQLDAO.Instance.GetPostsForEvaluation(fromDate, maxPost);

            if (posts.Rows.Count == 0)
            {
                XtraMessageBox.Show("Không có bài mới hoặc từ khóa không thay đổi.");
                return;
            }

            var keywords = SQLDAO.Instance.GetAllKeyword();
            var attentionDict = AnalyzeSQLDAO.Instance.LoadAttentionDictionary();
            var negativeDict = AnalyzeSQLDAO.Instance.LoadNegativeDictionary();
            var excludeSet = AnalyzeSQLDAO.Instance.GetExcludedKeywordIds();

            int count = PostAnalyzeDAO.Instance.AnalyzeAndSavePosts(
                posts,
                keywords,
                attentionDict,
                negativeDict,
                excludeSet
            );

            XtraMessageBox.Show($"Đã phân tích {count} bài.");
        }

        private void btn_reset_ItemClick(object sender, ItemClickEventArgs e)
        {
            var confirm = XtraMessageBox.Show(
         "Thao tác này sẽ xóa toàn bộ dữ liệu phân tích (bao gồm JSON vị trí). Tiếp tục?",
         "Xác nhận",
         MessageBoxButtons.YesNo,
         MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                AnalyzeSQLDAO.Instance.ClearPostEvaluation();

                XtraMessageBox.Show("Đã reset toàn bộ dữ liệu phân tích.");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi khi reset: " + ex.Message);
            }

        }

        private void btn_CheckDOM_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FCheckDOM>();
        }

        private void btn_ScanNew_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenMdiForm<FFirstScanPostPage>();
        }
    }
}