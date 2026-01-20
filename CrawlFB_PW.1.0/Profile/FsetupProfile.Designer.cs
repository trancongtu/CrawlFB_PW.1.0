namespace CrawlFB_PW._1._0.Profile
{
    partial class FsetupProfile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FsetupProfile));
            this.label1 = new System.Windows.Forms.Label();
            this.txbProfileId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txbName = new System.Windows.Forms.MaskedTextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Điền ID AdsBrower";
            // 
            // txbProfileId
            // 
            this.txbProfileId.Location = new System.Drawing.Point(239, 60);
            this.txbProfileId.Name = "txbProfileId";
            this.txbProfileId.Size = new System.Drawing.Size(443, 22);
            this.txbProfileId.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(61, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Tên Profile";
            // 
            // txbName
            // 
            this.txbName.Location = new System.Drawing.Point(239, 111);
            this.txbName.Name = "txbName";
            this.txbName.Size = new System.Drawing.Size(443, 22);
            this.txbName.TabIndex = 5;
            // 
            // btnCreate
            // 
            this.btnCreate.Image = ((System.Drawing.Image)(resources.GetObject("btnCreate.Image")));
            this.btnCreate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCreate.Location = new System.Drawing.Point(239, 174);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(163, 38);
            this.btnCreate.TabIndex = 6;
            this.btnCreate.Text = "Tạo Profile";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // FsetupProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(767, 296);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txbName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txbProfileId);
            this.Controls.Add(this.label1);
            this.Name = "FsetupProfile";
            this.Text = "FsetupProfile";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbProfileId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox txbName;
        private System.Windows.Forms.Button btnCreate;
    }
}