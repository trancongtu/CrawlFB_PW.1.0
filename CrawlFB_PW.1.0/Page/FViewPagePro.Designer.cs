namespace CrawlFB_PW._1._0.Page
{
    partial class FViewPagePro
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FViewPagePro));
            this.fluentDesignFormContainer1 = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer();
            this.accordionControl1 = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.ACMenu = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageMain = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageAdded = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageCrawl = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageNote = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPageMonitor = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACPostPage = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPostAllPage = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPostPageNote = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.ACEPostPageMonitor = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.fluentDesignFormControl1 = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl();
            this.fluentFormDefaultManager1 = new DevExpress.XtraBars.FluentDesignSystem.FluentFormDefaultManager(this.components);
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barManager2 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barDockControl1 = new DevExpress.XtraBars.BarDockControl();
            this.barDockControl2 = new DevExpress.XtraBars.BarDockControl();
            this.barDockControl3 = new DevExpress.XtraBars.BarDockControl();
            this.barDockControl4 = new DevExpress.XtraBars.BarDockControl();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentFormDefaultManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager2)).BeginInit();
            this.SuspendLayout();
            // 
            // fluentDesignFormContainer1
            // 
            this.fluentDesignFormContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fluentDesignFormContainer1.Location = new System.Drawing.Point(212, 39);
            this.fluentDesignFormContainer1.Name = "fluentDesignFormContainer1";
            this.fluentDesignFormContainer1.Size = new System.Drawing.Size(810, 480);
            this.fluentDesignFormContainer1.TabIndex = 0;
            // 
            // accordionControl1
            // 
            this.accordionControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.accordionControl1.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.ACMenu,
            this.ACPostPage});
            this.accordionControl1.Location = new System.Drawing.Point(0, 39);
            this.accordionControl1.Name = "accordionControl1";
            this.accordionControl1.ScrollBarMode = DevExpress.XtraBars.Navigation.ScrollBarMode.Touch;
            this.accordionControl1.Size = new System.Drawing.Size(212, 480);
            this.accordionControl1.TabIndex = 1;
            this.accordionControl1.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu;
            // 
            // ACMenu
            // 
            this.ACMenu.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.ACEPageMain,
            this.ACEPageAdded,
            this.ACEPageCrawl,
            this.ACEPageNote,
            this.ACEPageMonitor});
            this.ACMenu.Expanded = true;
            this.ACMenu.Name = "ACMenu";
            this.ACMenu.Text = "Xem Page";
            // 
            // ACEPageMain
            // 
            this.ACEPageMain.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPageMain.ImageOptions.Image")));
            this.ACEPageMain.Name = "ACEPageMain";
            this.ACEPageMain.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageMain.Text = "Tổng Page";
            // 
            // ACEPageAdded
            // 
            this.ACEPageAdded.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPageAdded.ImageOptions.Image")));
            this.ACEPageAdded.Name = "ACEPageAdded";
            this.ACEPageAdded.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageAdded.Text = "Page đã Add";
            // 
            // ACEPageCrawl
            // 
            this.ACEPageCrawl.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPageCrawl.ImageOptions.Image")));
            this.ACEPageCrawl.Name = "ACEPageCrawl";
            this.ACEPageCrawl.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageCrawl.Text = "Page Chưa quét";
            this.ACEPageCrawl.Click += new System.EventHandler(this.ACEPageCrawl_Click);
            // 
            // ACEPageNote
            // 
            this.ACEPageNote.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPageNote.ImageOptions.Image")));
            this.ACEPageNote.Name = "ACEPageNote";
            this.ACEPageNote.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageNote.Text = "Page Note";
            // 
            // ACEPageMonitor
            // 
            this.ACEPageMonitor.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPageMonitor.ImageOptions.Image")));
            this.ACEPageMonitor.Name = "ACEPageMonitor";
            this.ACEPageMonitor.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPageMonitor.Text = "Page Theo Dõi";
            // 
            // ACPostPage
            // 
            this.ACPostPage.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.ACEPostAllPage,
            this.ACEPostPageNote,
            this.ACEPostPageMonitor});
            this.ACPostPage.Expanded = true;
            this.ACPostPage.Name = "ACPostPage";
            this.ACPostPage.Text = "Bài Viết Page";
            // 
            // ACEPostAllPage
            // 
            this.ACEPostAllPage.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPostAllPage.ImageOptions.Image")));
            this.ACEPostAllPage.Name = "ACEPostAllPage";
            this.ACEPostAllPage.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPostAllPage.Text = "Bài Viết All Page";
            this.ACEPostAllPage.Click += new System.EventHandler(this.ACEPateMain_Click);
            // 
            // ACEPostPageNote
            // 
            this.ACEPostPageNote.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPostPageNote.ImageOptions.Image")));
            this.ACEPostPageNote.Name = "ACEPostPageNote";
            this.ACEPostPageNote.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPostPageNote.Text = "Bài viết Page Note";
            // 
            // ACEPostPageMonitor
            // 
            this.ACEPostPageMonitor.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ACEPostPageMonitor.ImageOptions.Image")));
            this.ACEPostPageMonitor.Name = "ACEPostPageMonitor";
            this.ACEPostPageMonitor.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.ACEPostPageMonitor.Text = "Bài Viết PageMoniter";
            // 
            // fluentDesignFormControl1
            // 
            this.fluentDesignFormControl1.FluentDesignForm = this;
            this.fluentDesignFormControl1.Location = new System.Drawing.Point(0, 0);
            this.fluentDesignFormControl1.Manager = this.fluentFormDefaultManager1;
            this.fluentDesignFormControl1.Name = "fluentDesignFormControl1";
            this.fluentDesignFormControl1.Size = new System.Drawing.Size(1022, 39);
            this.fluentDesignFormControl1.TabIndex = 2;
            this.fluentDesignFormControl1.TabStop = false;
            // 
            // fluentFormDefaultManager1
            // 
            this.fluentFormDefaultManager1.Form = this;
            // 
            // barManager1
            // 
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 39);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1022, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 519);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1022, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 39);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 480);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1022, 39);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 480);
            // 
            // barManager2
            // 
            this.barManager2.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar3});
            this.barManager2.DockControls.Add(this.barDockControl1);
            this.barManager2.DockControls.Add(this.barDockControl2);
            this.barManager2.DockControls.Add(this.barDockControl3);
            this.barManager2.DockControls.Add(this.barDockControl4);
            this.barManager2.Form = this;
            this.barManager2.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barButtonItem1,
            this.barButtonItem2});
            this.barManager2.MaxItemId = 2;
            this.barManager2.StatusBar = this.bar3;
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
            // barDockControl1
            // 
            this.barDockControl1.CausesValidation = false;
            this.barDockControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControl1.Location = new System.Drawing.Point(0, 39);
            this.barDockControl1.Manager = this.barManager2;
            this.barDockControl1.Size = new System.Drawing.Size(1022, 0);
            // 
            // barDockControl2
            // 
            this.barDockControl2.CausesValidation = false;
            this.barDockControl2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControl2.Location = new System.Drawing.Point(0, 519);
            this.barDockControl2.Manager = this.barManager2;
            this.barDockControl2.Size = new System.Drawing.Size(1022, 20);
            // 
            // barDockControl3
            // 
            this.barDockControl3.CausesValidation = false;
            this.barDockControl3.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControl3.Location = new System.Drawing.Point(0, 39);
            this.barDockControl3.Manager = this.barManager2;
            this.barDockControl3.Size = new System.Drawing.Size(0, 480);
            // 
            // barDockControl4
            // 
            this.barDockControl4.CausesValidation = false;
            this.barDockControl4.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControl4.Location = new System.Drawing.Point(1022, 39);
            this.barDockControl4.Manager = this.barManager2;
            this.barDockControl4.Size = new System.Drawing.Size(0, 480);
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "barButtonItem1";
            this.barButtonItem1.Id = 0;
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "barButtonItem2";
            this.barButtonItem2.Id = 1;
            this.barButtonItem2.Name = "barButtonItem2";
            // 
            // FViewPagePro
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 539);
            this.ControlContainer = this.fluentDesignFormContainer1;
            this.Controls.Add(this.fluentDesignFormContainer1);
            this.Controls.Add(this.accordionControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Controls.Add(this.barDockControl3);
            this.Controls.Add(this.barDockControl4);
            this.Controls.Add(this.barDockControl2);
            this.Controls.Add(this.barDockControl1);
            this.Controls.Add(this.fluentDesignFormControl1);
            this.FluentDesignFormControl = this.fluentDesignFormControl1;
            this.Name = "FViewPagePro";
            this.NavigationControl = this.accordionControl1;
            this.Text = "FViewPagePro";
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentFormDefaultManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer fluentDesignFormContainer1;
        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl1;
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl fluentDesignFormControl1;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACMenu;
        private DevExpress.XtraBars.FluentDesignSystem.FluentFormDefaultManager fluentFormDefaultManager1;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageMain;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageNote;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageMonitor;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarDockControl barDockControl3;
        private DevExpress.XtraBars.BarManager barManager2;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControl1;
        private DevExpress.XtraBars.BarDockControl barDockControl2;
        private DevExpress.XtraBars.BarDockControl barDockControl4;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACPostPage;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPostAllPage;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPostPageNote;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPostPageMonitor;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageAdded;
        private DevExpress.XtraBars.Navigation.AccordionControlElement ACEPageCrawl;
    }
}