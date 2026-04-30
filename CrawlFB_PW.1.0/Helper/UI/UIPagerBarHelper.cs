using System;
using DevExpress.XtraBars;
using DevExpress.XtraEditors.Repository;
// UI HỖ TRỢ PHÂN TRANG
public class UIPagerBarHelper
{
    public int PageIndex { get; private set; } = 1;
    public int PageSize { get; private set; } = 50;
    public int TotalRows { get; private set; }
    public int TotalPages { get; private set; }

    public Action<int, int> OnPageChanged; // (pageIndex, pageSize)

    // Bar items
    public BarButtonItem BtnFirst;
    public BarButtonItem BtnPrev;
    public BarStaticItem LblPage;
    public BarButtonItem BtnNext;
    public BarButtonItem BtnLast;
    public BarEditItem CbPageSize;

    public void Init(BarManager barManager, Bar barBottom)
    {
        // ⏮
        BtnFirst = CreateButton(barManager, "⏮", (s, e) => GoFirst());
        // ◀
        BtnPrev = CreateButton(barManager, "◀", (s, e) => GoPrev());
        // Trang x / y
        LblPage = new BarStaticItem { Caption = "Trang 1 / 1" };
        barManager.Items.Add(LblPage);
        // ▶
        BtnNext = CreateButton(barManager, "▶", (s, e) => GoNext());
        // ⏭
        BtnLast = CreateButton(barManager, "⏭", (s, e) => GoLast());

        // PageSize
        var repo = new RepositoryItemComboBox();
        repo.Items.AddRange(new object[] { 20, 50, 100, 200 });

        CbPageSize = new BarEditItem(barManager, repo)
        {
            EditValue = PageSize,
            Width = 80,
            Caption = "Hiển thị"
        };

        CbPageSize.EditValueChanged += (s, e) =>
        {
            if (int.TryParse(CbPageSize.EditValue.ToString(), out int size))
            {
                PageSize = size;
                PageIndex = 1;
                RaiseChange();
            }
        };

        // Add to bar
        barBottom.AddItem(BtnFirst);
        barBottom.AddItem(BtnPrev);
        barBottom.AddItem(LblPage);
        barBottom.AddItem(BtnNext);
        barBottom.AddItem(BtnLast);
        barBottom.AddItem(CbPageSize);
    }
    public void Reset()
    {
        PageIndex = 1;
    }
    public void Update(int totalRows)
    {
        TotalRows = totalRows;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalRows / PageSize));

        LblPage.Caption = $"Trang {PageIndex} / {TotalPages}";

        BtnFirst.Enabled = PageIndex > 1;
        BtnPrev.Enabled = PageIndex > 1;
        BtnNext.Enabled = PageIndex < TotalPages;
        BtnLast.Enabled = PageIndex < TotalPages;
    }

    // ===== PRIVATE =====

    void GoFirst() { PageIndex = 1; RaiseChange(); }
    void GoPrev() { if (PageIndex > 1) PageIndex--; RaiseChange(); }
    void GoNext() { if (PageIndex < TotalPages) PageIndex++; RaiseChange(); }
    void GoLast() { PageIndex = TotalPages; RaiseChange(); }

    void RaiseChange()
    {
        OnPageChanged?.Invoke(PageIndex, PageSize);
    }
    public void SetPageIndex(int pageIndex)
    {
        PageIndex = pageIndex < 1 ? 1 : pageIndex;
    }
    BarButtonItem CreateButton(BarManager bm, string caption, ItemClickEventHandler click)
    {
        var btn = new BarButtonItem(bm, caption);
        btn.ItemClick += click;
        bm.Items.Add(btn);
        return btn;
    }
}
