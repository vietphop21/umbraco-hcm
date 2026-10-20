using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace HcmPortal.Setup;

public class SiteSetupComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<UmbracoApplicationStartedNotification, SiteSetupHandler>();
}

/// <summary>
/// Tạo Document Types, Templates và nội dung demo.
/// Khi tăng <see cref="SeedVersion"/>, toàn bộ nội dung + media demo được xoá và tạo lại.
/// </summary>
public class SiteSetupHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private const int SeedVersion = 2;

    private readonly IContentTypeService _contentTypes;
    private readonly IDataTypeService _dataTypes;
    private readonly IFileService _files;
    private readonly IContentService _content;
    private readonly IMediaService _media;
    private readonly MediaFileManager _mediaFiles;
    private readonly MediaUrlGeneratorCollection _urlGenerators;
    private readonly IContentTypeBaseServiceProvider _typeProvider;
    private readonly IShortStringHelper _sh;
    private readonly IRuntimeState _runtime;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SiteSetupHandler> _logger;

    public SiteSetupHandler(IContentTypeService contentTypes, IDataTypeService dataTypes, IFileService files,
        IContentService content, IMediaService media, MediaFileManager mediaFiles, MediaUrlGeneratorCollection urlGenerators,
        IContentTypeBaseServiceProvider typeProvider, IShortStringHelper sh, IRuntimeState runtime,
        IWebHostEnvironment env, ILogger<SiteSetupHandler> logger)
    {
        _contentTypes = contentTypes;
        _dataTypes = dataTypes;
        _files = files;
        _content = content;
        _media = media;
        _mediaFiles = mediaFiles;
        _urlGenerators = urlGenerators;
        _typeProvider = typeProvider;
        _sh = sh;
        _runtime = runtime;
        _env = env;
        _logger = logger;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_runtime.Level != RuntimeLevel.Run) return;

        try
        {
            EnsureSchema();

            var versionFile = Path.Combine(_env.ContentRootPath, "umbraco", "Data", "seed-version.txt");
            var current = System.IO.File.Exists(versionFile) && int.TryParse(System.IO.File.ReadAllText(versionFile), out var v) ? v : 0;
            if (current < SeedVersion || !_content.GetRootContent().Any())
            {
                _logger.LogInformation("Tạo lại nội dung demo (phiên bản {Version})", SeedVersion);
                ClearAll();
                SeedContent();
                System.IO.File.WriteAllText(versionFile, SeedVersion.ToString());
            }
            EnsureDemoPages();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể khởi tạo cấu trúc website");
        }
    }

    // ================= Schema =================

    public static readonly (string Alias, string Name)[] HomeSections =
    {
        ("learnSection", "Khối: Học và làm theo Bác"),
        ("thoughtSection", "Khối: Tư tưởng, đạo đức, phong cách"),
        ("videoSection", "Khối: Tư liệu Video"),
        ("museumSection", "Khối: Bảo tàng 3D"),
        ("newsSection", "Khối: Tin tức"),
        ("photoSection", "Khối: Tư liệu Ảnh"),
        ("audioSection", "Khối: Tư liệu Audio"),
        ("megastorySection", "Khối Multimedia: Megastory"),
        ("infographicSection", "Khối Multimedia: Infographics"),
        ("worldSection", "Khối: Hồ Chí Minh và thế giới"),
    };

    private void EnsureSchema()
    {
        EnsureTemplate("Master", "master");
        var tHome = EnsureTemplate("Home", "home");
        var tCategory = EnsureTemplate("Category", "category");
        var tDetail = EnsureTemplate("Detail", "detail");
        var tSearch = EnsureTemplate("Search", "search");
        var tPage = EnsureTemplate("Page", "page");
        var tSitemap = EnsureTemplate("Sitemap", "sitemap");

        var museum = EnsureType("museumItem", "Ô Bảo tàng 3D", "icon-museum", null, ct =>
        {
            Prop(ct, "content", "Nội dung", "thumbnail", "Ảnh đại diện", "Image Media Picker");
            Prop(ct, "content", "Nội dung", "link", "Đường dẫn (URL)", "Textstring");
        });
        var article = EnsureType("article", "Bài viết", "icon-article", tDetail, ArticleProps);
        var video = EnsureType("videoItem", "Video", "icon-video", tDetail, ct =>
        {
            ArticleProps(ct);
            Prop(ct, "media", "Video", "youtubeUrl", "Link YouTube", "Textstring");
            Prop(ct, "media", "Video", "videoFile", "Hoặc tải file video (mp4)", "Media Picker");
        });
        var album = EnsureType("photoAlbum", "Album ảnh", "icon-pictures-alt-2", tDetail, ct =>
        {
            ArticleProps(ct);
            Prop(ct, "media", "Ảnh", "photos", "Danh sách ảnh", "Multiple Image Media Picker");
        });
        var audio = EnsureType("audioItem", "Audio", "icon-sound-waves", tDetail, ct =>
        {
            ArticleProps(ct);
            Prop(ct, "media", "Audio", "audioFile", "File audio (mp3)", "Media Picker");
        });
        var category = EnsureType("category", "Chuyên mục", "icon-folder", tCategory, ct =>
        {
            Prop(ct, "content", "Nội dung", "description", "Mô tả", "Textarea");
            Prop(ct, "content", "Nội dung", "umbracoNaviHide", "Ẩn khỏi menu", "True/false");
        });
        var search = EnsureType("searchPage", "Trang tìm kiếm", "icon-search", tSearch, ct =>
            Prop(ct, "content", "Nội dung", "umbracoNaviHide", "Ẩn khỏi menu", "True/false"));
        var page = EnsureType("textPage", "Trang nội dung", "icon-document", tPage, ct =>
        {
            Prop(ct, "content", "Nội dung", "bodyText", "Nội dung", "Richtext editor");
            Prop(ct, "content", "Nội dung", "showContactForm", "Hiện form liên hệ", "True/false");
            Prop(ct, "content", "Nội dung", "umbracoNaviHide", "Ẩn khỏi menu", "True/false");
        });
        var sitemap = EnsureType("sitemapPage", "Sơ đồ website", "icon-sitemap", tSitemap, ct =>
            Prop(ct, "content", "Nội dung", "umbracoNaviHide", "Ẩn khỏi menu", "True/false"));
        var message = EnsureType("contactMessage", "Thư liên hệ", "icon-message", null, ct =>
        {
            Prop(ct, "content", "Nội dung", "senderEmail", "Email người gửi", "Textstring");
            Prop(ct, "content", "Nội dung", "senderPhone", "Số điện thoại", "Textstring");
            Prop(ct, "content", "Nội dung", "messageText", "Nội dung", "Textarea");
        });
        var home = EnsureType("home", "Trang chủ", "icon-home", tHome, _ => { });
        home.AllowedAsRoot = true;
        Prop(home, "header", "Đầu trang", "siteName", "Tên website", "Textstring");
        Prop(home, "header", "Đầu trang", "slogan", "Khẩu hiệu dưới tên website", "Textstring");
        Prop(home, "header", "Đầu trang", "logo", "Logo", "Image Media Picker");
        Prop(home, "header", "Đầu trang", "bannerImage", "Ảnh banner (desktop) – thay toàn bộ đầu trang", "Image Media Picker");
        Prop(home, "header", "Đầu trang", "bannerMobile", "Ảnh banner (mobile)", "Image Media Picker");
        foreach (var (alias, name) in HomeSections)
            Prop(home, "sections", "Khối trang chủ", alias, name, "Content Picker");
        Prop(home, "sections", "Khối trang chủ", "wideBanner", "Banner ngang giữa trang", "Image Media Picker");
        Prop(home, "sections", "Khối trang chủ", "wideBannerLink", "Link banner ngang", "Content Picker");
        Prop(home, "sections", "Khối trang chủ", "adBanners", "Banner quảng cáo cột phải", "Multiple Image Media Picker");
        Prop(home, "footer", "Chân trang", "footerOrg", "Cơ quan chủ quản", "Textstring");
        Prop(home, "footer", "Chân trang", "footerCopyright", "Bản quyền", "Textarea");
        Prop(home, "footer", "Chân trang", "footerAddress", "Địa chỉ", "Textstring");
        Prop(home, "footer", "Chân trang", "footerEmail", "Email", "Textstring");
        Prop(home, "footer", "Chân trang", "websiteLinks", "Liên kết website (mỗi dòng: Tên|URL)", "Textarea");
        _contentTypes.Save(home);

        AllowChildren(category, category, article, video, album, audio, museum);
        AllowChildren(page, message);
        AllowChildren(home, category, search, page, sitemap);
    }

    private void ArticleProps(IContentType ct)
    {
        Prop(ct, "content", "Nội dung", "summary", "Sapo / tóm tắt", "Textarea");
        Prop(ct, "content", "Nội dung", "thumbnail", "Ảnh đại diện", "Image Media Picker");
        Prop(ct, "content", "Nội dung", "bodyText", "Nội dung", "Richtext editor");
        Prop(ct, "content", "Nội dung", "publishDate", "Ngày đăng", "Date Picker with time");
        Prop(ct, "content", "Nội dung", "source", "Nguồn", "Textstring");
    }

    private ITemplate EnsureTemplate(string name, string alias)
    {
        var existing = _files.GetTemplate(alias);
        if (existing != null) return existing;
        var path = Path.Combine(_env.ContentRootPath, "Views", name + ".cshtml");
        var body = System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
        return _files.CreateTemplateWithIdentity(name, alias, body);
    }

    private IContentType EnsureType(string alias, string name, string icon, ITemplate? template, Action<IContentType> configure)
    {
        var existing = _contentTypes.Get(alias);
        if (existing != null) return existing;

        var ct = new ContentType(_sh, -1) { Alias = alias, Name = name, Icon = icon };
        if (template != null)
        {
            ct.AllowedTemplates = new[] { template };
            ct.SetDefaultTemplate(template);
        }
        configure(ct);
        _contentTypes.Save(ct);
        return ct;
    }

    private void Prop(IContentType ct, string groupAlias, string groupName, string alias, string name, string dataTypeName)
    {
        if (ct.PropertyTypeExists(alias)) return;
        var dt = _dataTypes.GetAll().FirstOrDefault(d => d.Name == dataTypeName)
                 ?? throw new InvalidOperationException($"Không tìm thấy data type '{dataTypeName}'");
        ct.AddPropertyType(new PropertyType(_sh, dt, alias) { Name = name }, groupAlias, groupName);
    }

    private void AllowChildren(IContentType parent, params IContentType[] children)
    {
        parent.AllowedContentTypes = children
            .Select((c, i) => new ContentTypeSort(new Lazy<int>(() => c.Id), i, c.Alias))
            .ToList();
        _contentTypes.Save(parent);
    }

    // ================= Media =================

    private readonly Random _rnd = new(2026);
    private readonly List<IMedia> _pool = new();
    private int _poolIndex;

    /// <summary>Tải ảnh minh họa miễn phí (Lorem Picsum / Unsplash license) vào thư mục Media.</summary>
    private void BuildImagePool(int count)
    {
        var folder = _media.CreateMedia("Ảnh minh họa (demo)", Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
        _media.Save(folder);

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        var downloads = Enumerable.Range(1, count).AsParallel().WithDegreeOfParallelism(8).Select(i =>
        {
            try { return (i, data: http.GetByteArrayAsync($"https://picsum.photos/seed/hcm{i}/1200/750").Result); }
            catch { return (i, data: (byte[]?)null); }
        }).Where(x => x.data != null).OrderBy(x => x.i).ToList();

        foreach (var (i, data) in downloads)
        {
            var m = _media.CreateMedia($"anh-minh-hoa-{i:00}", folder.Id, Constants.Conventions.MediaTypes.Image);
            using var ms = new MemoryStream(data!);
            m.SetValue(_mediaFiles, _urlGenerators, _sh, _typeProvider, Constants.Conventions.Media.File, $"anh-minh-hoa-{i:00}.jpg", ms);
            _media.Save(m);
            _pool.Add(m);
        }
        _logger.LogInformation("Đã tải {Count} ảnh minh họa", _pool.Count);
    }

    private IMedia? NextImage() => _pool.Count == 0 ? null : _pool[_poolIndex++ % _pool.Count];

    private static string Pick(params IMedia[] media)
        => JsonSerializer.Serialize(media.Select(m => new { key = Guid.NewGuid(), mediaKey = m.Key }));

    private string? ImgUrl(IMedia? m) => m?.GetUrl(Constants.Conventions.Media.File, _urlGenerators);

    // ================= Nội dung =================

    private void ClearAll()
    {
        foreach (var c in _content.GetRootContent().ToList()) _content.Delete(c);
        _content.EmptyRecycleBin(Constants.Security.SuperUserId);
        foreach (var m in _media.GetRootMedia().ToList()) _media.Delete(m);
        _media.EmptyRecycleBin(Constants.Security.SuperUserId);
    }

    private int _hours;

    private IContent Create(string name, IContent parent, string type, Action<IContent>? set = null)
    {
        var c = _content.Create(name, parent.Id, type);
        set?.Invoke(c);
        _content.SaveAndPublish(c);
        return c;
    }

    private void Items(IContent parent, string type, string[] titles, string[] pool)
    {
        foreach (var title in titles)
        {
            _hours += _rnd.Next(5, 30);
            var img = NextImage();
            Create(title, parent, type, c =>
            {
                c.SetValue("summary", SeedData.Summary(pool, _rnd));
                var inline = ImgUrl(NextImage()) ?? "";
                c.SetValue("bodyText", SeedData.Body(pool, _rnd).Replace("{IMG}", inline));
                c.SetValue("publishDate", DateTime.Now.AddHours(-_hours));
                c.SetValue("source", SeedData.Sources[_rnd.Next(SeedData.Sources.Length)]);
                if (img != null) c.SetValue("thumbnail", Pick(img));
                if (type == "photoAlbum" && _pool.Count > 0)
                    c.SetValue("photos", Pick(Enumerable.Range(0, 8).Select(_ => NextImage()!).ToArray()));
            });
        }
    }

    private static string Udi(IContent c) => Umbraco.Cms.Core.Udi.Create(Constants.UdiEntityType.Document, c.Key).ToString();

    private void SeedContent()
    {
        BuildImagePool(48);

        var home = _content.Create("Trang chủ", Constants.System.Root, "home");
        home.SetValue("siteName", "Hồ Chí Minh");
        home.SetValue("slogan", "Cổng thông tin tư liệu – học tập – nghiên cứu");
        home.SetValue("footerOrg", "Cơ quan chủ quản: Ban Biên tập Cổng thông tin (demo)");
        home.SetValue("footerCopyright", "Bản quyền thuộc về Ban Biên tập. Ghi rõ nguồn khi phát hành lại thông tin từ website này.");
        home.SetValue("footerAddress", "Số 1, đường Mẫu, quận Demo, Hà Nội");
        home.SetValue("footerEmail", "toasoan@example.vn");
        home.SetValue("websiteLinks", "Cổng Thông tin điện tử Chính phủ|https://chinhphu.vn\nBáo Nhân Dân|https://nhandan.vn\nThông tấn xã Việt Nam|https://vnanet.vn\nĐài Truyền hình Việt Nam|https://vtv.vn\nĐài Tiếng nói Việt Nam|https://vov.vn");
        _content.SaveAndPublish(home);

        var news = Create("Tin tức", home, "category");
        Items(news, "article", SeedData.News, SeedData.ParagraphsNews);

        var life = Create("Cuộc đời, sự nghiệp", home, "category");
        Items(Create("Tiểu sử", life, "category"), "article", SeedData.Biography, SeedData.ParagraphsThought);
        var chronicle = Create("Biên niên tiểu sử", life, "category");
        Items(chronicle, "article", SeedData.Chronicle, SeedData.ParagraphsThought);

        var thought = Create("Tư tưởng, đạo đức, phong cách Hồ Chí Minh", home, "category");
        Items(Create("Nguồn gốc hình thành", thought, "category"), "article", SeedData.ThoughtOrigin, SeedData.ParagraphsThought);
        Items(Create("Nội dung tư tưởng đạo đức", thought, "category"), "article", SeedData.ThoughtContent, SeedData.ParagraphsThought);
        Items(Create("Nghiên cứu", thought, "category"), "article", SeedData.Research, SeedData.ParagraphsThought);

        var library = Create("Tủ sách điện tử Hồ Chí Minh", home, "category");
        Items(Create("Tác phẩm về Hồ Chí Minh", library, "category"), "article", SeedData.BooksAbout, SeedData.ParagraphsLibrary);
        Items(Create("Tác phẩm của Hồ Chí Minh", library, "category"), "article", SeedData.BooksBy, SeedData.ParagraphsLibrary);

        var world = Create("Hồ Chí Minh và thế giới", home, "category");
        Items(Create("Hoạt động quốc tế", world, "category"), "article", SeedData.WorldActivities, SeedData.ParagraphsWorld);
        Items(Create("Bạn bè quốc tế", world, "category"), "article", SeedData.WorldFriends, SeedData.ParagraphsWorld);

        var learn = Create("Học và làm theo Bác", home, "category");
        Items(Create("Di chúc", learn, "category"), "article", SeedData.Testament, SeedData.ParagraphsThought);
        Items(Create("Văn bản", learn, "category"), "article", SeedData.Documents, SeedData.ParagraphsNews);
        Items(Create("Gương điển hình", learn, "category"), "article", SeedData.Learn, SeedData.ParagraphsLearn);

        IContent Hidden(string name) => Create(name, home, "category", c => c.SetValue("umbracoNaviHide", true));

        var videos = Hidden("Tư liệu Video");
        Items(videos, "videoItem", SeedData.Videos, SeedData.ParagraphsNews);
        var photos = Hidden("Tư liệu Ảnh");
        Items(photos, "photoAlbum", SeedData.Albums, SeedData.ParagraphsNews);
        var audios = Hidden("Tư liệu Audio");
        Items(audios, "audioItem", SeedData.Audios, SeedData.ParagraphsLearn);
        var mega = Hidden("Megastory");
        Items(mega, "article", SeedData.Megastory, SeedData.ParagraphsWorld);
        var info = Hidden("Infographics");
        Items(info, "article", SeedData.Infographics, SeedData.ParagraphsThought);

        var museum = Hidden("Bảo tàng 3D");
        foreach (var name in SeedData.Museums)
        {
            var img = NextImage();
            Create(name, museum, "museumItem", c =>
            {
                c.SetValue("link", "#");
                if (img != null) c.SetValue("thumbnail", Pick(img));
            });
        }

        Create("Tìm kiếm", home, "searchPage", c => c.SetValue("umbracoNaviHide", true));

        home.SetValue("learnSection", Udi(learn));
        home.SetValue("thoughtSection", Udi(thought));
        home.SetValue("videoSection", Udi(videos));
        home.SetValue("museumSection", Udi(museum));
        home.SetValue("newsSection", Udi(news));
        home.SetValue("photoSection", Udi(photos));
        home.SetValue("audioSection", Udi(audios));
        home.SetValue("megastorySection", Udi(mega));
        home.SetValue("infographicSection", Udi(info));
        home.SetValue("worldSection", Udi(world));
        home.SetValue("wideBannerLink", Udi(chronicle));
        _content.SaveAndPublish(home);
    }

    private void EnsureDemoPages()
    {
        var home = _content.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == "home");
        if (home == null) return;
        var existing = _content.GetPagedChildren(home.Id, 0, 500, out _).Select(x => x.Name).ToHashSet();

        void Page(string name, string type, string body = "", bool form = false)
        {
            if (existing.Contains(name)) return;
            Create(name, home, type, c =>
            {
                c.SetValue("umbracoNaviHide", true);
                if (type != "textPage") return;
                c.SetValue("bodyText", body);
                c.SetValue("showContactForm", form);
            });
        }

        Page("Giới thiệu", "textPage",
            "<p>Cổng thông tin là kênh cung cấp tư liệu, tin tức về cuộc đời, sự nghiệp, tư tưởng, đạo đức, phong cách Hồ Chí Minh; phục vụ công tác tuyên truyền, giáo dục, nghiên cứu và học tập.</p>" +
            "<h2>Mục tiêu</h2><ul><li>Cung cấp nguồn tư liệu chính thống, tin cậy.</li><li>Số hóa, lưu trữ ảnh, video, audio tư liệu.</li><li>Lan tỏa phong trào học tập và làm theo tấm gương đạo đức.</li></ul>" +
            "<h2>Chuyên mục chính</h2><p>Tin tức; Cuộc đời, sự nghiệp; Tư tưởng, đạo đức, phong cách; Tủ sách điện tử; Hồ Chí Minh và thế giới; Học và làm theo Bác; Multimedia.</p>" +
            "<p><em>Nội dung trang này là dữ liệu demo, quản trị viên chỉnh sửa trong trang quản trị.</em></p>");
        Page("Liên hệ", "textPage",
            "<p>Mọi ý kiến đóng góp, đề nghị cung cấp tư liệu xin gửi về Ban Biên tập theo biểu mẫu dưới đây hoặc qua email ở chân trang. Chúng tôi sẽ phản hồi trong thời gian sớm nhất.</p>", form: true);
        Page("Sơ đồ website", "sitemapPage");
    }
}
