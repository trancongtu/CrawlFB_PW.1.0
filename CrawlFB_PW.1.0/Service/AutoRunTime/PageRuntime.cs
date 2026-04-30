using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using CrawlFB_PW._1._0.DTO;
namespace CrawlFB_PW._1._0.Service.AutoRunTime
{
    public class PageRuntime
    {
        // ====== Thông tin cơ bản ======
        public string PageId { get; set; }
        public string PageName { get; set; }
        public string PageLink { get; set; }

        // ====== Playwright ======
        public IPage Page { get; set; }   // tab đang dùng

        // ====== Trạng thái runtime ======
        public bool IsRunning { get; set; } = false;
        public bool IsWaiting { get; set; } = true;

        // ====== Điều phối ======
        public DateTime NextRunTime { get; set; } = DateTime.Now;

        // ====== Thống kê ======
        public int NoPostCount { get; set; } = 0;
        public int TotalPostCrawled { get; set; } = 0;
        public HashSet<string> SavedPostIds { get; set; } = new HashSet<string>();

        // ====== Debug / log ======
        public DateTime LastRunTime { get; set; }
        public string LastStatus { get; set; } = "Init";
        public PageInfo PageInfo { get; set; }
        public bool IsSecondRun { get; set; } = false;
        public RecentPostCache Cache { get; set; } = new RecentPostCache();
        public DateTime? LastLoopNewestTime { get; set; }
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
    }
}