using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Helper
{
    public class ExcelTemplateHelper
    {
        public static void CreateKeywordNormalTemplate(string filePath)
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Keyword");

                ws.Cell("A1").Value = "STT";
                ws.Cell("B1").Value = "Keyword";
                ws.Cell("C1").Value = "Điểm theo dõi";
                ws.Cell("D1").Value = "Level theo dõi";
                ws.Cell("E1").Value = "Điểm tiêu cực";
                ws.Cell("F1").Value = "Level tiêu cực";

                ws.Range("A1:F1").Style.Font.Bold = true;
                ws.Range("A1:F1").Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Range("A1:F1").Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightGray;

                ws.Range("C2:C1000").CreateDataValidation().WholeNumber.Between(0, 30);
                ws.Range("D2:D1000").CreateDataValidation().WholeNumber.Between(1, 7);
                ws.Range("E2:E1000").CreateDataValidation().WholeNumber.Between(0, 50);
                ws.Range("F2:F1000").CreateDataValidation().WholeNumber.Between(1, 7);

                ws.Cell("A2").Value = 1;
                ws.Cell("B2").Value = "hoa";

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                wb.SaveAs(filePath);
            }
        }

        public static void CreateKeywordExcludeTemplate(string filePath)
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("ExcludeKeyword");

                ws.Cell("A1").Value = "STT";
                ws.Cell("B1").Value = "Keyword";
                ws.Cell("C1").Value = "Level";

                ws.Range("A1:C1").Style.Font.Bold = true;
                ws.Range("A1:C1").Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Range("A1:C1").Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightGray;

                ws.Range("C2:C1000").CreateDataValidation()
                    .WholeNumber.Between(1, 7);

                ws.Cell("A2").Value = 1;
                ws.Cell("B2").Value = "spam";
                ws.Cell("C2").Value = 3;

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                wb.SaveAs(filePath);
            }
        }

        public static void CreateTopicKeywordTemplate(string filePath)
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.AddWorksheet("TopicKeyword");

                // ===== HEADER =====
                ws.Cell("A1").Value = "STT";
                ws.Cell("B1").Value = "Topic";
                ws.Cell("C1").Value = "Keyword";
                ws.Cell("D1").Value = "Loại";
                ws.Cell("E1").Value = "Level";
                ws.Cell("F1").Value = "Điểm";
                ws.Cell("G1").Value = "IsCritical";
                ws.Cell("H1").Value = "Ghi chú";

                ws.Range("A1:H1").Style.Font.Bold = true;
                ws.Range("A1:H1").Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightGray;
                ws.Range("A1:H1").Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ===== COMMENT =====
                ws.Cell("A1").GetComment().AddText("Số thứ tự (chỉ để soát dữ liệu)");
                ws.Cell("D1").GetComment().AddText("Theo dõi | Tiêu cực | Loại trừ");
                ws.Cell("E1").GetComment().AddText("Level từ 1 đến 5");
                ws.Cell("F1").GetComment().AddText("Điểm > 0 mới ghi");
                ws.Cell("G1").GetComment().AddText("0 = thường, 1 = xấu độc");

                // ===== DATA VALIDATION =====
                ws.Range("D2:D1000").CreateDataValidation()
                    .List("Theo dõi,Tiêu cực,Loại trừ");

                ws.Range("E2:E1000").CreateDataValidation()
                    .WholeNumber.Between(1, 5);

                ws.Range("F2:F1000").CreateDataValidation()
                    .WholeNumber.Between(0, 50);

                ws.Range("G2:G1000").CreateDataValidation()
                    .WholeNumber.Between(0, 1);

                // ===== SAMPLE =====
                ws.Cell("A2").Value = 1;
                ws.Cell("B2").Value = "Kinh tế";
                ws.Cell("C2").Value = "lạm phát";
                ws.Cell("D2").Value = "Theo dõi";
                ws.Cell("E2").Value = 2;
                ws.Cell("F2").Value = 10;
                ws.Cell("G2").Value = 0;
                ws.Cell("H2").Value = "Theo dõi vĩ mô";

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                wb.SaveAs(filePath);
            }
        }

    }
}
