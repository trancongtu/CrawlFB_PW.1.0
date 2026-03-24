using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.Helper.Text;

namespace CrawlFB_PW._1._0
{
    public partial class FTestRule : Form
    {
        public FTestRule()
        {
            InitializeComponent();
        }

        private void btn_Test_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string a = txb_Text1.Text;
            string b = txb_Text2.Text;

            var sb = new StringBuilder();

            sb.AppendLine("=== TEST SO SÁNH VIP ===");
            sb.AppendLine($"Chuỗi 1: {a}");
            sb.AppendLine($"Chuỗi 2: {b}");
            sb.AppendLine();

            sb.AppendLine($"[1] STRICT (á ≠ a): {SosanhChuoi.ContainsExactVietnamesePhraseStrict(a, b)}");
            richTextBox1.Text = sb.ToString();
        }
    }
}
