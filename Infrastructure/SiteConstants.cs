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

public static class LibraryQueryKeys
{
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
