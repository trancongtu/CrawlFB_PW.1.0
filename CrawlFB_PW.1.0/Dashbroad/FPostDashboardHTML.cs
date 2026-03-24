using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.Helper.dashbroad;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.Dashbroad
{
    public partial class FPostDashboardHTML : Form
    {
        private PostInfoViewModel _post;

        public FPostDashboardHTML(PostInfoViewModel post)
        {
            InitializeComponent();
            _post = post;
            this.Load += FPostDashboard_Load;
        }

        private async void FPostDashboard_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();
            webView21.NavigateToString(PostHtmlBuilder.Build(_post));
        }
    }
}
