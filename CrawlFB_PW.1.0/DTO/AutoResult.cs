using CrawlFB_PW._1._0.DTO;
using System.Collections.Generic;
using System;
public class AutoResult
{
    public int TotalRead { get; set; }
    public int NewPosts { get; set; }
    public bool StopBecauseOld { get; set; }

    // 🔥 NEW
    public bool IsStopped { get; set; }           // bị stop từ ngoài
    public string StopReason { get; set; }        // lý do dừng

    public string PageName { get; set; }
    public DateTime? NewestTime { get; set; }
    public List<PostPage> Posts { get; set; } = new List<PostPage>();
    public List<ShareItem> Shares { get; set; } = new List<ShareItem>();
    public string Summary
    {
        get
        {
            return $"Đọc: {TotalRead} | Mới: {NewPosts} | StopOld: {StopBecauseOld} | Stop: {IsStopped}";
        }
    }
}
