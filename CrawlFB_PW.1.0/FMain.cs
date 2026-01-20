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
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DB;
using CrawlFB_PW._1._0.Page;
using CrawlFB_PW._1._0.Person;
using CrawlFB_PW._1._0.Post;
using CrawlFB_PW._1._0.Profile;
using DevExpress.XtraBars;
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
        private void OpenMdiForm<T>() where T : Form, new()
        {
            // Nếu đã mở rồi → kích hoạt
            foreach (Form f in this.MdiChildren)
            {
                if (f is T)
                {
                    f.Activate();
                    return;
                }
            }
            // Nếu chưa mở → khởi tạo mới
            var form = new T();
            form.MdiParent = this;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.WindowState = FormWindowState.Maximized; // Bắt buộc với XtraTabbedMdiManager
            form.Show();
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
            OpenMdiForm<FFirstScanPostPage>();
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
            OpenMdiForm<FTestPhantich>();
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
    }
}