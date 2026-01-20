namespace CrawlFB_PW._1._0.Page
{
    partial class FFindPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FFindPage));
            this.panelControlSetup = new DevExpress.XtraEditors.PanelControl();
            this.textEditMaxPage = new DevExpress.XtraEditors.TextEdit();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.barButtonItemSelectAll = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemReset = new DevExpress.XtraBars.BarButtonItem();
            this.barSubItemFilter = new DevExpress.XtraBars.BarSubItem();
            this.barButtonItemSaveDB = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemCheckTimeLastPost = new DevExpress.XtraBars.BarButtonItem();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.barButtonItemRun = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemRunAndCheck = new DevExpress.XtraBars.BarButtonItem();
            this.barSubItemSetup = new DevExpress.XtraBars.BarSubItem();
            this.barEditItemMinFlow = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.barCheckItemFanPage = new DevExpress.XtraBars.BarCheckItem();
            this.barCheckItemGroups = new DevExpress.XtraBars.BarCheckItem();
            this.barEditItemTimeLastPost = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.barButtonItemSeleProfile = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.labelmaxpage = new System.Windows.Forms.Label();
            this.textEditKeyword = new DevExpress.XtraEditors.TextEdit();
            this.labelKeywordShearch = new System.Windows.Forms.Label();
            this.panelControlgrid = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.picLoading = new System.Windows.Forms.PictureBox();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).BeginInit();
            this.panelControlSetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditMaxPage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditKeyword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlgrid)).BeginInit();
            this.panelControlgrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlSetup
            // 
            this.panelControlSetup.Controls.Add(this.lblStatus);
            this.panelControlSetup.Controls.Add(this.picLoading);
            this.panelControlSetup.Controls.Add(this.textEditMaxPage);
            this.panelControlSetup.Controls.Add(this.labelmaxpage);
            this.panelControlSetup.Controls.Add(this.textEditKeyword);
            this.panelControlSetup.Controls.Add(this.labelKeywordShearch);
            this.panelControlSetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlSetup.Location = new System.Drawing.Point(0, 61);
            this.panelControlSetup.Name = "panelControlSetup";
            this.panelControlSetup.Size = new System.Drawing.Size(1067, 46);
            this.panelControlSetup.TabIndex = 0;
            // 
            // textEditMaxPage
            // 
            this.textEditMaxPage.Location = new System.Drawing.Point(523, 12);
            this.textEditMaxPage.MenuManager = this.barManager1;
            this.textEditMaxPage.Name = "textEditMaxPage";
            this.textEditMaxPage.Size = new System.Drawing.Size(97, 22);
            this.textEditMaxPage.TabIndex = 3;
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
            this.barButtonItemRunAndCheck,
            this.barSubItemSetup,
            this.barEditItemMinFlow,
            this.barCheckItemFanPage,
            this.barCheckItemGroups,
            this.barEditItemTimeLastPost,
            this.barButtonItemSeleProfile,
            this.barButtonItemSelectAll,
            this.barButtonItemReset,
            this.barSubItemFilter,
            this.barButtonItemSaveDB,
            this.barButtonItemCheckTimeLastPost});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 16;
            this.barManager1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1,
            this.repositoryItemComboBox1});
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
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemReset, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSubItemFilter, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemSaveDB, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemCheckTimeLastPost, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // barButtonItemSelectAll
            // 
            this.barButtonItemSelectAll.Caption = "Chọn Tất cả";
            this.barButtonItemSelectAll.Id = 11;
            this.barButtonItemSelectAll.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemSelectAll.ImageOptions.SvgImage")));
            this.barButtonItemSelectAll.Name = "barButtonItemSelectAll";
            this.barButtonItemSelectAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSelectAll_ItemClick);
            // 
            // barButtonItemReset
            // 
            this.barButtonItemReset.Caption = "Bỏ chọn";
            this.barButtonItemReset.Id = 12;
            this.barButtonItemReset.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemReset.ImageOptions.SvgImage")));
            this.barButtonItemReset.Name = "barButtonItemReset";
            this.barButtonItemReset.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemReset_ItemClick);
            // 
            // barSubItemFilter
            // 
            this.barSubItemFilter.Caption = "Lọc ";
            this.barSubItemFilter.Id = 13;
            this.barSubItemFilter.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barSubItemFilter.ImageOptions.SvgImage")));
            this.barSubItemFilter.Name = "barSubItemFilter";
            // 
            // barButtonItemSaveDB
            // 
            this.barButtonItemSaveDB.Caption = "Lưu DB Page";
            this.barButtonItemSaveDB.Id = 14;
            this.barButtonItemSaveDB.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemSaveDB.ImageOptions.SvgImage")));
            this.barButtonItemSaveDB.Name = "barButtonItemSaveDB";
            this.barButtonItemSaveDB.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSaveDB_ItemClick);
            // 
            // barButtonItemCheckTimeLastPost
            // 
            this.barButtonItemCheckTimeLastPost.Caption = "Check Time Last Post";
            this.barButtonItemCheckTimeLastPost.Id = 15;
            this.barButtonItemCheckTimeLastPost.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemCheckTimeLastPost.ImageOptions.SvgImage")));
            this.barButtonItemCheckTimeLastPost.Name = "barButtonItemCheckTimeLastPost";
            this.barButtonItemCheckTimeLastPost.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemCheckTimeLastPost_ItemClick);
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemRun, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItemRunAndCheck, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barSubItemSetup, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // barButtonItemRun
            // 
            this.barButtonItemRun.Caption = "Run";
            this.barButtonItemRun.Id = 1;
            this.barButtonItemRun.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemRun.ImageOptions.SvgImage")));
            this.barButtonItemRun.Name = "barButtonItemRun";
            this.barButtonItemRun.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemRun_ItemClick);
            // 
            // barButtonItemRunAndCheck
            // 
            this.barButtonItemRunAndCheck.Caption = "Run And Check";
            this.barButtonItemRunAndCheck.Id = 2;
            this.barButtonItemRunAndCheck.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barButtonItemRunAndCheck.ImageOptions.SvgImage")));
            this.barButtonItemRunAndCheck.Name = "barButtonItemRunAndCheck";
            this.barButtonItemRunAndCheck.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemRunAndCheck_ItemClick);
            // 
            // barSubItemSetup
            // 
            this.barSubItemSetup.Caption = "Setup";
            this.barSubItemSetup.Id = 4;
            this.barSubItemSetup.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("barSubItemSetup.ImageOptions.SvgImage")));
            this.barSubItemSetup.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barEditItemMinFlow),
            new DevExpress.XtraBars.LinkPersistInfo(this.barCheckItemFanPage),
            new DevExpress.XtraBars.LinkPersistInfo(this.barCheckItemGroups),
            new DevExpress.XtraBars.LinkPersistInfo(this.barEditItemTimeLastPost),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItemSeleProfile)});
            this.barSubItemSetup.Name = "barSubItemSetup";
            // 
            // barEditItemMinFlow
            // 
            this.barEditItemMinFlow.Caption = "Min Flower/Member (K)";
            this.barEditItemMinFlow.Edit = this.repositoryItemTextEdit1;
            this.barEditItemMinFlow.Id = 5;
            this.barEditItemMinFlow.Name = "barEditItemMinFlow";
            this.barEditItemMinFlow.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barEditItemMinFlow_ItemClick);
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // barCheckItemFanPage
            // 
            this.barCheckItemFanPage.Caption = "FanPage (Trang)";
            this.barCheckItemFanPage.Id = 6;
            this.barCheckItemFanPage.Name = "barCheckItemFanPage";
            // 
            // barCheckItemGroups
            // 
            this.barCheckItemGroups.Caption = "Groups (Nhóm)";
            this.barCheckItemGroups.Id = 7;
            this.barCheckItemGroups.Name = "barCheckItemGroups";
            // 
            // barEditItemTimeLastPost
            // 
            this.barEditItemTimeLastPost.Caption = "Time Last Post";
            this.barEditItemTimeLastPost.Edit = this.repositoryItemComboBox1;
            this.barEditItemTimeLastPost.Id = 8;
            this.barEditItemTimeLastPost.Name = "barEditItemTimeLastPost";
            // 
            // repositoryItemComboBox1
            // 
            this.repositoryItemComboBox1.AutoHeight = false;
            this.repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox1.Name = "repositoryItemComboBox1";
            // 
            // barButtonItemSeleProfile
            // 
            this.barButtonItemSeleProfile.Caption = "Select Profile";
            this.barButtonItemSeleProfile.Id = 10;
            this.barButtonItemSeleProfile.Name = "barButtonItemSeleProfile";
            this.barButtonItemSeleProfile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemSeleProfile_ItemClick);
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
            this.barDockControlTop.Size = new System.Drawing.Size(1067, 61);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 507);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1067, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 61);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 446);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1067, 61);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 446);
            // 
            // labelmaxpage
            // 
            this.labelmaxpage.AutoSize = true;
            this.labelmaxpage.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelmaxpage.Location = new System.Drawing.Point(407, 15);
            this.labelmaxpage.Name = "labelmaxpage";
            this.labelmaxpage.Size = new System.Drawing.Size(69, 18);
            this.labelmaxpage.TabIndex = 2;
            this.labelmaxpage.Text = "MaxPage";
            // 
            // textEditKeyword
            // 
            this.textEditKeyword.Location = new System.Drawing.Point(154, 11);
            this.textEditKeyword.MenuManager = this.barManager1;
            this.textEditKeyword.Name = "textEditKeyword";
            this.textEditKeyword.Size = new System.Drawing.Size(210, 22);
            this.textEditKeyword.TabIndex = 1;
            // 
            // labelKeywordShearch
            // 
            this.labelKeywordShearch.AutoSize = true;
            this.labelKeywordShearch.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelKeywordShearch.Location = new System.Drawing.Point(38, 14);
            this.labelKeywordShearch.Name = "labelKeywordShearch";
            this.labelKeywordShearch.Size = new System.Drawing.Size(89, 18);
            this.labelKeywordShearch.TabIndex = 0;
            this.labelKeywordShearch.Text = "Nhập từ Tìm";
            // 
            // panelControlgrid
            // 
            this.panelControlgrid.Controls.Add(this.gridControl1);
            this.panelControlgrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlgrid.Location = new System.Drawing.Point(0, 107);
            this.panelControlgrid.Name = "panelControlgrid";
            this.panelControlgrid.Size = new System.Drawing.Size(1067, 400);
            this.panelControlgrid.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.MenuManager = this.barManager1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1063, 396);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // picLoading
            // 
            this.picLoading.Image = ((System.Drawing.Image)(resources.GetObject("picLoading.Image")));
            this.picLoading.Location = new System.Drawing.Point(671, 14);
            this.picLoading.Name = "picLoading";
            this.picLoading.Size = new System.Drawing.Size(100, 18);
            this.picLoading.TabIndex = 4;
            picLoading.Visible = false;
            this.picLoading.TabStop = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(833, 17);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 16);
            this.lblStatus.TabIndex = 5;
            // 
            // FFindPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 527);
            this.Controls.Add(this.panelControlgrid);
            this.Controls.Add(this.panelControlSetup);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FFindPage";
            this.Text = "FFindPage";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).EndInit();
            this.panelControlSetup.ResumeLayout(false);
            this.panelControlSetup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEditMaxPage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditKeyword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlgrid)).EndInit();
            this.panelControlgrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControlSetup;
        private DevExpress.XtraEditors.PanelControl panelControlgrid;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.TextEdit textEditKeyword;
        private System.Windows.Forms.Label labelKeywordShearch;
        private DevExpress.XtraEditors.TextEdit textEditMaxPage;
        private System.Windows.Forms.Label labelmaxpage;
        private DevExpress.XtraBars.BarButtonItem barButtonItemRun;
        private DevExpress.XtraBars.BarButtonItem barButtonItemRunAndCheck;
        private DevExpress.XtraBars.BarSubItem barSubItemSetup;
        private DevExpress.XtraBars.BarEditItem barEditItemMinFlow;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraBars.BarCheckItem barCheckItemFanPage;
        private DevExpress.XtraBars.BarCheckItem barCheckItemGroups;
        private DevExpress.XtraBars.BarEditItem barEditItemTimeLastPost;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox1;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSeleProfile;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSelectAll;
        private DevExpress.XtraBars.BarButtonItem barButtonItemReset;
        private DevExpress.XtraBars.BarSubItem barSubItemFilter;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSaveDB;
        private DevExpress.XtraBars.BarButtonItem barButtonItemCheckTimeLastPost;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox picLoading;
    }
}