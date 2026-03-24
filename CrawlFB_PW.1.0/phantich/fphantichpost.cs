using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.UC;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.UC.phantich;
using CrawlFB_PW._1._0.DAO.phantich;
using DevExpress.XtraEditors;
namespace CrawlFB_PW._1._0.phantich
{
    public partial class fphantichpost : DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm
    {
        UCViewAnalyzeNegative _UcViewAnalyzeNegative;
        public fphantichpost()
        {
            InitializeComponent();
            fluentDesignFormControl1.FluentDesignForm = null;
            AcordingStyleHelper.SetSizeControl(accordionControl1, 200);
            AcordingStyleHelper.StyleAllAccordionElements(accordionControl1);
            this.NavigationControl = accordionControl1; // bắt buộc để set control là hiệu ứng trung tâm
            accordionControl1.ElementClick += AccordionElement_Click;
            InitUC();// GỌI CẤU HÌNH CHÍNH
        }
        private void InitUC()
        {
            _UcViewAnalyzeNegative = new UCViewAnalyzeNegative();   // UserControl tổng
            _UcViewAnalyzeNegative.Dock = DockStyle.Fill;
            fluentDesignFormContainer1.Controls.Add(_UcViewAnalyzeNegative);         
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
                    ShowUC(_UcViewAnalyzeNegative);
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

        private void btn_AnalyzePost_ItemClick(object sender, ItemClickEventArgs e)
        {
           
        }
    }
}
