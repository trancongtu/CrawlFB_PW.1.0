namespace CrawlFB_PW._1._0.Post
{
    partial class FscanCommenPostFull
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FscanCommenPostFull));
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.txd_PostLink = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.btn_SelectAll = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.btn_RunScan = new DevExpress.XtraBars.BarButtonItem();
            this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
            this.btn_ExportFull = new DevExpress.XtraBars.BarButtonItem();
            this.btn_ExportFullAndPrint = new DevExpress.XtraBars.BarButtonItem();
            this.btn_StopScan = new DevExpress.XtraBars.BarButtonItem();
            this.btn_ScanIDFB = new DevExpress.XtraBars.BarButtonItem();
            this.barSubItem2 = new DevExpress.XtraBars.BarSubItem();
            this.btn_SaveJson = new DevExpress.XtraBars.BarButtonItem();
            this.barSubItem3 = new DevExpress.XtraBars.BarSubItem();
            this.btn_LoadDataJson = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
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
            this.txd_PostLink,
            this.barButtonItem1,
            this.btn_RunScan,
            this.barSubItem1,
            this.btn_StopScan,
            this.btn_ExportFull,
            this.btn_ScanIDFB,
            this.btn_SelectAll,
            this.barButtonItem2,
            this.btn_ExportFullAndPrint,
            this.barSubItem2,
            this.btn_SaveJson,
            this.barSubItem3,
            this.btn_LoadDataJson});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 15;
            this.barManager1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1});
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 1;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.txd_PostLink, DevExpress.XtraBars.BarItemPaintStyle.Caption),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItem1, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_SelectAll, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItem2, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // txd_PostLink
            // 
            this.txd_PostLink.Caption = "Địa Chỉ Bài Viết";
            this.txd_PostLink.Edit = this.repositoryItemTextEdit1;
            this.txd_PostLink.Id = 0;
            this.txd_PostLink.Name = "txd_PostLink";
            this.txd_PostLink.Size = new System.Drawing.Size(400, 22);
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "Load File Url";
            this.barButtonItem1.Id = 1;
            this.barButtonItem1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItem1.ImageOptions.Image")));
            this.barButtonItem1.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItem1.ImageOptions.LargeImage")));
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // btn_SelectAll
            // 
            this.btn_SelectAll.Caption = "Select All";
            this.btn_SelectAll.Id = 7;
            this.btn_SelectAll.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_SelectAll.ImageOptions.Image")));
            this.btn_SelectAll.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_SelectAll.ImageOptions.LargeImage")));
            this.btn_SelectAll.Name = "btn_SelectAll";
            this.btn_SelectAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_SelectAll_ItemClick);
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "Remove ALl";
            this.barButtonItem2.Id = 8;
            this.barButtonItem2.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItem2.ImageOptions.Image")));
            this.barButtonItem2.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItem2.ImageOptions.LargeImage")));
            this.barButtonItem2.Name = "barButtonItem2";
            this.barButtonItem2.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem2_ItemClick);
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_RunScan, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSubItem1, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_StopScan, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_ScanIDFB, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSubItem2, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSubItem3, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // btn_RunScan
            // 
            this.btn_RunScan.Caption = "Run Scan";
            this.btn_RunScan.Id = 2;
            this.btn_RunScan.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_RunScan.ImageOptions.Image")));
            this.btn_RunScan.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_RunScan.ImageOptions.LargeImage")));
            this.btn_RunScan.Name = "btn_RunScan";
            this.btn_RunScan.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_RunScan_ItemClick);
            // 
            // barSubItem1
            // 
            this.barSubItem1.Caption = "Export";
            this.barSubItem1.Id = 3;
            this.barSubItem1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barSubItem1.ImageOptions.Image")));
            this.barSubItem1.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barSubItem1.ImageOptions.LargeImage")));
            this.barSubItem1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_ExportFull),
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_ExportFullAndPrint)});
            this.barSubItem1.Name = "barSubItem1";
            // 
            // btn_ExportFull
            // 
            this.btn_ExportFull.Caption = "Export Full";
            this.btn_ExportFull.Id = 5;
            this.btn_ExportFull.Name = "btn_ExportFull";
            this.btn_ExportFull.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_ExportFull_ItemClick);
            // 
            // btn_ExportFullAndPrint
            // 
            this.btn_ExportFullAndPrint.Caption = "Export Full And Print";
            this.btn_ExportFullAndPrint.Id = 9;
            this.btn_ExportFullAndPrint.Name = "btn_ExportFullAndPrint";
            this.btn_ExportFullAndPrint.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_ExportFullAndPrint_ItemClick);
            // 
            // btn_StopScan
            // 
            this.btn_StopScan.Caption = "stop Scan";
            this.btn_StopScan.Id = 4;
            this.btn_StopScan.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_StopScan.ImageOptions.Image")));
            this.btn_StopScan.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_StopScan.ImageOptions.LargeImage")));
            this.btn_StopScan.Name = "btn_StopScan";
            this.btn_StopScan.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_StopScan_ItemClick);
            // 
            // btn_ScanIDFB
            // 
            this.btn_ScanIDFB.Caption = "Lấy IDFB";
            this.btn_ScanIDFB.Id = 6;
            this.btn_ScanIDFB.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btn_ScanIDFB.ImageOptions.Image")));
            this.btn_ScanIDFB.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btn_ScanIDFB.ImageOptions.LargeImage")));
            this.btn_ScanIDFB.Name = "btn_ScanIDFB";
            this.btn_ScanIDFB.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_ScanIDFB_ItemClick);
            // 
            // barSubItem2
            // 
            this.barSubItem2.Caption = "Save";
            this.barSubItem2.Id = 10;
            this.barSubItem2.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barSubItem2.ImageOptions.Image")));
            this.barSubItem2.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barSubItem2.ImageOptions.LargeImage")));
            this.barSubItem2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_SaveJson)});
            this.barSubItem2.Name = "barSubItem2";
            // 
            // btn_SaveJson
            // 
            this.btn_SaveJson.Caption = "Lưu tạm Json";
            this.btn_SaveJson.Id = 11;
            this.btn_SaveJson.Name = "btn_SaveJson";
            this.btn_SaveJson.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_SaveJson_ItemClick);
            // 
            // barSubItem3
            // 
            this.barSubItem3.Caption = "Load Data";
            this.barSubItem3.Id = 13;
            this.barSubItem3.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barSubItem3.ImageOptions.Image")));
            this.barSubItem3.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barSubItem3.ImageOptions.LargeImage")));
            this.barSubItem3.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_LoadDataJson)});
            this.barSubItem3.Name = "barSubItem3";
            // 
            // btn_LoadDataJson
            // 
            this.btn_LoadDataJson.Caption = "Data Json Temp";
            this.btn_LoadDataJson.Id = 14;
            this.btn_LoadDataJson.Name = "btn_LoadDataJson";
            this.btn_LoadDataJson.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_LoadDataJson_ItemClick);
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockRow = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
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
            this.barDockControlTop.Size = new System.Drawing.Size(1000, 61);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 406);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1000, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 61);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 345);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1000, 61);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 345);
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(6);
            this.gridControl1.Location = new System.Drawing.Point(0, 61);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(6);
            this.gridControl1.MenuManager = this.barManager1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1000, 345);
            this.gridControl1.TabIndex = 4;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 546;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // FscanCommenPostFull
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 426);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FscanCommenPostFull";
            this.Text = "FscanCommenPostFull";
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraBars.BarEditItem txd_PostLink;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarButtonItem btn_RunScan;
        private DevExpress.XtraBars.BarSubItem barSubItem1;
        private DevExpress.XtraBars.BarButtonItem btn_StopScan;
        private DevExpress.XtraBars.BarButtonItem btn_ExportFull;
        private DevExpress.XtraBars.BarButtonItem btn_ScanIDFB;
        private DevExpress.XtraBars.BarButtonItem btn_SelectAll;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraBars.BarButtonItem btn_ExportFullAndPrint;
        private DevExpress.XtraBars.BarSubItem barSubItem2;
        private DevExpress.XtraBars.BarButtonItem btn_SaveJson;
        private DevExpress.XtraBars.BarSubItem barSubItem3;
        private DevExpress.XtraBars.BarButtonItem btn_LoadDataJson;
    }
}