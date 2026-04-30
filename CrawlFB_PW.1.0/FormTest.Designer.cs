namespace CrawlFB_PW._1._0
{
    partial class FormTest
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
            this.txbUrl = new System.Windows.Forms.TextBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.txbProfileId = new System.Windows.Forms.TextBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.btn_TestPopup = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txbUrl
            // 
            this.txbUrl.Location = new System.Drawing.Point(40, 66);
            this.txbUrl.Name = "txbUrl";
            this.txbUrl.Size = new System.Drawing.Size(654, 22);
            this.txbUrl.TabIndex = 0;
            this.txbUrl.Text = "https://www.facebook.com/groups/2795223370777132/";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(323, 145);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(159, 51);
            this.btnTest.TabIndex = 1;
            this.btnTest.Text = "button1";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click_1);
            // 
            // txbProfileId
            // 
            this.txbProfileId.Location = new System.Drawing.Point(40, 103);
            this.txbProfileId.Name = "txbProfileId";
            this.txbProfileId.Size = new System.Drawing.Size(654, 22);
            this.txbProfileId.TabIndex = 2;
            this.txbProfileId.Text = "k122im4k";
            // 
            // rtbLog
            // 
            this.rtbLog.Location = new System.Drawing.Point(40, 213);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.Size = new System.Drawing.Size(713, 205);
            this.rtbLog.TabIndex = 3;
            this.rtbLog.Text = "";
            // 
            // btn_TestPopup
            // 
            this.btn_TestPopup.Location = new System.Drawing.Point(40, 12);
            this.btn_TestPopup.Name = "btn_TestPopup";
            this.btn_TestPopup.Size = new System.Drawing.Size(97, 23);
            this.btn_TestPopup.TabIndex = 4;
            this.btn_TestPopup.Text = "btn_TestPopup";
            this.btn_TestPopup.UseVisualStyleBackColor = true;
            this.btn_TestPopup.Click += new System.EventHandler(this.btn_TestPopup_Click);
            // 
            // FormTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_TestPopup);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.txbProfileId);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.txbUrl);
            this.Name = "FormTest";
            this.Text = "FormTest";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txbUrl;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.TextBox txbProfileId;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Button btn_TestPopup;
    }
}