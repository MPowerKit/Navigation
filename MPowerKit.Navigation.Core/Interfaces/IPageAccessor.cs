namespace MPowerKit.Navigation.Interfaces;

public interface IPageAccessor
{
    Page? Page { get; set; }
    string? SegmentName { get; set; }
}