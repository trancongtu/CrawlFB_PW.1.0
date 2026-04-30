using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
namespace CrawlFB_PW._1._0.Helper
{
    public class ScrollManager
    {
        private readonly SemaphoreSlim _scrollLock;
        private readonly int _maxRetry = 3;

        public ScrollManager(int maxConcurrent = 3)
        {
            _scrollLock = new SemaphoreSlim(maxConcurrent);
        }

        public async Task<bool> ScrollAsync(IPage page, string pageId = "")
        {
            if (page == null || page.IsClosed)
            {
                Log(pageId, "⚠️ Page null/closed → skip");
                return false;
            }

            await _scrollLock.WaitAsync();

            try
            {
                return await SmartScrollInternal(page, pageId);
            }
            finally
            {
                _scrollLock.Release();
            }
        }

        private async Task<bool> SmartScrollInternal(IPage page, string pageId)
        {
            int retry = 0;

            while (retry < _maxRetry)
            {
                try
                {
                    // 🧠 Detect trạng thái
                    if (await IsBlocked(page))
                    {
                        Log(pageId, "⛔ Page bị block → skip luôn");
                        return false;
                    }

                    if (await IsPopupVisible(page))
                    {
                        Log(pageId, "⚠️ Popup detected → closing...");
                        await TryClosePopup(page);
                        await Task.Delay(RandomDelay(200, 400));
                    }

                    if (await IsLoading(page))
                    {
                        Log(pageId, "⏳ Page loading...");
                        await Task.Delay(RandomDelay(400, 800));
                        retry++;
                        continue;
                    }

                    // 🚀 Scroll chính
                    await ScrollHumanLike(page, pageId);

                    Log(pageId, "✅ Scroll success");
                    return true;
                }
                catch (Exception ex)
                {
                    Log(pageId, $"⚠️ Scroll fail ({retry + 1}): {ex.Message}");

                    await HandleRetryFallback(page, retry);

                    await Task.Delay(RandomDelay(200, 500));
                }

                retry++;
            }

            Log(pageId, "❌ Scroll failed after retries");
            return false;
        }

        // ================= CORE ACTION =================

        private async Task ScrollHumanLike(IPage page, string pageId)
        {
            var rnd = new Random();

            int offset1 = rnd.Next(200, 400);
            await SafeWheel(page, offset1);

            await Task.Delay(RandomDelay(200, 350));

            int offset2 = rnd.Next(300, 600);
            await SafeWheel(page, offset2);

            await Task.Delay(RandomDelay(250, 400));

            int offset3 = rnd.Next(150, 350);
            await SafeWheel(page, -offset3);

            Log(pageId, $"🖱️ Scroll: {offset1}, {offset2}, -{offset3}");
        }

        private async Task SafeWheel(IPage page, int delta)
        {
            await Task.WhenAny(
                page.Mouse.WheelAsync(0, delta),
                Task.Delay(3000) // chống treo
            );
        }

        // ================= DETECT =================

        private async Task<bool> IsPopupVisible(IPage page)
        {
            try
            {
                return await page.QuerySelectorAsync("div[role='dialog']") != null;
            }
            catch { return false; }
        }

        private async Task<bool> IsLoading(IPage page)
        {
            try
            {
                return await page.QuerySelectorAsync("div[aria-busy='true']") != null;
            }
            catch { return false; }
        }

        private async Task<bool> IsBlocked(IPage page)
        {
            try
            {
                var body = await page.QuerySelectorAsync("body");
                var text = (await body.InnerTextAsync())?.ToLower() ?? "";

                return text.Contains("temporarily blocked") ||
                       text.Contains("bị chặn");
            }
            catch { return false; }
        }

        // ================= ACTION =================

        private async Task TryClosePopup(IPage page)
        {
            try
            {
                await page.Keyboard.PressAsync("Escape");
                await Task.Delay(200);

                var btn = await page.QuerySelectorAsync("div[aria-label='Close'], div[aria-label='Đóng']");
                if (btn != null)
                    await btn.ClickAsync();
            }
            catch { }
        }

        private async Task HandleRetryFallback(IPage page, int retry)
        {
            try
            {
                if (retry == 0)
                {
                    await page.Keyboard.PressAsync("Escape");
                }
                else if (retry == 1)
                {
                    await page.Mouse.MoveAsync(200, 300);
                }
                else
                {
                    await page.ReloadAsync();
                }
            }
            catch { }
        }

        // ================= UTIL =================

        private int RandomDelay(int min, int max)
        {
            return new Random().Next(min, max);
        }

        private void Log(string pageId, string message)
        {
            Libary.Instance.CreateLog($"[{pageId}] {message}");
        }
    }
}
