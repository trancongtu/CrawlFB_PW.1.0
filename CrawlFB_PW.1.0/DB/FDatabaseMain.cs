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
namespace CrawlFB_PW._1._0.DB
{
    public partial class FDatabaseMain : DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm
    {
        private UCPageDB _ucPageDB;
        private UCPostMainDBcs _ucPostMainDB;
        public FDatabaseMain()
        {
            InitializeComponent();
            // Tắt auto layout của FluentDesign (bắt buộc)
            fluentDesignFormControl1.FluentDesignForm = null;
            // Set width menu
            AcordingStyleHelper.SetSizeControl(accordionControl1, 200);        
            AcordingStyleHelper.StyleAllAccordionElements(accordionControl1);
            accordionControl1.ElementClick += AccordionElement_Click;
            this.NavigationControl = accordionControl1; // bắt buộc để set control là hiệu ứng trung tâm
            InitUC();// GỌI CẤU HÌNH CHÍNH
        }
        private void InitUC()
        {
            _ucPageDB = new UCPageDB();   // UserControl tổng
            _ucPageDB.Dock = DockStyle.Fill;
            fluentDesignFormContainer1.Controls.Add(_ucPageDB);
            _ucPostMainDB = new UCPostMainDBcs();
            _ucPostMainDB.Dock = DockStyle.Fill;
            fluentDesignFormContainer1.Controls.Add(_ucPostMainDB);
            _ucPostMainDB.Visible = false;
        }
        // ======================== CLICK MENU ===========================
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
                case "ACEPageAll":// điền đúng name trong desin
                    ShowUC(_ucPageDB);
                    _ucPageDB.LoadSource(PageSourceType.PageInfo);                 
                    break;
                case "ACEPageNote":
                    _ucPageDB.LoadSource(PageSourceType.PageNote); 
                    break;
                case "ACEPageMonitor":
                    break;
                case "ACEPostAll":
                    ShowUC(_ucPostMainDB);
                    _ucPostMainDB.LoadSource();
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
