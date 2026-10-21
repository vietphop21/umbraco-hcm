using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace HcmPortal.Infrastructure;

public static class SiteHelpers
{
    public static readonly string[] ItemTypes = { "article", "videoItem", "photoAlbum", "audioItem" };

    /// <summary>Bài viết (mọi loại) nằm dưới một chuyên mục, mới nhất trước.</summary>
    public static IEnumerable<IPublishedContent> Items(this IPublishedContent? section, int take = int.MaxValue)
        => section == null
            ? Enumerable.Empty<IPublishedContent>()
            : section.Descendants()
                .Where(x => ItemTypes.Contains(x.ContentType.Alias))
                .OrderByDescending(x => x.Date())
                .Take(take);

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

    public static IPublishedContent? Picked(this IPublishedContent home, string alias)
        => home.Value<IPublishedContent>(alias);

    public static bool InMenu(this IPublishedContent x)
        => x.ContentType.Alias == "category" && !x.Value<bool>("umbracoNaviHide");

    public static IEnumerable<IPublishedContent> MenuChildren(this IPublishedContent x)
        => x.Children().Where(InMenu);

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
