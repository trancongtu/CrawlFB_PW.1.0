namespace CrawlFB_PW._1._0.Page
{
    partial class FSupervisePage
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
            this.panelControlSetup = new DevExpress.XtraEditors.PanelControl();
            this.cbSelectProfile = new System.Windows.Forms.ComboBox();
            this.btnShearch = new System.Windows.Forms.Button();
            this.ChbAuto = new System.Windows.Forms.CheckBox();
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.CbTime = new System.Windows.Forms.ComboBox();
            this.txbLinkPage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelControlGrid = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.panelControlStatus = new DevExpress.XtraEditors.PanelControl();
            this.labelStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).BeginInit();
            this.panelControlSetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlGrid)).BeginInit();
            this.panelControlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlStatus)).BeginInit();
            this.panelControlStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelControlSetup
            // 
            this.panelControlSetup.Controls.Add(this.cbSelectProfile);
            this.panelControlSetup.Controls.Add(this.btnShearch);
            this.panelControlSetup.Controls.Add(this.ChbAuto);
            this.panelControlSetup.Controls.Add(this.btnLoadFile);
            this.panelControlSetup.Controls.Add(this.CbTime);
            this.panelControlSetup.Controls.Add(this.txbLinkPage);
            this.panelControlSetup.Controls.Add(this.label1);
            this.panelControlSetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlSetup.Location = new System.Drawing.Point(0, 0);
            this.panelControlSetup.Name = "panelControlSetup";
            this.panelControlSetup.Size = new System.Drawing.Size(1137, 100);
            this.panelControlSetup.TabIndex = 0;
            // 
            // cbSelectProfile
            // 
            this.cbSelectProfile.FormattingEnabled = true;
            this.cbSelectProfile.Location = new System.Drawing.Point(779, 61);
            this.cbSelectProfile.Name = "cbSelectProfile";
            this.cbSelectProfile.Size = new System.Drawing.Size(135, 24);
            this.cbSelectProfile.TabIndex = 6;
            this.cbSelectProfile.Text = "Chọn Profile";
            this.cbSelectProfile.SelectedIndexChanged += new System.EventHandler(this.cbSelectProfile_SelectedIndexChanged);
            // 
            // btnShearch
            // 
            this.btnShearch.Location = new System.Drawing.Point(975, 62);
            this.btnShearch.Name = "btnShearch";
            this.btnShearch.Size = new System.Drawing.Size(109, 23);
            this.btnShearch.TabIndex = 5;
            this.btnShearch.Text = "Quét";
            this.btnShearch.UseVisualStyleBackColor = true;
            this.btnShearch.Click += new System.EventHandler(this.btnShearch_Click_1);
            // 
            // ChbAuto
            // 
            this.ChbAuto.AutoSize = true;
            this.ChbAuto.Location = new System.Drawing.Point(131, 62);
            this.ChbAuto.Name = "ChbAuto";
            this.ChbAuto.Size = new System.Drawing.Size(118, 20);
            this.ChbAuto.TabIndex = 4;
            this.ChbAuto.Text = "Tự động lấy bài";
            this.ChbAuto.UseVisualStyleBackColor = true;
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(779, 18);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(109, 23);
            this.btnLoadFile.TabIndex = 3;
            this.btnLoadFile.Text = "Lấy từ File";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            // 
            // CbTime
            // 
            this.CbTime.FormattingEnabled = true;
            this.CbTime.Items.AddRange(new object[] {
            "1 ngày",
            "1 tuần",
            "10 ngày",
            "1 tháng",
            "1 năm (hạn chế dùng)"});
            this.CbTime.Location = new System.Drawing.Point(936, 15);
            this.CbTime.Name = "CbTime";
            this.CbTime.Size = new System.Drawing.Size(159, 24);
            this.CbTime.TabIndex = 2;
            this.CbTime.Text = "Thời gian lấy";
            this.CbTime.SelectedIndexChanged += new System.EventHandler(this.CbTime_SelectedIndexChanged);
            // 
            // txbLinkPage
            // 
            this.txbLinkPage.Location = new System.Drawing.Point(131, 15);
            this.txbLinkPage.Name = "txbLinkPage";
            this.txbLinkPage.Size = new System.Drawing.Size(626, 23);
            this.txbLinkPage.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Địa chỉ Page";
            // 
            // panelControlGrid
            // 
            this.panelControlGrid.Controls.Add(this.gridControl1);
            this.panelControlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlGrid.Location = new System.Drawing.Point(0, 100);
            this.panelControlGrid.Name = "panelControlGrid";
            this.panelControlGrid.Size = new System.Drawing.Size(1137, 417);
            this.panelControlGrid.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1133, 413);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // panelControlStatus
            // 
            this.panelControlStatus.Controls.Add(this.labelStatus);
            this.panelControlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelControlStatus.Location = new System.Drawing.Point(0, 459);
            this.panelControlStatus.Name = "panelControlStatus";
            this.panelControlStatus.Size = new System.Drawing.Size(1137, 58);
            this.panelControlStatus.TabIndex = 2;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.ForeColor = System.Drawing.Color.Red;
            this.labelStatus.Location = new System.Drawing.Point(49, 21);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(75, 19);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "Trạng thái";
            // 
            // FSupervisePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 517);
            this.Controls.Add(this.panelControlStatus);
            this.Controls.Add(this.panelControlGrid);
            this.Controls.Add(this.panelControlSetup);
            this.Name = "FSupervisePage";
            this.Text = "FSupervisePage";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).EndInit();
            this.panelControlSetup.ResumeLayout(false);
            this.panelControlSetup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlGrid)).EndInit();
            this.panelControlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlStatus)).EndInit();
            this.panelControlStatus.ResumeLayout(false);
            this.panelControlStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControlSetup;
        private DevExpress.XtraEditors.PanelControl panelControlGrid;
        private DevExpress.XtraEditors.PanelControl panelControlStatus;
        private System.Windows.Forms.ComboBox CbTime;
        private System.Windows.Forms.TextBox txbLinkPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnShearch;
        private System.Windows.Forms.CheckBox ChbAuto;
        private System.Windows.Forms.Button btnLoadFile;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ComboBox cbSelectProfile;
    }
}