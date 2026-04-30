using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Helper.Mapper;
using CrawlFB_PW._1._0.Helper.Data;

namespace CrawlFB_PW._1._0.Helper.Mapper
{
    public static class PostMapper
    {
        public static PostInfoViewModel ToViewModel(this PostPage dto)
        {
            if (dto == null) return null;

            return new PostInfoViewModel
            {
                // ===== BASIC =====
                PostID = dto.PostID,
                PostLink = dto.PostLink,
                Content = dto.Content,

                // ===== TIME =====
                RealPostTime = dto.RealPostTime,
                PostTimeRaw = dto.PostTime,

                // ===== POSTER =====
                PosterName = dto.PosterName,
                PosterLink = dto.PosterLink,
                PosterIdFB = dto.PosterIdFB,
                PosterNote = dto.PosterNote,

                // ===== PAGE =====
                PageID = dto.PageID,
                PageName = dto.PageName,
                PageLink = dto.PageLink,
                ContainerIdFB = dto.ContainerIdFB,
                ContainerType = dto.ContainerType,

                // ===== INTERACTION =====
                Like = dto.LikeCount ?? 0,
                Comment = dto.CommentCount ?? 0,
                Share = dto.ShareCount ?? 0,

                // ===== EXTRA =====
                Attachment = dto.Attachment,
                Topic = dto.Topic,
                PostType = dto.PostType,

                // ===== FLAG (DÙNG HELPER) =====
                HasVideo = PostTypeHelper.IsVideo(dto.PostType),
                HasPhoto = PostTypeHelper.IsPhoto(dto.PostType),
                HasReel = PostTypeHelper.IsReel(dto.PostType),

                // ===== OPTIONAL (nếu cần sau này) =====
                // ví dụ dùng cho filter / analytics
                // IsShare = PostTypeHelper.IsShare(dto.PostType),
                // IsPagePost = PostTypeHelper.IsPage(dto.PostType),
                // IsPersonPost = PostTypeHelper.IsPerson(dto.PostType)
            };
        }

        public static List<PostInfoViewModel> ToViewModelList(this List<PostPage> list)
        {
            return list?.Select(x => x.ToViewModel()).ToList() ?? new List<PostInfoViewModel>();
        }
    }
}
