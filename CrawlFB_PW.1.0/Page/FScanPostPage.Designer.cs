namespace CrawlFB_PW._1._0.Page
{
    partial class FScanPostPage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FScanPostPage));
            this.panelControlGrid = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.txb_UrlPage = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit3 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.btn_LoadFile = new DevExpress.XtraBars.BarButtonItem();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.btn_StartScan = new DevExpress.XtraBars.BarButtonItem();
            this.btn_StopScan = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem3 = new DevExpress.XtraBars.BarButtonItem();
            this.btnSetup = new DevExpress.XtraBars.BarSubItem();
            this.txb_SetupMaxPost = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.txb_Maxday = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.btn_SaveDB = new DevExpress.XtraBars.BarSubItem();
            this.btn_SaveAll = new DevExpress.XtraBars.BarButtonItem();
            this.btn_SavePage = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.lbl_Status = new DevExpress.XtraBars.BarLargeButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlGrid)).BeginInit();
            this.panelControlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlGrid
            // 
            this.panelControlGrid.Controls.Add(this.gridControl1);
            this.panelControlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlGrid.Location = new System.Drawing.Point(0, 61);
            this.panelControlGrid.Margin = new System.Windows.Forms.Padding(4);
            this.panelControlGrid.Name = "panelControlGrid";
            this.panelControlGrid.Size = new System.Drawing.Size(1086, 448);
            this.panelControlGrid.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1082, 444);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 437;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1,
            this.bar2,
            this.bar3});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.btn_StartScan,
            this.btn_StopScan,
            this.barButtonItem3,
            this.btnSetup,
            this.txb_SetupMaxPost,
            this.txb_Maxday,
            this.barButtonItem1,
            this.btn_SaveDB,
            this.btn_SaveAll,
            this.btn_SavePage,
            this.txb_UrlPage,
            this.btn_LoadFile,
            this.lbl_Status});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 14;
            this.barManager1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1,
            this.repositoryItemTextEdit2,
            this.repositoryItemTextEdit3});
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 1;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.txb_UrlPage, DevExpress.XtraBars.BarItemPaintStyle.Caption),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_LoadFile, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // txb_UrlPage
            // 
            this.txb_UrlPage.Caption = "Địa chỉ Page";
            this.txb_UrlPage.Edit = this.repositoryItemTextEdit3;
            this.txb_UrlPage.Id = 10;
            this.txb_UrlPage.Name = "txb_UrlPage";
            // txb_UrlPage
            this.txb_UrlPage.Caption = "Địa chỉ Page";
            this.txb_UrlPage.Edit = this.repositoryItemTextEdit3;
            this.txb_UrlPage.Id = 10;
            this.txb_UrlPage.Name = "txb_UrlPage";
            this.txb_UrlPage.Width = 350; // ⭐ QUAN TRỌNG: cho URL dài
                                          // repositoryItemTextEdit3
            this.repositoryItemTextEdit3.AutoHeight = false;
            this.repositoryItemTextEdit3.Appearance.TextOptions.Trimming = DevExpress.Utils.Trimming.None;
            this.repositoryItemTextEdit3.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
            this.repositoryItemTextEdit3.NullText = "https://www.facebook.com/...";
            

            // Cho nhập URL dài


            // 
            // repositoryItemTextEdit3
            // 
            this.repositoryItemTextEdit3.AutoHeight = false;
            this.repositoryItemTextEdit3.Name = "repositoryItemTextEdit3";
            // 
            // btn_LoadFile
            // 
            this.btn_LoadFile.Caption = "Load Từ File";
            this.btn_LoadFile.Id = 11;
            this.btn_LoadFile.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_LoadFile.ImageOptions.Image")));
            this.btn_LoadFile.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_LoadFile.ImageOptions.LargeImage")));
            this.btn_LoadFile.Name = "btn_LoadFile";
            this.btn_LoadFile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_LoadFile_ItemClick);
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_StartScan, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_StopScan, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem3),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnSetup, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem1),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_SaveDB, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // btn_StartScan
            // 
            this.btn_StartScan.Caption = "Start Scan";
            this.btn_StartScan.Id = 0;
            this.btn_StartScan.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_StartScan.ImageOptions.Image")));
            this.btn_StartScan.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_StartScan.ImageOptions.LargeImage")));
            this.btn_StartScan.Name = "btn_StartScan";
            this.btn_StartScan.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_StartScan_ItemClick);
            // 
            // btn_StopScan
            // 
            this.btn_StopScan.Caption = "Stop Scan";
            this.btn_StopScan.Id = 1;
            this.btn_StopScan.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_StopScan.ImageOptions.Image")));
            this.btn_StopScan.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_StopScan.ImageOptions.LargeImage")));
            this.btn_StopScan.Name = "btn_StopScan";
            // 
            // barButtonItem3
            // 
            this.barButtonItem3.Id = 2;
            this.barButtonItem3.Name = "barButtonItem3";
            // 
            // btnSetup
            // 
            this.btnSetup.Caption = "Setup";
            this.btnSetup.Id = 3;
            this.btnSetup.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnSetup.ImageOptions.Image")));
            this.btnSetup.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnSetup.ImageOptions.LargeImage")));
            this.btnSetup.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.txb_SetupMaxPost),
            new DevExpress.XtraBars.LinkPersistInfo(this.txb_Maxday)});
            this.btnSetup.Name = "btnSetup";
            // 
            // txb_SetupMaxPost
            // 
            this.txb_SetupMaxPost.Caption = "Max Post";
            this.txb_SetupMaxPost.Edit = this.repositoryItemTextEdit1;
            this.txb_SetupMaxPost.Id = 4;
            this.txb_SetupMaxPost.Name = "txb_SetupMaxPost";
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // txb_Maxday
            // 
            this.txb_Maxday.Caption = "Max Day";
            this.txb_Maxday.Edit = this.repositoryItemTextEdit2;
            this.txb_Maxday.Id = 5;
            this.txb_Maxday.Name = "txb_Maxday";
            // 
            // repositoryItemTextEdit2
            // 
            this.repositoryItemTextEdit2.AutoHeight = false;
            this.repositoryItemTextEdit2.Name = "repositoryItemTextEdit2";
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Id = 6;
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // btn_SaveDB
            // 
            this.btn_SaveDB.Caption = "Save DataBase";
            this.btn_SaveDB.Id = 7;
            this.btn_SaveDB.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_SaveDB.ImageOptions.Image")));
            this.btn_SaveDB.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_SaveDB.ImageOptions.LargeImage")));
            this.btn_SaveDB.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_SaveAll),
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_SavePage)});
            this.btn_SaveDB.Name = "btn_SaveDB";
            // 
            // btn_SaveAll
            // 
            this.btn_SaveAll.Caption = "Save All";
            this.btn_SaveAll.Id = 8;
            this.btn_SaveAll.Name = "btn_SaveAll";
            // 
            // btn_SavePage
            // 
            this.btn_SavePage.Caption = "Chỉ Save Page";
            this.btn_SavePage.Id = 9;
            this.btn_SavePage.Name = "btn_SavePage";
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockRow = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.bar3.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.lbl_Status)});
            this.bar3.OptionsBar.AllowQuickCustomization = false;
            this.bar3.OptionsBar.DrawDragBorder = false;
            this.bar3.OptionsBar.UseWholeRow = true;
            this.bar3.Text = "Status bar";
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1086, 61);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 509);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1086, 27);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 61);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 448);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1086, 61);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 448);
            // 
            // lbl_Status
            // 
            this.lbl_Status.Caption = "Trạng Thái";
            this.lbl_Status.Id = 13;
            this.lbl_Status.Name = "lbl_Status";
            // 
            // FScanPostPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 536);
            this.Controls.Add(this.panelControlGrid);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FScanPostPage";
            this.Text = "Quét bài viết trên Page";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlGrid)).EndInit();
            this.panelControlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private DevExpress.XtraEditors.PanelControl panelControlGrid;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        #endregion
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem btn_StartScan;
        private DevExpress.XtraBars.BarButtonItem btn_StopScan;
        private DevExpress.XtraBars.BarButtonItem barButtonItem3;
        private DevExpress.XtraBars.BarSubItem btnSetup;
        private DevExpress.XtraBars.BarEditItem txb_SetupMaxPost;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraBars.BarEditItem txb_UrlPage;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit3;
        private DevExpress.XtraBars.BarButtonItem btn_LoadFile;
        private DevExpress.XtraBars.BarEditItem txb_Maxday;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit2;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarSubItem btn_SaveDB;
        private DevExpress.XtraBars.BarButtonItem btn_SaveAll;
        private DevExpress.XtraBars.BarButtonItem btn_SavePage;
        private DevExpress.XtraBars.BarLargeButtonItem lbl_Status;
    }
}