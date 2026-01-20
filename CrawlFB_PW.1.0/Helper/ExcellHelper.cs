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
        public bool LoadPageFromExcel(List<ExcelPageRow> rows,string initialFolder = null)
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
            SetWidth(ws, table, "ContainerPageLink",10);

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


    }


}
