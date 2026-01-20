namespace CrawlFB_PW._1._0.Profile
{
    partial class SelectProfile
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
            this.btnRefesh = new System.Windows.Forms.Button();
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
            this.panelControlSetup.Controls.Add(this.btnRefesh);
            this.panelControlSetup.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlSetup.Location = new System.Drawing.Point(0, 0);
            this.panelControlSetup.Margin = new System.Windows.Forms.Padding(19, 19, 19, 19);
            this.panelControlSetup.Name = "panelControlSetup";
            this.panelControlSetup.Size = new System.Drawing.Size(1028, 142);
            this.panelControlSetup.TabIndex = 0;
            // 
            // btnRefesh
            // 
            this.btnRefesh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRefesh.Location = new System.Drawing.Point(1648, 72);
            this.btnRefesh.Margin = new System.Windows.Forms.Padding(10, 10, 10, 10);
            this.btnRefesh.Name = "btnRefesh";
            this.btnRefesh.Size = new System.Drawing.Size(299, 88);
            this.btnRefesh.TabIndex = 0;
            this.btnRefesh.Text = "Làm Mới";
            this.btnRefesh.UseVisualStyleBackColor = true;
            // 
            // panelControlMain
            // 
            this.panelControlMain.Controls.Add(this.gridControl1);
            this.panelControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControlMain.Location = new System.Drawing.Point(0, 142);
            this.panelControlMain.Margin = new System.Windows.Forms.Padding(19, 19, 19, 19);
            this.panelControlMain.Name = "panelControlMain";
            this.panelControlMain.Size = new System.Drawing.Size(1028, 379);
            this.panelControlMain.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(15, 15, 15, 15);
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(15, 15, 15, 15);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1024, 375);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 1331;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // SelectProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 521);
            this.Controls.Add(this.panelControlMain);
            this.Controls.Add(this.panelControlSetup);
            this.Name = "SelectProfile";
            this.Text = "SelectProfile";
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
        private System.Windows.Forms.Button btnRefesh;
    }
}