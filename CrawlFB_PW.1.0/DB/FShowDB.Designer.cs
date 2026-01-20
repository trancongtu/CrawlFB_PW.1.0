namespace CrawlFB_PW._1._0.DB
{
    partial class FShowDB
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FShowDB));
            this.panelControlSetup = new DevExpress.XtraEditors.PanelControl();
            this.label1 = new System.Windows.Forms.Label();
            this.txbBinding = new System.Windows.Forms.TextBox();
            this.btnDeleteone = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnView = new System.Windows.Forms.Button();
            this.cbMaxCount = new System.Windows.Forms.ComboBox();
            this.cbTime = new System.Windows.Forms.ComboBox();
            this.CbSource = new System.Windows.Forms.ComboBox();
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnClearTable = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).BeginInit();
            this.panelControlSetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).BeginInit();
            this.panelControlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControlSetup
            // 
            this.panelControlSetup.Controls.Add(this.btnClearTable);
            this.panelControlSetup.Controls.Add(this.label1);
            this.panelControlSetup.Controls.Add(this.txbBinding);
            this.panelControlSetup.Controls.Add(this.btnDeleteone);
            this.panelControlSetup.Controls.Add(this.labelStatus);
            this.panelControlSetup.Controls.Add(this.btnClearAll);
            this.panelControlSetup.Controls.Add(this.btnView);
            this.panelControlSetup.Controls.Add(this.cbMaxCount);
            this.panelControlSetup.Controls.Add(this.cbTime);
            this.panelControlSetup.Controls.Add(this.CbSource);
            this.panelControlSetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlSetup.Location = new System.Drawing.Point(0, 0);
            this.panelControlSetup.Name = "panelControlSetup";
            this.panelControlSetup.Size = new System.Drawing.Size(1204, 79);
            this.panelControlSetup.TabIndex = 0;
            this.panelControlSetup.Paint += new System.Windows.Forms.PaintEventHandler(this.panelControlSetup_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Maroon;
            this.label1.Location = new System.Drawing.Point(506, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 18);
            this.label1.TabIndex = 8;
            this.label1.Text = "Binding";
            // 
            // txbBinding
            // 
            this.txbBinding.Location = new System.Drawing.Point(618, 50);
            this.txbBinding.Name = "txbBinding";
            this.txbBinding.Size = new System.Drawing.Size(574, 23);
            this.txbBinding.TabIndex = 7;
            // 
            // btnDeleteone
            // 
            this.btnDeleteone.BackColor = System.Drawing.Color.White;
            this.btnDeleteone.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnDeleteone.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteone.Image")));
            this.btnDeleteone.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDeleteone.Location = new System.Drawing.Point(618, 6);
            this.btnDeleteone.Name = "btnDeleteone";
            this.btnDeleteone.Size = new System.Drawing.Size(114, 43);
            this.btnDeleteone.TabIndex = 6;
            this.btnDeleteone.Text = "Xóa Ô";
            this.btnDeleteone.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDeleteone.UseVisualStyleBackColor = false;
            this.btnDeleteone.Click += new System.EventHandler(this.btnDeleteone_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.ForeColor = System.Drawing.Color.Maroon;
            this.labelStatus.Location = new System.Drawing.Point(12, 44);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(79, 18);
            this.labelStatus.TabIndex = 5;
            this.labelStatus.Text = "Trạng Thái";
            // 
            // btnClearAll
            // 
            this.btnClearAll.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnClearAll.Image = ((System.Drawing.Image)(resources.GetObject("btnClearAll.Image")));
            this.btnClearAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClearAll.Location = new System.Drawing.Point(971, 5);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(189, 42);
            this.btnClearAll.TabIndex = 4;
            this.btnClearAll.Text = "Xóa Hết Database";
            this.btnClearAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnView
            // 
            this.btnView.BackColor = System.Drawing.Color.White;
            this.btnView.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnView.Image = ((System.Drawing.Image)(resources.GetObject("btnView.Image")));
            this.btnView.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnView.Location = new System.Drawing.Point(490, 3);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(107, 45);
            this.btnView.TabIndex = 3;
            this.btnView.Text = "Xem";
            this.btnView.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnView.UseVisualStyleBackColor = false;
            // 
            // cbMaxCount
            // 
            this.cbMaxCount.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbMaxCount.FormattingEnabled = true;
            this.cbMaxCount.Items.AddRange(new object[] {
            "100",
            "500",
            "1000",
            "5000",
            "Tất cả"});
            this.cbMaxCount.Location = new System.Drawing.Point(278, 5);
            this.cbMaxCount.Name = "cbMaxCount";
            this.cbMaxCount.Size = new System.Drawing.Size(161, 26);
            this.cbMaxCount.TabIndex = 2;
            this.cbMaxCount.Text = "Chọn số lượng tối đa";
            // 
            // cbTime
            // 
            this.cbTime.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbTime.FormattingEnabled = true;
            this.cbTime.Items.AddRange(new object[] {
            "1 Ngày trước",
            "1 Tuần trước",
            "1 Tháng trước",
            "Toàn thời gian"});
            this.cbTime.Location = new System.Drawing.Point(139, 5);
            this.cbTime.Name = "cbTime";
            this.cbTime.Size = new System.Drawing.Size(133, 26);
            this.cbTime.TabIndex = 1;
            this.cbTime.Text = "Chọn Thời gian";
            // 
            // CbSource
            // 
            this.CbSource.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CbSource.FormattingEnabled = true;
            this.CbSource.Items.AddRange(new object[] {
            "Hội nhóm",
            "Bài Viết",
            "Đối tượng"});
            this.CbSource.Location = new System.Drawing.Point(12, 5);
            this.CbSource.Name = "CbSource";
            this.CbSource.Size = new System.Drawing.Size(121, 26);
            this.CbSource.TabIndex = 0;
            this.CbSource.Text = "Chọn Nguồn";
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.gridControl1);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 79);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(1204, 468);
            this.panelControlMain.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1200, 464);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // btnClearTable
            // 
            this.btnClearTable.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearTable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnClearTable.Image = ((System.Drawing.Image)(resources.GetObject("btnClearTable.Image")));
            this.btnClearTable.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClearTable.Location = new System.Drawing.Point(777, 5);
            this.btnClearTable.Name = "btnClearTable";
            this.btnClearTable.Size = new System.Drawing.Size(178, 42);
            this.btnClearTable.TabIndex = 9;
            this.btnClearTable.Text = "Xóa Hết Bảng";
            this.btnClearTable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnClearTable.UseVisualStyleBackColor = true;
            this.btnClearTable.Click += new System.EventHandler(this.btnClearTable_Click);
            // 
            // FShowDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1204, 547);
            this.Controls.Add(this.panelControlMain);
            this.Controls.Add(this.panelControlSetup);
            this.Name = "FShowDB";
            this.Text = "FShowDB";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).EndInit();
            this.panelControlSetup.ResumeLayout(false);
            this.panelControlSetup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).EndInit();
            this.panelControlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControlSetup;
        private DevExpress.XtraEditors.PanelControl panelControlMain;
        private System.Windows.Forms.ComboBox CbSource;
        private System.Windows.Forms.ComboBox cbTime;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.ComboBox cbMaxCount;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button btnDeleteone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbBinding;
        private System.Windows.Forms.Button btnClearTable;
    }
}