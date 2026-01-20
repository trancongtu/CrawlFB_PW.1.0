using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid;
using DevExpress.XtraBars.Navigation;
namespace CrawlFB_PW._1._0.Helper
{
    public class AcordingStyleHelper
    {
        private static AccordionControlElement _selectedElement = null;
        public static void SetSizeControl(AccordionControl acc, int width)
        {
            acc.Width = width;
            acc.MinimumSize = new Size(width, 0);
            acc.MaximumSize = new Size(width, 9999);
        }
        public static void StyleAllAccordionElements(AccordionControl acc)
        {
            var normalFont = new Font("Segoe UI", 8, FontStyle.Regular);
            var hoverFont = new Font("Segoe UI", 9, FontStyle.Bold);

            // Style root
            acc.Appearance.Item.Normal.Font = normalFont;
            acc.Appearance.Item.Normal.ForeColor = Color.White;
            acc.Appearance.Item.Normal.Options.UseFont = true;
            acc.Appearance.Item.Normal.Options.UseForeColor = true;

            acc.Appearance.Item.Hovered.BackColor = Color.FromArgb(230, 240, 255);
            acc.Appearance.Item.Hovered.Font = hoverFont;
            acc.Appearance.Item.Hovered.ForeColor = Color.DodgerBlue;

            acc.Appearance.Item.Pressed.BackColor = Color.FromArgb(230, 240, 255);
            acc.Appearance.Item.Pressed.Font = hoverFont;
            acc.Appearance.Item.Pressed.ForeColor = Color.DeepSkyBlue;

            foreach (var ele in acc.Elements)
                ApplyElementStyle(ele, normalFont, hoverFont);
        }
        public static void ApplyElementStyle(AccordionControlElement ele, Font normal, Font hover)
        {
            ele.Appearance.Normal.Font = normal;
            ele.Appearance.Normal.ForeColor = Color.White;

            ele.Appearance.Hovered.Font = hover;
            ele.Appearance.Hovered.ForeColor = Color.DodgerBlue;
            ele.Appearance.Hovered.BackColor = Color.FromArgb(230, 240, 255);

            ele.Appearance.Pressed.Font = hover;
            ele.Appearance.Pressed.ForeColor = Color.DeepSkyBlue;

            foreach (var child in ele.Elements)
                ApplyElementStyle(child, normal, hover);
        }
        //=========Style Element=============
        public static void ApplySelectedStyle(AccordionControlElement ele)
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
    }
}
