using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.ViewModels.Keyword;
using CrawlFB_PW._1._0.ViewModels.phan_tich;

namespace CrawlFB_PW._1._0.DAO.phantich
{
    public partial class FcheckkeywordPost : Form
    {
        private List<PostHighlightDTO> _posts;

        public FcheckkeywordPost(List<PostHighlightDTO> posts)
        {
            InitializeComponent();
            _posts = posts ?? new List<PostHighlightDTO>();
            this.Load += FcheckkeywordPost_Load;
        }

        private void FcheckkeywordPost_Load(object sender, EventArgs e)
        {
            rtbContent.ReadOnly = false;
            rtbContent.Clear();

            foreach (var post in _posts)
            {
                int baseOffset = rtbContent.TextLength;

                // Tiêu đề
                rtbContent.SelectionFont = new Font(rtbContent.Font, FontStyle.Bold);
                rtbContent.SelectionColor = Color.Blue;
                rtbContent.AppendText($"POST ID: {post.PostId}\n\n");

                baseOffset = rtbContent.TextLength;

                rtbContent.SelectionFont = new Font(rtbContent.Font, FontStyle.Regular);
                rtbContent.SelectionColor = Color.Black;
                rtbContent.AppendText(post.Content + "\n");

                // 🔴 Negative
                foreach (var m in post.Negative)
                {
                    rtbContent.Select(baseOffset + m.Start, m.Length);
                    rtbContent.SelectionColor = Color.Red;
                    rtbContent.SelectionFont =
                        new Font(rtbContent.Font, FontStyle.Bold);
                }

                // 🟢 Attention
                foreach (var m in post.Attention)
                {
                    rtbContent.Select(baseOffset + m.Start, m.Length);
                    rtbContent.SelectionColor = Color.Green;
                    rtbContent.SelectionFont =
                        new Font(rtbContent.Font, FontStyle.Bold);
                }

                // Separator
                rtbContent.AppendText("\n────────────────────────────────────────\n\n");
            }

            rtbContent.SelectionStart = 0;
            rtbContent.SelectionLength = 0;
            rtbContent.ReadOnly = true;
        }


    }
}
