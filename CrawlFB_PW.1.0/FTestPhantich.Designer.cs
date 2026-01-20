namespace CrawlFB_PW._1._0
{
    partial class FTestPhantich
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
            this.panelControlsetup = new DevExpress.XtraEditors.PanelControl();
            this.panelControlmain = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemconvert = new DevExpress.XtraBars.BarButtonItem();
            this.UpdateTopicEngine = new DevExpress.XtraBars.BarButtonItem();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrev = new System.Windows.Forms.Button();
            this.barButtonItemExportTopic = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItemXuhuong = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlsetup)).BeginInit();
            this.panelControlsetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlmain)).BeginInit();
            this.panelControlmain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlsetup
            // 
            this.panelControlsetup.Controls.Add(this.btnPrev);
            this.panelControlsetup.Controls.Add(this.btnNext);
            this.panelControlsetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlsetup.Location = new System.Drawing.Point(0, 51);
            this.panelControlsetup.Name = "panelControlsetup";
            this.panelControlsetup.Size = new System.Drawing.Size(1034, 45);
            this.panelControlsetup.TabIndex = 0;
            // 
            // panelControlmain
            // 
            this.panelControlmain.Controls.Add(this.gridControl1);
            this.panelControlmain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlmain.Location = new System.Drawing.Point(0, 96);
            this.panelControlmain.Name = "panelControlmain";
            this.panelControlmain.Size = new System.Drawing.Size(1034, 365);
            this.panelControlmain.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1030, 361);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ShowGroupPanel = false;
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
            this.barButtonItem1,
            this.barButtonItemconvert,
            this.UpdateTopicEngine,
            this.barButtonItemExportTopic,
            this.barButtonItemXuhuong});
            this.barManager1.MainMenu = this.bar2;
            this.barManager1.MaxItemId = 5;
            this.barManager1.StatusBar = this.bar3;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1034, 51);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 461);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1034, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 51);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 410);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1034, 51);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 410);
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItemconvert),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItemXuhuong)});
            this.bar1.Text = "Tools";
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem1),
            new DevExpress.XtraBars.LinkPersistInfo(this.UpdateTopicEngine),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItemExportTopic)});
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.bar3.OptionsBar.AllowQuickCustomization = false;
            this.bar3.OptionsBar.DrawDragBorder = false;
            this.bar3.OptionsBar.UseWholeRow = true;
            this.bar3.Text = "Status bar";
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "Lọc Chủ đề";
            this.barButtonItem1.Id = 0;
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // barButtonItemconvert
            // 
            this.barButtonItemconvert.Caption = "Convert";
            this.barButtonItemconvert.Id = 1;
            this.barButtonItemconvert.Name = "barButtonItemconvert";
            this.barButtonItemconvert.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemconvert_ItemClick);
            // 
            // UpdateTopicEngine
            // 
            this.UpdateTopicEngine.Caption = "Cập nhật";
            this.UpdateTopicEngine.Id = 2;
            this.UpdateTopicEngine.Name = "UpdateTopicEngine";
            this.UpdateTopicEngine.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.UpdateTopicEngine_ItemClick);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(885, 16);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(49, 23);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(818, 16);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(49, 23);
            this.btnPrev.TabIndex = 1;
            this.btnPrev.Text = "Prev";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click_1);
            // 
            // barButtonItemExportTopic
            // 
            this.barButtonItemExportTopic.Caption = "Export Topic";
            this.barButtonItemExportTopic.Id = 3;
            this.barButtonItemExportTopic.Name = "barButtonItemExportTopic";
            this.barButtonItemExportTopic.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemExportTopic_ItemClick);
            // 
            // barButtonItemXuhuong
            // 
            this.barButtonItemXuhuong.Caption = "Xu hướng";
            this.barButtonItemXuhuong.Id = 4;
            this.barButtonItemXuhuong.Name = "barButtonItemXuhuong";
            this.barButtonItemXuhuong.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItemXuhuong_ItemClick);
            // 
            // FTestPhantich
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 481);
            this.Controls.Add(this.panelControlmain);
            this.Controls.Add(this.panelControlsetup);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "FTestPhantich";
            this.Text = "FTestPhantich";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlsetup)).EndInit();
            this.panelControlsetup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControlmain)).EndInit();
            this.panelControlmain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControlsetup;
        private DevExpress.XtraEditors.PanelControl panelControlmain;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.BarButtonItem barButtonItemconvert;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem UpdateTopicEngine;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnNext;
        private DevExpress.XtraBars.BarButtonItem barButtonItemExportTopic;
        private DevExpress.XtraBars.BarButtonItem barButtonItemXuhuong;
    }
}