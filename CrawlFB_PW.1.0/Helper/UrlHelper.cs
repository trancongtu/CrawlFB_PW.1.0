using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Helper
{
    public static class UrlHelper
    {
        public const string FB_CONST = "https://facebook.com";

        public static string NormalizeFacebookUrl(string originalLink)
        {
            if (string.IsNullOrWhiteSpace(originalLink))
                return "";

            originalLink = originalLink.Trim();

            // =========================
            // Chuẩn domain về facebook.com
            // =========================
            if (originalLink.StartsWith("https://www.facebook.com", StringComparison.OrdinalIgnoreCase))
            {
                originalLink = FB_CONST + originalLink.Substring("https://www.facebook.com".Length);
            }
            else if (originalLink.StartsWith("https://web.facebook.com", StringComparison.OrdinalIgnoreCase))
            {
                originalLink = FB_CONST + originalLink.Substring("https://web.facebook.com".Length);
            }
            else if (originalLink.StartsWith("https://fb.com", StringComparison.OrdinalIgnoreCase))
            {
                originalLink = FB_CONST + originalLink.Substring("https://fb.com".Length);
            }

            // =========================
            // Nếu đã đúng const → xong
            // =========================
            if (originalLink.StartsWith(FB_CONST, StringComparison.OrdinalIgnoreCase))
                return originalLink;

            // =========================
            // Gắn const nếu chưa có
            // =========================
            if (originalLink.StartsWith("/"))
                return FB_CONST + originalLink;

            return FB_CONST + "/" + originalLink;
        }
        //POst Link
        public static string ShortenFacebookPostLink(string originalLink)
        {
            if (string.IsNullOrWhiteSpace(originalLink))
                return "N/A";

            try
            {
                originalLink = originalLink.Trim();

                // =========================
                // CASE: permalink.php
                // giữ story_fbid + id
                // =========================
                if (originalLink.Contains("permalink.php") &&
                    originalLink.Contains("story_fbid"))
                {
                    int idx = originalLink.IndexOf("&__cft__", StringComparison.OrdinalIgnoreCase);
                    if (idx != -1)
                    {
                        originalLink = originalLink.Substring(0, idx);
                    }

                    originalLink = originalLink.TrimEnd('&');

                    // 👉 chuẩn hóa link cuối cùng
                    return UrlHelper.NormalizeFacebookUrl(originalLink);
                }

                // =========================
                // CASE NORMAL POST
                // =========================
                int qIndex = originalLink.IndexOf("?");
                if (qIndex != -1)
                    originalLink = originalLink.Substring(0, qIndex);

                // 👉 chuẩn hóa link cuối cùng
                return UrlHelper.NormalizeFacebookUrl(originalLink);
            }
            catch
            {
                // fallback vẫn chuẩn hóa
                return UrlHelper.NormalizeFacebookUrl(originalLink);
            }
        }

        //Poster
        //1.Poster trong bài Share
        public static string ShortenPagePersonLink(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return "";

            try
            {
                // 🔹 Chuẩn hóa link NGAY TỪ ĐẦU
                rawUrl = UrlHelper.NormalizeFacebookUrl(rawUrl);

                var uri = new Uri(rawUrl);

                string host = "facebook.com";
                string path = uri.AbsolutePath;
                string query = uri.Query;

                // =========================
                // CASE 0: PROFILE.PHP (ID)
                // =========================
                if (path.Equals("/profile.php", StringComparison.OrdinalIgnoreCase)
                    && query.Contains("id="))
                {
                    int idIndex = query.IndexOf("id=", StringComparison.OrdinalIgnoreCase);
                    string idPart = query.Substring(idIndex);

                    int end = idPart.IndexOf("&");
                    if (end != -1)
                        idPart = idPart.Substring(0, end);

                    return UrlHelper.NormalizeFacebookUrl(
                        $"https://{host}/profile.php?{idPart}");
                }

                // =========================
                // CASE 1: GROUP
                // =========================
                if (path.StartsWith("/groups/", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = path.Split('/');
                    if (parts.Length >= 3)
                    {
                        string groupId = parts[2];
                        return UrlHelper.NormalizeFacebookUrl(
                            $"https://{host}/groups/{groupId}");
                    }
                }

                // =========================
                // CASE 2: FANPAGE / USERNAME
                // =========================
                var segments = path.Split(
                    new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length >= 1)
                {
                    string first = segments[0].ToLower();

                    string[] invalid =
                    {
                "posts", "watch", "events", "stories",
                "photo", "photos", "videos", "reel",
                "marketplace"
            };

                    if (!invalid.Contains(first))
                    {
                        return UrlHelper.NormalizeFacebookUrl(
                            $"https://{host}/{segments[0]}");
                    }
                }

                // fallback
                return UrlHelper.NormalizeFacebookUrl(
                    $"https://{host}{path}");
            }
            catch
            {
                // luôn đảm bảo output là link chuẩn
                return UrlHelper.NormalizeFacebookUrl(rawUrl);
            }
        }
        public static string ShortenPostVideoLink(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return "";

            rawUrl = rawUrl.Trim();

            // =========================
            // 1️⃣ CẮT QUERY (?xxx)
            // =========================
            int qIndex = rawUrl.IndexOf("?");
            if (qIndex > 0)
                rawUrl = rawUrl.Substring(0, qIndex);

            // =========================
            // 2️⃣ CẮT SAU VIDEO ID
            // =========================
            // Giữ nguyên https://facebook.com/{user}/videos/{id}/
            var match = System.Text.RegularExpressions.Regex.Match(
                rawUrl,
                @"^(https?:\/\/[^\/]+\/[^\/]+\/videos\/\d+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!match.Success)
                return rawUrl; // fallback an toàn

            return match.Groups[1].Value + "/";
        }
        public static string ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // profile.php?id=123
            var m1 = Regex.Match(url, @"id=(\d+)");
            if (m1.Success) return m1.Groups[1].Value;

            // /groups/123
            var m2 = Regex.Match(url, @"/groups/(\d+)");
            if (m2.Success) return m2.Groups[1].Value;

            // /pages/123
            var m3 = Regex.Match(url, @"/pages/(\d+)");
            if (m3.Success) return m3.Groups[1].Value;

            // /people/.../123
            var m4 = Regex.Match(url, @"/people/.+?/(\d+)");
            if (m4.Success) return m4.Groups[1].Value;

            // 🔥 QUAN TRỌNG: facebook.com/123456
            var m5 = Regex.Match(url, @"facebook\.com/(\d+)");
            if (m5.Success) return m5.Groups[1].Value;

            // 🔥 reel / posts
            var m6 = Regex.Match(url, @"/(reel|posts)/(\d+)");
            if (m6.Success) return m6.Groups[2].Value;

            return null;
        }
    }

}
