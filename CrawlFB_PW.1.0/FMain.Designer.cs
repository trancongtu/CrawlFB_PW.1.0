using System.Drawing;

namespace CrawlFB_PW._1._0
{
    partial class FMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FMain));
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.btnLogin = new DevExpress.XtraBars.BarButtonItem();
            this.btnLoadProfile = new DevExpress.XtraBars.BarButtonItem();
            this.barbtnScanOnePage = new DevExpress.XtraBars.BarButtonItem();
            this.barbtnGiamsatPage = new DevExpress.XtraBars.BarButtonItem();
            this.btnViewDatabase = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemSlotManager = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemNewAuto = new DevExpress.XtraBars.BarButtonItem();
            this.btnViewPageGS = new DevExpress.XtraBars.BarButtonItem();
            this.btnAddPage = new DevExpress.XtraBars.BarButtonItem();
            this.btnViewPage = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemScanFindPage = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemTest = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemUpdatePostPage = new DevExpress.XtraBars.BarButtonItem();
            this.btnViewDBMain = new DevExpress.XtraBars.BarButtonItem();
            this.btn_CrawlPostPerson = new DevExpress.XtraBars.BarButtonItem();
            this.btn_ScanCommentPost = new DevExpress.XtraBars.BarButtonItem();
            this.ribbonPageSetup = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroupConfig = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageCrawl = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageThuthap = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGiamsat = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageDatabase = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroupPAge = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPagePhanTich = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroupPhanTichPage = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageForensic = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.btn_ScanCommentPostFull = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.EmptyAreaImageOptions.ImagePadding = new System.Windows.Forms.Padding(46);
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.ribbon.SearchEditItem,
            this.btnLogin,
            this.btnLoadProfile,
            this.barbtnScanOnePage,
            this.barbtnGiamsatPage,
            this.btnViewDatabase,
            this.barButtonItemSlotManager,
            this.barButtonItemNewAuto,
            this.btnViewPageGS,
            this.btnAddPage,
            this.btnViewPage,
            this.barButtonItemScanFindPage,
            this.barButtonItemTest,
            this.barButtonItemUpdatePostPage,
            this.btnViewDBMain,
            this.btn_CrawlPostPerson,
            this.btn_ScanCommentPost,
            this.btn_ScanCommentPostFull});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ribbon.MaxItemId = 20;
            this.ribbon.Name = "ribbon";
            this.ribbon.OptionsMenuMinWidth = 515;
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPageSetup,
            this.ribbonPageCrawl,
            this.ribbonPagePhanTich,
            this.ribbonPageForensic});
            this.ribbon.Size = new System.Drawing.Size(1300, 193);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            // 
            // btnLogin
            // 
            this.btnLogin.Caption = "SetupProfile";
            this.btnLogin.Id = 1;
            this.btnLogin.ImageOptions.LargeImage = global::CrawlFB_PW._1._0.Properties.Resources.boemployee_32x32;
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLogin_ItemClick);
            // 
            // btnLoadProfile
            // 
            this.btnLoadProfile.Caption = "CheckProfile";
            this.btnLoadProfile.Id = 2;
            this.btnLoadProfile.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnLoadProfile.ImageOptions.Image")));
            this.btnLoadProfile.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnLoadProfile.ImageOptions.LargeImage")));
            this.btnLoadProfile.Name = "btnLoadProfile";
            this.btnLoadProfile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnLoadProfile_ItemClick);
            // 
            // barbtnScanOnePage
            // 
            this.barbtnScanOnePage.Caption = "Quét Bài viết Page";
            this.barbtnScanOnePage.Id = 4;
            this.barbtnScanOnePage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barbtnScanOnePage.ImageOptions.Image")));
            this.barbtnScanOnePage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barbtnScanOnePage.ImageOptions.LargeImage")));
            this.barbtnScanOnePage.Name = "barbtnScanOnePage";
            this.barbtnScanOnePage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barbtnScanOnePage_ItemClick);
            // 
            // barbtnGiamsatPage
            // 
            this.barbtnGiamsatPage.Caption = "Giám sát Page";
            this.barbtnGiamsatPage.Id = 6;
            this.barbtnGiamsatPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barbtnGiamsatPage.ImageOptions.Image")));
            this.barbtnGiamsatPage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barbtnGiamsatPage.ImageOptions.LargeImage")));
            this.barbtnGiamsatPage.Name = "barbtnGiamsatPage";
            this.barbtnGiamsatPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barbtnGiamsatPage_ItemClick);
            // 
            // btnViewDatabase
            // 
            this.btnViewDatabase.Caption = "Xem Database";
            this.btnViewDatabase.Id = 7;
            this.btnViewDatabase.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnViewDatabase.ImageOptions.Image")));
            this.btnViewDatabase.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnViewDatabase.ImageOptions.LargeImage")));
            this.btnViewDatabase.Name = "btnViewDatabase";
            this.btnViewDatabase.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnViewDatabase_ItemClick);
            // 
            // barButtonItemSlotManager
            // 
            this.barButtonItemSlotManager.Caption = "Kiểm tra Slot";
            this.barButtonItemSlotManager.Id = 8;
            this.barButtonItemSlotManager.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItemSlotManager.ImageOptions.Image")));
            this.barButtonItemSlotManager.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItemSlotManager.ImageOptions.LargeImage")));
            this.barButtonItemSlotManager.Name = "barButtonItemSlotManager";
            this.barButtonItemSlotManager.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSlotManager_ItemClick);
            // 
            // barButtonItemNewAuto
            // 
            this.barButtonItemNewAuto.Caption = "Giám sát mới";
            this.barButtonItemNewAuto.Id = 9;
            this.barButtonItemNewAuto.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItemNewAuto.ImageOptions.Image")));
            this.barButtonItemNewAuto.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItemNewAuto.ImageOptions.LargeImage")));
            this.barButtonItemNewAuto.Name = "barButtonItemNewAuto";
            this.barButtonItemNewAuto.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemNewAuto_ItemClick);
            // 
            // btnViewPageGS
            // 
            this.btnViewPageGS.Caption = "Page giám sát";
            this.btnViewPageGS.Id = 10;
            this.btnViewPageGS.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnViewPageGS.ImageOptions.Image")));
            this.btnViewPageGS.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnViewPageGS.ImageOptions.LargeImage")));
            this.btnViewPageGS.Name = "btnViewPageGS";
            this.btnViewPageGS.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnViewPageGS_ItemClick);
            // 
            // btnAddPage
            // 
            this.btnAddPage.Caption = "Thêm Page";
            this.btnAddPage.Id = 11;
            this.btnAddPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnAddPage.ImageOptions.Image")));
            this.btnAddPage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnAddPage.ImageOptions.LargeImage")));
            this.btnAddPage.Name = "btnAddPage";
            this.btnAddPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddPage_ItemClick);
            // 
            // btnViewPage
            // 
            this.btnViewPage.Caption = "Xem DB Page";
            this.btnViewPage.Id = 12;
            this.btnViewPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnViewPage.ImageOptions.Image")));
            this.btnViewPage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnViewPage.ImageOptions.LargeImage")));
            this.btnViewPage.Name = "btnViewPage";
            this.btnViewPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnViewPage_ItemClick);
            // 
            // barButtonItemScanFindPage
            // 
            this.barButtonItemScanFindPage.Caption = "Thu Thập Page";
            this.barButtonItemScanFindPage.Id = 13;
            this.barButtonItemScanFindPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItemScanFindPage.ImageOptions.Image")));
            this.barButtonItemScanFindPage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItemScanFindPage.ImageOptions.LargeImage")));
            this.barButtonItemScanFindPage.Name = "barButtonItemScanFindPage";
            this.barButtonItemScanFindPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemScanFindPage_ItemClick);
            // 
            // barButtonItemTest
            // 
            this.barButtonItemTest.Caption = "Test";
            this.barButtonItemTest.Id = 14;
            this.barButtonItemTest.Name = "barButtonItemTest";
            this.barButtonItemTest.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemTest_ItemClick);
            // 
            // barButtonItemUpdatePostPage
            // 
            this.barButtonItemUpdatePostPage.Caption = "Update Post Page";
            this.barButtonItemUpdatePostPage.Id = 15;
            this.barButtonItemUpdatePostPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItemUpdatePostPage.ImageOptions.Image")));
            this.barButtonItemUpdatePostPage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItemUpdatePostPage.ImageOptions.LargeImage")));
            this.barButtonItemUpdatePostPage.Name = "barButtonItemUpdatePostPage";
            this.barButtonItemUpdatePostPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemUpdatePostPage_ItemClick);
            // 
            // btnViewDBMain
            // 
            this.btnViewDBMain.Caption = "Xem DB tổng";
            this.btnViewDBMain.Id = 16;
            this.btnViewDBMain.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnViewDBMain.ImageOptions.Image")));
            this.btnViewDBMain.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnViewDBMain.ImageOptions.LargeImage")));
            this.btnViewDBMain.Name = "btnViewDBMain";
            this.btnViewDBMain.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnViewDBMain_ItemClick);
            // 
            // btn_CrawlPostPerson
            // 
            this.btn_CrawlPostPerson.Caption = "Thu thập bài viết";
            this.btn_CrawlPostPerson.Id = 17;
            this.btn_CrawlPostPerson.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_CrawlPostPerson.ImageOptions.Image")));
            this.btn_CrawlPostPerson.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_CrawlPostPerson.ImageOptions.LargeImage")));
            this.btn_CrawlPostPerson.Name = "btn_CrawlPostPerson";
            this.btn_CrawlPostPerson.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_CrawlPostPerson_ItemClick);
            // 
            // btn_ScanCommentPost
            // 
            this.btn_ScanCommentPost.Caption = "Quét Bình Luận Post";
            this.btn_ScanCommentPost.Id = 18;
            this.btn_ScanCommentPost.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_ScanCommentPost.ImageOptions.Image")));
            this.btn_ScanCommentPost.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_ScanCommentPost.ImageOptions.LargeImage")));
            this.btn_ScanCommentPost.Name = "btn_ScanCommentPost";
            this.btn_ScanCommentPost.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_ScanCommentPost_ItemClick);
            // 
            // ribbonPageSetup
            // 
            this.ribbonPageSetup.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroupConfig});
            this.ribbonPageSetup.Name = "ribbonPageSetup";
            this.ribbonPageSetup.Text = "Cài đặt";
            // 
            // ribbonPageGroupConfig
            // 
            this.ribbonPageGroupConfig.ItemLinks.Add(this.btnLogin);
            this.ribbonPageGroupConfig.ItemLinks.Add(this.btnLoadProfile);
            this.ribbonPageGroupConfig.ItemLinks.Add(this.barButtonItemSlotManager);
            this.ribbonPageGroupConfig.Name = "ribbonPageGroupConfig";
            this.ribbonPageGroupConfig.Text = "Setup";
            // 
            // ribbonPageCrawl
            // 
            this.ribbonPageCrawl.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageThuthap,
            this.ribbonPageGiamsat,
            this.ribbonPageDatabase,
            this.ribbonPageGroupPAge});
            this.ribbonPageCrawl.Name = "ribbonPageCrawl";
            this.ribbonPageCrawl.Text = "Thu Thập";
            // 
            // ribbonPageThuthap
            // 
            this.ribbonPageThuthap.ItemLinks.Add(this.barbtnScanOnePage);
            this.ribbonPageThuthap.ItemLinks.Add(this.barButtonItemScanFindPage);
            this.ribbonPageThuthap.ItemLinks.Add(this.barButtonItemUpdatePostPage);
            this.ribbonPageThuthap.Name = "ribbonPageThuthap";
            this.ribbonPageThuthap.Text = "Thu thập thủ công";
            // 
            // ribbonPageGiamsat
            // 
            this.ribbonPageGiamsat.ItemLinks.Add(this.barbtnGiamsatPage);
            this.ribbonPageGiamsat.ItemLinks.Add(this.barButtonItemNewAuto);
            this.ribbonPageGiamsat.Name = "ribbonPageGiamsat";
            this.ribbonPageGiamsat.Text = "Giám Sát";
            // 
            // ribbonPageDatabase
            // 
            this.ribbonPageDatabase.ItemLinks.Add(this.btnViewDatabase);
            this.ribbonPageDatabase.ItemLinks.Add(this.btnViewPageGS);
            this.ribbonPageDatabase.ItemLinks.Add(this.btnViewDBMain);
            this.ribbonPageDatabase.Name = "ribbonPageDatabase";
            this.ribbonPageDatabase.Text = "Lưu trữ";
            // 
            // ribbonPageGroupPAge
            // 
            this.ribbonPageGroupPAge.ItemLinks.Add(this.btnAddPage);
            this.ribbonPageGroupPAge.ItemLinks.Add(this.btnViewPage);
            this.ribbonPageGroupPAge.Name = "ribbonPageGroupPAge";
            this.ribbonPageGroupPAge.Text = "PAGE";
            // 
            // ribbonPagePhanTich
            // 
            this.ribbonPagePhanTich.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroupPhanTichPage});
            this.ribbonPagePhanTich.Name = "ribbonPagePhanTich";
            this.ribbonPagePhanTich.Text = "Phân Tích";
            // 
            // ribbonPageGroupPhanTichPage
            // 
            this.ribbonPageGroupPhanTichPage.ItemLinks.Add(this.barButtonItemTest);
            this.ribbonPageGroupPhanTichPage.Name = "ribbonPageGroupPhanTichPage";
            this.ribbonPageGroupPhanTichPage.Text = "Phân Tích Page";
            // 
            // ribbonPageForensic
            // 
            this.ribbonPageForensic.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1});
            this.ribbonPageForensic.Name = "ribbonPageForensic";
            this.ribbonPageForensic.Text = "Điều Tra";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.ItemLinks.Add(this.btn_CrawlPostPerson);
            this.ribbonPageGroup1.ItemLinks.Add(this.btn_ScanCommentPost);
            this.ribbonPageGroup1.ItemLinks.Add(this.btn_ScanCommentPostFull);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "Cá nhân";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 770);
            this.ribbonStatusBar.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1300, 30);
            // 
            // btn_ScanCommentPostFull
            // 
            this.btn_ScanCommentPostFull.Caption = "Quét bình luận Post Full";
            this.btn_ScanCommentPostFull.Id = 19;
            this.btn_ScanCommentPostFull.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_ScanCommentPostFull.ImageOptions.Image")));
            this.btn_ScanCommentPostFull.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_ScanCommentPostFull.ImageOptions.LargeImage")));
            this.btn_ScanCommentPostFull.Name = "btn_ScanCommentPostFull";
            this.btn_ScanCommentPostFull.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_ScanCommentPostFull_ItemClick);
            // 
            // FMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1300, 800);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbon);
            this.Name = "FMain";
            this.Ribbon = this.ribbon;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "FMain";
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPageSetup;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupConfig;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.BarButtonItem btnLogin;
        private DevExpress.XtraBars.BarButtonItem btnLoadProfile;
        private DevExpress.XtraBars.BarButtonItem barbtnScanOnePage;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPageCrawl;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageThuthap;
        private DevExpress.XtraBars.BarButtonItem barbtnGiamsatPage;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGiamsat;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageDatabase;
        private DevExpress.XtraBars.BarButtonItem btnViewDatabase;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSlotManager;
        private DevExpress.XtraBars.BarButtonItem barButtonItemNewAuto;
        private DevExpress.XtraBars.BarButtonItem btnViewPageGS;
        private DevExpress.XtraBars.BarButtonItem btnAddPage;
        private DevExpress.XtraBars.BarButtonItem btnViewPage;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupPAge;
        private DevExpress.XtraBars.BarButtonItem barButtonItemScanFindPage;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPagePhanTich;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupPhanTichPage;
        private DevExpress.XtraBars.BarButtonItem barButtonItemTest;
        private DevExpress.XtraBars.BarButtonItem barButtonItemUpdatePostPage;
        private DevExpress.XtraBars.BarButtonItem btnViewDBMain;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPageForensic;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.BarButtonItem btn_CrawlPostPerson;
        private DevExpress.XtraBars.BarButtonItem btn_ScanCommentPost;
        private DevExpress.XtraBars.BarButtonItem btn_ScanCommentPostFull;
    }
}