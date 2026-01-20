using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using CrawlFB_PW._1._0.Post;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.Helper
{
    internal class ExcellHeper
    {
        /// <summary>
        /// Xuất danh sách bình luận Reel ra Excel (đẹp – in được – link đầy đủ)
        /// </summary>
        public static void ExportCommentsToExcel(
            List<FScanCommentPost.CommentGridRow> rows,
            string postLink,
            string filePath)
        {
            if (rows == null || rows.Count == 0)
                throw new ArgumentException("Danh sách bình luận rỗng!");

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("BinhLuan");

                int colCount = 7;

                // ===============================
                // 1️⃣ TIÊU ĐỀ TRÊN CÙNG
                // ===============================
                ws.Range(1, 1, 1, colCount).Merge();
                ws.Cell(1, 1).Value = $"BÌNH LUẬN TẠI BÀI VIẾT: {postLink}";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 12;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(1).Height = 28;

                // ===============================
                // 2️⃣ HEADER
                // ===============================
                int headerRow = 3;

                ws.Cell(headerRow, 1).Value = "STT";
                ws.Cell(headerRow, 2).Value = "Người bình luận";
                ws.Cell(headerRow, 3).Value = "Link Facebook";
                ws.Cell(headerRow, 4).Value = "Thời gian";
                ws.Cell(headerRow, 5).Value = "Nội dung";
                ws.Cell(headerRow, 6).Value = "Ghi chú";
                ws.Cell(headerRow, 7).Value = "Comment ID";

                var headerRange = ws.Range(headerRow, 1, headerRow, colCount);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(235, 235, 235);
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ===============================
                // 3️⃣ DỮ LIỆU
                // ===============================
                int row = headerRow + 1;

                foreach (var c in rows)
                {
                    ws.Cell(row, 1).Value = c.STT;
                    ws.Cell(row, 2).Value = c.PosterName;
                    ws.Cell(row, 3).Value = c.PosterLink;
                    ws.Cell(row, 4).Value = c.Time;
                    ws.Cell(row, 5).Value = c.Content;
                    ws.Cell(row, 6).Value = c.Status;
                    ws.Cell(row, 7).Value = c.CommentId;

                    // 🔗 Link đầy đủ – phục vụ in & click
                    if (!string.IsNullOrWhiteSpace(c.PosterLink))
                    {
                        ws.Cell(row, 3).Value = c.PosterLink;
                        ws.Cell(row, 3).SetHyperlink(new XLHyperlink(c.PosterLink));
                        ws.Cell(row, 3).Style.Font.FontColor = XLColor.Blue;
                        ws.Cell(row, 3).Style.Font.Underline = XLFontUnderlineValues.Single;
                    }


                    row++;
                }

                // ===============================
                // 4️⃣ STYLE & IN ẤN
                // ===============================
                ws.Columns().AdjustToContents();
                ws.Column(3).Width = 45; // Link
                ws.Column(5).Width = 60; // Nội dung

                ws.SheetView.FreezeRows(headerRow);

                ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
                ws.PageSetup.FitToPages(1, 0);
                ws.PageSetup.CenterHorizontally = true;
                ws.PageSetup.Margins.Top = 0.5;
                ws.PageSetup.Margins.Bottom = 0.5;

                wb.SaveAs(filePath);
            }
        }
        // xuất comment full
        public static void ExportCommentsFullToExcel( List<CommentGridRow> rows, string postLink, string filePath)
        {
            if (rows == null || rows.Count == 0)
                throw new ArgumentException("Danh sách bình luận rỗng!");

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("BinhLuan_Full");

                int colCount = 5;

                // ===============================
                // 1️⃣ TIÊU ĐỀ
                // ===============================
                ws.Range(1, 1, 1, colCount).Merge();
                ws.Cell(1, 1).Value = "BÌNH LUẬN BÀI VIẾT";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Range(2, 1, 2, colCount).Merge();
                ws.Cell(2, 1).Value = postLink;
                ws.Cell(2, 1).Style.Font.FontColor = XLColor.Blue;
                ws.Cell(2, 1).SetHyperlink(new XLHyperlink(postLink));

                // ===============================
                // 2️⃣ HEADER
                // ===============================
                int headerRow = 4;

                ws.Cell(headerRow, 1).Value = "STT";
                ws.Cell(headerRow, 2).Value = "Người bình luận";
                ws.Cell(headerRow, 3).Value = "Thời gian";
                ws.Cell(headerRow, 4).Value = "Link";
                ws.Cell(headerRow, 5).Value = "Nội dung";

                var headerRange = ws.Range(headerRow, 1, headerRow, colCount);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontName = "Times New Roman";
                headerRange.Style.Font.FontSize = 9;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(91, 155, 213);
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ===============================
                // 3️⃣ DỮ LIỆU
                // ===============================
                int row = headerRow + 1;

                foreach (var c in rows)
                {
                    ws.Cell(row, 1).Value = c.STT;
                    ws.Cell(row, 2).Value = c.ActorName;
                    ws.Cell(row, 3).Value = c.Time;
                    ws.Cell(row, 4).Value = "Xem link";
                    ws.Cell(row, 5).Value = c.Content;

                    // 🔗 Link comment
                    if (!string.IsNullOrWhiteSpace(c.Link))
                    {
                        ws.Cell(row, 4).SetHyperlink(new XLHyperlink(c.Link));
                        ws.Cell(row, 4).Style.Font.FontColor = XLColor.Blue;
                        ws.Cell(row, 4).Style.Font.Underline = XLFontUnderlineValues.Single;
                    }

                    // ===== STYLE CHA / CON =====
                    if (c.Level == 0)
                    {
                        // Comment cha
                        ws.Row(row).Style.Font.FontName = "Times New Roman";
                        ws.Row(row).Style.Font.FontSize = 9;
                        ws.Row(row).Style.Font.Bold = true;
                        ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 242, 204);
                    }
                    else
                    {
                        // Comment con
                        ws.Row(row).Style.Font.FontName = "Times New Roman";
                        ws.Row(row).Style.Font.FontSize = 8;

                        // Thụt nội dung theo level
                        ws.Cell(row, 5).Style.Alignment.Indent = c.Level * 2;
                    }

                    row++;
                }

                // ===============================
                // 4️⃣ FORMAT
                // ===============================
                ws.Columns(1, 3).AdjustToContents();
                ws.Column(4).Width = 14;
                ws.Column(5).Width = 70;

                ws.SheetView.FreezeRows(headerRow);

                ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
                ws.PageSetup.FitToPages(1, 0);
                ws.PageSetup.CenterHorizontally = true;

                wb.SaveAs(filePath);
            }
        }
        public static void ExportCommentsFullForPrint( List<CommentGridRow> rows,string postLink, string posterName, string filePath)
        {
            if (rows == null || rows.Count == 0)
                throw new ArgumentException("Danh sách bình luận rỗng!");

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("BinhLuan");
                int colCount = 6;

                // ===============================
                // 1️⃣ HEADER THÔNG TIN BÀI VIẾT
                // ===============================
                ws.Range(1, 1, 1, colCount).Merge();
                ws.Cell(1, 1).Value = "BÌNH LUẬN TẠI BÀI VIẾT";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 13;

                ws.Range(2, 1, 2, colCount).Merge();
                ws.Cell(2, 1).Value = $"Địa chỉ: {postLink ?? ""}";
                if (!string.IsNullOrWhiteSpace(postLink))
                {
                    ws.Cell(2, 1).SetHyperlink(new XLHyperlink(postLink));
                    ws.Cell(2, 1).Style.Font.FontColor = XLColor.Blue;
                    ws.Cell(2, 1).Style.Font.Underline = XLFontUnderlineValues.Single;
                }

                ws.Range(3, 1, 3, colCount).Merge();
                ws.Cell(3, 1).Value = $"Người đăng bài viết: {posterName ?? ""}";

                ws.Row(1).Height = 24;

                // ===============================
                // 2️⃣ HEADER BẢNG (KHÔNG CÓ LINK)
                // ===============================
                int headerRow = 5;

                ws.Cell(headerRow, 1).Value = "STT";
                ws.Cell(headerRow, 2).Value = "Người bình luận";
                ws.Cell(headerRow, 3).Value = "Loại";
                ws.Cell(headerRow, 4).Value = "ID FB";
                ws.Cell(headerRow, 5).Value = "Thời gian";
                ws.Cell(headerRow, 6).Value = "Nội dung";

                var header = ws.Range(headerRow, 1, headerRow, colCount);
                header.Style.Font.Bold = true;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                header.Style.Fill.BackgroundColor = XLColor.White;

                // ===============================
                // 3️⃣ DỮ LIỆU
                // ===============================
                int row = headerRow + 1;

                foreach (var c in rows.Where(x => x != null))
                {
                    ws.Cell(row, 1).Value = c.STT ?? "";
                    ws.Cell(row, 2).Value = c.ActorName ?? "";
                    ws.Cell(row, 3).Value = c.PosterFBType.ToString();
                    ws.Cell(row, 4).Value = c.IDFBPerson ?? "";   // ✅ IDFB
                    ws.Cell(row, 5).Value = c.Time ?? "";
                    ws.Cell(row, 6).Value = c.Content ?? "";

                    // thụt comment con
                    if (c.Level > 0)
                        ws.Cell(row, 6).Style.Alignment.Indent = c.Level * 2;

                    row++;
                }

                // ===============================
                // 4️⃣ STYLE CHUNG – IN
                // ===============================
                int startDataRow = headerRow + 1;
                int endDataRow = row - 1;

                var dataRange = ws.Range(startDataRow, 1, endDataRow, colCount);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                dataRange.Style.Fill.BackgroundColor = XLColor.White;

                ws.Column(2).Width = 28;
                ws.Column(4).Width = 28; // ID FB
                ws.Column(6).Width = 70;
                ws.Column(6).Style.Alignment.WrapText = true;

                ws.SheetView.FreezeRows(headerRow);

                ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
                ws.PageSetup.PagesWide = 1;
                ws.PageSetup.CenterHorizontally = true;

                wb.SaveAs(filePath);
            }
        }


        // xuaasts bài viết person
        public static void ExportPostPersonWithSTT( List<(int STT, PersonPostVM Data)> list, string file)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Post_Person");

                ws.Cell(1, 1).Value = "STT";
                ws.Cell(1, 2).Value = "Người đăng";
                ws.Cell(1, 3).Value = "Thời gian";
                ws.Cell(1, 4).Value = "Link";
                ws.Cell(1, 5).Value = "Trạng thái";
                ws.Cell(1, 6).Value = "Like";
                ws.Cell(1, 7).Value = "Comment";
                ws.Cell(1, 8).Value = "Share";
                ws.Cell(1, 9).Value = "Nội dung";

                int row = 2;
                foreach (var item in list)
                {
                    var p = item.Data;

                    ws.Cell(row, 1).Value = item.STT;
                    ws.Cell(row, 2).Value = p.PosterName;
                    ws.Cell(row, 3).Value = p.TimeView;
                    ws.Cell(row, 4).Value = p.PostLink;
                    ws.Cell(row, 5).Value = p.PostStatus;
                    ws.Cell(row, 6).Value = p.Like;
                    ws.Cell(row, 7).Value = p.Comment;
                    ws.Cell(row, 8).Value = p.Share;
                    ws.Cell(row, 9).Value = p.Content;
                    row++;
                }
                ws.Columns().AdjustToContents();
                wb.SaveAs(file);
            }
        }

        // hàn xuất full link
        public static string ToFullFacebookLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return "";

            if (link.StartsWith("http://") || link.StartsWith("https://"))
                return link;

            if (link.StartsWith("fb.com"))
                return "https://" + link;

            if (link.StartsWith("/"))
                return "https://Fb.com" + link;

            return "https://Fb.com/" + link;
        }

    }
}
