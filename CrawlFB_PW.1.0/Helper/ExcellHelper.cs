using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Data;
using System.Windows.Forms;
using System.IO;
using static CrawlFB_PW._1._0.Page.FFirstScanPostPage;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.DAO
{
    public class ExcellHelper
    {
        // 🔹 Singleton instance
        private static ExcellHelper _instance;
        public static ExcellHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ExcellHelper();
                return _instance;
            }
        }
        private static readonly Dictionary<string, string> HeaderMapVN =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "STT", "STT" },
    { "PostLink", "Link bài viết" },
    { "PostContent", "Nội dung bài viết" },
    { "RealPostTime", "Thời gian đăng" },
    { "LikeCount", "Lượt thích" },
    { "ShareCount", "Lượt chia sẻ" },
    { "CommentCount", "Bình luận" },
    { "PosterName", "Người đăng" },
    { "PosterLink", "Link người đăng" },
    { "ContainerPageName", "Trang / Nhóm chứa" },
    { "ContainerPageLink", "Link Trang / Nhóm" }
};

        //----------Doc url tu File,cot 2
        public bool LoadUrlPageFromExcel(List<string> listUrl, string initialFolder = null)
        {
            try
            {
                // 🔹 Xác định thư mục mặc định
                string folderPath = initialFolder ?? Path.Combine(Application.StartupPath, "Data", "Page");

                // 🔹 Nếu chưa có thì tự tạo
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 🔹 Mở hộp thoại chọn file
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = folderPath,
                    Filter = "Excel Files|*.xlsx",
                    Title = "Chọn file danh sách Url"
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return false;

                // 🔹 Đọc file Excel
                using (var workbook = new XLWorkbook(openFileDialog.FileName))
                {
                    var worksheet = workbook.Worksheets.First();

                    foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(1))
                    {
                        string url = row.Cell(2).GetString().Trim();
                        if (!string.IsNullOrEmpty(url) && !listUrl.Contains(url))
                            listUrl.Add(url);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        //---- doc file nap class để chia url
        public class ExcelPageRow
        {
            public string PageUrl { get; set; }
            public string PageName { get; set; }
        }
        public bool LoadPageFromExcel(List<ExcelPageRow> rows, string initialFolder = null)
        {
            try
            {
                string folderPath = initialFolder
                    ?? Path.Combine(Application.StartupPath, "Data", "Page");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                OpenFileDialog dlg = new OpenFileDialog
                {
                    InitialDirectory = folderPath,
                    Filter = "Excel Files|*.xlsx",
                    Title = "Chọn file danh sách URL Page"
                };

                if (dlg.ShowDialog() != DialogResult.OK)
                    return false;

                using (var workbook = new XLWorkbook(dlg.FileName))
                {
                    var ws = workbook.Worksheets.First();

                    foreach (var row in ws.RangeUsed().RowsUsed().Skip(1))
                    {
                        string url = row.Cell(2).GetString().Trim();
                        string name = row.Cell(3).GetString().Trim();

                        if (string.IsNullOrEmpty(url))
                            continue;

                        rows.Add(new ExcelPageRow
                        {
                            PageUrl = url,
                            PageName = string.IsNullOrEmpty(name) ? "N/A" : name
                        });
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi đọc file Excel: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        public static void StylePostWorksheet(IXLWorksheet ws, DataTable dt)
        {
            if (ws == null || dt == null || dt.Rows.Count == 0)
                return;

            var table = ws.Tables.First();
            //ApplyVietnameseHeader(table);
            // 1️⃣ Freeze header
            ws.SheetView.FreezeRows(1);

            // 2️⃣ AutoFilter (đúng cho Table)
            table.ShowAutoFilter = true;

            // 3️⃣ Style header
            // Style header (ClosedXML cũ)
            var header = table.AsRange().FirstRow();

            header.Style.Font.Bold = true;
            header.Style.Font.FontColor = XLColor.Black;   // 🔥 chữ đen
            header.Style.Fill.BackgroundColor = XLColor.FromArgb(180, 205, 255); // 🔥 xanh rõ
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;



            // 4️⃣ Column width
            SetWidth(ws, table, "STT", 6);
            SetWidth(ws, table, "PostLink", 10);
            SetWidth(ws, table, "PostContent", 70);
            SetWidth(ws, table, "RealPostTime", 20);
            SetWidth(ws, table, "LikeCount", 6);
            SetWidth(ws, table, "ShareCount", 6);
            SetWidth(ws, table, "CommentCount", 6);
            SetWidth(ws, table, "PosterName", 30);
            SetWidth(ws, table, "PosterLink", 10);
            SetWidth(ws, table, "ContainerPageName", 30);
            SetWidth(ws, table, "ContainerPageLink", 10);

            // 5️⃣ Wrap content
            Wrap(ws, table, "PostContent");

            // 6️⃣ Format datetime
            FormatDate(ws, table, "RealPostTime", "dd/MM/yyyy HH:mm");

            // 7️⃣ Align number
            AlignCenter(ws, table, "STT");
            AlignCenter(ws, table, "LikeCount");
            AlignCenter(ws, table, "ShareCount");
            AlignCenter(ws, table, "CommentCount");

            // 8️⃣ Hyperlink
            MakeHyperlink(ws, dt, table, "PostLink");
            MakeHyperlink(ws, dt, table, "PosterLink");
            MakeHyperlink(ws, dt, table, "ContainerPageLink");
        }


        // =========================
        // 🔧 HELPER METHODS
        // =========================

        private static int? GetColumnIndexByHeader(IXLTable table, string colName)
        {
            var headerRow = table.AsRange().FirstRow();

            foreach (var cell in headerRow.Cells())
            {
                if (cell.GetString().Equals(colName, StringComparison.OrdinalIgnoreCase))
                    return cell.Address.ColumnNumber;
            }

            return null;


        }

        private static void SetWidth(IXLWorksheet ws, IXLTable table, string colName, double width)
        {
            var colIndex = GetColumnIndexByHeader(table, colName);
            if (colIndex.HasValue)
                ws.Column(colIndex.Value).Width = width;
        }

        private static void Wrap(IXLWorksheet ws, IXLTable table, string colName)
        {
            var colIndex = GetColumnIndexByHeader(table, colName);
            if (colIndex.HasValue)
                ws.Column(colIndex.Value).Style.Alignment.WrapText = true;
        }

        private static void FormatDate(IXLWorksheet ws, IXLTable table, string colName, string format)
        {
            var colIndex = GetColumnIndexByHeader(table, colName);
            if (colIndex.HasValue)
                ws.Column(colIndex.Value).Style.DateFormat.Format = format;
        }

        private static void AlignCenter(IXLWorksheet ws, IXLTable table, string colName)
        {
            var colIndex = GetColumnIndexByHeader(table, colName);
            if (colIndex.HasValue)
                ws.Column(colIndex.Value).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
        }

        private static void MakeHyperlink(
    IXLWorksheet ws,
    DataTable dt,
    IXLTable table,
    string colName)
        {
            if (!dt.Columns.Contains(colName))
                return;

            var colIndex = GetColumnIndexByHeader(table, colName);
            if (!colIndex.HasValue)
                return;

            for (int r = 2; r <= dt.Rows.Count + 1; r++)
            {
                var cell = ws.Cell(r, colIndex.Value);
                var url = cell.GetString();

                if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("http"))
                {
                    // 🔗 gán link
                    cell.SetHyperlink(new XLHyperlink(url));

                    // 🔥 đổi text hiển thị
                    cell.Value = "Xem link";

                    // style giống link web
                    cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                    cell.Style.Font.FontColor = XLColor.Blue;
                }
            }
        }
        private static void ApplyVietnameseHeader(IXLTable table)
        {
            var headerRow = table.AsRange().FirstRow();

            foreach (var cell in headerRow.Cells())
            {
                string original = cell.GetString();

                if (HeaderMapVN.TryGetValue(original, out string vn))
                {
                    cell.Value = vn;
                }
            }
        }

        // share post
        private void ExportSingleShareCommentSheet(
    IXLWorksheet ws,
    SharePostVM share)
        {
            int row = 1;
            ws.Column(1).Width = 22;
            // =========================
            // TIÊU ĐỀ
            // =========================
            ws.Cell(row, 1).Value = "Bình luận của bài viết";
            ws.Range(row, 1, row, 4).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;

            // =========================
            // ĐỊA CHỈ BÀI VIẾT
            // =========================
            ws.Cell(row, 1).Value = "Địa chỉ bài viết:";
            //ws.Cell(row, 2).Value = "Mở bài";
            ws.Cell(row, 2).Value = share.PostLinkShare;
            ws.Cell(row, 2).SetHyperlink(new XLHyperlink(share.PostLinkShare));
            ws.Range(row, 2, row, 4).Merge();
            ws.Cell(row, 2).Style.Alignment.WrapText = true;
            row++;

            // =========================
            // NGƯỜI SHARE
            // =========================
            ws.Cell(row, 1).Value = "Người share:";
            ws.Cell(row, 2).Value = share.SharerName;
            ws.Cell(row, 3).Value = "Người share";
            ws.Cell(row, 3).SetHyperlink(new XLHyperlink(share.SharerLink));
            ws.Range(row, 2, row, 4).Merge();
            row++;

            // =========================
            // NƠI CHỨA
            // =========================
            ws.Cell(row, 1).Value = "Nơi chứa:";
            ws.Cell(row, 2).Value = share.TargetName;
            ws.Cell(row, 3).Value = "Nơi share";
            ws.Cell(row, 3).SetHyperlink(new XLHyperlink(share.TargetLink));
            ws.Range(row, 2, row, 4).Merge();
            row++;
            // =========================
            // HEADER COMMENT
            // =========================
            ws.Cell(row, 1).Value = "STT";
            ws.Cell(row, 2).Value = "Người bình luận";
            ws.Cell(row, 3).Value = "Link người BL";
            ws.Cell(row, 4).Value = "Nội dung";
            ws.Cell(row, 5).Value = "Thời gian";

            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5)
              .Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;
            // =========================
            // COMMENT DATA
            // =========================
            if (share.Comments != null && share.Comments.Count > 0)
            {
                int stt = 1;
                foreach (var c in share.Comments)
                {
                    ws.Cell(row, 1).Value = stt++;

                    // 👤 Người bình luận
                    ws.Cell(row, 2).Value = c.ActorName;

                    // 🔗 Link người BL (IN RA THẲNG)
                    ws.Cell(row, 3).Value = c.Link;
                    if (!string.IsNullOrWhiteSpace(c.Link))
                    {
                        ws.Cell(row, 3).SetHyperlink(new XLHyperlink(c.Link));
                        ws.Cell(row, 3).Style.Font.Underline = XLFontUnderlineValues.Single;
                        ws.Cell(row, 3).Style.Font.FontColor = XLColor.Blue;
                        ws.Cell(row, 3).Style.Alignment.WrapText = true;
                    }

                    // 💬 Nội dung
                    ws.Cell(row, 4).Value = c.Content;
                    ws.Cell(row, 4).Style.Alignment.WrapText = true;

                    // ⏰ Thời gian
                    if (c.RealPostTime.HasValue)
                    {
                        ws.Cell(row, 5).Value = c.RealPostTime.Value;
                        ws.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                    }
                    else
                    {
                        ws.Cell(row, 5).Value = c.Time;
                    }

                    row++;
                }
            }
            else
            {
                ws.Cell(row, 2).Value = "(Không có bình luận)";
            }

            // PREVIEW A4 NGANG
            // =========================
            ExcelPreview(
             ws,
             1, 5,
             new Dictionary<int, double>
             {
                {1, 5},    // STT
                {2, 22},   // Người BL
                {3, 35},   // Link người BL
                {4, 55},   // Nội dung
                {5, 18}    // Thời gian
             }
         );

        }


        public void ExportSharePostFullExcel(
    List<SharePostVM> shares,
    string filePath)
        {
            using (var wb = new XLWorkbook())
            {
                // =====================
                // SHEET 1 – DANH SÁCH SHARE
                // =====================
                var wsList = wb.Worksheets.Add("Danh sách share");

                int row = 1;
                wsList.Cell(row, 1).Value = "STT";
                wsList.Cell(row, 2).Value = "Người share";
                wsList.Cell(row, 3).Value = "Link người share";
                wsList.Cell(row, 4).Value = "Nơi share";
                wsList.Cell(row, 5).Value = "Link nơi share";
                wsList.Cell(row, 6).Value = "Thời gian";
                wsList.Cell(row, 7).Value = "Link bài viết";
                wsList.Cell(row, 8).Value = "Tổng bình luận";

                wsList.Range(row, 1, row, 8).Style.Font.Bold = true;
                row++;

                int stt = 1;
                foreach (var s in shares)
                {
                    wsList.Cell(row, 1).Value = stt++;
                    wsList.Cell(row, 2).Value = s.SharerName;
                    wsList.Cell(row, 3).Value = s.SharerLink;
                    wsList.Cell(row, 3).SetHyperlink(new XLHyperlink(s.SharerLink));
                    wsList.Cell(row, 4).Value = s.TargetName;
                    wsList.Cell(row, 5).Value = s.TargetLink;
                    wsList.Cell(row, 5).SetHyperlink(new XLHyperlink(s.TargetLink));
                    wsList.Cell(row, 6).Value = s.TimeShare;
                    wsList.Cell(row, 7).Value = s.PostLinkShare;
                    wsList.Cell(row, 7).SetHyperlink(new XLHyperlink(s.PostLinkShare));
                    wsList.Cell(row, 8).Value = s.TotalComment;

                    row++;
                }

                // Preview A4 ngang cho sheet list
                ExcelPreview(
                    wsList,
                    1, 8,
                    new Dictionary<int, double>
                    {
                {1, 5},
                {2, 20},
                {3, 18},
                {4, 20},
                {5, 18},
                {6, 15},
                {7, 18},
                {8, 12}
                    }
                );


                // =====================
                // SHEET 2..N – MỖI SHARE 1 SHEET COMMENT
                // =====================
                int idx = 1;
                foreach (var share in shares)
                {
                    string safeName = share.SharerName;
                    if (string.IsNullOrWhiteSpace(safeName))
                        safeName = "Share";

                    // Excel giới hạn 31 ký tự
                    safeName = safeName.Length > 20
                        ? safeName.Substring(0, 20)
                        : safeName;

                    string sheetName = $"BL_{safeName}_{idx++}";

                    var wsComment = wb.Worksheets.Add(sheetName);
                    ExportSingleShareCommentSheet(wsComment, share);
                }

                wb.SaveAs(filePath);
            }
        }

public void ExcelPreview(
    IXLWorksheet ws,
    int fromCol,
    int toCol,
    Dictionary<int, double> columnWidths = null)
        {
            // =========================
            // 1️⃣ PAGE SETUP – A4 NGANG
            // =========================
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;

            ws.PageSetup.Margins.Top = 0.5;
            ws.PageSetup.Margins.Bottom = 0.5;
            ws.PageSetup.Margins.Left = 0.5;
            ws.PageSetup.Margins.Right = 0.5;

            // =========================
            // 2️⃣ FIT TO 1 TRANG
            // =========================
            ws.PageSetup.FitToPages(1, 1);

            // =========================
            // 3️⃣ SET WIDTH CỘT (NẾU CÓ)
            // =========================
            if (columnWidths != null)
            {
                foreach (var kv in columnWidths)
                {
                    ws.Column(kv.Key).Width = kv.Value;
                }
            }

            // =========================
            // 4️⃣ WRAP TEXT + CANH TRÊN
            // =========================
            ws.Columns(fromCol, toCol).Style.Alignment.WrapText = true;
            ws.Columns(fromCol, toCol).Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Top;
        }


    }
}
