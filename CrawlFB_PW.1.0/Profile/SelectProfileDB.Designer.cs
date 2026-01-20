namespace CrawlFB_PW._1._0.Profile
{
    partial class SelectProfileDB
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectProfileDB));
            this.panelControlSetup = new DevExpress.XtraEditors.PanelControl();
            this.btnSelectProfile = new System.Windows.Forms.Button();
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
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
            this.panelControlSetup.Controls.Add(this.btnCancel);
            this.panelControlSetup.Controls.Add(this.btnReset);
            this.panelControlSetup.Controls.Add(this.btnSelectAll);
            this.panelControlSetup.Controls.Add(this.btnSelectProfile);
            this.panelControlSetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlSetup.Location = new System.Drawing.Point(0, 0);
            this.panelControlSetup.Name = "panelControlSetup";
            this.panelControlSetup.Size = new System.Drawing.Size(1015, 36);
            this.panelControlSetup.TabIndex = 0;
            // 
            // btnSelectProfile
            // 
            this.btnSelectProfile.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectProfile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnSelectProfile.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectProfile.Image")));
            this.btnSelectProfile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSelectProfile.Location = new System.Drawing.Point(64, 0);
            this.btnSelectProfile.Name = "btnSelectProfile";
            this.btnSelectProfile.Size = new System.Drawing.Size(138, 38);
            this.btnSelectProfile.TabIndex = 0;
            this.btnSelectProfile.Text = "Chọn Profile";
            this.btnSelectProfile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSelectProfile.UseVisualStyleBackColor = true;
            this.btnSelectProfile.Click += new System.EventHandler(this.btnSelectProfile_Click);
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.gridControl1);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 36);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(1015, 459);
            this.panelControlMain.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1011, 455);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAll.Image")));
            this.btnSelectAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSelectAll.Location = new System.Drawing.Point(352, 0);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(138, 38);
            this.btnSelectAll.TabIndex = 1;
            this.btnSelectAll.Text = "Chọn Hết";
            this.btnSelectAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnReset
            // 
            this.btnReset.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnReset.Image = ((System.Drawing.Image)(resources.GetObject("btnReset.Image")));
            this.btnReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReset.Location = new System.Drawing.Point(614, 0);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(138, 38);
            this.btnReset.TabIndex = 2;
            this.btnReset.Text = "Bỏ chọn";
            this.btnReset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Times New Roman", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(872, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(138, 38);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Thoát";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SelectProfileDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1015, 495);
            this.Controls.Add(this.panelControlMain);
            this.Controls.Add(this.panelControlSetup);
            this.Name = "SelectProfileDB";
            this.Text = "SelectProfileDB";
            ((System.ComponentModel.ISupportInitialize)(this.panelControlSetup)).EndInit();
            this.panelControlSetup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControlMain)).EndInit();
            this.panelControlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControlSetup;
        private DevExpress.XtraEditors.PanelControl panelControlMain;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.Button btnSelectProfile;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnSelectAll;
    }
}