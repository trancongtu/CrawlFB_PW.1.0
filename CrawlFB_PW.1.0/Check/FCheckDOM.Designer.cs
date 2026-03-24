namespace CrawlFB_PW._1._0.Check
{
    partial class FCheckDOM
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
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.Edit_UrlOrText = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
            this.btn_TestFindPage = new DevExpress.XtraBars.BarButtonItem();
            this.Btn_TestCrawPostPage = new DevExpress.XtraBars.BarButtonItem();
            this.btn_TestCrawtShareOrCommentPost = new DevExpress.XtraBars.BarButtonItem();
            this.barSubItem2 = new DevExpress.XtraBars.BarSubItem();
            this.btn_SaveDOM = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.Rtb_Result = new System.Windows.Forms.RichTextBox();
            this.btn_CheckDOM = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
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
            this.Edit_UrlOrText,
            this.barSubItem1,
            this.btn_TestFindPage,
            this.Btn_TestCrawPostPage,
            this.btn_TestCrawtShareOrCommentPost,
            this.barSubItem2,
            this.btn_SaveDOM,
            this.btn_CheckDOM});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 10;
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
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.Edit_UrlOrText, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // Edit_UrlOrText
            // 
            this.Edit_UrlOrText.Caption = "URL";
            this.Edit_UrlOrText.Edit = this.repositoryItemTextEdit1;
            this.Edit_UrlOrText.Id = 0;
            this.Edit_UrlOrText.Name = "Edit_UrlOrText";
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
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem1),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem2)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // barSubItem1
            // 
            this.barSubItem1.Caption = "Test Nhanh";
            this.barSubItem1.Id = 3;
            this.barSubItem1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_TestFindPage),
            new DevExpress.XtraBars.LinkPersistInfo(this.Btn_TestCrawPostPage),
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_TestCrawtShareOrCommentPost)});
            this.barSubItem1.Name = "barSubItem1";
            // 
            // btn_TestFindPage
            // 
            this.btn_TestFindPage.Caption = "Test FindPage";
            this.btn_TestFindPage.Id = 4;
            this.btn_TestFindPage.Name = "btn_TestFindPage";
            this.btn_TestFindPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_TestFindPage_ItemClick);
            // 
            // Btn_TestCrawPostPage
            // 
            this.Btn_TestCrawPostPage.Caption = "Test CrawlPostPage";
            this.Btn_TestCrawPostPage.Id = 5;
            this.Btn_TestCrawPostPage.Name = "Btn_TestCrawPostPage";
            this.Btn_TestCrawPostPage.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.Btn_TestCrawPostPage_ItemClick);
            // 
            // btn_TestCrawtShareOrCommentPost
            // 
            this.btn_TestCrawtShareOrCommentPost.Caption = "Test Crawl ShareOrComent Post";
            this.btn_TestCrawtShareOrCommentPost.Id = 6;
            this.btn_TestCrawtShareOrCommentPost.Name = "btn_TestCrawtShareOrCommentPost";
            this.btn_TestCrawtShareOrCommentPost.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_TestCrawtShareOrCommentPost_ItemClick);
            // 
            // barSubItem2
            // 
            this.barSubItem2.Caption = "DOM Check";
            this.barSubItem2.Id = 7;
            this.barSubItem2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_SaveDOM),
            new DevExpress.XtraBars.LinkPersistInfo(this.btn_CheckDOM)});
            this.barSubItem2.Name = "barSubItem2";
            // 
            // btn_SaveDOM
            // 
            this.btn_SaveDOM.Caption = "Save DOM";
            this.btn_SaveDOM.Id = 8;
            this.btn_SaveDOM.Name = "btn_SaveDOM";
            this.btn_SaveDOM.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_SaveDOM_ItemClick);
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
            this.barDockControlTop.Size = new System.Drawing.Size(922, 51);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 460);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(922, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 51);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 409);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(922, 51);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 409);
            // 
            // Rtb_Result
            // 
            this.Rtb_Result.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Rtb_Result.Location = new System.Drawing.Point(0, 51);
            this.Rtb_Result.Name = "Rtb_Result";
            this.Rtb_Result.Size = new System.Drawing.Size(922, 409);
            this.Rtb_Result.TabIndex = 4;
            this.Rtb_Result.Text = "";
            // 
            // btn_CheckDOM
            // 
            this.btn_CheckDOM.Caption = "Check DOM";
            this.btn_CheckDOM.Id = 9;
            this.btn_CheckDOM.Name = "btn_CheckDOM";
            this.btn_CheckDOM.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btn_CheckDOM_ItemClick);
            // 
            // FCheckDOM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 480);
            this.Controls.Add(this.Rtb_Result);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FCheckDOM";
            this.Text = "FCheckDOM";
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
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
        private System.Windows.Forms.RichTextBox Rtb_Result;
        private DevExpress.XtraBars.BarEditItem Edit_UrlOrText;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraBars.BarSubItem barSubItem1;
        private DevExpress.XtraBars.BarButtonItem btn_TestFindPage;
        private DevExpress.XtraBars.BarButtonItem Btn_TestCrawPostPage;
        private DevExpress.XtraBars.BarButtonItem btn_TestCrawtShareOrCommentPost;
        private DevExpress.XtraBars.BarSubItem barSubItem2;
        private DevExpress.XtraBars.BarButtonItem btn_SaveDOM;
        private DevExpress.XtraBars.BarButtonItem btn_CheckDOM;
    }
}