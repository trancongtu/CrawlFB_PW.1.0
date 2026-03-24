using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;

namespace CrawlFB_PW._1._0.Helper   // ❗ đổi namespace cho khớp project
{
    public static class UIHelper
    {
        // ===============================
        // TIME FILTER – BAR EDIT ITEM
        // ===============================
        public static void InitTimeFilterCombo(BarEditItem barItem)
        {
            if (barItem?.Edit == null) return;

            var repo = barItem.Edit as RepositoryItemComboBox;
            if (repo == null) return;

            repo.Items.Clear();

            repo.Items.Add(new ImageComboBoxItem("Hôm nay", 1));
            repo.Items.Add(new ImageComboBoxItem("3 ngày", 3));
            repo.Items.Add(new ImageComboBoxItem("5 ngày", 5));
            repo.Items.Add(new ImageComboBoxItem("1 tuần", 7));
            repo.Items.Add(new ImageComboBoxItem("10 ngày", 10));
            repo.Items.Add(new ImageComboBoxItem("15 ngày", 15));
            repo.Items.Add(new ImageComboBoxItem("1 tháng", 30));

            barItem.EditValue = 7; // 🔥 default
        }

        // ===============================
        // MAX POST – BAR EDIT ITEM
        // ===============================
        public static void InitMaxPostCombo(BarEditItem barItem)
        {
            if (barItem?.Edit == null) return;

            var repo = barItem.Edit as RepositoryItemComboBox;
            if (repo == null) return;

            repo.Items.Clear();

            repo.Items.Add(10);
            repo.Items.Add(20);
            repo.Items.Add(30);
            repo.Items.Add(40);
            repo.Items.Add(50);

            barItem.EditValue = 20; // 🔥 default
        }
    }
}
