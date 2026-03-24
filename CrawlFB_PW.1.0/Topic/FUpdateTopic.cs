using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrawlFB_PW._1._0.Topic
{
    public partial class FUpdateTopic : Form
    {
        private int _topicId;

        public FUpdateTopic(int topicId, string topicName)
        {
            InitializeComponent();

            _topicId = topicId;
            txb_NameOld.Text = topicName;
            txb_NameNew.Text = topicName;
        }

        private void btn_Update_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string newName = txb_NameNew.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Tên chủ đề mới không được để trống!");
                return;
            }

            // Không đổi tên
            if (newName.Equals(txb_NameOld.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Tên mới trùng với tên cũ!");
                return;
            }

            // Update DB
            SQLDAO.Instance.UpdateTopic(_topicId, newName);

            MessageBox.Show("Cập nhật chủ đề thành công!");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_Canncel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }
    }
}
