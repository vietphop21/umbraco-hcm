namespace HcmPortal.Infrastructure;

public static class Cultures
{
    public const string Vi = "vi-VN";
    public const string En = "en-US";
    public const string ViName = "Tiếng Việt";
    public const string EnName = "English";
    public const string EnDomain = "/en";
    public const string ViHtml = "vi";
    public const string EnHtml = "en";
    public const string ViLabel = "VI";
    public const string EnLabel = "EN";
}

public static class DocTypes
{
    public const string Home = "home";
    public const string Category = "category";
    public const string Article = "article";
    public const string Video = "videoItem";
    public const string Album = "photoAlbum";
    public const string Audio = "audioItem";
    public const string TextPage = "textPage";
    public const string SearchPage = "searchPage";
    public const string SitemapPage = "sitemapPage";
    public const string LibraryRoot = "libraryRoot";
    public const string Period = "period";
    public const string PeriodTopic = "periodTopic";
    public const string PdfDocument = "pdfDocument";
}

public static class LibraryProps
{
    public const string FromYear = "fromYear";
    public const string ToYear = "toYear";
    public const string EventYear = "eventYear";
    public const string Periods = "periods";
    public const string PdfFile = "pdfFile";
    public const string Author = "author";
    public const string Thumbnail = "thumbnail";
    public const string VideoFile = "videoFile";
    public const string AudioFile = "audioFile";
    public const string Hide = "umbracoNaviHide";
}

public record SiteSection(string Code, string Name, string MenuName, string UrlName, string Icon, string Description);

/// <summary>11 chuyên mục nội dung theo Đề xuất site map (21/10/2026); thứ tự và URL cố định.</summary>
public static class SiteMap
{
    public const string UrlNameProp = "umbracoUrlName";

    /// <summary>Đơn vị thực hiện CSDL chuyên đề (theo Đề xuất site map 21/10/2026).</summary>
    public const string OrgName = "Thư viện Khoa học Tổng hợp TP. Hồ Chí Minh";
    public const string Copyright = "Bản quyền thuộc " + OrgName + ". Ghi rõ nguồn khi phát hành lại thông tin từ cơ sở dữ liệu này.";
    public const string Intro = "gioi-thieu";
    public const string Life = "cuoc-doi-su-nghiep";
    public const string Thought = "tu-tuong-dao-duc-phong-cach";
    public const string Saigon = "ho-chi-minh-voi-tphcm";
    public const string Works = "tac-pham";
    public const string Kids = "chuyen-ke-thieu-nhi";
    public const string Follow = "hoc-tap-lam-theo";
    public const string Sectors = "cac-nganh-gioi";
    public const string Documents = "van-ban";
    public const string Media = "tu-lieu";
    public const string Map = "ban-do";
    public const string Periods = "giai-doan";
    public const string Search = "tra-cuu";
    public const string News = "tin-tuc";

    public static readonly SiteSection[] Sections =
    {
        new("2.1", "Giới thiệu Không gian Văn hóa Hồ Chí Minh", "Giới thiệu", Intro, "fa-landmark-flag",
            "Mục đích, nội dung và cách sử dụng Không gian Văn hóa Hồ Chí Minh trên nền tảng số."),
        new("2.2", "Cuộc đời và sự nghiệp Chủ tịch Hồ Chí Minh", "Cuộc đời & sự nghiệp", Life, "fa-timeline",
            "Tiểu sử, biên niên và dòng thời gian các mốc lịch sử quan trọng."),
        new("2.3", "Tư tưởng – Đạo đức – Phong cách Hồ Chí Minh", "Tư tưởng – Đạo đức – Phong cách", Thought, "fa-lightbulb",
            "Tư tưởng, đạo đức, phong cách và quan điểm về văn hóa, giáo dục."),
        new("2.4", "Hồ Chí Minh với Sài Gòn – Gia Định và TP.HCM", "Với TP.HCM", Saigon, "fa-city",
            "Dấu ấn của Chủ tịch Hồ Chí Minh với Sài Gòn – Gia Định và Thành phố mang tên Người."),
        new("2.5", "Tác phẩm của Chủ tịch Hồ Chí Minh", "Tác phẩm", Works, "fa-book",
            "Các tác phẩm, bài viết, bài nói của Chủ tịch Hồ Chí Minh."),
        new("2.6", "Chuyện kể về Bác Hồ & Góc thiếu nhi", "Góc thiếu nhi", Kids, "fa-child-reaching",
            "Chuyện kể, sách thiếu nhi, đố vui và trò chơi."),
        new("2.7", "Học tập và làm theo tư tưởng, đạo đức, phong cách Hồ Chí Minh", "Học tập & làm theo", Follow, "fa-hand-holding-heart",
            "Gương điển hình, mô hình hay trong học tập và làm theo Bác."),
        new("2.8", "Hồ Chí Minh với các ngành, các giới", "Các ngành, các giới", Sectors, "fa-people-group",
            "Chủ tịch Hồ Chí Minh với các ngành, các giới và tầng lớp nhân dân."),
        new("2.9", "Văn bản và chính sách liên quan", "Văn bản", Documents, "fa-file-contract",
            "Văn bản, chính sách, hướng dẫn liên quan (PDF)."),
        new("2.10", "Thư viện tư liệu số", "Tư liệu số", Media, "fa-photo-film",
            "Phim tư liệu, hình ảnh, sách nói và tủ sách điện tử."),
        new("2.11", "Bản đồ số – Địa danh gắn với Hồ Chí Minh", "Bản đồ số", Map, "fa-map-location-dot",
            "Các địa danh, di tích gắn với cuộc đời Chủ tịch Hồ Chí Minh."),
    };

    /// <summary>Dòng thời gian tương tác ở chuyên mục 2.2 (8 mốc theo đề xuất).</summary>
    public static readonly int[] Milestones = { 1890, 1911, 1920, 1930, 1945, 1954, 1960, 1969 };
}

public static class SiteMapCovers
{
    public const string Introduction = "/media/sections/2.1-gioi-thieu-bao-tang-ho-chi-minh.jpg";
    public const string Life = "/media/j3jnagiz/ho-chi-minh-1946.jpg";
    public const string Thought = "/media/0yjf02uo/ho-chi-minh-museum-hanoi-2.jpg";
    public const string Saigon = "/media/zmbjlwqb/h%E1%BB%8Dc-vi%C3%AAn-an-ninh-tham-quan-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng.jpg";
    public const string Works = "/media/ahahdmp1/hanoi-vietnam-ho-chi-minh-museum-02.jpg";
    public const string Children = "/media/3dhkhyrz/%C4%91%C3%B4i-d%C3%A9p-b%C3%A1c-h%E1%BB%93-t%E1%BA%A1i-b%E1%BA%A3o-t%C3%A0ng-b%E1%BA%BFn-nh%C3%A0-r%E1%BB%93ng-n%C4%83m-2018-2.jpg";
    public const string Follow = "/media/i55mnvfw/honour-guards-at-ho-chi-minh-mausoleum-hanoi-20240123-1133-3240.jpg";
    public const string Sectors = "/media/d3njnpcd/hanoi-vietnam-visitors-at-ho-chi-minh-museum.jpg";
    public const string Documents = "/media/ffoml1n5/ba-dinh-square-september-2nd-1945.jpg";
    public const string Media = "/media/dc4dkpna/oct2025visit-to-the-ho-chi-minh-museum-hanoi-vietnam-03.jpg";
    public const string Map = "/media/o3earjbj/kim-lien-monuments-park.jpg";
}

/// <summary>Bản đồ số (2.11): mỗi địa danh là một bài viết có thêm vĩ độ, kinh độ.</summary>
public static class MapProps
{
    public const string Lat = "latitude";
    public const string Lng = "longitude";
    public const double DefaultLat = 16.2;
    public const double DefaultLng = 106.0;
    public const int DefaultZoom = 5;
    public const double EmbedDelta = 0.01;
}

public static class LibraryQueryKeys
{
    public const string Author = "author";
    public const string Keyword = "q";
    public const string Period = "period";
    public const string Type = "type";
    public const string FromYear = "from";
    public const string ToYear = "to";
    public const string Page = "page";
}

public static class LibrarySettings
{
    public const int PageSize = 12;
    public const int MinYear = 1890;
    public const int MaxYear = 1969;
}

public static class Accessibility
{
    public const string ZoomKey = "a11y-zoom";
    public const string ContrastKey = "a11y-contrast";
    public const string SpeechLang = "vi-VN";
    public const int MinZoom = 100;
    public const int MaxZoom = 200;
    public const int ZoomStep = 25;
}

public static class Props
{
    public const string SiteName = "siteName";
    public const string Slogan = "slogan";
    public const string FooterOrg = "footerOrg";
    public const string FooterCopyright = "footerCopyright";
    public const string FooterAddress = "footerAddress";
    public const string WebsiteLinks = "websiteLinks";
    public const string Description = "description";
    public const string Summary = "summary";
    public const string BodyText = "bodyText";
    public const string Source = "source";
}

public static class Theme
{
    public const string StorageKey = "theme";
    public const string Dark = "dark";
    public const string Light = "light";
}
