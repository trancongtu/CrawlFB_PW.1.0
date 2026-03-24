using System.Text;
using System.Net;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.Helper.dashbroad
{
    public static class PostHtmlBuilder
    {
        public static string Build(PostInfoViewModel post)
        {
            string avatar = "https://via.placeholder.com/45";

            var sb = new StringBuilder();

            sb.Append($@"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<style>
body {{
    background:#f0f2f5;
    font-family:Segoe UI, Arial;
}}

.post {{
    width:650px;
    margin:30px auto;
    background:white;
    border-radius:12px;
    padding:15px;
    box-shadow:0 2px 8px rgba(0,0,0,0.1);
}}

.header {{
    display:flex;
    align-items:center;
    gap:10px;
}}

.avatar {{
    width:45px;
    height:45px;
    border-radius:50%;
}}

.name {{
    font-weight:600;
    font-size:15px;
}}

.meta {{
    color:#65676b;
    font-size:13px;
}}

.content {{
    margin-top:12px;
    font-size:15px;
    line-height:1.5;
    white-space:pre-wrap;
}}

.media {{
    margin-top:12px;
}}

.reaction {{
    margin-top:12px;
    padding-top:8px;
    border-top:1px solid #eee;
    font-size:14px;
    color:#65676b;
}}

.count {{
    margin-right:15px;
}}
</style>
</head>
<body>

<div class='post'>

<div class='header'>
<img class='avatar' src='{avatar}' />
<div>
<div class='name'>{WebUtility.HtmlEncode(post.PosterName)}</div>
<div class='meta'>
{WebUtility.HtmlEncode(post.PageName)} · {post.TimeView}
</div>
</div>
</div>

<div class='content'>
{WebUtility.HtmlEncode(post.Content)}
</div>
");

            // Hiển thị ảnh nếu có
            if (!string.IsNullOrWhiteSpace(post.AttachmentView) &&
                post.AttachmentView != "N/A")
            {
                sb.Append($@"
<div class='media'>
<img src='{post.AttachmentView}' style='width:100%;border-radius:8px;' />
</div>
");
            }

            sb.Append($@"
<div class='reaction'>
<span class='count'>👍 {post.Like}</span>
<span class='count'>💬 {post.Comment}</span>
<span class='count'>🔁 {post.Share}</span>
</div>

</div>
</body>
</html>
");

            return sb.ToString();
        }
    }
}