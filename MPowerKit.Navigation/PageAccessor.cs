using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation;

public class PageAccessor : IPageAccessor
{
    private WeakReference<Page?>? _weakPage;

    public Page? Page
    {
        get => _weakPage?.TryGetTarget(out var target) is true ? target : null;
        set => _weakPage = value is null ? null : new(value);
    }

    public string? SegmentName { get; set; }
}