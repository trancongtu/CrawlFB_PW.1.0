namespace CrawlFB_PW._1._0.DB
{
    partial class FDatabaseMain
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
            this.fluentDesignFormContainer1 = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer();
            this.accordionControl1 = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.ACPageMain = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageAll = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageNote = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageMonitor = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACPost = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPostAll = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACPerson = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPerson = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.fluentDesignFormControl1 = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl();
            this.fluentFormDefaultManager1 = new DevExpress.XtraBars.FluentDesignSystem.FluentFormDefaultManager(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentFormDefaultManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // fluentDesignFormContainer1
            // 
            this.fluentDesignFormContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fluentDesignFormContainer1.Location = new System.Drawing.Point(260, 39);
            this.fluentDesignFormContainer1.Name = "fluentDesignFormContainer1";
            this.fluentDesignFormContainer1.Size = new System.Drawing.Size(695, 501);
            this.fluentDesignFormContainer1.TabIndex = 0;
            // 
            // accordionControl1
            // 
            this.accordionControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.accordionControl1.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.ACPageMain,
            this.ACPost,
            this.ACPerson});
            this.accordionControl1.Location = new System.Drawing.Point(0, 39);
            this.accordionControl1.Name = "accordionControl1";
            this.accordionControl1.ScrollBarMode = DevExpress.XtraBars.Navigation.ScrollBarMode.Touch;
            this.accordionControl1.Size = new System.Drawing.Size(260, 501);
            this.accordionControl1.TabIndex = 1;
            this.accordionControl1.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu;
            // 
            // ACPageMain
            // 
            this.ACPageMain.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.ACEPageAll,
            this.ACEPageNote,
            this.ACEPageMonitor});
            this.ACPageMain.Expanded = true;
            this.ACPageMain.Name = "ACPageMain";
            this.ACPageMain.Text = "Hội Nhóm";
            // 
            // ACEPageAll
            // 
            this.ACEPageAll.Name = "ACEPageAll";
            this.ACEPageAll.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageAll.Text = "Tổng Hội nhóm";
            // 
            // ACEPageNote
            // 
            this.ACEPageNote.Name = "ACEPageNote";
            this.ACEPageNote.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageNote.Text = "Hội Nhóm Note";
            // 
            // ACEPageMonitor
            // 
            this.ACEPageMonitor.Name = "ACEPageMonitor";
            this.ACEPageMonitor.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageMonitor.Text = "Hội Nhóm Giám sát";
            // 
            // ACPost
            // 
            this.ACPost.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.ACEPostAll});
            this.ACPost.Expanded = true;
            this.ACPost.Name = "ACPost";
            this.ACPost.Text = "Bài Viết";
            // 
            // ACEPostAll
            // 
            this.ACEPostAll.Name = "ACEPostAll";
            this.ACEPostAll.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPostAll.Text = "Tổng Bài Viết";
            // 
            // ACPerson
            // 
            this.ACPerson.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.ACEPerson});
            this.ACPerson.Name = "ACPerson";
            this.ACPerson.Text = "Đối Tượng";
            // 
            // ACEPerson
            // 
            this.ACEPerson.Name = "ACEPerson";
            this.ACEPerson.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPerson.Text = "Tổng Đối tượng";
            // 
            // fluentDesignFormControl1
            // 
            this.fluentDesignFormControl1.FluentDesignForm = this;
            this.fluentDesignFormControl1.Location = new System.Drawing.Point(0, 0);
            this.fluentDesignFormControl1.Manager = this.fluentFormDefaultManager1;
            this.fluentDesignFormControl1.Name = "fluentDesignFormControl1";
            this.fluentDesignFormControl1.Size = new System.Drawing.Size(955, 39);
            this.fluentDesignFormControl1.TabIndex = 2;
            this.fluentDesignFormControl1.TabStop = false;
            // 
            // fluentFormDefaultManager1
            // 
            this.fluentFormDefaultManager1.Form = this;
            // 
            // FDatabaseMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(955, 540);
            this.ControlContainer = this.fluentDesignFormContainer1;
            this.Controls.Add(this.fluentDesignFormContainer1);
            this.Controls.Add(this.accordionControl1);
            this.Controls.Add(this.fluentDesignFormControl1);
            this.FluentDesignFormControl = this.fluentDesignFormControl1;
            this.Name = "FDatabaseMain";
            this.NavigationControl = this.accordionControl1;
            this.Text = "FDatabaseMain";
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentFormDefaultManager1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer fluentDesignFormContainer1;
        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl1;
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl fluentDesignFormControl1;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACPageMain;
        private DevExpress.XtraBars.FluentDesignSystem.FluentFormDefaultManager fluentFormDefaultManager1;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageAll;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageNote;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageMonitor;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACPost;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPostAll;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACPerson;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPerson;
    }
}