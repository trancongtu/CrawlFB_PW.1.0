using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.UC.phantich;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;

namespace CrawlFB_PW._1._0.phantich
{
    public partial class FSetupAnalyze : DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm
    {
        UCAddAttentionGroup _UcAddGroups;

        public FSetupAnalyze()
        {
            fluentDesignFormControl1.FluentDesignForm = null;
            AcordingStyleHelper.SetSizeControl(accordionControl1, 200);
            AcordingStyleHelper.StyleAllAccordionElements(accordionControl1);
            this.NavigationControl = accordionControl1; // bắt buộc để set control là hiệu ứng trung tâm
            accordionControl1.ElementClick += AccordionElement_Click;
            InitUC();// GỌI CẤU HÌNH CHÍNH
        }
        private void InitUC()
        {
            _UcAddGroups = new UCAddAttentionGroup();   // UserControl tổng
            _UcAddGroups.Dock = DockStyle.Fill;
            fluentDesignFormContainer1.Controls.Add(_UcAddGroups);
        }
        private void AccordionElement_Click(object sender, ElementClickEventArgs e)
        {
            AcordingStyleHelper.ApplySelectedStyle(e.Element);
            LoadPageUC(e.Element.Name);
        }
        private void LoadPageUC(string elementName)
        {
            // Các element có name giống trong Designer
            // (ACEPageMain, ACEPageNote, ACEPageMonitor)
            // file Designer: :contentReference[oaicite:2]{index=2}

            switch (elementName)
            {
                case "ACE_TieuCuc":
                    ShowUC(_UcAddGroups);
                    break;
            }
        }
        void ShowUC(Control uc)
        {
            foreach (Control c in fluentDesignFormContainer1.Controls)
                c.Visible = false;

            uc.Visible = true;
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();
        }
    }
}
