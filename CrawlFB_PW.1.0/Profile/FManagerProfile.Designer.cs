namespace CrawlFB_PW._1._0.Profile
{
    partial class FManagerProfile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FManagerProfile));
            this.panelControlSetup = new DevExpress.XtraEditors.PanelControl();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panelControlgrid = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.btnAddProfile = new DevExpress.XtraBars.BarButtonItem();
            this.btnCheckProfile = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteProfile = new DevExpress.XtraBars.BarButtonItem();
            this.btnCannel = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barButtonItemCheckTime = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).BeginInit();
            this.panelControlSetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlgrid)).BeginInit();
            this.panelControlgrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlSetup
            // 
            this.panelControlSetup.Controls.Add(this.lblStatus);
            this.panelControlSetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlSetup.Location = new System.Drawing.Point(0, 30);
            this.panelControlSetup.Margin = new System.Windows.Forms.Padding(4);
            this.panelControlSetup.Name = "panelControlSetup";
            this.panelControlSetup.Size = new System.Drawing.Size(1062, 28);
            this.panelControlSetup.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(5, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(88, 19);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Trạng Thái: ";
            // 
            // panelControlgrid
            // 
            this.panelControlgrid.Controls.Add(this.gridControl1);
            this.panelControlgrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlgrid.Location = new System.Drawing.Point(0, 58);
            this.panelControlgrid.Margin = new System.Windows.Forms.Padding(4);
            this.panelControlgrid.Name = "panelControlgrid";
            this.panelControlgrid.Size = new System.Drawing.Size(1062, 439);
            this.panelControlgrid.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1058, 435);
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
            this.bar2,
            this.bar3});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.btnAddProfile,
            this.btnCheckProfile,
            this.btnDeleteProfile,
            this.btnCannel,
            this.barButtonItemCheckTime});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 5;
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnAddProfile, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnCheckProfile, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnDeleteProfile, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.btnCannel, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItemCheckTime)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // btnAddProfile
            // 
            this.btnAddProfile.Caption = "Thêm Profile";
            this.btnAddProfile.Id = 0;
            this.btnAddProfile.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnAddProfile.ImageOptions.Image")));
            this.btnAddProfile.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnAddProfile.ImageOptions.LargeImage")));
            this.btnAddProfile.Name = "btnAddProfile";
            this.btnAddProfile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddProfile_ItemClick);
            // 
            // btnCheckProfile
            // 
            this.btnCheckProfile.Caption = "Check Profile";
            this.btnCheckProfile.Id = 1;
            this.btnCheckProfile.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnCheckProfile.ImageOptions.Image")));
            this.btnCheckProfile.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnCheckProfile.ImageOptions.LargeImage")));
            this.btnCheckProfile.Name = "btnCheckProfile";
            this.btnCheckProfile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnCheckProfile_ItemClick);
            // 
            // btnDeleteProfile
            // 
            this.btnDeleteProfile.Caption = "Xóa Profile";
            this.btnDeleteProfile.Id = 2;
            this.btnDeleteProfile.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteProfile.ImageOptions.Image")));
            this.btnDeleteProfile.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnDeleteProfile.ImageOptions.LargeImage")));
            this.btnDeleteProfile.Name = "btnDeleteProfile";
            this.btnDeleteProfile.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteProfile_ItemClick);
            // 
            // btnCannel
            // 
            this.btnCannel.Caption = "Thoát";
            this.btnCannel.Id = 3;
            this.btnCannel.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnCannel.ImageOptions.Image")));
            this.btnCannel.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("btnCannel.ImageOptions.LargeImage")));
            this.btnCannel.Name = "btnCannel";
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
            this.barDockControlTop.Size = new System.Drawing.Size(1062, 30);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 497);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1062, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 30);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 467);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1062, 30);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 467);
            // 
            // barButtonItemCheckTime
            // 
            this.barButtonItemCheckTime.Caption = "checkTime";
            this.barButtonItemCheckTime.Id = 4;
            this.barButtonItemCheckTime.Name = "barButtonItemCheckTime";
            this.barButtonItemCheckTime.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemCheckTime_ItemClick);
            // 
            // FManagerProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 517);
            this.Controls.Add(this.panelControlgrid);
            this.Controls.Add(this.panelControlSetup);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FManagerProfile";
            this.Text = "FManagerProfile";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).EndInit();
            this.panelControlSetup.ResumeLayout(false);
            this.panelControlSetup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlgrid)).EndInit();
            this.panelControlgrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControlSetup;
        private DevExpress.XtraEditors.PanelControl panelControlgrid;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.Label lblStatus;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.BarButtonItem btnAddProfile;
        private DevExpress.XtraBars.BarButtonItem btnCheckProfile;
        private DevExpress.XtraBars.BarButtonItem btnDeleteProfile;
        private DevExpress.XtraBars.BarButtonItem btnCannel;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem barButtonItemCheckTime;
    }
}