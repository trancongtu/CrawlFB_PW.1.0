using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.ViewModels;

namespace CrawlFB_PW._1._0.Helper.UI
{
    public static class PostMappingHelper
    {
        public static PostPage ToPostPage(this PostInfoViewModel vm)
        {
            return new PostPage
            {
                PostLink = vm.PostLink,
                Content = vm.Content,
                PostTime = vm.PostTimeRaw,
                RealPostTime = vm.RealPostTime,

                LikeCount = vm.Like,
                CommentCount = vm.Comment,
                ShareCount = vm.Share,

                Attachment = vm.Attachment,
                PostType = vm.PostType.ToString(),

                PageName = vm.PageName,
                PageLink = vm.PageLink,

                PosterName = vm.PosterName,
                PosterLink = vm.PosterLink,
                PosterNote = vm.PosterNote,
                PosterIdFB = vm.PosterIdFB,
                ContainerIdFB = vm.ContainerIdFB
            };
        }
    }

}
