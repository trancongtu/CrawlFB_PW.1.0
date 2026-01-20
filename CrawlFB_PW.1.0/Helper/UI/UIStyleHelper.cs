using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CrawlFB_PW._1._0.Helpers
{
    public static class UIStyleHelper
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);

        public static void ApplyOutline(Button btn, string iconName = null)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btn.BackColor = Color.White;
            btn.ForeColor = Color.FromArgb(60, 60, 60);
            btn.Height = Math.Max(btn.Height, 34);
            btn.Padding = new Padding(10, 2, 10, 2);

            btn.Resize -= Btn_Resize;
            btn.Resize += Btn_Resize;
            Btn_Resize(btn, EventArgs.Empty);

            if (!string.IsNullOrEmpty(iconName))
                TrySetIcon(btn, iconName);
        }

        private static void Btn_Resize(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                try
                {
                    btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 8, 8));
                }
                catch { }
            }
        }

        private static void TrySetIcon(Button btn, string iconName)
        {
            string folder = Path.Combine(Application.StartupPath, "Icons");
            string png = Path.Combine(folder, iconName);
            string svg = Path.Combine(Application.StartupPath, "IconsSVG", Path.GetFileNameWithoutExtension(iconName) + ".svg");
            if (File.Exists(png))
            {
                try { using (var img = Image.FromFile(png)) { btn.Image = new Bitmap(img, new Size(16, 16)); btn.ImageAlign = ContentAlignment.MiddleLeft; btn.TextImageRelation = TextImageRelation.ImageBeforeText; } }
                catch { }
            }
            else if (File.Exists(svg))
            {
                // fallback: DevExpress can load SVG via SvgImageCollection; leave for developer
            }
        }
        //==============BarManager
        public static void StyleBarManager(DevExpress.XtraBars.BarManager barManager)
        {
            // Style tất cả bar items
            foreach (var item in barManager.Items)
            {
                if (item is DevExpress.XtraBars.BarButtonItem btn)
                    StyleBarButton(btn);
            }

            // Style cho từng Bar
            foreach (DevExpress.XtraBars.Bar bar in barManager.Bars)
            {
                bar.OptionsBar.MultiLine = true;
                bar.OptionsBar.UseWholeRow = true;
                bar.OptionsBar.DrawDragBorder = false;
                bar.OptionsBar.AllowQuickCustomization = false;
                bar.OptionsBar.AllowCollapse = false;

                bar.Appearance.Font = new Font("Segoe UI", 10);
                bar.Appearance.ForeColor = Color.Black;
            }
        }

        public static void StyleBarButton(DevExpress.XtraBars.BarButtonItem btn)
        {
            btn.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;

            btn.ItemAppearance.Normal.Font = new Font("Segoe UI", 10);
            btn.ItemAppearance.Normal.ForeColor = Color.Black;

            btn.ItemAppearance.Hovered.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.ItemAppearance.Hovered.ForeColor = Color.FromArgb(33, 150, 243);

            btn.ItemAppearance.Pressed.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.ItemAppearance.Pressed.ForeColor = Color.FromArgb(25, 118, 210);

            btn.ItemAppearance.Disabled.ForeColor = Color.Gray;
        }
    }
}
