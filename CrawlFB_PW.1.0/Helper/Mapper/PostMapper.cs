using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Helper.Mapper;

namespace CrawlFB_PW._1._0.Helper.Mapper
{
    public static class PostMapper
    {
        public static PostInfoViewModel ToViewModel(this PostPage dto)
        {
            if (dto == null) return null;

            return new PostInfoViewModel
            {
                PostID = dto.PostID,
                PostLink = dto.PostLink,
                Content = dto.Content,
                PosterName = dto.PosterName,
                PosterLink = dto.PosterLink,
                PosterNote = dto.PosterNote,
                PageName = dto.PageName,
                PageLink = dto.PageLink,
                Like = dto.LikeCount ?? 0,
                Comment = dto.CommentCount ?? 0,
                Share = dto.ShareCount ?? 0,
                Attachment = dto.Attachment,
                AttachmentView = dto.Attachment,
                RealPostTime = dto.RealPostTime,
                PostType = Enum.TryParse(dto.PostType, out PostType pt)
                    ? pt
                    : PostType.Page_Unknow
            };
        }
    }
}
