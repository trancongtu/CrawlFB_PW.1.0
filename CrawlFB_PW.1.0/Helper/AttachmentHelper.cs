using System;
using System.Collections.Generic;
using CrawlFB_PW._1._0.ViewModels;
using Newtonsoft.Json;

namespace CrawlFB_PW._1._0.Helper
{
    public static class AttachmentHelper
    {
        // =====================================================
        // BUILD JSON ATTACHMENT (VIDEO + PHOTO)
        // =====================================================
        public static string BuildAttachmentJson(
            bool hasVideo,
            string videoLink,
            string videoTime,
            List<(string Src, string Alt)> photos)
        {
            AttachmentRaw attachment = new AttachmentRaw();

            // ===== VIDEO =====
            if (hasVideo && !string.IsNullOrWhiteSpace(videoLink))
            {
                attachment.Videos.Add(new VideoAttachment
                {
                    Url = videoLink,
                    Time = videoTime
                });
            }

            // ===== PHOTO =====
            if (photos != null && photos.Count > 0)
            {
                foreach (var ph in photos)
                {
                    attachment.Photos.Add(new PhotoAttachment
                    {
                        Src = ph.Src,
                        Alt = ph.Alt
                    });
                }
            }

            // ===== NOTHING =====
            if (attachment.Videos.Count == 0 && attachment.Photos.Count == 0)
                return "N/A";

            return JsonConvert.SerializeObject(attachment);
        }

        // =====================================================
        // GET ATTACHMENT LINK FOR VIEW (VIDEO > PHOTO)
        // =====================================================
        public static string GetAttachmentForView(string attachmentJson)
        {
            if (string.IsNullOrWhiteSpace(attachmentJson) ||
                attachmentJson == "N/A")
                return "N/A";

            try
            {
                AttachmentRaw att =
                    JsonConvert.DeserializeObject<AttachmentRaw>(attachmentJson);

                if (att == null)
                    return "N/A";

                // Ưu tiên VIDEO
                if (att.Videos != null && att.Videos.Count > 0 &&
                    !string.IsNullOrWhiteSpace(att.Videos[0].Url))
                {
                    return att.Videos[0].Url;
                }

                // Fallback PHOTO
                if (att.Photos != null && att.Photos.Count > 0 &&
                    !string.IsNullOrWhiteSpace(att.Photos[0].Src))
                {
                    return att.Photos[0].Src;
                }
            }
            catch
            {
                // ignore → return N/A
            }

            return "N/A";
        }
    }
}
