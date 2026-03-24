using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper.Log;
using CrawlFB_PW._1._0.Profile;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
namespace CrawlFB_PW._1._0.Check
{
    public partial class FCheckDOM : Form
    {
        List<ProfileDB> _selectedProfiles;
        public FCheckDOM()
        {
            InitializeComponent();
            DOMCheck.LoadSelectors();
        }
        void Log(string text)
        {
            if (Rtb_Result.InvokeRequired)
            {
                Rtb_Result.Invoke(new Action(() => Log(text)));
                return;
            }

            Rtb_Result.AppendText(text + Environment.NewLine);
        }
        private bool SelectProfilesForScan(out List<ProfileDB> profiles)
        {
            profiles = null;

            try
            {
                using (var frm = new SelectProfileDB())
                {
                    if (frm.ShowDialog() != DialogResult.OK)
                        return false;

                    var selected = frm.Tag as List<ProfileDB>;

                    if (selected == null || selected.Count == 0)
                    {
                        MessageBox.Show("❌ Chưa chọn profile!");
                        return false;
                    }

                    var invalid = selected.Where(p => p.UseTab >= 3).ToList();

                    if (invalid.Count > 0)
                    {
                        string msg = string.Join("\n",
                            invalid.Select(p =>
                                $"{p.ProfileName} ({p.UseTab}/3 tab)"));

                        MessageBox.Show(
                            $"❌ Profile đã đủ tab:\n\n{msg}");

                        return false;
                    }

                    profiles = selected;

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi chọn profile: " + ex.Message);
                return false;
            }
        }
        private void btn_TestFindPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private async void Btn_TestCrawPostPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Rtb_Result.Clear();

            if (!SelectProfilesForScan(out _selectedProfiles))
            {
                MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                return;
            }

            string url = Edit_UrlOrText.EditValue?.ToString();

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Nhập URL page");
                return;
            }

            var profile = _selectedProfiles.First();

            await DomCrawlerTester.RunTestPage(
                profile,
                url,
                Log);
        }

        private void btn_TestCrawtShareOrCommentPost_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btn_SaveDOM_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private async void btn_CheckDOM_ItemClick(
     object sender,
     DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Rtb_Result.Clear();

            if (!SelectProfilesForScan(out _selectedProfiles))
            {
                MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                return;
            }

            string url = Edit_UrlOrText.EditValue?.ToString();

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Nhập URL page");
                return;
            }

            var profile = _selectedProfiles.First();

            try
            {
                TestLogHelper.Section(Log, "OPEN PAGE");

                var mainPage =
                    await Ads.Instance
                    .GetPageEnsureSingleTabAsync(profile.IDAdbrowser);

                if (mainPage == null)
                {
                    Log("❌ Không mở được browser");
                    return;
                }

                var page =
                    await Ads.Instance
                    .OpenNewTabAsync(profile.IDAdbrowser);

                if (page == null)
                {
                    Log("❌ Không mở được tab mới");
                    return;
                }

                TestLogHelper.Step(
                    Log,
                    "FCheckDOM",
                    "btn_CheckDOM",
                    "Goto",
                    url);

                await page.GotoAsync(url);

                await page.WaitForLoadStateAsync(
                    Microsoft.Playwright.LoadState.DOMContentLoaded);

                await page.WaitForTimeoutAsync(1500);

                TestLogHelper.Step(
                    Log,
                    "FCheckDOM",
                    "btn_CheckDOM",
                    "PageLoaded",
                    "OK");

                // ===============================
                // CHECK DOM SELECTORS
                // ===============================

                await DOMCheck.CheckCrawlerPipeline(page, Log);

                TestLogHelper.Section(Log, "CHECK DONE");

                await Ads.Instance.ClosePageAsync(page);
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message);
            }
        }
    }
}
