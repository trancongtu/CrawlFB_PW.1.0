using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.DTO
{
    public class PostPage
    {
        // Primary logical id (hash từ PostLink)
        public string PostID { get; set; } = "N/A";
        public string PageID { get; set; }
        // 🔹 Thông tin bài đăng
        public string PostTime { get; set; } = "N/A";
        // 🔥 Thời gian gốc – DÙNG CHO LOGIC
        public DateTime? RealPostTime { get; set; } = null;
        public string PostLink { get; set; } = "N/A";
        public string Content { get; set; } = "N/A";

        // 🔹 Số liệu tương tác (nullable để tránh lỗi khi không có dữ liệu)
        public int? CommentCount { get; set; } = null;
        public int? ShareCount { get; set; } = null;
        public int? LikeCount { get; set; } = null; // dự phòng nếu cần phân tích thêm

        // 🔹 Người đăng
        public string PosterName { get; set; } = "N/A";
        public string PosterLink { get; set; } = "N/A";
        public string PosterNote { get; set; } = "N/A";
        public string PosterIdFB { get; set; } = "N/A";

        // 🔹 Trang hoặc Group chứa bài đăng
        public string PageName { get; set; } = "N/A";
        public string PageLink { get; set; } = "N/A";
        public string ContainerIdFB { get; set; } = "N/A";
        public string Attachment { get; set; } = "N/A";   // 🆕 link hoặc thumbnail

        // 🔹 Trạng thái bài viết
        public string PostType { get; set; }

        // 🔹 Chủ đề phân tích (AI phân loại sau này)
        public string Topic { get; set; } = "N/A";

        // 🔹 Constructor rỗng
        public PostPage() { }

        // 🔹 Constructor đầy đủ (đã đồng bộ tên & kiểu)
        public PostPage(
            DateTime? realPostTime,
            string postTime,
            string postLink,
            string content,
            int? likeCount,
            int? shareCount,
            int? commentCount,
            string posterName,
            string posterLink,
            string PosterNote,
            string pageName,
            string pageLink,          
            string postType,
            string attachment,
            string topic)
        {
            RealPostTime = realPostTime;
            this.PostTime = postTime;
            this.PostLink = postLink;
            this.Content = content;
            this.LikeCount = likeCount;
            this.ShareCount = shareCount;
            this.CommentCount = commentCount;
            this.PosterName = posterName;
            this.PosterLink = posterLink;
            this.PageName = pageName;
            this.PageLink = pageLink;
            this.PosterNote = PosterNote;           
            this.PostType = postType;
            this.Attachment = attachment;
            this.Topic = topic;
        }
    }
}
