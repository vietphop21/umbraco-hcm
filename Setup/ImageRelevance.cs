using System.Text.RegularExpressions;
using HcmPortal.Infrastructure;

namespace HcmPortal.Setup;

/// <summary>
/// Đối chiếu ảnh minh họa với nội dung: ảnh chỉ được gắn khi tên ảnh và tên tư liệu cùng chỉ một địa danh,
/// di tích hoặc sự kiện. Không khớp thì để trống (giao diện hiện nền trung tính), không lấy ảnh khác bù vào.
/// </summary>
public static class ImageRelevance
{
    private const string Mausoleum = "lang";
    private const string Square = "badinh";
    private const string Independence = "doclap";
    private const string Residence = "phuchutich";
    private const string MuseumHanoi = "baotang";
    private const string NhaRong = "nharong";
    private const string PacBo = "pacbo";
    private const string KimLien = "kimlien";
    private const string TraVinh = "travinh";
    private const string SaigonStatue = "tphcm";
    private const string Portrait = "chandung";
    private const string Sandals = "dep";

    private static readonly string[] Relics = { Mausoleum, Square, Residence, MuseumHanoi, NhaRong, PacBo, KimLien, TraVinh };

    /// <summary>Ảnh có phương tiện giao thông làm chủ thể (xe xích lô…) không dùng cho bất kỳ nội dung nào.</summary>
    private static readonly string[] Excluded = { "xe xich lo" };

    private static readonly (string Keyword, string[] Topics)[] ImageRules =
    {
        ("september 2nd 1945", new[] { Independence, Square }),
        ("mausoleum", new[] { Mausoleum }),
        ("ba dinh square", new[] { Square }),
        ("presidential palace", new[] { Residence }),
        ("house", new[] { Residence }),
        ("museum", new[] { MuseumHanoi }),
        // Hiện vật, ban thờ, không gian phục dựng trong bảo tàng không minh họa cho sự kiện tại bến cảng nên không gắn chủ đề này.
        ("tham quan ben nha rong", new[] { NhaRong }),
        ("coc bo", new[] { PacBo }),
        ("kim lien", new[] { KimLien }),
        ("tra vinh", new[] { TraVinh }),
        ("ho chi minh city", new[] { SaigonStatue }),
        ("ho chi minh 1946", new[] { Portrait }),
        ("doi dep", new[] { Sandals }),
    };

    private static readonly (string Keyword, string[] Topics)[] ContentRules =
    {
        ("lang chu tich", new[] { Mausoleum }),
        ("lang bac", new[] { Mausoleum }),
        ("ba dinh", new[] { Square }),
        ("tuyen ngon doc lap", new[] { Independence }),
        ("ngay doc lap", new[] { Independence }),
        ("2 9 1945", new[] { Independence }),
        ("mua thu nam 1945", new[] { Independence }),
        ("cach mang thang tam", new[] { Independence }),
        ("phu chu tich", new[] { Residence }),
        ("nha san", new[] { Residence }),
        ("bao tang ho chi minh", new[] { MuseumHanoi }),
        ("ben nha rong", new[] { NhaRong }),
        ("5 6 1911", new[] { NhaRong }),
        ("tim duong cuu nuoc", new[] { NhaRong }),
        ("pac bo", new[] { PacBo }),
        ("kim lien", new[] { KimLien }),
        ("lang sen", new[] { KimLien }),
        ("hoang tru", new[] { KimLien }),
        ("nam dan", new[] { KimLien }),
        ("nien thieu", new[] { KimLien }),
        ("que huong", new[] { KimLien }),
        ("tra vinh", new[] { TraVinh }),
        ("tieu su", new[] { Portrait }),
        ("chan dung", new[] { Portrait }),
        ("cuoc doi va su nghiep", new[] { Portrait }),
        ("doi dep", new[] { Sandals }),
        ("ha noi nhung dia chi", new[] { Mausoleum, Square, Residence, MuseumHanoi }),
        ("di tich luu niem", Relics),
        ("dia danh", Relics),
    };

    /// <summary>Tên thành phố chỉ là chủ đề chung: chỉ dùng khi tiêu đề không nêu di tích, sự kiện cụ thể nào.</summary>
    private static readonly (string Keyword, string[] Topics)[] CityRules =
    {
        ("sai gon", new[] { SaigonStatue, NhaRong }),
        ("thanh pho mang ten bac", new[] { SaigonStatue, NhaRong }),
        ("mien nam", new[] { SaigonStatue, NhaRong }),
        ("tp hcm", new[] { SaigonStatue, NhaRong }),
        ("tp ho chi minh", new[] { SaigonStatue, NhaRong }),
    };

    /// <summary>Phần "tại [địa phương]" ở cuối tiêu đề tin chỉ nơi tổ chức sự kiện, không phải chủ đề của ảnh.</summary>
    private static readonly Regex EventPlace = new(@"\s+tại\s+.*$", RegexOptions.IgnoreCase);
    private static readonly Regex NonWord = new("[^a-z0-9]+");

    private static string Normalize(string? s) => " " + NonWord.Replace(SiteHelpers.Unaccent(s), " ").Trim() + " ";

    private static HashSet<string> Match(string text, (string Keyword, string[] Topics)[] rules)
        => rules.Where(r => text.Contains($" {r.Keyword} ")).SelectMany(r => r.Topics).ToHashSet();

    public static bool Fits(string? title, string? imageName) => ContentTopics(title).Overlaps(ImageTopics(imageName));

    public static bool IsExcluded(string? imageName) => Excluded.Any(Normalize(imageName).Contains);

    public static HashSet<string> ImageTopics(string? imageName)
        => IsExcluded(imageName) ? new() : Match(Normalize(imageName), ImageRules);

    public static HashSet<string> ContentTopics(string? title)
    {
        var text = Normalize(EventPlace.Replace(title ?? string.Empty, string.Empty));
        var topics = Match(text, ContentRules);
        return topics.Count > 0 ? topics : Match(text, CityRules);
    }
}
