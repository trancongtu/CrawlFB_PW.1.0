using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Helper.Text;
namespace CrawlFB_PW._1._0.Topic
{
    
    public partial class FCheckKeywordOnPost : Form
    {
        private List<PostInfoViewModel> _posts;
        private List<List<string>> _matchedKeywordsByPost;
        public FCheckKeywordOnPost()
        {
            InitializeComponent();
        }
        public void InitData(List<PostInfoViewModel> posts, List<string> keywords)
        {
            _posts = new List<PostInfoViewModel>();
            _matchedKeywordsByPost = new List<List<string>>();

            var gridData = new List<object>();
            int stt = 1;

            foreach (var post in posts)
            {
                var matched = keywords
                    .Where(k => SosanhChuoi.SosanhkeywordAddTopic(post.Content, k))
                    .ToList();

                if (matched.Count == 0)
                    continue;

                _posts.Add(post);
                _matchedKeywordsByPost.Add(matched);

                gridData.Add(new
                {
                    STT = stt++,
                    Preview = post.Content.Length > 150
                        ? post.Content.Substring(0, 150) + "..."
                        : post.Content,
                    Keywords = string.Join(", ", matched)
                });
            }

            gridControl1.DataSource = gridData;
            gridView1.BestFitColumns();

            gridView1.FocusedRowChanged -= GridView1_FocusedRowChanged;
            gridView1.FocusedRowChanged += GridView1_FocusedRowChanged;

            if (_posts.Count > 0)
            {
                gridView1.FocusedRowHandle = 0;
                HighlightContent(_posts[0].Content, _matchedKeywordsByPost[0]);
            }
        }


        private void GridView1_FocusedRowChanged(
    object sender,
    DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0) return;

            var post = _posts[e.FocusedRowHandle];
            var keywords = _matchedKeywordsByPost[e.FocusedRowHandle];

            HighlightContent(post.Content, keywords);
        }

        private void HighlightContent(string content, List<string> keywords)
        {
            richTextBox1.Clear();
            richTextBox1.Text = content;

            foreach (var kw in keywords)
            {
                int start = 0;
                while (start < content.Length)
                {
                    int idx = IndexOfIgnoreCaseAndAccent(content, kw, start);
                    if (idx < 0) break;

                    richTextBox1.Select(idx, kw.Length);
                    richTextBox1.SelectionBackColor = Color.Yellow;
                    richTextBox1.SelectionColor = Color.Black;

                    start = idx + kw.Length;
                }
            }

            richTextBox1.Select(0, 0);
        }
        private int IndexOfIgnoreCaseAndAccent(string source, string keyword, int startIndex)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(keyword))
                return -1;

            string srcNorm = TextNormalizeHelper.Normalize(source);
            string keyNorm = TextNormalizeHelper.Normalize(keyword);

            return srcNorm.IndexOf(keyNorm, startIndex, StringComparison.Ordinal);
        }


    }
}
