using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using DevExpress.Xpo.DB;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraReports;
using CrawlFB_PW._1._0.Helper;

using CrawlFB_PW._1._0.Helpers;
namespace CrawlFB_PW._1._0.DB
{
    public partial class FShowDB : Form
    {
        private const string MAIN_DB = "MainDatabase.db";
        public FShowDB()
        {
            InitializeComponent();         
            btnView.Click += BtnView_Click;
            gridView1.RowCellClick += gridView1_RowCellClick;
        }
        private void BtnView_Click(object sender, EventArgs e)
        {
            try
            {
                string source = CbSource.SelectedItem?.ToString() ?? "";
                int days = GetDaysFilter();
                int maxCount = GetMaxCount();          
                DataTable dt = null;
                switch (source)
                {
                    case "Hội nhóm":
                        dt = SQLDAO.Instance.GetAllPagesDB();
                        break;

                    case "Đối tượng":
                        dt = SQLDAO.Instance.GetAllPersonDB();
                        break;

                    case "Bài Viết":
                        dt = SQLDAO.Instance.GetAllPostsDB(days, maxCount);                    
                        break;
                    default:
                        MessageBox.Show("⚠️ Vui lòng chọn nguồn dữ liệu.", "Thông báo");
                        return;
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để hiển thị.", "Thông báo");
                    return;
                }
                gridControl1.DataSource = dt;
                var gv = gridView1;
                gv.PopulateColumns();
                gv.OptionsBehavior.Editable = false;
                gv.OptionsView.RowAutoHeight = true;
                gv.OptionsView.ColumnAutoWidth = false;
                gv.BestFitColumns();
                foreach (DevExpress.XtraGrid.Columns.GridColumn col in gv.Columns)
                {
                    if (col.Width > 60)
                        col.Width = 60;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi");
            }
        }
        private void panelControlSetup_Paint(object sender, PaintEventArgs e)
        {

        }
        private int GetDaysFilter()
        {
            switch (cbTime.Text)
            {
                case "1 Ngày trước": return 1;
                case "1 Tuần trước": return 7;
                case "1 Tháng trước": return 30;
                case "Toàn thời gian": return 0;
            }
            return 0;
        }

        private int GetMaxCount()
        {
            if (cbMaxCount.Text == "Tất cả")
                return 0;

            if (int.TryParse(cbMaxCount.Text, out int val))
                return val;

            return 0;
        }

        private string Safe(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return "";
            return r[col]?.ToString() ?? "";
        }
        private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            try
            {
                var gv = sender as DevExpress.XtraGrid.Views.Grid.GridView;

                string field = e.Column.FieldName;
                object value = gv.GetRowCellValue(e.RowHandle, e.Column);

                txbBinding.Text = value?.ToString() ?? "";
            }
            catch { }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
        "⚠ Bạn có chắc muốn xóa TẤT CẢ dữ liệu?",
        "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            // ========== THỨ TỰ XÓA (tránh lỗi FK) ==========
            var tables = new List<string>
    {
        "TablePostComment",
        "TableCommentInfo",
        "TablePostShare",
        "TableTopicPost",
        "TableTopicKey",
        "TableKeyword",
        "TableTopic",
        "TableManagerProfile",
        "TablePageNote",
        "TablePost",       // mapping
        "TablePostInfo",
        "TablePageMonitor",
        // cuối cùng bảng chính
        "TablePageInfo",
        "TablePersonInfo"
    };

            using (var conn = SQLDAO.Instance.OpenConnection())
            {
                SQLDAO.Instance.DeleteAllRowsInTables(conn, tables);
                MessageBox.Show("✔ Đã xóa sạch toàn bộ dữ liệu!", "Thành công");
            }        
        }

        private void btnDeleteone_Click(object sender, EventArgs e)
        {
            var gv = gridView1;
            if (gv.SelectedRowsCount == 0)
            {
                MessageBox.Show("Chưa chọn dòng nào");
                return;
            }

            string source = CbSource.SelectedItem?.ToString();

            foreach (int rh in gv.GetSelectedRows())
            {
                DataRow row = gv.GetDataRow(rh);

                if (source == "Bài Viết")
                {
                    string postId = row["PostID"].ToString();
                    SQLDAO.Instance.DeletePostFull(postId);
                }
                else if (source == "Hội nhóm")
                {
                    string pageId = row["PageID"].ToString();
                    SQLDAO.Instance.DeletePageFull(pageId);
                }
                else if (source == "Đối tượng")
                {
                    string personId = row["PersonID"].ToString();
                    SQLDAO.Instance.DeletePersonFull(personId);
                }
            }

            MessageBox.Show("✔ Xoá dữ liệu thành công!");
            BtnView_Click(null, null);  // reload grid
        }

        private void btnClearTable_Click(object sender, EventArgs e)
        {
            string source = CbSource.SelectedItem?.ToString();

            if (source == "Bài Viết")
                ClearAllPosts();
            else if (source == "Hội nhóm")
                ClearAllPages();
            else if (source == "Đối tượng")
                ClearAllPersons();
            else
                MessageBox.Show("⚠ Chưa chọn loại dữ liệu để xoá.");
            BtnView_Click(null, null);
        }
        private void ClearAllPosts()
        {
            SQLDAO.Instance.DeleteAllPosts();
            BtnView_Click(null, null);
            MessageBox.Show("✔ Đã xoá toàn bộ dữ liệu bài viết.");
        }
        private void ClearAllPages()
        {
            // 1️⃣ Lấy danh sách PageID
            DataTable dt = SQLDAO.Instance.GetAllPagesDB();

            foreach (DataRow r in dt.Rows)
            {
                string pageId = r["PageID"].ToString();

                // Xóa toàn bộ bài thuộc Page
                SQLDAO.Instance.DeleteAllPostsOfPage(pageId);

                // Xóa ghi chú
                SQLDAO.Instance.DeletePageNote(pageId);

                // Xóa monitor
                SQLDAO.Instance.DeletePageMonitor(pageId);
            }

            // 2️⃣ Cuối cùng xoá PageInfo
            SQLDAO.Instance.ExecuteNonQuery("DELETE FROM TablePageInfo");
            BtnView_Click(null, null);
            MessageBox.Show("✔ Đã xoá toàn bộ Hội nhóm + bài viết liên quan.");
        }
        private void ClearAllPersons()
        {
            using (var conn = SQLDAO.Instance.OpenConnection())
            using (var tran = conn.BeginTransaction())
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;

                // COMMENT
                cmd.CommandText = @"
            DELETE FROM TablePostComment
            WHERE CommentID IN (SELECT CommentID FROM TableCommentInfo);

            DELETE FROM TableCommentInfo;
        ";
                cmd.ExecuteNonQuery();

                // SHARE FROM PERSON
                cmd.CommandText = "DELETE FROM TablePostShare WHERE PersonID IS NOT NULL;";
                cmd.ExecuteNonQuery();

                // POSTS CREATED BY PERSON
                cmd.CommandText = @"
            DELETE FROM TablePost
            WHERE PersonIDCreate IS NOT NULL;

            DELETE FROM TablePostInfo
            WHERE PostID NOT IN (SELECT PostID FROM TablePost);
        ";
                cmd.ExecuteNonQuery();

                // DELETE PERSON
                cmd.CommandText = "DELETE FROM TablePersonInfo;";
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            BtnView_Click(null, null);
            MessageBox.Show("✔ Đã xoá toàn bộ Đối tượng + dữ liệu liên quan.");
        }



        // 🧱 1️⃣ Load TablePageInfo


    }
}
