namespace CrawlFB_PW._1._0.Page
{
    partial class FUpdatePostPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FUpdatePostPage));
            DevExpress.XtraGrid.GridLevelNode gridLevelNode1 = new DevExpress.XtraGrid.GridLevelNode();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.barButtonItemSelectAll = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemremoveAll = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemSaveDB = new DevExpress.XtraBars.BarButtonItem();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.barButtonItemRun = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemLoadPage = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.gridControlPost = new DevExpress.XtraGrid.GridControl();
            this.gridViewPost = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridControlPage = new DevExpress.XtraGrid.GridControl();
            this.gridViewPage = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).BeginInit();
            this.splitContainerControl1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).BeginInit();
            this.splitContainerControl1.Panel2.SuspendLayout();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlPost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewPost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlPage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewPage)).BeginInit();
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
            this.barButtonItemRun,
            this.barButtonItemLoadPage,
            this.barButtonItemSelectAll,
            this.barButtonItemremoveAll,
            this.barButtonItemSaveDB});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 7;
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 1;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemSelectAll, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemremoveAll, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemSaveDB, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // barButtonItemSelectAll
            // 
            this.barButtonItemSelectAll.Caption = "Chọn hết Page";
            this.barButtonItemSelectAll.Id = 4;
            this.barButtonItemSelectAll.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemSelectAll.ImageOptions.SvgImage")));
            this.barButtonItemSelectAll.Name = "barButtonItemSelectAll";
            this.barButtonItemSelectAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSelectAll_ItemClick);
            // 
            // barButtonItemremoveAll
            // 
            this.barButtonItemremoveAll.Caption = "Remove ALL Page";
            this.barButtonItemremoveAll.Id = 5;
            this.barButtonItemremoveAll.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemremoveAll.ImageOptions.SvgImage")));
            this.barButtonItemremoveAll.Name = "barButtonItemremoveAll";
            this.barButtonItemremoveAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemremoveAll_ItemClick);
            // 
            // barButtonItemSaveDB
            // 
            this.barButtonItemSaveDB.Caption = "Save Database";
            this.barButtonItemSaveDB.Id = 6;
            this.barButtonItemSaveDB.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemSaveDB.ImageOptions.SvgImage")));
            this.barButtonItemSaveDB.Name = "barButtonItemSaveDB";
            this.barButtonItemSaveDB.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSaveDB_ItemClick);
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemRun, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemLoadPage, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // barButtonItemRun
            // 
            this.barButtonItemRun.Caption = "Run";
            this.barButtonItemRun.Id = 0;
            this.barButtonItemRun.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemRun.ImageOptions.SvgImage")));
            this.barButtonItemRun.Name = "barButtonItemRun";
            this.barButtonItemRun.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemRun_ItemClick);
            // 
            // barButtonItemLoadPage
            // 
            this.barButtonItemLoadPage.Caption = "Load Page";
            this.barButtonItemLoadPage.Id = 2;
            this.barButtonItemLoadPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItemLoadPage.ImageOptions.Image")));
            this.barButtonItemLoadPage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItemLoadPage.ImageOptions.LargeImage")));
            this.barButtonItemLoadPage.Name = "barButtonItemLoadPage";
            this.barButtonItemLoadPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemLoadPage_ItemClick);
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
            this.barDockControlTop.Size = new System.Drawing.Size(1090, 61);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 434);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1090, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 61);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 373);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1090, 61);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 373);
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 61);
            this.splitContainerControl1.Name = "splitContainerControl1";
            // 
            // splitContainerControl1.Panel1
            // 
            this.splitContainerControl1.Panel1.Controls.Add(this.gridControlPage);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            // 
            // splitContainerControl1.Panel2
            // 
            this.splitContainerControl1.Panel2.Controls.Add(this.gridControlPost);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(1090, 373);
            this.splitContainerControl1.SplitterPosition = 412;
            this.splitContainerControl1.TabIndex = 12;
            // 
            // gridControlPost
            // 
            this.gridControlPost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlPost.Location = new System.Drawing.Point(0, 0);
            this.gridControlPost.MainView = this.gridViewPost;
            this.gridControlPost.MenuManager = this.barManager1;
            this.gridControlPost.Name = "gridControlPost";
            this.gridControlPost.Size = new System.Drawing.Size(666, 373);
            this.gridControlPost.TabIndex = 0;
            this.gridControlPost.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewPost});
            // 
            // gridViewPost
            // 
            this.gridViewPost.GridControl = this.gridControlPost;
            this.gridViewPost.Name = "gridViewPost";
            this.gridViewPost.OptionsView.ShowGroupPanel = false;
            // 
            // gridControlPage
            // 
            this.gridControlPage.Dock = System.Windows.Forms.DockStyle.Fill;
            gridLevelNode1.RelationName = "Level1";
            this.gridControlPage.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] {
            gridLevelNode1});
            this.gridControlPage.Location = new System.Drawing.Point(0, 0);
            this.gridControlPage.MainView = this.gridViewPage;
            this.gridControlPage.MenuManager = this.barManager1;
            this.gridControlPage.Name = "gridControlPage";
            this.gridControlPage.Size = new System.Drawing.Size(412, 373);
            this.gridControlPage.TabIndex = 0;
            this.gridControlPage.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewPage});
            // 
            // gridViewPage
            // 
            this.gridViewPage.GridControl = this.gridControlPage;
            this.gridViewPage.Name = "gridViewPage";
            this.gridViewPage.OptionsView.ShowGroupPanel = false;
            // 
            // FUpdatePostPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1090, 454);
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FUpdatePostPage";
            this.Text = "FUpdatePostPage";
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).EndInit();
            this.splitContainerControl1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).EndInit();
            this.splitContainerControl1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlPost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewPost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlPage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewPage)).EndInit();
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
        private DevExpress.XtraBars.BarButtonItem barButtonItemRun;
        private DevExpress.XtraBars.BarButtonItem barButtonItemLoadPage;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSelectAll;
        private DevExpress.XtraBars.BarButtonItem barButtonItemremoveAll;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSaveDB;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraGrid.GridControl gridControlPost;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewPost;
        private DevExpress.XtraGrid.GridControl gridControlPage;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewPage;
    }
}