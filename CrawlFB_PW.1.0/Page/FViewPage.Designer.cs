namespace CrawlFB_PW._1._0.Page
{
    partial class FViewPage
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
            this.btnDeletePageMoniter = new System.Windows.Forms.Button();
            this.btnAddPageMoniter = new System.Windows.Forms.Button();
            this.BtnDeletePageNote = new System.Windows.Forms.Button();
            this.btnAddPageNote = new System.Windows.Forms.Button();
            this.cbSelectSource = new System.Windows.Forms.ComboBox();
            this.panelControlMain = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
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
            this.panelControlSetup.Controls.Add(this.btnDeletePageMoniter);
            this.panelControlSetup.Controls.Add(this.btnAddPageMoniter);
            this.panelControlSetup.Controls.Add(this.BtnDeletePageNote);
            this.panelControlSetup.Controls.Add(this.btnAddPageNote);
            this.panelControlSetup.Controls.Add(this.cbSelectSource);
            this.panelControlSetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlSetup.Location = new System.Drawing.Point(0, 0);
            this.panelControlSetup.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.panelControlSetup.Name = "panelControlSetup";
            this.panelControlSetup.Size = new System.Drawing.Size(1201, 104);
            this.panelControlSetup.TabIndex = 0;
            // 
            // btnDeletePageMoniter
            // 
            this.btnDeletePageMoniter.Location = new System.Drawing.Point(982, 24);
            this.btnDeletePageMoniter.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.btnDeletePageMoniter.Name = "btnDeletePageMoniter";
            this.btnDeletePageMoniter.Size = new System.Drawing.Size(205, 51);
            this.btnDeletePageMoniter.TabIndex = 4;
            this.btnDeletePageMoniter.Text = "xóa khỏi moniter";
            this.btnDeletePageMoniter.UseVisualStyleBackColor = true;
            // 
            // btnAddPageMoniter
            // 
            this.btnAddPageMoniter.Location = new System.Drawing.Point(756, 24);
            this.btnAddPageMoniter.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.btnAddPageMoniter.Name = "btnAddPageMoniter";
            this.btnAddPageMoniter.Size = new System.Drawing.Size(205, 51);
            this.btnAddPageMoniter.TabIndex = 3;
            this.btnAddPageMoniter.Text = "Thêm vào PageMoniter";
            this.btnAddPageMoniter.UseVisualStyleBackColor = true;
            this.btnAddPageMoniter.Click += new System.EventHandler(this.btnAddPageMoniter_Click);
            // 
            // BtnDeletePageNote
            // 
            this.BtnDeletePageNote.Location = new System.Drawing.Point(518, 24);
            this.BtnDeletePageNote.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.BtnDeletePageNote.Name = "BtnDeletePageNote";
            this.BtnDeletePageNote.Size = new System.Drawing.Size(205, 51);
            this.BtnDeletePageNote.TabIndex = 2;
            this.BtnDeletePageNote.Text = "Xóa Khỏi PageNote";
            this.BtnDeletePageNote.UseVisualStyleBackColor = true;
            this.BtnDeletePageNote.Click += new System.EventHandler(this.BtnDeletePageNote_Click_1);
            // 
            // btnAddPageNote
            // 
            this.btnAddPageNote.Location = new System.Drawing.Point(281, 24);
            this.btnAddPageNote.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.btnAddPageNote.Name = "btnAddPageNote";
            this.btnAddPageNote.Size = new System.Drawing.Size(205, 51);
            this.btnAddPageNote.TabIndex = 1;
            this.btnAddPageNote.Text = "Thêm vào PageNote";
            this.btnAddPageNote.UseVisualStyleBackColor = true;
            // 
            // cbSelectSource
            // 
            this.cbSelectSource.FormattingEnabled = true;
            this.cbSelectSource.Location = new System.Drawing.Point(7, 38);
            this.cbSelectSource.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.cbSelectSource.Name = "cbSelectSource";
            this.cbSelectSource.Size = new System.Drawing.Size(226, 24);
            this.cbSelectSource.TabIndex = 0;
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.gridControl1);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 104);
            this.panelControlMain.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(1201, 422);
            this.panelControlMain.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1197, 418);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 546;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // FViewPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1201, 526);
            this.Controls.Add(this.panelControlMain);
            this.Controls.Add(this.panelControlSetup);
            this.Name = "FViewPage";
            this.Text = "FViewPage";
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
        private System.Windows.Forms.Button btnAddPageNote;
        private System.Windows.Forms.ComboBox cbSelectSource;
        private System.Windows.Forms.Button BtnDeletePageNote;
        private System.Windows.Forms.Button btnDeletePageMoniter;
        private System.Windows.Forms.Button btnAddPageMoniter;
    }
}