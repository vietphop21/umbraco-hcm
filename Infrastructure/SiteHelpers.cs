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

    private static readonly Dictionary<string, (string Name, string Url)[]> SectionImageChoices = new()
    {
        [SiteMap.Intro] = new[] { ("museum interior", "/media/eiqnfkok/hanoi-vietnam-interiors-of-ho-chi-minh-museum.jpg"), ("museum pyramid", "/media/dtjdmsue/hanoi-vietnam-ho-chi-minh-museum-the-pyramid.jpg"), ("museum visitors", "/media/d3njnpcd/hanoi-vietnam-visitors-at-ho-chi-minh-museum.jpg"), ("museum visit", "/media/msiksn4w/oct2025visit-to-the-ho-chi-minh-museum-hanoi-vietnam-01.jpg") },
        [SiteMap.Life] = new[] { ("ho chi minh 1946 portrait", "/media/j3jnagiz/ho-chi-minh-1946.jpg"), ("ho chi minh stilt house", "/media/ogthylib/ho-chi-minhs-stilt-house-hanoi-1-38439116266.jpg"), ("ho chi minh house", "/media/p4yexqht/ho-chi-minhs-stilt-house-hanoi-2-24622695398.jpg"), ("mausoleum", "/media/hlqpoqqr/hanoi-vietnam-mausoleum-of-ho-chi-minh-01.jpg"), ("presidential palace", "/media/oofjatxw/presidential-palace-hanoi-2012.jpg") },
        [SiteMap.Thought] = new[] { ("ho chi minh portrait", "/media/j3jnagiz/ho-chi-minh-1946.jpg"), ("museum interior", "/media/eiqnfkok/hanoi-vietnam-interiors-of-ho-chi-minh-museum.jpg"), ("museum visitors", "/media/d3njnpcd/hanoi-vietnam-visitors-at-ho-chi-minh-museum.jpg"), ("museum hanoi", "/media/0yjf02uo/ho-chi-minh-museum-hanoi-2.jpg") },
        [SiteMap.Saigon] = new[] { ("nha rong visitors", "/media/zmbjlwqb/h%E1%BB%8Dc-vi%C3%AAn-an-ninh-tham-quan-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng.jpg"), ("nha rong reconstruction", "/media/fpbddsa3/ph%E1%BB%A5c-d%E1%BB%B1ng-ng%C3%B5-c%C3%B4ng-poanh-t%E1%BA%A1i-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng.jpg"), ("ho chi minh city statue", "/media/t2kdvknj/ho-chi-minh-city-ho-chi-minh-statue-2020-01-cn-01.jpg"), ("city statue", "/media/hoylnvaw/ho-chi-minh-city-ho-chi-minh-statue-2020-01-cn-02.jpg"), ("nha rong altar", "/media/0fkl25ti/ban-th%E1%BB%9D-b%C3%A1c-h%E1%BB%93-t%E1%BA%A1i-b%E1%BA%A3o-t%C3%A0ng-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng-n%C4%83m-2018.jpg") },
        [SiteMap.Works] = new[] { ("museum works", "/media/ahahdmp1/hanoi-vietnam-ho-chi-minh-museum-02.jpg"), ("museum archive", "/media/be3ovcqm/hanoi-vietnam-ho-chi-minh-museum-01.jpg"), ("museum pyramid", "/media/dtjdmsue/hanoi-vietnam-ho-chi-minh-museum-the-pyramid.jpg"), ("ho chi minh portrait", "/media/j3jnagiz/ho-chi-minh-1946.jpg") },
        [SiteMap.Kids] = new[] { ("doi dep sandals", "/media/3dhkhyrz/%C4%91%C3%B4i-d%C3%A9p-b%C3%A1c-h%E1%BB%93-t%E1%BA%A1i-b%E1%BA%A3o-t%C3%A0ng-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng-n%C4%83m-2018-2.jpg"), ("nha rong visitors", "/media/zmbjlwqb/h%E1%BB%8Dc-vi%C3%AAn-an-ninh-tham-quan-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng.jpg"), ("ho chi minh portrait", "/media/j3jnagiz/ho-chi-minh-1946.jpg"), ("museum visitors", "/media/d3njnpcd/hanoi-vietnam-visitors-at-ho-chi-minh-museum.jpg") },
        [SiteMap.Follow] = new[] { ("mausoleum honour guards", "/media/i55mnvfw/honour-guards-at-ho-chi-minh-mausoleum-hanoi-20240123-1133-3240.jpg"), ("stilt house", "/media/ogthylib/ho-chi-minhs-stilt-house-hanoi-1-38439116266.jpg"), ("museum visitors", "/media/d3njnpcd/hanoi-vietnam-visitors-at-ho-chi-minh-museum.jpg"), ("ho chi minh portrait", "/media/j3jnagiz/ho-chi-minh-1946.jpg") },
        [SiteMap.Sectors] = new[] { ("museum visitors", "/media/d3njnpcd/hanoi-vietnam-visitors-at-ho-chi-minh-museum.jpg"), ("city statue", "/media/t2kdvknj/ho-chi-minh-city-ho-chi-minh-statue-2020-01-cn-01.jpg"), ("museum interior", "/media/eiqnfkok/hanoi-vietnam-interiors-of-ho-chi-minh-museum.jpg"), ("ho chi minh portrait", "/media/j3jnagiz/ho-chi-minh-1946.jpg") },
        [SiteMap.Documents] = new[] { ("ba dinh september 1945", "/media/ffoml1n5/ba-dinh-square-september-2nd-1945.jpg"), ("ba dinh square", "/media/ze1dj1ts/ba-dinh-square-2007.jpg"), ("ba dinh panorama", "/media/fdrp4qsi/ba-dinh-square-panorama.jpg"), ("mausoleum on ba dinh square", "/media/kdfftt23/hanoi-vietnam-ba-dinh-square-and-ho-chi-minh-mausoleum.jpg") },
        [SiteMap.Media] = new[] { ("museum visit october 2025", "/media/msiksn4w/oct2025visit-to-the-ho-chi-minh-museum-hanoi-vietnam-01.jpg"), ("museum visit", "/media/dc4dkpna/oct2025visit-to-the-ho-chi-minh-museum-hanoi-vietnam-03.jpg"), ("museum visitors", "/media/d3njnpcd/hanoi-vietnam-visitors-at-ho-chi-minh-museum.jpg"), ("museum interior", "/media/eiqnfkok/hanoi-vietnam-interiors-of-ho-chi-minh-museum.jpg"), ("doi dep", "/media/3dhkhyrz/%C4%91%C3%B4i-d%C3%A9p-b%C3%A1c-h%E1%BB%93-t%E1%BA%A1i-b%E1%BA%A3o-t%C3%A0ng-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng-n%C4%83m-2018-2.jpg") },
        [SiteMap.Map] = new[] { ("kim lien monuments", "/media/o3earjbj/kim-lien-monuments-park.jpg"), ("coc bo pac bo", "/media/4x5jr20o/c%E1%BB%91c-b%C3%B3.jpg"), ("tra vinh memorial", "/media/tkfljsx1/%C4%91%E1%BB%81n-th%E1%BB%9D-b%C3%A1c-h%E1%BB%93-%E1%BB%9F-tr%C3%A0-vinh.jpg"), ("nha rong", "/media/zmbjlwqb/h%E1%BB%8Dc-vi%C3%AAn-an-ninh-tham-quan-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng.jpg") },
    };
    private static readonly (string Name, string Url)[] GeneralImageChoices = SectionImageChoices.Values
        .SelectMany(images => images).DistinctBy(image => image.Url).ToArray();

    /// <summary>Returns a topic-matched image and avoids reusing a URL within the current page.</summary>
    public static string? ContentImage(this IPublishedContent? item, int width, int height, ISet<string>? usedImages = null)
    {
        if (item == null) return null;
        if (item.ContentType.Alias == DocTypes.Album)
        {
            var photo = item.Value<IEnumerable<IPublishedContent>>("photos")?.FirstOrDefault();
            var photoUrl = photo?.GetCropUrl(width: width, height: height, imageCropMode: ImageCropMode.Crop);
            var photoKey = photo?.Url() ?? photoUrl;
            if (photoUrl != null && (usedImages == null || usedImages.Add(ImageUsageKey(photoKey!)))) return photoUrl;
        }

        var ownMedia = item.Value<IPublishedContent>(LibraryProps.Thumbnail);
        string? own = ownMedia?.GetCropUrl(width: width, height: height, imageCropMode: ImageCropMode.Crop);
        var ownKey = ownMedia?.Url() ?? own;
        if (own != null && (usedImages == null || usedImages.Add(ImageUsageKey(ownKey!)))) return own;

        var section = item.Ancestors().Append(item).FirstOrDefault(x =>
            SiteMap.Sections.Any(s => (x.Value<string>(SiteMap.UrlNameProp, Cultures.Vi) ?? x.Value<string>(SiteMap.UrlNameProp)) == s.UrlName));
        var slug = section?.Value<string>(SiteMap.UrlNameProp, Cultures.Vi) ?? section?.Value<string>(SiteMap.UrlNameProp);
        var choices = SectionImageChoices.GetValueOrDefault(slug ?? string.Empty);
        if (choices == null && item.Ancestors().Any(x => x.ContentType.Alias == DocTypes.LibraryRoot))
            choices = SectionImageChoices[SiteMap.Life];
        if (choices == null) return null;

        var relevant = choices.Where(c => Setup.ImageRelevance.Fits(item.Name, c.Name)).ToArray();
        var ordered = relevant.Concat(choices.Except(relevant)).Concat(GeneralImageChoices)
            .DistinctBy(c => c.Url).ToArray();
        var candidate = ordered.FirstOrDefault(c => usedImages == null || !usedImages.Contains(ImageUsageKey(c.Url)));
        if (candidate == default) candidate = ordered[0];
        usedImages?.Add(ImageUsageKey(candidate.Url));
        return candidate.Url;
    }

    private static string ImageUsageKey(string path) => Uri.UnescapeDataString(path.Split('?')[0]);

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
