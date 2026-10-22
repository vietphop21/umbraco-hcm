using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace HcmPortal.Infrastructure;

public record LibraryFilter(string? Keyword, int? PeriodId, string? Type, int? FromYear, int? ToYear, string? Author = null);

public static class LibraryQuery
{
    /// <summary>Loại tư liệu dùng cho bộ lọc (FR-05).</summary>
    public static readonly (string Alias, string Label, string Icon)[] Types =
    {
        (DocTypes.Article, "Văn bản", "fa-file-lines"),
        (DocTypes.Album, "Hình ảnh", "fa-image"),
        (DocTypes.Video, "Video", "fa-film"),
        (DocTypes.Audio, "Âm thanh", "fa-headphones"),
        (DocTypes.PdfDocument, "Tài liệu", "fa-file-pdf"),
    };

    public static string TypeLabel(string alias) => SiteHelpers.T(Types.FirstOrDefault(t => t.Alias == alias).Label ?? alias);

    public static string TypeIcon(string alias) => Types.FirstOrDefault(t => t.Alias == alias).Icon ?? "fa-file";

    public static IPublishedContent? LibraryRoot(this IPublishedContent any)
        => any.Root().Children().FirstOrDefault(x => x.ContentType.Alias == DocTypes.LibraryRoot);

    public static IEnumerable<IPublishedContent> Periods(this IPublishedContent libraryRoot)
        => libraryRoot.Children().Where(x => x.ContentType.Alias == DocTypes.Period);

    public static IEnumerable<IPublishedContent> Topics(this IPublishedContent period)
        => period.Children().Where(x => x.ContentType.Alias == DocTypes.PeriodTopic && !x.Value<bool>(LibraryProps.Hide));

    public static IPublishedContent? PeriodOf(this IPublishedContent node)
        => node.AncestorOrSelf(DocTypes.Period);

    /// <summary>Nhánh chủ đề (không có mốc năm) hiển thị nhãn thay cho khoảng năm.</summary>
    public const string ThemeBranchLabel = "TP.HCM";

    public static string YearRange(this IPublishedContent period)
    {
        var from = period.Value<int>(LibraryProps.FromYear);
        var to = period.Value<int>(LibraryProps.ToYear);
        return from > 0 ? $"{from}–{to}" : ThemeBranchLabel;
    }

    /// <summary>Tên ngắn của giai đoạn (bỏ phần mốc năm phía trước dấu ":").</summary>
    public static string ShortName(this IPublishedContent period)
        => period.Name.Contains(':') ? period.Name.Split(':', 2)[1].Trim() : period.Name;

    public static int Year(this IPublishedContent item)
    {
        var y = item.Value<int>(LibraryProps.EventYear);
        return y > 0 ? y : item.Date().Year;
    }

    /// <summary>Giai đoạn mà tư liệu thuộc về: giai đoạn chứa nó và các giai đoạn được liên kết chéo (FR-10).</summary>
    public static IEnumerable<IPublishedContent> LinkedPeriods(this IPublishedContent item)
    {
        var own = item.PeriodOf();
        var linked = item.Value<IEnumerable<IPublishedContent>>(LibraryProps.Periods) ?? Enumerable.Empty<IPublishedContent>();
        return (own == null ? linked : linked.Prepend(own)).DistinctBy(p => p.Id);
    }

    /// <summary>Toàn bộ tư liệu thuộc phạm vi một nút cây, kể cả tư liệu ở nơi khác được gắn vào giai đoạn đó.</summary>
    public static IEnumerable<IPublishedContent> ItemsIn(IPublishedContent scope)
    {
        var inside = scope.Items();
        var linked = scope.ContentType.Alias switch
        {
            DocTypes.LibraryRoot => scope.Root().Items().Where(x => x.HasValue(LibraryProps.Periods)),
            DocTypes.Period => scope.Root().Items().Where(x => x.LinkedPeriods().Any(p => p.Id == scope.Id)),
            _ => Enumerable.Empty<IPublishedContent>(),
        };

        return inside.Concat(linked).DistinctBy(x => x.Id);
    }

    public static List<IPublishedContent> Find(IPublishedContent scope, LibraryFilter f)
    {
        var items = ItemsIn(scope);

        if (f.PeriodId is int pid)
            items = items.Where(x => x.LinkedPeriods().Any(p => p.Id == pid));
        if (!string.IsNullOrEmpty(f.Type))
            items = items.Where(x => x.ContentType.Alias == f.Type);
        if (f.FromYear is int from)
            items = items.Where(x => x.Year() >= from);
        if (f.ToYear is int to)
            items = items.Where(x => x.Year() <= to);
        if (!string.IsNullOrWhiteSpace(f.Author))
        {
            var author = SiteHelpers.Unaccent(f.Author);
            items = items.Where(x => SiteHelpers.Unaccent(x.Value<string>(LibraryProps.Author) + " " + x.Value<string>(Props.Source)).Contains(author));
        }

        if (string.IsNullOrWhiteSpace(f.Keyword))
            return items.OrderByDescending(x => x.Date()).ToList();

        var hits = scope.Root().Search(f.Keyword).Select(x => x.Id).ToList();
        var rank = hits.Select((id, i) => (id, i)).ToDictionary(t => t.id, t => t.i);
        return items.Where(x => rank.ContainsKey(x.Id)).OrderBy(x => rank[x.Id]).ToList();
    }

    public static Dictionary<string, int> CountByType(IEnumerable<IPublishedContent> items)
        => Types.ToDictionary(t => t.Alias, t => items.Count(x => x.ContentType.Alias == t.Alias));
}
