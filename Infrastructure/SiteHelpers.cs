using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace HcmPortal.Infrastructure;

public static class SiteHelpers
{
    public static readonly string[] ItemTypes = { DocTypes.Article, DocTypes.Video, DocTypes.Album, DocTypes.Audio, DocTypes.PdfDocument };

    /// <summary>Bản tiếng Anh nằm dưới /en; còn lại là tiếng Việt.</summary>
    public static bool IsEn =>
        Umbraco.Cms.Web.Common.DependencyInjection.StaticServiceProvider.Instance.GetService<IVariationContextAccessor>()?.VariationContext?.Culture is { } c && c.Equals(Cultures.En, StringComparison.OrdinalIgnoreCase);

    public static string HtmlLang => IsEn ? Cultures.EnHtml : Cultures.ViHtml;

    public static string OtherCulture => IsEn ? Cultures.Vi : Cultures.En;

    public static string OtherLabel => IsEn ? Cultures.ViLabel : Cultures.EnLabel;

    public static string T(string vi) => IsEn && UiText.En.TryGetValue(vi, out var en) ? en : vi;

    public static string DayName(DateTime d) => (IsEn ? UiText.EnDays : UiText.ViDays)[(int)d.DayOfWeek];

    /// <summary>Trang chưa có bản ngôn ngữ kia thì chuyển về trang chủ của ngôn ngữ đó.</summary>
    public static string SwitchLanguageUrl(this IPublishedContent page, IPublishedContent home)
        => page.Cultures.ContainsKey(OtherCulture) ? page.Url(OtherCulture) : home.Url(OtherCulture);

    /// <summary>Bài viết (mọi loại) nằm dưới một chuyên mục, mới nhất trước.</summary>
    public static IEnumerable<IPublishedContent> Items(this IPublishedContent? section, int take = int.MaxValue)
        => section == null
            ? Enumerable.Empty<IPublishedContent>()
            : section.Descendants()
                .Where(x => ItemTypes.Contains(x.ContentType.Alias))
                .OrderByDescending(x => x.Date())
                .Take(take);

    /// <summary>Tổng số bài (mọi loại) trong chuyên mục, gồm cả chuyên mục con.</summary>
    public static int ItemCount(this IPublishedContent? section) => section.Items().Count();

    public static Microsoft.AspNetCore.Html.IHtmlContent CountBadge(int count)
        => new Microsoft.AspNetCore.Html.HtmlString($"<span class=\"item-count\">({count:N0})</span>");

    public static DateTime Date(this IPublishedContent item)
    {
        var d = item.Value<DateTime>("publishDate");
        return d == default ? item.CreateDate : d;
    }

    public static string DateText(this IPublishedContent item) => item.Date().ToString("dd/MM/yyyy HH:mm");

    /// <summary>URL ảnh cắt theo kích thước; null nếu chưa có ảnh.</summary>
    public static string? Img(this IPublishedContent? item, int width, int height, string alias = "thumbnail")
    {
        var media = item?.Value<IPublishedContent>(alias);
        return media?.GetCropUrl(width: width, height: height, imageCropMode: ImageCropMode.Crop);
    }

    /// <summary>URL ảnh thu nhỏ theo chiều rộng, giữ nguyên tỉ lệ (không cắt); dùng khi cần thấy đủ ảnh.</summary>
    public static string? ImgFull(this IPublishedContent? item, int width, string alias = "thumbnail")
        => item?.Value<IPublishedContent>(alias)?.GetCropUrl(width: width);

    /// <summary>Ảnh đại diện có phải ảnh ngang (rộng ≥ 1,2 lần cao) – dùng cho ảnh bìa thẻ để phủ kín khung mà ít bị cắt.</summary>
    public static bool HasLandscapeImage(this IPublishedContent item, string alias = "thumbnail")
    {
        var media = item.Value<IPublishedContent>(alias);
        if (media == null) return false;
        var w = media.Value<int>(Umbraco.Cms.Core.Constants.Conventions.Media.Width);
        var h = media.Value<int>(Umbraco.Cms.Core.Constants.Conventions.Media.Height);
        return h > 0 && w >= h * 1.2;
    }

    /// <summary>Ảnh gốc của ảnh đại diện (dùng làm khoá để không lặp ảnh giữa các thẻ).</summary>
    public static string? ImgKey(this IPublishedContent item, string alias = "thumbnail")
        => item.Value<IPublishedContent>(alias)?.Key.ToString();

    public static IPublishedContent? Picked(this IPublishedContent home, string alias)
        => home.Value<IPublishedContent>(alias);

    public static bool InMenu(this IPublishedContent x)
        => x.ContentType.Alias is DocTypes.Category or DocTypes.LibraryRoot && !x.Value<bool>(LibraryProps.Hide);

    public static IEnumerable<IPublishedContent> MenuChildren(this IPublishedContent x)
        => x.Children().Where(InMenu);

    /// <summary>Tọa độ địa danh (Bản đồ số); null nếu bài chưa nhập đủ vĩ độ, kinh độ.</summary>
    public static (double Lat, double Lng)? Coordinates(this IPublishedContent item)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var ns = System.Globalization.NumberStyles.Float;
        return double.TryParse(item.Value<string>(MapProps.Lat), ns, inv, out var lat)
               && double.TryParse(item.Value<string>(MapProps.Lng), ns, inv, out var lng)
            ? (lat, lng) : null;
    }

    /// <summary>Khung nhúng OpenStreetMap cho một địa danh.</summary>
    public static string MapEmbedUrl(double lat, double lng)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var d = MapProps.EmbedDelta;
        return string.Create(inv, $"https://www.openstreetmap.org/export/embed.html?bbox={lng - d},{lat - d},{lng + d},{lat + d}&layer=mapnik&marker={lat},{lng}");
    }

    /// <summary>Node của một chuyên mục trong site map, tìm theo đường dẫn cố định.</summary>
    public static IPublishedContent? Section(this IPublishedContent any, string urlName)
        => any.Root().Children().FirstOrDefault(c =>
            (c.Value<string>(SiteMap.UrlNameProp, Cultures.Vi) ?? c.Value<string>(SiteMap.UrlNameProp)) == urlName);

    /// <summary>11 chuyên mục theo đúng thứ tự site map; chuyên mục chưa tạo trên cây thì bỏ qua.</summary>
    public static IEnumerable<(SiteSection Info, IPublishedContent Node)> Sections(this IPublishedContent any)
        => SiteMap.Sections.Select(s => (s, any.Section(s.UrlName))).Where(t => t.Item2 != null)!;

    /// <summary>Bỏ dấu tiếng Việt + chữ thường, dùng cho tìm kiếm không dấu.</summary>
    public static string Unaccent(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var d = s.ToLowerInvariant().Replace('đ', 'd').Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(d.Length);
        foreach (var ch in d)
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        return sb.ToString();
    }

    /// <summary>Tìm bài theo tiêu đề/sapo/nội dung, có dấu hoặc không dấu đều được. Tiêu đề khớp được xếp trước.</summary>
    public static List<IPublishedContent> Search(this IPublishedContent root, string query)
    {
        var words = Unaccent(query).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return new();
        var strip = new System.Text.RegularExpressions.Regex("<[^>]+>");
        return root.Items()
            .Select(x =>
            {
                var title = Unaccent(x.Name);
                var text = title + " " + Unaccent(x.Value<string>("summary")) + " " + Unaccent(strip.Replace(x.Value<string>("bodyText") ?? "", " "));
                if (!words.All(text.Contains)) return (x, score: -1);
                return (x, score: words.Count(title.Contains));
            })
            .Where(r => r.score >= 0)
            .OrderByDescending(r => r.score).ThenByDescending(r => r.x.Date())
            .Select(r => r.x)
            .ToList();
    }

    /// <summary>Chuyển link YouTube bất kỳ thành link embed.</summary>
    public static string? YoutubeEmbed(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri) return null;
        string? id = null;
        if (uri.Host.Contains("youtu.be")) id = uri.AbsolutePath.Trim('/');
        else if (uri.AbsolutePath.StartsWith("/embed/")) id = uri.AbsolutePath[7..];
        else
        {
            var q = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (q.TryGetValue("v", out var v)) id = v.ToString();
        }
        return string.IsNullOrEmpty(id) ? null : $"https://www.youtube.com/embed/{id}";
    }
}
