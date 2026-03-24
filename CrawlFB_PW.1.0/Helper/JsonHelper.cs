using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.Helper
{
    public static class JsonHelper
    {
        // ===============================
        // SAVE
        // ===============================
        public static void Save<T>(IEnumerable<T> data, string filePath)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            EnsureFolder(filePath);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        // ===============================
        // LOAD
        // ===============================
        public static List<T> Load<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<T>();

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<T>>(json)
                   ?? new List<T>();
        }

        // ===============================
        // APPEND (ghi thêm – ít dùng)
        // ===============================
        public static void Append<T>(IEnumerable<T> data, string filePath)
        {
            var list = Load<T>(filePath);
            list.AddRange(data);
            Save(list, filePath);
        }

        // ===============================
        // UTIL
        // ===============================
        private static void EnsureFolder(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        //=============DÙNG CHO COMMENT
        public class CommentJsonFile
        {
            public string PostLink { get; set; }
            public string PosterName { get; set; }

            // C# 7.3 phải viết đầy đủ
            public List<CommentJsonItem> Comments { get; set; } = new List<CommentJsonItem>();
        }

        public class CommentJsonItem
        {
            public string STT { get; set; }
            public string ActorName { get; set; }
            public string Link { get; set; }
            public string IDFB { get; set; }
            public FBType FBType { get; set; }
            public string Time { get; set; }
            public string Content { get; set; }
            public int Level { get; set; }
        }
        private static readonly JsonSerializerOptions _options =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters =
                {
            new JsonStringEnumConverter()
                }
            };


        // ===============================
        // SAVE
        // ===============================
        public static void SaveCommentsJson(
            string filePath,
            string postLink,
            string posterName,
            List<CommentGridRow> rows)
        {
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));

            var data = new CommentJsonFile
            {
                PostLink = postLink ?? "",
                PosterName = posterName ?? "",
                Comments = rows
                    .Where(r => r != null)
                    .Select(r => new CommentJsonItem
                    {
                        STT = r.STT ?? "",
                        ActorName = r.ActorName ?? "",
                        Link = r.Link ?? "",              // ✅ LƯU LINK COMMENT
                        IDFB = r.IDFBPerson ?? "",
                        FBType = r.PosterFBType,
                        Time = r.Time ?? "",
                        Content = r.Content ?? "",
                        Level = r.Level
                    })
                    .ToList()
            };

            string json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(filePath, json);
        }

        // ===============================
        // LOAD
        // ===============================
        public static CommentJsonFile LoadCommentsJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Không tìm thấy file JSON", filePath);

            string json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<CommentJsonFile>(json, _options);

            if (data == null)
                throw new Exception("File JSON không hợp lệ");

            return data;
        }
        public static List<CommentGridRow> ToGridRows(CommentJsonFile json)
        {
            return json.Comments.Select(c => new CommentGridRow
            {
                STT = c.STT,
                ActorName = c.ActorName,
                Link = c.Link,
                IDFBPerson = c.IDFB,
                PosterFBType = c.FBType,
                Time = c.Time,
                Content = c.Content,
                Level = c.Level
            }).ToList();
        }

        public static List<SharePostVM> LoadSharePostJson(string filePath)
        {
            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<SharePostVM>>(json, options);
        }
        //analyze
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
                return null;

            return System.Text.Json.JsonSerializer.Serialize(obj);
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}
