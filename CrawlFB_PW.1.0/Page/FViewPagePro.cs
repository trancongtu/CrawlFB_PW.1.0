using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars.Navigation;
using CrawlFB_PW._1._0.UC;
using CrawlFB_PW._1._0.Helper;

namespace CrawlFB_PW._1._0.Page
{
    public partial class FViewPagePro : DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm
    {
        private UPageView _ucPageView;
        private AccordionControlElement _selectedElement = null;

        public FViewPagePro()
        {
            InitializeComponent();
            InitUC();

            this.Load += FViewPagePro_Load;
        }

        private void FViewPagePro_Load(object sender, EventArgs e)
        {
            // Tắt auto layout của FluentDesign (bắt buộc)
            fluentDesignFormControl1.FluentDesignForm = null;

            AcordingStyleHelper.SetSizeControl(accordionControl1, 200);
            AcordingStyleHelper.StyleAllAccordionElements(accordionControl1);
            accordionControl1.ElementClick += AccordionElement_Click;

            this.NavigationControl = accordionControl1;
        }

        private void InitUC()
        {
            _ucPageView = new UPageView();   // UserControl tổng
            _ucPageView.Dock = DockStyle.Fill;
            fluentDesignFormContainer1.Controls.Add(_ucPageView);
        }
        // ======================== CLICK MENU ===========================
        private void AccordionElement_Click(object sender, ElementClickEventArgs e)
        {
            ApplySelectedStyle(e.Element);
            LoadPageUC(e.Element.Name);
        }

        private void ApplySelectedStyle(AccordionControlElement ele)
        {
            if (_selectedElement != null)
            {
                _selectedElement.Appearance.Normal.BackColor = Color.Transparent;
                _selectedElement.Appearance.Normal.ForeColor = Color.White;
                _selectedElement.Appearance.Normal.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            }

            _selectedElement = ele;

            ele.Appearance.Normal.BackColor = Color.FromArgb(230, 240, 255);
            ele.Appearance.Normal.ForeColor = Color.DodgerBlue;
            ele.Appearance.Normal.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        }

        // ====================== LOAD USER CONTROL ======================
        private void LoadPageUC(string elementName)
        {
            // Các element có name giống trong Designer
            // (ACEPageMain, ACEPageNote, ACEPageMonitor)
            // file Designer: :contentReference[oaicite:2]{index=2}

            switch (elementName)
            {
                case "ACEPageMain":
                    _ucPageView.LoadSource("PageInfo");
                    break;

                case "ACEPageNote":
                    _ucPageView.LoadSource("PageNote");
                    break;

                case "ACEPageMonitor":
                    _ucPageView.LoadSource("PageMonitor");
                    break;

                // ================== PHẦN BÀI VIẾT PAGE ==================
                case "ACEPostPageNote":
                    _ucPageView.LoadSource("PostPageNote");
                    break;

                case "ACEPostPageMonitor":
                    _ucPageView.LoadSource("PostPageMonitor");
                    break;

                case "ACEPostAllPage":
                    _ucPageView.LoadSource("PostAllPage");
                    break;
            }
        }

        private void ACEPateMain_Click(object sender, EventArgs e)
        {

        }
    }
}
