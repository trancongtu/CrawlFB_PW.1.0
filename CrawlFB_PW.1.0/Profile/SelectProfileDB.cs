using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using System.Drawing;
using System.Threading.Tasks;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
namespace CrawlFB_PW._1._0.Profile
{
    public partial class SelectProfileDB : Form
    {
        private ProfileInfoDAO profileDao;
        private List<ProfileDB> listProfiles;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repoCheck;
        private Dictionary<int, bool> selectedState = new Dictionary<int, bool>();
        private string _mode;
        private ManagerProfileDAO managerDao = new ManagerProfileDAO();
        
        public SelectProfileDB(string mode = "normal")
        {
            InitializeComponent();
            _mode = mode;
            profileDao = new ProfileInfoDAO();
            InitGrid();
            this.Load += SelectProfileDB_Load;
        }

        private void SelectProfileDB_Load(object sender, EventArgs e)
        {
            LoadProfilesFromDB();
            if (_mode == "auto")
            {
                 LoadTabSmart();
            }
        }
        private void LoadTabSmart()
        {
            var managerDao = new ManagerProfileDAO();
            var adsManager = AdsPowerPlaywrightManager.Instance;

            foreach (var p in listProfiles)
            {
                bool isActive = adsManager.IsProfileActive(p.IDAdbrowser);

                if (isActive)
                {
                    // 🔥 đang chạy thật → dùng mapping
                    p.UseTab = managerDao.CountMappingByProfile(p.ID);
                }
                else
                {
                    // 🔥 không chạy → clear rác
                    managerDao.DeleteByProfile(p.ID);
                    p.UseTab = 0;
                }
            }

            gridView1.RefreshData();
        }

        private void InitGrid()
        {
            UICommercialHelper.StyleGrid(gridView1);
            var gv = gridView1;

            gv.OptionsBehavior.Editable = true; // cho phép sửa cột 
            gv.OptionsView.ShowIndicator = false;
            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsSelection.MultiSelect = true;
            gv.Columns.Clear();

            // ============================
            // CỘT CHÍNH TỪ DATA
            // ============================
            gv.Columns.AddVisible("IDAdbrowser", "ID Adsbrowser").Width = 150;
            gv.Columns.AddVisible("ProfileName", "Tên Profile").Width = 200;
            gv.Columns.AddVisible("ProfileStatus", "Trạng thái").Width = 100;
            gv.Columns.AddVisible("UseTab", "Tab").Width = 60;
            gv.Columns["UseTab"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            gv.Columns["UseTab"].DisplayFormat.FormatString = "{0}/3";
            // ============================
            // CỘT STT (UNBOUND)
            // ============================
            if (!gv.Columns.Any(c => c.FieldName == "STT"))
            {
                var colSTT = gv.Columns.AddField("STT");
                colSTT.Caption = "STT";
                colSTT.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                colSTT.OptionsColumn.AllowEdit = false;
                colSTT.VisibleIndex = 0;
                colSTT.Width = 50;
                colSTT.OptionsColumn.AllowEdit = false;
            }

            // ============================
            // CỘT CHỌN (CHECKBOX)
            // ============================
            if (!gv.Columns.Any(c => c.FieldName == "Selected"))
            {
                repoCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                gridControl1.RepositoryItems.Add(repoCheck);

                var colSel = gv.Columns.AddField("Selected");
                colSel.Caption = "Chọn";
                colSel.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                colSel.OptionsColumn.AllowEdit = true;
                colSel.VisibleIndex = 1;
                colSel.Width = 50;
                colSel.ColumnEdit = repoCheck;
                colSel.OptionsColumn.AllowEdit = true;
               
            }
           
            gv.Columns["IDAdbrowser"].OptionsColumn.AllowEdit = false;
            gv.Columns["ProfileName"].OptionsColumn.AllowEdit = false;
            gv.Columns["ProfileStatus"].OptionsColumn.AllowEdit = false;
            gv.Columns["UseTab"].OptionsColumn.AllowEdit = false;

            // ============================
            // EVENT ĐỔ DỮ LIỆU CHO CỘT UNBOUND
            // ============================
            gv.CustomUnboundColumnData -= Gv_CustomUnboundColumnData;
            gv.CustomUnboundColumnData += Gv_CustomUnboundColumnData;
            gv.ShowingEditor += (s, e) =>
            {
                if (_mode != "auto") return;

                var view = s as DevExpress.XtraGrid.Views.Grid.GridView;

                int used = Convert.ToInt32(
                    view.GetRowCellValue(view.FocusedRowHandle, "UseTab")
                );

                if (used >= 3)
                {
                    e.Cancel = true;

                    MessageBox.Show(
                        "❌ Profile đã dùng đủ 3 tab!",
                        "Giới hạn tab",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };
            gv.RowStyle += (s, e) =>
            {
                if (e.RowHandle < 0) return;

                var view = s as DevExpress.XtraGrid.Views.Grid.GridView;

                int used = Convert.ToInt32(
                    view.GetRowCellValue(e.RowHandle, "UseTab")
                );

                if (_mode == "auto" && used >= 3)
                {
                    e.Appearance.BackColor = Color.LightCoral;
                    e.Appearance.ForeColor = Color.White;
                }
            };
        }

        private void Gv_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            // ======= STT =======
            if (e.Column.FieldName == "STT" && e.IsGetData)
                e.Value = e.ListSourceRowIndex + 1;

            // ======= Selected (checkbox) =======
            if (e.Column.FieldName == "Selected")
            {
                int rowIndex = e.ListSourceRowIndex;

                // Lấy giá trị
                if (e.IsGetData)
                {
                    if (!selectedState.ContainsKey(rowIndex))
                        selectedState[rowIndex] = false;

                    e.Value = selectedState[rowIndex];
                }

                // Gán giá trị (khi tick)
                if (e.IsSetData)
                {
                    bool newVal = Convert.ToBoolean(e.Value);
                    selectedState[rowIndex] = newVal;
                }
            }
        }
        private void LoadProfilesFromDB()
        {
            listProfiles = profileDao.GetAllProfiles();

            foreach (var p in listProfiles)
            {
                // 🔥 AUTO MODE → lấy realtime tab
                if (_mode == "auto")
                {
                    p.UseTab = managerDao.CountMappingByProfile(p.ID);
                }
                else
                {
                    if (p.UseTab < 0) p.UseTab = 0;
                }

                if (string.IsNullOrEmpty(p.ProfileStatus))
                    p.ProfileStatus = "Chưa kiểm tra";
            }

            gridControl1.DataSource = listProfiles;
            gridView1.RefreshData();
        }
        private void gridView1_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            if (e.Column.FieldName != "Selected")
            {
                int idx = e.RowHandle;

                bool cur = false;
                selectedState.TryGetValue(idx, out cur);

                selectedState[idx] = !cur;

                gridView1.RefreshRow(idx);
            }
        }

        // Trả về selected items
        public List<ProfileDB> GetSelectedProfiles()
        {
            var list = new List<ProfileDB>();

            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                if (selectedState.ContainsKey(i) && selectedState[i])
                {
                    var row = gridView1.GetRow(i) as ProfileDB;
                    if (row != null)
                        list.Add(row);
                }
            }

            return list;
        }
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            var selectedList = GetSelectedProfiles();
            if (selectedList.Count == 0) return;

            this.Tag = selectedList;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void btnSelectProfile_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedProfiles();

            if (selected.Count == 0)
            {
                MessageBox.Show("⚠ Vui lòng chọn ít nhất một profile!");
                return;
            }

            this.Tag = selected;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            selectedState.Clear();

            for (int i = 0; i < gridView1.DataRowCount; i++)
                selectedState[i] = true;

            gridView1.RefreshData();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            selectedState.Clear();
            gridView1.RefreshData();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
