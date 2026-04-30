using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.Helper.Data
{
    public static class PostTypeHelper
    {
        public static bool IsVideo(PostType type)
        {
            return type.ToString().ToLower().Contains("video");
        }

        public static bool IsReel(PostType type)
        {
            return type.ToString().ToLower().Contains("reel");
        }

        public static bool IsPhoto(PostType type)
        {
            return type.ToString().ToLower().Contains("photo");
        }

        public static bool HasContent(PostType type)
        {
            return !type.ToString().ToLower().Contains("nocap") &&
                   !type.ToString().ToLower().Contains("nocontent");
        }

        public static bool IsShare(PostType type)
        {
            return type.ToString().ToLower().StartsWith("share");
        }

        public static bool IsPage(PostType type)
        {
            return type.ToString().ToLower().StartsWith("page");
        }

        public static bool IsPerson(PostType type)
        {
            return type.ToString().ToLower().StartsWith("person");
        }
    }
}
