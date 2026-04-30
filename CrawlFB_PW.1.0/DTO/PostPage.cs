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
        public string PostID { get; set; } 
        public string PageID { get; set; }
        // 🔹 Thông tin bài đăng
        public string PostTime { get; set; } 
        // 🔥 Thời gian gốc – DÙNG CHO LOGIC
        public DateTime? RealPostTime { get; set; } = null;
        public string PostLink { get; set; } 
        public string Content { get; set; } 

        // 🔹 Số liệu tương tác (nullable để tránh lỗi khi không có dữ liệu)
        public int? CommentCount { get; set; } = null;
        public int? ShareCount { get; set; } = null;
        public int? LikeCount { get; set; } = null; // dự phòng nếu cần phân tích thêm

        // 🔹 Người đăng
        public string PosterName { get; set; } 
        public string PosterLink { get; set; } 
        public FBType PosterNote { get; set; }
        public string PosterIdFB { get; set; } 

        // 🔹 Trang hoặc Group chứa bài đăng
        public string PageName { get; set; } 
        public string PageLink { get; set; }
        public FBType ContainerType { get; set; } = FBType.Unknown;
        public string ContainerIdFB { get; set; } 
        public string Attachment { get; set; }   // 🆕 link hoặc thumbnail

        // 🔹 Trạng thái bài viết
        public PostType PostType { get; set; }

        // 🔹 Chủ đề phân tích (AI phân loại sau này)
        public string Topic { get; set; } 

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
            FBType PosterNote,
            string pageName,
            string pageLink,          
            PostType postType,
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
