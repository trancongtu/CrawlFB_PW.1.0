using System.Drawing;

namespace CrawlFB_PW._1._0.Page
{
    partial class FSelectPageScanNew
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSelectPageScanNew));
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.txtShearch = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.btnAdd = new DevExpress.XtraBars.BarButtonItem();
            this.btnFilterPage = new DevExpress.XtraBars.BarSubItem();
            this.btnSelectAll = new DevExpress.XtraBars.BarButtonItem();
            this.btnOnlyScan = new DevExpress.XtraBars.BarButtonItem();
            this.btnOnlyNotScan = new DevExpress.XtraBars.BarButtonItem();
            this.btn_Reset = new DevExpress.XtraBars.BarButtonItem();
            this.btnCannel = new DevExpress.XtraBars.BarButtonItem();
            this.btn_Update = new DevExpress.XtraBars.BarSubItem();
            this.barButtonItemUpdateIscan = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.btn_SelectAll = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).BeginInit();
            this.panelControlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.gridControl1);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 53);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(1057, 417);
            this.panelControlMain.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1053, 413);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
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
            this.btnAdd,
            this.btnCannel,
            this.btnFilterPage,
            this.btnSelectAll,
            this.btnOnlyScan,
            this.btnOnlyNotScan,
            this.btn_Reset,
            this.txtShearch,
            this.btn_Update,
            this.barButtonItemUpdateIscan,
            this.btn_SelectAll});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 14;
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
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.Width, this.txtShearch, "", false, true, true, 152)});
            this.bar1.Text = "Tools";
            // 
            // txtShearch
            // 
            this.txtShearch.Caption = "Tìm kiếm";
            this.txtShearch.Edit = this.repositoryItemTextEdit1;
            this.txtShearch.Id = 10;
            this.txtShearch.Name = "txtShearch";
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnAdd, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnFilterPage, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_Reset, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnCannel, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_Update, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btn_SelectAll, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // btnAdd
            // 
            this.btnAdd.Caption = "Chọn Page";
            this.btnAdd.Id = 0;
            this.btnAdd.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.ImageOptions.Image")));
            this.btnAdd.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnAdd.ImageOptions.LargeImage")));
            this.btnAdd.Name = "btnAdd";
            // 
            // btnFilterPage
            // 
            this.btnFilterPage.Caption = "Lọc Page";
            this.btnFilterPage.Id = 5;
            this.btnFilterPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnReset.ImageOptions.Image")));
            this.btnFilterPage.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnReset.ImageOptions.LargeImage")));
            this.btnFilterPage.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btnSelectAll),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnOnlyScan),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnOnlyNotScan)});
            this.btnFilterPage.Name = "btnFilterPage";
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Caption = "Chọn Tất Cả";
            this.btnSelectAll.Id = 6;
            this.btnSelectAll.Name = "btnSelectAll";
            // 
            // btnOnlyScan
            // 
            this.btnOnlyScan.Caption = "Page đã quét";
            this.btnOnlyScan.Id = 7;
            this.btnOnlyScan.Name = "btnOnlyScan";
            // 
            // btnOnlyNotScan
            // 
            this.btnOnlyNotScan.Caption = "Page Chưa Quét";
            this.btnOnlyNotScan.Id = 8;
            this.btnOnlyNotScan.Name = "btnOnlyNotScan";
            // 
            // btn_Reset
            // 
            this.btn_Reset.Caption = "Reset";
            this.btn_Reset.Id = 9;
            this.btn_Reset.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barButtonItem2.ImageOptions.Image")));
            this.btn_Reset.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItem2.ImageOptions.LargeImage")));
            this.btn_Reset.Name = "btn_Reset";
            // 
            // btnCannel
            // 
            this.btnCannel.Caption = "Thoát";
            this.btnCannel.Id = 1;
            this.btnCannel.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnCannel.ImageOptions.Image")));
            this.btnCannel.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnCannel.ImageOptions.LargeImage")));
            this.btnCannel.Name = "btnCannel";
            // 
            // btn_Update
            // 
            this.btn_Update.Caption = "Update Page";
            this.btn_Update.Id = 11;
            this.btn_Update.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btn_Update.ImageOptions.SvgImage")));
            this.btn_Update.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItemUpdateIscan)});
            this.btn_Update.Name = "btn_Update";
            // 
            // barButtonItemUpdateIscan
            // 
            this.barButtonItemUpdateIscan.Caption = "UpdateIscan";
            this.barButtonItemUpdateIscan.Id = 12;
            this.barButtonItemUpdateIscan.Name = "barButtonItemUpdateIscan";
            this.barButtonItemUpdateIscan.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemUpdateIscan_ItemClick);
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
            this.barDockControlTop.Size = new System.Drawing.Size(1057, 53);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 470);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1057, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 53);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 417);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1057, 53);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 417);
            // 
            // btn_SelectAll
            // 
            this.btn_SelectAll.Caption = "Chọn Tất";
            this.btn_SelectAll.Id = 13;
            this.btn_SelectAll.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btn_SelectAll.ImageOptions.SvgImage")));
            this.btn_SelectAll.Name = "btn_SelectAll";
            this.btn_SelectAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_SelectAll_ItemClick);
            // 
            // FSelectPageScanNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1057, 490);
            this.Controls.Add(this.panelControlMain);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FSelectPageScanNew";
            this.Text = "FSelectPage";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).EndInit();
            this.panelControlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.PanelControl panelControlMain;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.BarButtonItem btnAdd;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem btnCannel;
        private DevExpress.XtraBars.BarSubItem btnFilterPage;
        private DevExpress.XtraBars.BarButtonItem btnSelectAll;
        private DevExpress.XtraBars.BarButtonItem btnOnlyScan;
        private DevExpress.XtraBars.BarButtonItem btnOnlyNotScan;
        private DevExpress.XtraBars.BarButtonItem btn_Reset;
        private DevExpress.XtraBars.BarEditItem txtShearch;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraBars.BarSubItem btn_Update;
        private DevExpress.XtraBars.BarButtonItem barButtonItemUpdateIscan;
        private DevExpress.XtraBars.BarButtonItem btn_SelectAll;
    }
}