using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.KeyWord;
using CrawlFB_PW._1._0.ViewModels.phan_tich;
using DevExpress.XtraPrinting.Native;

namespace CrawlFB_PW._1._0.UC.phantich
{
    public partial class UCAddAttentionGroup : UserControl
    {
        public UCAddAttentionGroup()
        {
            InitializeComponent();
        }           
        private void btn_Save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
                {
                   /* var groups = groupBindingSource.Cast<GroupVM>().ToList();

                    GroupJsonService.Save(groups);

                    MessageBox.Show("Đã lưu cấu hình nhóm (JSON)");
                   */
                }

                private void btn_AddKeyword_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
                {
            /*
                    var type = Cb_TypeGroups.EditValue?.ToString();
                    if (string.IsNullOrEmpty(type))
                    {
                        MessageBox.Show("Vui lòng chọn loại nhóm!");
                        return;
                    }

                    using (var frm = new FAddKeyword(type)) // form add chung
                    {
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            // Form này tự Save keyword vào DB
                            ReloadKeywordAndGroup();
                        }
                    }
            */
                }

                private void btn_LoadKeyword_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
                {
            /*
                    var type = Cb_TypeGroups.EditValue?.ToString();
                    if (string.IsNullOrEmpty(type))
                    {
                        MessageBox.Show("Vui lòng chọn loại nhóm!");
                        return;
                    }

                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        dlg.Filter = "Excel (*.xlsx)|*.xlsx";
                        if (dlg.ShowDialog() != DialogResult.OK) return;

                        switch (type)
                        {
                            case "Theo dõi":
                                ImportAttentionExcel(dlg.FileName);
                                break;

                            case "Tiêu cực":
                                ImportNegativeExcel(dlg.FileName);
                                break;

                            case "Loại trừ":
                                ImportExcludeExcel(dlg.FileName);
                                break;
                        }

                        ReloadKeywordAndGroup();
                    }
            */
                }
        /*       
        void ImportAttentionExcel(string file)
                {
                    var rows = ExcelHelper.ReadAttention(file);
                    // Keyword | Score | TrackingLevel | Note

                    foreach (var r in rows)
                    {
                        int keywordId = EnsureKeyword(r.Keyword);

                        SQLDAO.Instance.ExecuteNonQuery(@"
                    MERGE TableAttentionKeywordScore AS t
                    USING (SELECT @KeywordId AS KeywordId) s
                    ON t.KeywordId = s.KeywordId
                    WHEN MATCHED THEN
                        UPDATE SET Score=@Score, TrackingLevel=@Level, Note=@Note
                    WHEN NOT MATCHED THEN
                        INSERT (KeywordId, Score, TrackingLevel, Note)
                        VALUES (@KeywordId, @Score, @Level, @Note)
                ", new
                        {
                            KeywordId = keywordId,
                            r.Score,
                            Level = r.TrackingLevel,
                            r.Note
                        });
                    }
                }
                void ImportNegativeExcel(string file)
                {
                    var rows = ExcelHelper.ReadNegative(file);
                    // Keyword | Score | NegativeLevel | IsCritical | Note

                    foreach (var r in rows)
                    {
                        int keywordId = EnsureKeyword(r.Keyword);

                        SQLDAO.Instance.ExecuteNonQuery(@"
                    MERGE TableNegativeKeywordScore AS t
                    USING (SELECT @KeywordId AS KeywordId) s
                    ON t.KeywordId = s.KeywordId
                    WHEN MATCHED THEN
                        UPDATE SET Score=@Score, NegativeLevel=@Level, IsCritical=@IsCritical, Note=@Note
                    WHEN NOT MATCHED THEN
                        INSERT (KeywordId, Score, NegativeLevel, IsCritical, Note)
                        VALUES (@KeywordId, @Score, @Level, @IsCritical, @Note)
                ", new
                        {
                            KeywordId = keywordId,
                            r.Score,
                            Level = r.NegativeLevel,
                            r.IsCritical,
                            r.Note
                        });
                    }
                }
                void ImportExcludeExcel(string file)
                {
                    var rows = ExcelHelper.ReadExclude(file);
                    // Keyword | Note

                    foreach (var r in rows)
                    {
                        int keywordId = EnsureKeyword(r.Keyword);

                        SQLDAO.Instance.ExecuteNonQuery(@"
                    IF NOT EXISTS (SELECT 1 FROM TableExcludeKeyword WHERE KeywordId=@KeywordId)
                        INSERT INTO TableExcludeKeyword (KeywordId, Note)
                        VALUES (@KeywordId, @Note)
                ", new
                        {
                            KeywordId = keywordId,
                            r.Note
                        });
                    }
                }
                int EnsureKeyword(string keyword)
                {
                    var id = SQLDAO.Instance.ExecuteScalar<int?>(
                        "SELECT KeywordId FROM TableKeyword WHERE KeywordName=@k",
                        new { k = keyword });

                    if (id.HasValue) return id.Value;

                    return SQLDAO.Instance.ExecuteScalar<int>(
                        @"INSERT INTO TableKeyword (KeywordName) 
                  OUTPUT INSERTED.KeywordId 
                  VALUES (@k)", new { k = keyword });
                }
        */
    }

}
