using CrawlFB_PW._1._0.DTO;
using System.Collections.Generic;

public class AutoResult
{
    public int TotalRead { get; set; }          // tổng bài đã đọc
    public int NewPosts { get; set; }           // số bài mới
    public bool StopBecauseOld { get; set; }    // dừng vì gặp bài cũ
    public string PageName { get; set; }        // tên page đang giám sát

    // C# 7.3 bắt buộc phải explicit kiểu
    public List<PostPage> Posts { get; set; } = new List<PostPage>();

    // Chuỗi tóm tắt tiện dùng ở popup auto
    public string Summary
    {
        get
        {
            return $"Đọc: {TotalRead} | Mới: {NewPosts} | Dừng vì bài cũ: {StopBecauseOld}";
        }
    }
}
